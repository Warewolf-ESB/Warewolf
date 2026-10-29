package warewolf.studio.eval

import kotlinx.serialization.json.JsonArray
import kotlinx.serialization.json.JsonPrimitive
import kotlinx.serialization.json.buildJsonObject
import kotlinx.serialization.json.JsonObject
import kotlinx.serialization.json.put
import warewolf.studio.generator.LlmChoiceChooser
import warewolf.studio.generator.Tool
import java.nio.file.Files
import java.nio.file.Path

/**
 * Training data for the trained System 1 classifier (Plan.md §2, option a). Each row's text is
 * [warewolf.studio.generator.ChoiceContext.describeState] with rich steps and the last [RECENT_STEPS] of them, so the model is trained on exactly what
 * the phone will build.
 */
object Features {
    /** The classifier sees only this many recent steps, so most state texts fit its 256-token window. */
    const val RECENT_STEPS = 6

    /** Training families whose hash starts below this byte (about 15%) are held out for model selection. */
    private const val VALIDATION_CUTOFF = 38

    /** Families are SHA-1 prefixes, so the first byte splits them evenly and the same way every run. */
    fun isValidation(family: String): Boolean = family.take(2).toInt(16) < VALIDATION_CUTOFF

    /** The classifier's outputs: every placeable tool in catalog order, then END. */
    fun labels(tools: List<Tool>): List<String> = tools.map { it.name } + LlmChoiceChooser.END

    /**
     * Writes `train.jsonl`, `val.jsonl`, `eval.jsonl`, `labels.json` and `features.json` (how the text was built,
     * which training copies into the model's `config.json`) to [out]. Examples whose answer is not one
     * of [labels] (unknown activities, the legacy Decision) are left out, as they are when scoring.
     */
    fun export(split: Split, tools: List<Tool>, out: Path): Map<String, Int> {
        Files.createDirectories(out)
        val labels = labels(tools)
        val known = labels.toSet()
        val (validation, train) = split.train.partition { isValidation(it.family) }
        val files = mapOf("train" to train, "val" to validation, "eval" to split.eval)
        files.forEach { (name, examples) ->
            Files.newBufferedWriter(out.resolve("$name.jsonl")).use { writer ->
                examples.filter { it.next in known }.forEach { example ->
                    writer.write(row(example, tools).toString())
                    writer.newLine()
                }
            }
        }
        Files.writeString(out.resolve("labels.json"), JsonArray(labels.map(::JsonPrimitive)).toString())
        Files.writeString(out.resolve("features.json"), JsonObject(mapOf("recentSteps" to JsonPrimitive(RECENT_STEPS))).toString())
        return files.mapValues { (_, examples) -> examples.count { it.next in known } }
    }

    internal fun row(example: CorpusExample, tools: List<Tool>) = buildJsonObject {
        put("text", example.context(tools, rich = true).describeState(RECENT_STEPS))
        put("label", example.next)
        put("family", example.family)
        put("set", example.set)
        put("id", example.workflowId)
    }
}
