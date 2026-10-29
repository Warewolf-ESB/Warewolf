package warewolf.studio.eval

import kotlinx.serialization.json.Json
import kotlinx.serialization.json.JsonObject
import kotlinx.serialization.json.buildJsonObject
import kotlinx.serialization.json.double
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive
import kotlinx.serialization.json.put
import kotlinx.serialization.json.putJsonObject
import warewolf.studio.generator.Action
import warewolf.studio.generator.ChoiceContext
import warewolf.studio.generator.InvalidChoiceException
import warewolf.studio.generator.NextActionChooser

/**
 * System 1 as two decisions in one Jev request (https://docs.typesafe.ai/api): a yes/no (`noul`) "is this
 * branch finished?" and a `choice` over the tools alone. The branch ends when the finished probability reaches
 * [endThreshold]; otherwise the chosen tool is placed. Asking Jev for one choice over the tools plus END named
 * the right tool most often of the models tried, but rarely ended a branch where the author did (16% of ENDs).
 */
class JevTwoStepChooser(
    private val endThreshold: Double = 0.5,
    private val model: String = "jev-latest",
    /** Posts a request body and returns the response body; replaced by a fake in tests. */
    private val post: (String) -> String = JevClient.httpPost(
        "https://api.typesafe.ai/v1/systemone",
        System.getenv("JEV_API_KEY") ?: error("JEV_API_KEY is not set"),
    ),
) : NextActionChooser {
    /** The last decision's finished probability and the tool Jev would have placed, kept so a run can be re-scored at other thresholds. */
    var lastFinished: Double? = null
        private set
    var lastTool: String? = null
        private set
    var resolvedModel: String? = null
        private set

    override fun choose(context: ChoiceContext): Action {
        val response = Json.parseToJsonElement(post(request(context).toString())).jsonObject
        resolvedModel = response["model"]?.jsonPrimitive?.content
        val answers = response.getValue("answers").jsonObject
        val finished = answers.getValue(FINISHED).jsonObject.getValue("noul").jsonPrimitive.double
        val toolName = answers.getValue(NEXT_TOOL).jsonObject["choice"]?.jsonPrimitive?.content
        lastFinished = finished
        lastTool = toolName

        if (finished >= endThreshold) {
            return Action.End()
        }

        val tool = context.tools.firstOrNull { it.name == toolName }
            ?: throw InvalidChoiceException("Jev chose '$toolName', which is not a placeable tool")
        return Action.Place(tool)
    }

    internal fun request(context: ChoiceContext): JsonObject = buildJsonObject {
        put("model", model)
        put("state", context.describeState())
        putJsonObject("questions") {
            putJsonObject(FINISHED) {
                put("type", "noul")
                put("instructions", "Is this branch of the workflow complete, with no further step needed to reach the goal?")
            }
            putJsonObject(NEXT_TOOL) {
                put("type", "choice")
                put("instructions", "Which tool should be the next step on this branch?")
                putJsonObject("criteria") { context.tools.forEach { put(it.name, it.description) } }
            }
        }
    }

    companion object {
        internal const val FINISHED = "finished"
        internal const val NEXT_TOOL = "next_tool"
    }
}
