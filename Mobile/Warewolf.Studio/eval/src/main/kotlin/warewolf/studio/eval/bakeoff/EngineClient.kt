package warewolf.studio.eval.bakeoff

import kotlinx.serialization.json.Json
import kotlinx.serialization.json.JsonElement
import kotlinx.serialization.json.JsonObject
import kotlinx.serialization.json.JsonPrimitive
import kotlinx.serialization.json.boolean
import kotlinx.serialization.json.buildJsonObject
import kotlinx.serialization.json.jsonArray
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive
import kotlinx.serialization.json.put
import java.net.URI
import java.net.http.HttpClient
import java.net.http.HttpRequest
import java.net.http.HttpResponse
import java.time.Duration

/**
 * The engine's `mcp-api/<tool>` routes (`Functions/McpApiFunctions.cs`), as the comparison uses them. A rejected call
 * comes back as a [Reply] carrying the engine's message rather than an exception, because a contender's mistakes
 * are results, not failures of the harness.
 */
class EngineClient(
    /** Posts a JSON body to `mcp-api/<tool>` and returns the status code and response body; replaced by a fake in tests. */
    private val call: (tool: String, body: String) -> Pair<Int, String>,
) {
    data class Reply(val ok: Boolean, val json: JsonElement, val error: String?)

    data class Validation(val valid: Boolean, val errors: List<String>) {
        /** Every error is an undeclared variable: the envelope can only declare inputs and outputs, never locals. */
        val validIgnoringDeclarations: Boolean get() = valid || errors.all { UNDECLARED in it }
    }

    fun invoke(tool: String, args: JsonObject): Reply {
        val (status, text) = call(tool, args.toString())
        val json = runCatching { Json.parseToJsonElement(text) }.getOrElse { JsonPrimitive(text) }
        val error = if (status == 200) null else ((json as? JsonObject)?.get("message") as? JsonPrimitive)?.content ?: text.take(500)
        return Reply(status == 200, json, error)
    }

    fun validate(envelope: JsonElement, body: JsonElement): Validation {
        val reply = invoke("validate_workflow", buildJsonObject { put("envelope", envelope); put("body", body) })
        if (!reply.ok) return Validation(false, listOf(reply.error ?: "validate_workflow failed"))
        val result = reply.json.jsonObject
        val errors = result["errors"]?.jsonArray.orEmpty().map { it.jsonObject }
            .filter { it["severity"]?.jsonPrimitive?.content != "warning" }
            .map { e -> listOfNotNull(e["path"]?.jsonPrimitive?.content, e["message"]?.jsonPrimitive?.content).joinToString(": ") }
        return Validation(result["valid"]?.jsonPrimitive?.boolean == true, errors)
    }

    /** The `stepId` of the new node, or the engine's reason for rejecting it. */
    fun addStep(name: String, step: JsonObject, afterStepId: String?, branch: String?): Result<String> {
        val reply = invoke("add_step", buildJsonObject {
            put("name", name)
            put("step", step)
            afterStepId?.let { put("afterStepId", it) }
            branch?.let { put("branch", it) }
        })
        return if (reply.ok) Result.success(reply.json.jsonObject.getValue("stepId").jsonPrimitive.content)
        else Result.failure(IllegalArgumentException(reply.error))
    }

    fun create(name: String, envelope: JsonElement, body: JsonElement): Reply =
        invoke("create_workflow", buildJsonObject { put("name", name); put("envelope", envelope); put("body", body) })

    fun definition(name: String): Reply = invoke("get_workflow_definition", buildJsonObject { put("name", name) })

    fun delete(name: String): Reply = invoke("delete_workflow", buildJsonObject { put("name", name) })

    companion object {
        const val UNDECLARED = "is not declared in envelope"

        fun http(baseUrl: String): (String, String) -> Pair<Int, String> {
            val http = HttpClient.newBuilder().connectTimeout(Duration.ofSeconds(10)).build()
            return { tool, body ->
                val request = HttpRequest.newBuilder(URI.create("${baseUrl.trimEnd('/')}/mcp-api/$tool"))
                    .timeout(Duration.ofMinutes(1))
                    .header("Content-Type", "application/json")
                    .POST(HttpRequest.BodyPublishers.ofString(body))
                    .build()
                val response = http.send(request, HttpResponse.BodyHandlers.ofString())
                response.statusCode() to response.body()
            }
        }
    }
}
