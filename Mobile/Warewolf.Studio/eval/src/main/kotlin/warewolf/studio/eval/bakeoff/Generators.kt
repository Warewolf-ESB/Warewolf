package warewolf.studio.eval.bakeoff

import kotlinx.serialization.json.JsonArray
import kotlinx.serialization.json.JsonElement
import kotlinx.serialization.json.JsonObject
import kotlinx.serialization.json.JsonPrimitive
import kotlinx.serialization.json.buildJsonArray
import kotlinx.serialization.json.buildJsonObject
import kotlinx.serialization.json.contentOrNull
import kotlinx.serialization.json.jsonArray
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive
import kotlinx.serialization.json.put
import kotlinx.serialization.json.putJsonArray
import kotlinx.serialization.json.putJsonObject
import warewolf.studio.generator.Action
import warewolf.studio.generator.BranchKind
import warewolf.studio.generator.BranchTail
import warewolf.studio.generator.Budget
import warewolf.studio.generator.ChoiceContext
import warewolf.studio.generator.FrontierLoop
import warewolf.studio.generator.NextActionChooser
import warewolf.studio.generator.StepSink
import warewolf.studio.generator.StepSummary
import warewolf.studio.generator.Tool

/** What one contender produced for one goal. */
data class Outcome(
    /** `{envelope, body}`, or null when nothing usable came back. */
    val workflow: JsonObject?,
    val validation: EngineClient.Validation,
    val firstTryValid: Boolean,
    /** Model calls spent on the workflow: the first try plus repairs. */
    val attempts: Int,
    val note: String? = null,
)

fun interface WorkflowGenerator {
    fun generate(id: String, goal: String): Outcome
}

/**
 * Contenders B and C: one model writes the whole workflow, the validator checks it, and up to [repairs] more
 * attempts each see the validator's errors. [constrained] holds a local model to [workflowFormat]; the frontier
 * model gets the same prompt with no constraint.
 */
class WholeWorkflowGenerator(
    private val model: TextModel,
    private val catalog: Catalog,
    private val engine: EngineClient,
    private val workflowSchema: String,
    private val constrained: Boolean,
    private val repairs: Int = 2,
) : WorkflowGenerator {
    private val format = if (constrained) workflowFormat(catalog) else null

    override fun generate(id: String, goal: String): Outcome {
        val base = WorkflowText.prompt(goal, catalog, workflowSchema)
        var prompt = base
        var best: JsonObject? = null
        var firstTryValid = false
        var validation = EngineClient.Validation(false, listOf("no attempt made"))
        for (attempt in 1..repairs + 1) {
            val reply = model.complete(prompt, format)
            val workflow = WorkflowText.extractWorkflow(reply)
            validation = if (workflow == null) {
                EngineClient.Validation(false, listOf(NOT_A_WORKFLOW))
            } else {
                engine.validate(workflow.getValue("envelope"), workflow.getValue("body"))
            }
            if (workflow != null) best = workflow
            if (attempt == 1) firstTryValid = validation.valid
            if (validation.valid) return Outcome(workflow, validation, firstTryValid, attempt)
            prompt = base + "\n\n" + WorkflowText.repairPrompt(workflow?.toString() ?: reply.take(4_000), validation.errors)
        }
        return Outcome(best, validation, firstTryValid, repairs + 1)
    }

    companion object {
        const val NOT_A_WORKFLOW = "the reply is not a JSON object with \"envelope\" and \"body\""
    }
}

/**
 * Contender A: System 1 picks each step ([chooser]) and the small model fills in that step's fields ([filler]).
 * The body is assembled here, with stable node ids, and the engine validates the partial workflow after every
 * step; a step that brings new errors is refilled with them, up to [repairs] times. (`add_step` cannot be used:
 * a Decision's node id changes on every read, so nothing can be attached to its arms, and a Decision placed
 * without a condition leaves the saved workflow unreadable.) Variables are declared by code, as outputs.
 */
class System1Generator(
    private val chooser: NextActionChooser,
    private val filler: FieldFiller,
    private val tools: List<Tool>,
    private val catalog: Catalog,
    private val engine: EngineClient,
    private val budget: Budget = Budget(maxSteps = 30, maxDepth = 15),
    private val repairs: Int = 2,
) : WorkflowGenerator {
    override fun generate(id: String, goal: String): Outcome {
        val sink = BodySink(goal, filler, catalog, engine, repairs)
        val result = FrontierLoop(CaseNamingChooser(chooser, filler, goal), sink, budget).run(goal, tools)
        if (result.steps.isEmpty()) {
            return Outcome(null, EngineClient.Validation(false, listOf("System 1 ended the workflow before placing a step")), false, sink.calls)
        }
        val workflow = sink.workflow()
        val validation = engine.validate(workflow.getValue("envelope"), workflow.getValue("body"))
        return Outcome(workflow, validation, validation.valid && sink.refills == 0, sink.calls,
            "stop: ${result.stopReason}, truncated ${result.truncatedBranches}, steps still failing ${sink.stillFailing}")
    }
}

/** Builds System 1's workflow body step by step, filling each step's fields and checking it with the validator. */
internal class BodySink(
    private val goal: String,
    private val filler: FieldFiller,
    private val catalog: Catalog,
    private val engine: EngineClient,
    private val repairs: Int,
) : StepSink {
    private val cells = mutableListOf<JsonObject>(buildJsonObject {
        put("id", START); put("shape", START); putJsonObject("data") { put("type", START) }
    })
    private val summaries = mutableMapOf<String, StepSummary>()

    /** Model calls spent filling fields, how many were refills, and steps kept despite errors after every refill. */
    var calls = 0
        private set
    var refills = 0
        private set
    var stillFailing = 0
        private set

    override fun place(tail: BranchTail, tool: Tool): String {
        val entry = catalog.named(tool.name) ?: throw IllegalStateException("'${tool.name}' is not in the engine's catalog")
        val id = "n${summaries.size + 1}"
        val before = errorsNow().toSet()
        var error: String? = null
        var node: JsonObject? = null
        var label = tool.name
        for (attempt in 0..repairs) {
            if (attempt > 0) refills++
            calls++
            val fill = filler.fill(goal, tail, entry, error)
            label = fill.label
            val data = JsonObject(fill.fields + mapOf("type" to JsonPrimitive(entry.dataType), "displayname" to JsonPrimitive(fill.label)))
            node = buildJsonObject { put("id", id); put("shape", entry.dataType); put("data", data) }
            val trial = cells + node + edge(tail, id)
            val newErrors = errorsOf(trial).filter { it !in before && !it.contains(EngineClient.UNDECLARED) }
            if (newErrors.isEmpty()) break
            error = newErrors.take(5).joinToString("; ")
            if (attempt == repairs) stillFailing++
        }
        cells += node!!
        cells += edge(tail, id)
        summaries[id] = StepSummary(tool.name, label, WorkflowText.variablesIn(node))
        return id
    }

    override fun summary(stepId: String, tool: Tool): StepSummary = summaries[stepId] ?: StepSummary(tool.name)

    fun workflow(): JsonObject = buildJsonObject {
        put("envelope", envelope(cells))
        put("body", body(cells))
    }

    private fun edge(tail: BranchTail, target: String) = buildJsonObject {
        put("shape", "edge")
        putJsonObject("source") { put("cell", tail.afterStepId ?: START) }
        putJsonObject("target") { put("cell", target) }
        when (tail.arm) {
            null -> {}
            "True", "False" -> putJsonObject("data") { put("isDecisionArm", true); put("isTrue", tail.arm == "True") }
            else -> put("label", tail.arm)
        }
    }

    private fun errorsNow(): List<String> = if (cells.size <= 1) emptyList() else errorsOf(cells)

    private fun errorsOf(trial: List<JsonObject>): List<String> = engine.validate(envelope(trial), body(trial)).errors

    private fun envelope(trial: List<JsonObject>) = buildJsonObject {
        put("name", "system1")
        put("inputs", buildJsonArray { })
        put("outputs", WorkflowText.declarations(WorkflowText.variablesIn(JsonArray(trial))))
    }

    private fun body(trial: List<JsonObject>) = buildJsonObject {
        put("resourcename", "system1")
        put("cells", JsonArray(trial))
    }

    private companion object {
        const val START = "start"
    }
}

/** System 1 picks Switch but not its cases; the small model names them, as it fills the Switch's fields. */
internal class CaseNamingChooser(
    private val inner: NextActionChooser,
    private val filler: FieldFiller,
    private val goal: String,
) : NextActionChooser {
    override fun choose(context: ChoiceContext): Action {
        val action = inner.choose(context)
        if (action is Action.Place && action.tool.branchKind == BranchKind.SWITCH && action.arms.isEmpty()) {
            return action.copy(arms = filler.cases(goal, context.tail))
        }
        return action
    }
}

/**
 * The small model's half of System 1 (Plan.md §2, step 1): fill one tool's fields, held by a JSON schema to
 * that tool's field names, with string, array, boolean and number types read from the schema's prose.
 */
class FieldFiller(private val model: TextModel) {
    data class Fill(val label: String, val fields: Map<String, JsonElement>)

    fun fill(goal: String, tail: BranchTail, tool: CatalogTool, error: String?): Fill {
        val prompt = buildString {
            appendLine("You are filling in one step of a Warewolf workflow.")
            appendLine("Goal: $goal")
            appendLine("Steps so far on this branch: ${tail.describePath()}")
            tail.arm?.let { appendLine("This step is on the '$it' branch of the previous step.") }
            val inScope = tail.steps.flatMap { it.variables }.distinct()
            appendLine("Variables already in use: ${inScope.joinToString(", ") { "[[$it]]" }.ifEmpty { "(none)" }}")
            appendLine("The new step uses the '${tool.name}' tool: ${tool.description}")
            appendLine("Its fields:")
            tool.fields.forEach { (field, description) -> appendLine("- $field: $description") }
            appendLine("Write a short label for the step and fill in its fields. Use Warewolf variables like [[name]], [[recordset(*).field]] or [[@object.property]]; reuse variables already in use where they fit.")
            error?.let { appendLine("Your previous attempt was rejected: $it") }
        }
        val reply = model.complete(prompt, format(tool))
        val parsed = WorkflowText.firstObject(reply) ?: return Fill(tool.name, emptyMap())
        val label = parsed["label"]?.jsonPrimitive?.contentOrNull?.takeIf { it.isNotBlank() } ?: tool.name
        return Fill(label, (parsed["fields"] as? JsonObject).orEmpty())
    }

    fun cases(goal: String, tail: BranchTail): List<String> {
        val prompt = "A Warewolf workflow for this goal branches with a Switch.\nGoal: $goal\nSteps so far: ${tail.describePath()}\n" +
            "Name the Switch's cases (2 to 4 short labels)."
        val format = buildJsonObject {
            put("type", "object"); putJsonArray("required") { add(JsonPrimitive("cases")) }
            putJsonObject("properties") {
                putJsonObject("cases") { put("type", "array"); put("minItems", 2); put("maxItems", 4); putJsonObject("items") { put("type", "string") } }
            }
        }
        val cases = WorkflowText.firstObject(model.complete(prompt, format))?.get("cases")?.jsonArray
            ?.mapNotNull { it.jsonPrimitive.contentOrNull?.trim()?.takeIf(String::isNotEmpty) }?.distinct().orEmpty()
        return cases.ifEmpty { listOf("Case 1", "Case 2") }
    }

    /** `{label, fields}` with one property per field, typed and required as the field's prose says. */
    internal fun format(tool: CatalogTool): JsonObject = buildJsonObject {
        put("type", "object")
        putJsonArray("required") { add(JsonPrimitive("label")); add(JsonPrimitive("fields")) }
        putJsonObject("properties") {
            putJsonObject("label") { put("type", "string") }
            putJsonObject("fields") {
                put("type", "object")
                putJsonObject("properties") {
                    tool.fields.forEach { (field, description) -> put(field, fieldType(description)) }
                }
                putJsonArray("required") {
                    tool.fields.filterValues { isRequired(it) }.keys.forEach { add(JsonPrimitive(it)) }
                }
            }
        }
    }

    internal companion object {
        /** "array, required — …" → `{"type": "array"}`; prose that names no JSON type is treated as a string. */
        fun fieldType(description: String): JsonObject {
            val type = description.substringBefore(',').substringBefore(' ').trim().lowercase()
            return buildJsonObject {
                when (type) {
                    "array" -> put("type", "array")
                    "boolean" -> put("type", "boolean")
                    "integer", "int" -> put("type", "integer")
                    "number" -> put("type", "number")
                    "object" -> put("type", "object")
                    else -> put("type", "string")
                }
            }
        }

        fun isRequired(description: String): Boolean = description.substringBefore('—').contains("required")
    }
}
