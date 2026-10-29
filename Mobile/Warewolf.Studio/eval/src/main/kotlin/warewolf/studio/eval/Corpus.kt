package warewolf.studio.eval

import kotlinx.serialization.json.Json
import kotlinx.serialization.json.JsonArray
import kotlinx.serialization.json.JsonNull
import kotlinx.serialization.json.JsonObject
import kotlinx.serialization.json.jsonArray
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive
import warewolf.studio.generator.BranchKind
import warewolf.studio.generator.BranchTail
import warewolf.studio.generator.ChoiceContext
import warewolf.studio.generator.StepSummary
import warewolf.studio.generator.Tool
import java.nio.file.Files
import java.nio.file.Path

/** One System 1 decision from the corpus, with the goal its workflow was generated for. */
data class CorpusExample(
    val workflowId: String,
    val set: String,
    val family: String,
    val goal: String,
    val path: List<String>,
    val arm: String?,
    val next: String,
    /** Each step of [path] with its label and variables; empty in corpora extracted before steps were recorded. */
    val steps: List<StepSummary> = emptyList(),
) {
    /** The corpus marks activities the catalog cannot place with a leading `?`. */
    val answerIsKnown: Boolean get() = !next.startsWith("?")

    /** What System 1 would see at this decision; [rich] adds each earlier step's label and variables. */
    fun context(tools: List<Tool>, rich: Boolean): ChoiceContext =
        ChoiceContext(goal, BranchTail(afterStepId = null, arm = arm, path = path, steps = if (rich) steps else emptyList()), tools)
}

data class Split(val train: List<CorpusExample>, val eval: List<CorpusExample>, val droppedForLeakage: Int)

/** Reads the extractor's output (`examples.jsonl`, `tools.json`) and the goal generator's `goals.jsonl`. */
object Corpus {
    fun loadTools(dir: Path): List<Tool> =
        Json.parseToJsonElement(Files.readString(dir.resolve("tools.json"))).jsonArray.map {
            val o = it.jsonObject
            Tool(o.string("name"), o.string("category"), o.string("description"), BranchKind.valueOf(o.string("branchKind")))
        }

    /** Examples whose workflow has a goal; workflows without one are left out. */
    fun loadExamples(dir: Path): List<CorpusExample> {
        val goals = jsonLines(dir.resolve("goals.jsonl")).associate { it.string("id") to it.string("goal") }
        return jsonLines(dir.resolve("examples.jsonl")).mapNotNull { o ->
            val goal = goals[o.string("id")] ?: return@mapNotNull null
            CorpusExample(
                workflowId = o.string("id"),
                set = o.string("set"),
                family = o.string("family"),
                goal = goal,
                path = (o["Path"] as JsonArray).map { it.jsonPrimitive.content },
                arm = o["Arm"].let { if (it == null || it is JsonNull) null else it.jsonPrimitive.content },
                next = o.string("Next"),
                steps = (o["Steps"] as? JsonArray).orEmpty().map { step ->
                    val s = step.jsonObject
                    StepSummary(s.string("tool"), s.string("label"), s.getValue("variables").jsonArray.map { it.jsonPrimitive.content })
                },
            )
        }
    }

    /**
     * Holds out whole sets for evaluation. An eval example whose workflow family (same branch structure) also
     * occurs in a training set is dropped, so near-copies cannot inflate accuracy.
     */
    fun split(examples: List<CorpusExample>, evalSets: Set<String>): Split {
        val (eval, train) = examples.partition { it.set in evalSets }
        val trainFamilies = train.mapTo(hashSetOf()) { it.family }
        val (leaked, clean) = eval.partition { it.family in trainFamilies }
        return Split(train, clean, leaked.size)
    }

    private fun jsonLines(file: Path): List<JsonObject> =
        Files.readAllLines(file).filter { it.isNotBlank() }.map { Json.parseToJsonElement(it).jsonObject }

    private fun JsonObject.string(key: String) = getValue(key).jsonPrimitive.content
}
