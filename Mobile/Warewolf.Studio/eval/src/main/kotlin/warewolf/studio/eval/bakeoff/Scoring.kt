package warewolf.studio.eval.bakeoff

import kotlinx.serialization.json.JsonObject
import kotlinx.serialization.json.contentOrNull
import kotlinx.serialization.json.int
import kotlinx.serialization.json.jsonPrimitive
import kotlinx.serialization.json.put
import kotlinx.serialization.json.buildJsonObject

/** How close a generated workflow's structure is to the author's. */
object Structure {
    /** Overlap of the tools used, counting repeats: 2·common / (generated + reference); 1 when both are empty. */
    fun toolF1(generated: List<String>, reference: List<String>): Double {
        if (generated.isEmpty() && reference.isEmpty()) return 1.0
        val g = generated.groupingBy { it }.eachCount()
        val r = reference.groupingBy { it }.eachCount()
        val common = g.entries.sumOf { (tool, n) -> minOf(n, r[tool] ?: 0) }
        return 2.0 * common / (generated.size + reference.size)
    }

    fun branches(tools: List<String>): Boolean = tools.any { it.startsWith("Decision") || it == "Switch" }
}

/**
 * The judge: a frontier model rates how well a workflow achieves the goal, 1 to 5, from [WorkflowText.summary]
 * alone, so it cannot tell which contender wrote it.
 */
class Judge(private val model: TextModel) {
    data class Verdict(val score: Int, val reason: String)

    fun rate(goal: String, summary: String): Verdict? {
        val reply = model.complete(prompt(goal, summary), null)
        return parse(reply)
    }

    internal fun prompt(goal: String, summary: String): String = buildString {
        appendLine("You are reviewing a Warewolf workflow that someone built for a goal. Warewolf workflows are flowcharts of tools")
        appendLine("(Assign, Decision, Switch, database and web calls, ...) that pass data in [[variables]].")
        appendLine()
        appendLine("Goal: $goal")
        appendLine()
        appendLine("Workflow:")
        appendLine(summary)
        appendLine()
        appendLine("Rate how well this workflow would achieve the goal if its sources were configured:")
        appendLine("5 = does everything the goal asks, sensibly; 4 = minor gaps; 3 = the main idea but important parts missing or wrong;")
        appendLine("2 = only loosely related; 1 = does not achieve the goal at all.")
        appendLine("Reply with JSON only: {\"score\": <1-5>, \"reason\": \"<one sentence>\"}")
    }

    internal companion object {
        fun parse(reply: String): Verdict? {
            val o = WorkflowText.firstObject(reply) ?: return null
            val score = runCatching { o["score"]?.jsonPrimitive?.int }.getOrNull() ?: return null
            if (score !in 1..5) return null
            return Verdict(score, o["reason"]?.jsonPrimitive?.contentOrNull.orEmpty())
        }
    }
}

internal fun verdictJson(id: String, contender: String, verdict: Judge.Verdict?) = buildJsonObject {
    put("id", id); put("contender", contender)
    if (verdict != null) { put("score", verdict.score); put("reason", verdict.reason) }
}
