package warewolf.studio.eval.bakeoff

import com.fasterxml.jackson.databind.ObjectMapper
import com.networknt.schema.JsonSchemaFactory
import com.networknt.schema.SchemaLocation
import com.networknt.schema.SpecVersion
import kotlinx.serialization.json.Json
import kotlinx.serialization.json.JsonArray
import kotlinx.serialization.json.JsonElement
import kotlinx.serialization.json.JsonObject
import kotlinx.serialization.json.JsonPrimitive
import kotlinx.serialization.json.buildJsonObject
import kotlinx.serialization.json.contentOrNull
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive
import kotlinx.serialization.json.put
import java.util.UUID

/*
 * The artifact comparison (Plan.md, "Service tests and input forms"): given a workflow that already exists, can a
 * model write a service test for it, or the input form a person fills in to run it? The subjects are the valid
 * workflows one contender (Opus) produced in the whole-workflow comparison, stored in the engine.
 */

/** A workflow stored in the engine for the artifact tasks: [definition] is `{envelope, body}` as the engine reads it back. */
data class Subject(val id: String, val goal: String, val name: String, val definition: JsonObject) {
    val envelope: JsonObject get() = definition.getValue("envelope").jsonObject
    val inputs: List<JsonObject> get() = (envelope["inputs"] as? JsonArray).orEmpty().map { it.jsonObject }
    val outputs: List<JsonObject> get() = (envelope["outputs"] as? JsonArray).orEmpty().map { it.jsonObject }
}

object Subjects {
    private val GUID = Regex("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$")

    /**
     * [workflow] with every node id that is not a GUID replaced by one derived from it (same id in, same GUID out),
     * wherever a node id is referenced: the node itself, edge ends, a nested node's `parentId` and `UniqueID`.
     * The engine stores non-GUID ids, but `execute_test` cannot mock a step whose id is not a GUID.
     */
    fun withGuidIds(workflow: JsonObject): JsonObject {
        val body = workflow.getValue("body").jsonObject
        val cells = (body["cells"] as? JsonArray).orEmpty().map { it.jsonObject }
        val ids = cells.filter { it["source"] !is JsonObject }.mapNotNull { it["id"]?.jsonPrimitive?.contentOrNull }
            .filterNot { GUID.matches(it) }
            .associateWith { UUID.nameUUIDFromBytes("bakeoff:$it".toByteArray()).toString() }
        if (ids.isEmpty()) return workflow
        fun mapped(e: JsonElement?): JsonElement? = (e as? JsonPrimitive)?.contentOrNull?.let { ids[it] }?.let(::JsonPrimitive)
        fun end(e: JsonElement?): JsonElement? = (e as? JsonObject)?.let { o -> mapped(o["cell"])?.let { JsonObject(o + ("cell" to it)) } }
        val rewritten = cells.map { cell ->
            val replaced = cell.toMutableMap()
            mapped(cell["id"])?.let { replaced["id"] = it }
            end(cell["source"])?.let { replaced["source"] = it }
            end(cell["target"])?.let { replaced["target"] = it }
            (cell["data"] as? JsonObject)?.let { data ->
                val d = data.toMutableMap()
                mapped(data["parentId"])?.let { d["parentId"] = it }
                mapped(data["UniqueID"])?.let { d["UniqueID"] = it }
                replaced["data"] = JsonObject(d)
            }
            JsonObject(replaced)
        }
        return JsonObject(workflow + ("body" to JsonObject(body + ("cells" to JsonArray(rewritten)))))
    }
}

/** What one contender produced for one artifact task on one subject. */
data class ArtifactOutcome(
    /** The test definition or form schema last produced, or null when no reply parsed. */
    val artifact: JsonObject?,
    /** Accepted: stored by `create_test`, or a form that passes [FormCheck]. */
    val accepted: Boolean,
    val firstTryAccepted: Boolean,
    val attempts: Int,
    /** Task-specific scores (test run result, coverage, …) recorded alongside. */
    val scores: JsonObject,
    val errors: List<String>,
)

fun interface ArtifactGenerator {
    fun generate(subject: Subject): ArtifactOutcome
}

/**
 * Task A: the model writes a service test, `create_test` stores it (a rejection goes back to the model, up to
 * [repairs] times), and `execute_test` runs the stored test. Sources are not configured, so a test only runs clean
 * when the model mocks every step that reaches outside the workflow.
 */
class ServiceTestGenerator(
    private val model: TextModel,
    private val engine: EngineClient,
    private val catalog: Catalog,
    private val testSchema: String,
    constrained: Boolean,
    /** Prefix for the stored test names, unique per run so a rerun never collides with an earlier test. */
    private val testNamePrefix: String,
    private val repairs: Int = 2,
) : ArtifactGenerator {
    private val format = if (constrained) testFormat() else null

    override fun generate(subject: Subject): ArtifactOutcome {
        val base = prompt(subject)
        var prompt = base
        var last: JsonObject? = null
        var errors = listOf("no attempt made")
        for (attempt in 1..repairs + 1) {
            val reply = model.complete(prompt, format)
            val test = WorkflowText.firstObject(reply)
            errors = if (test == null) listOf(NOT_A_TEST) else {
                val named = JsonObject(test + ("testName" to JsonPrimitive("${testNamePrefix}_$attempt")))
                last = named
                val stored = engine.invoke("create_test", buildJsonObject { put("name", subject.name); put("test", named) })
                if (stored.ok) return ArtifactOutcome(named, true, attempt == 1, attempt, run(subject, named), emptyList())
                listOf(stored.error ?: "create_test failed")
            }
            prompt = base + "\n\n" + repairPrompt(last?.toString() ?: reply.take(4_000), errors)
        }
        return ArtifactOutcome(last, false, false, repairs + 1, coverage(subject, last), errors)
    }

    private fun run(subject: Subject, test: JsonObject): JsonObject {
        val reply = engine.invoke("execute_test", buildJsonObject {
            put("name", subject.name); put("testName", test.getValue("testName").jsonPrimitive.content)
        })
        val result = if (reply.ok) (reply.json as? JsonObject)?.get("result")?.jsonPrimitive?.contentOrNull else null
        val message = if (reply.ok) (reply.json as? JsonObject)?.get("message")?.jsonPrimitive?.contentOrNull else reply.error
        return JsonObject(coverage(subject, test) + buildJsonObject {
            put("result", result ?: "NotRun")
            message?.let { put("message", it.take(500)) }
        })
    }

    internal fun prompt(subject: Subject): String = buildString {
        appendLine("You write a Warewolf service test for an existing workflow. Warewolf workflows are flowcharts of tools")
        appendLine("(Assign, Decision, database and web calls, ...) that pass data in [[variables]]. A test sets the workflow's")
        appendLine("inputs, can mock steps (replace a step's outputs) or assert on them, and asserts the workflow's outputs.")
        appendLine()
        appendLine("Goal of the workflow: ${subject.goal}")
        appendLine()
        appendLine("Workflow (steps are named by node id):")
        appendLine(WorkflowText.summary(subject.definition, catalog, showIds = true))
        appendLine("Test definition format (get_workflow_schema's test_schema):")
        appendLine(testSchema)
        appendLine()
        appendLine("Rules:")
        appendLine("- No sources (databases, web services, email, files, ...) are configured here, so every step that reaches")
        appendLine("  outside the workflow, including calls to other workflows, must be in testSteps with type Mock and realistic")
        appendLine("  stepOutputs for the variables it sets.")
        appendLine("- Do not put Decision or Switch steps in testSteps; their node ids are not stable.")
        appendLine("- activityId must be one of the node ids above; activityType is that step's type.")
        appendLine("- Choose realistic inputs that exercise the main path, and assert every workflow output with the exact value")
        appendLine("  the workflow would produce; use another assertOp only where the exact value cannot be known.")
        appendLine("- Reply with the test definition JSON only.")
    }

    private fun repairPrompt(previous: String, errors: List<String>): String = buildString {
        appendLine("Your previous test was rejected by the engine:")
        errors.forEach { appendLine("- $it") }
        appendLine()
        appendLine("Previous test:")
        appendLine(previous)
        appendLine()
        appendLine("Fix it and reply with the corrected test definition JSON only.")
    }

    internal companion object {
        const val NOT_A_TEST = "the reply is not a JSON object"

        /** How many of the workflow's outputs the test asserts, and how many steps it mocks or asserts. */
        fun coverage(subject: Subject, test: JsonObject?): JsonObject {
            val asserted = (test?.get("outputs") as? JsonArray).orEmpty()
                .mapNotNull { (it as? JsonObject)?.get("variable")?.jsonPrimitive?.contentOrNull }
                .flatMap(WorkflowText::variables).map { it.substringBefore('(').substringBefore('.') }.toSet()
            val outputs = subject.outputs.mapNotNull { it["name"]?.jsonPrimitive?.contentOrNull }
            val steps = (test?.get("testSteps") as? JsonArray).orEmpty().mapNotNull { it as? JsonObject }
            return buildJsonObject {
                put("outputs", outputs.size)
                put("outputsAsserted", outputs.count { it in asserted })
                put("mocks", steps.count { it["type"]?.jsonPrimitive?.contentOrNull.equals("Mock", true) })
                put("stepAsserts", steps.count { it["type"]?.jsonPrimitive?.contentOrNull.equals("Assert", true) })
            }
        }

        val ASSERT_OPS = listOf(
            "=", "<> (Not Equal)", "<", "<=", ">", ">=", "Contains", "Doesn't Contain", "Starts With", "Doesn't Start With",
            "Ends With", "Doesn't End With", "Is Between", "Not Between", "Is NULL", "Is Not NULL", "Is Numeric", "Not Numeric",
            "Is Text", "Not Text", "Is Alphanumeric", "Not Alphanumeric", "Is XML", "Not XML", "Is Date", "Not Date", "Is Email",
            "Not Email", "Is Regex", "Not Regex", "Is Binary", "Not Binary", "Is Hex", "Not Hex", "Is Base64", "Not Base64",
            "There is An Error", "There is No Error",
        )

        /** The JSON-schema constraint for a local model's test definition. */
        fun testFormat(): JsonObject {
            val ops = ASSERT_OPS.joinToString(",") { Json.encodeToString(it) }
            return Json.parseToJsonElement(
                """
                {"type":"object","required":["testName","inputs","outputs","testSteps"],"properties":{
                  "testName":{"type":"string"},
                  "inputs":{"type":"array","items":{"type":"object","required":["variable","value"],"properties":{
                    "variable":{"type":"string"},"value":{"type":"string"}}}},
                  "outputs":{"type":"array","items":{"${'$'}ref":"#/${'$'}defs/assertion"}},
                  "noErrorExpected":{"type":"boolean"},
                  "testSteps":{"type":"array","items":{"type":"object","required":["activityId","activityType","type","stepOutputs"],"properties":{
                    "activityId":{"type":"string"},"activityType":{"type":"string"},"type":{"enum":["Mock","Assert"]},
                    "stepDescription":{"type":"string"},
                    "stepOutputs":{"type":"array","items":{"${'$'}ref":"#/${'$'}defs/assertion"}}}}}},
                 "${'$'}defs":{"assertion":{"type":"object","required":["variable","assertOp","value"],"properties":{
                    "variable":{"type":"string"},"assertOp":{"enum":[$ops]},"value":{"type":"string"},
                    "from":{"type":"string"},"to":{"type":"string"}}}}}
                """.trimIndent(),
            ).jsonObject
        }
    }
}

/**
 * Task B: the model writes the workflow's input form as a JSON Schema (2020-12) document for the studio to render,
 * checked by [FormCheck]; a failing form goes back to the model with the problems, up to [repairs] times.
 */
class InputFormGenerator(
    private val model: TextModel,
    private val catalog: Catalog,
    constrained: Boolean,
    private val repairs: Int = 2,
) : ArtifactGenerator {
    private val format = if (constrained) FORM_FORMAT else null

    override fun generate(subject: Subject): ArtifactOutcome {
        val base = prompt(subject)
        var prompt = base
        var last: JsonObject? = null
        var check = FormCheck.Result(listOf(NOT_A_FORM), emptyList(), emptyList(), emptyList(), 0)
        for (attempt in 1..repairs + 1) {
            val reply = model.complete(prompt, format)
            val form = WorkflowText.firstObject(reply)
            check = if (form == null) FormCheck.Result(listOf(NOT_A_FORM), emptyList(), emptyList(), emptyList(), 0) else {
                last = form
                FormCheck.check(form, subject.inputs)
            }
            if (check.ok) return ArtifactOutcome(form, true, attempt == 1, attempt, check.scores(subject.inputs.size), emptyList())
            prompt = base + "\n\n" + repairPrompt(form?.toString() ?: reply.take(4_000), check.errors)
        }
        return ArtifactOutcome(last, false, false, repairs + 1, check.scores(subject.inputs.size), check.errors)
    }

    internal fun prompt(subject: Subject): String = buildString {
        appendLine("Design the input form a person fills in to run a Warewolf workflow. Warewolf workflows are flowcharts of")
        appendLine("tools that pass data in [[variables]]; the form collects the workflow's inputs. Write it as one JSON Schema")
        appendLine("(draft 2020-12) document, which the studio renders as the form.")
        appendLine()
        appendLine("Goal of the workflow: ${subject.goal}")
        appendLine()
        appendLine("Inputs:")
        subject.inputs.forEach { input ->
            val fields = (input["fields"] as? JsonArray)?.mapNotNull { it.jsonPrimitive.contentOrNull }.orEmpty()
            appendLine("- ${input.str("name")} (${input.str("kind")}${if (fields.isEmpty()) "" else ", fields: ${fields.joinToString(", ")}"})")
        }
        appendLine()
        appendLine("How the workflow uses them:")
        appendLine(WorkflowText.summary(subject.definition, catalog))
        appendLine("Rules:")
        appendLine("- The root is {\"type\": \"object\", \"properties\": {...}} with exactly one property per input, named exactly as")
        appendLine("  the input is, and nothing else.")
        appendLine("- A scalar input is a string, number, integer or boolean, whichever its use calls for; an object input is an")
        appendLine("  object; a recordset input is an array of objects whose items have one property per field.")
        appendLine("- Give every property a title and a description a person would understand, and add format, enum, default,")
        appendLine("  minimum/maximum, maxLength, pattern and required where the workflow's use of the input justifies them.")
        appendLine("- Reply with the JSON Schema only.")
    }

    private fun repairPrompt(previous: String, errors: List<String>): String = buildString {
        appendLine("Your previous form has these problems:")
        errors.forEach { appendLine("- $it") }
        appendLine()
        appendLine("Previous form:")
        appendLine(previous)
        appendLine()
        appendLine("Fix them and reply with the corrected JSON Schema only.")
    }

    private fun JsonObject.str(key: String) = this[key]?.jsonPrimitive?.contentOrNull.orEmpty()

    internal companion object {
        const val NOT_A_FORM = "the reply is not a JSON object"

        /** Loose on purpose: a JSON Schema's own shape cannot be pinned down in a decoding grammar. */
        val FORM_FORMAT: JsonObject = Json.parseToJsonElement(
            """
            {"type":"object","required":["type","properties"],"properties":{
              "title":{"type":"string"},"description":{"type":"string"},"type":{"const":"object"},
              "required":{"type":"array","items":{"type":"string"}},
              "properties":{"type":"object","additionalProperties":{"type":"object"}}}}
            """.trimIndent(),
        ).jsonObject
    }
}

/** Whether a form schema is a valid JSON Schema and covers exactly the workflow's inputs with the right shapes. */
object FormCheck {
    data class Result(
        val metaErrors: List<String>,
        val missing: List<String>,
        val extra: List<String>,
        /** Inputs present with the wrong shape, each with what was expected. */
        val wrongShape: List<String>,
        /** Inputs present with the right shape. */
        val right: Int,
    ) {
        val ok: Boolean get() = metaErrors.isEmpty() && missing.isEmpty() && extra.isEmpty() && wrongShape.isEmpty()

        val errors: List<String>
            get() = metaErrors.map { "not a valid JSON Schema: $it" } + missing.map { "no property for input '$it'" } +
                extra.map { "property '$it' is not an input" } + wrongShape

        fun scores(inputs: Int) = buildJsonObject {
            put("inputs", inputs)
            put("metaValid", metaErrors.isEmpty())
            put("inputsCovered", inputs - missing.size)
            put("shapesRight", right)
            put("extra", extra.size)
        }
    }

    private val mapper = ObjectMapper()
    private val metaSchema by lazy {
        JsonSchemaFactory.getInstance(SpecVersion.VersionFlag.V202012)
            .getSchema(SchemaLocation.of("https://json-schema.org/draft/2020-12/schema"))
    }

    fun check(form: JsonObject, inputs: List<JsonObject>): Result {
        val metaErrors = metaSchema.validate(mapper.readTree(form.toString())).map { it.message }.sorted()
        val properties = form["properties"] as? JsonObject ?: JsonObject(emptyMap())
        val names = inputs.mapNotNull { it["name"]?.jsonPrimitive?.contentOrNull }
        val wrong = mutableListOf<String>()
        var right = 0
        inputs.forEach { input ->
            val name = input["name"]?.jsonPrimitive?.contentOrNull ?: return@forEach
            val property = properties[name] as? JsonObject ?: return@forEach
            val problem = shapeProblem(input, property)
            if (problem == null) right++ else wrong += "input '$name': $problem"
        }
        return Result(metaErrors, names.filter { it !in properties }, properties.keys.filter { it !in names }, wrong, right)
    }

    private fun shapeProblem(input: JsonObject, property: JsonObject): String? {
        val type = types(property)
        return when (input["kind"]?.jsonPrimitive?.contentOrNull) {
            "recordset" -> {
                if ("array" !in type) return "a recordset must be an array of objects, not ${type.ifEmpty { setOf("untyped") }}"
                val items = property["items"] as? JsonObject
                if (items == null || "object" !in types(items)) return "a recordset's items must be objects"
                val fields = (input["fields"] as? JsonArray)?.mapNotNull { it.jsonPrimitive.contentOrNull }.orEmpty()
                val present = (items["properties"] as? JsonObject)?.keys.orEmpty()
                val missing = fields.filter { it !in present }
                if (missing.isEmpty()) null else "the recordset's items have no property for field(s) ${missing.joinToString(", ")}"
            }
            "object" -> if ("object" in type) null else "an object input must be an object, not ${type.ifEmpty { setOf("untyped") }}"
            else -> if (type.isNotEmpty() && type.all { it in SCALAR_TYPES }) null
                else "a scalar input must be a string, number, integer or boolean, not ${type.ifEmpty { setOf("untyped") }}"
        }
    }

    private val SCALAR_TYPES = setOf("string", "number", "integer", "boolean", "null")

    private fun types(schema: JsonObject): Set<String> = when (val t = schema["type"]) {
        is JsonPrimitive -> setOfNotNull(t.contentOrNull)
        is JsonArray -> t.mapNotNull { it.jsonPrimitive.contentOrNull }.toSet()
        else -> emptySet()
    }
}

/** The blind judge for an artifact: it sees the goal, the workflow summary and the artifact, never the contender. */
class ArtifactJudge(private val model: TextModel, private val rubric: Rubric) {
    enum class Rubric(val what: String, val scale: String) {
        TEST(
            "a service test someone wrote for the workflow",
            "Rate how much this test would prove that the workflow does what the goal asks: 5 = realistic inputs, mocks only the " +
                "steps that reach outside the workflow, and asserts the key outputs with specific expected values; 4 = minor gaps; " +
                "3 = checks something real but weakly (for example only that outputs are not empty); 2 = barely tests the goal; " +
                "1 = proves nothing about the goal or is wrong.",
        ),
        FORM(
            "the input form (a JSON Schema the studio renders) someone designed for running the workflow",
            "Rate this form for a person who has to run the workflow: 5 = every input is there with a clear title and help text, the " +
                "right type, and sensible formats, choices, defaults and limits; 4 = minor gaps; 3 = usable but bare or with some " +
                "wrong types; 2 = confusing or largely wrong; 1 = unusable.",
        ),
    }

    fun rate(goal: String, summary: String, artifact: JsonObject): Judge.Verdict? = Judge.parse(model.complete(prompt(goal, summary, artifact), null))

    internal fun prompt(goal: String, summary: String, artifact: JsonObject): String = buildString {
        appendLine("You are reviewing ${rubric.what}. Warewolf workflows are flowcharts of tools (Assign, Decision, Switch, database")
        appendLine("and web calls, ...) that pass data in [[variables]].")
        appendLine()
        appendLine("Goal: $goal")
        appendLine()
        appendLine("Workflow:")
        appendLine(summary)
        appendLine()
        appendLine("To review:")
        appendLine(artifact.toString())
        appendLine()
        appendLine(rubric.scale)
        appendLine("Reply with JSON only: {\"score\": <1-5>, \"reason\": \"<one sentence>\"}")
    }
}
