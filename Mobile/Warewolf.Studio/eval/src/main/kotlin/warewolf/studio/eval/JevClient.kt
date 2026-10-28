package warewolf.studio.eval

import kotlinx.serialization.json.Json
import kotlinx.serialization.json.JsonNull
import kotlinx.serialization.json.JsonObject
import kotlinx.serialization.json.buildJsonObject
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive
import kotlinx.serialization.json.put
import kotlinx.serialization.json.putJsonObject
import warewolf.studio.generator.LlmClient
import java.net.URI
import java.net.http.HttpClient
import java.net.http.HttpRequest
import java.net.http.HttpResponse
import java.time.Duration

/**
 * TypeSafe AI's Jev, a non-generative decision model, as an [LlmClient] for comparison with the local
 * baselines. One `choice` question per decision (https://docs.typesafe.ai/api): the chooser's prompt is the
 * `state`, and the option names are the `criteria` (at most 255; the catalog has 66). Jev can only answer
 * with one of them, so no output constraint is needed.
 */
class JevClient(
    private val model: String = "jev-latest",
    /** Posts a request body and returns the response body; replaced by a fake in tests. */
    private val post: (String) -> String = httpPost(
        "https://api.typesafe.ai/v1/systemone",
        System.getenv("JEV_API_KEY") ?: error("JEV_API_KEY is not set"),
    ),
) : LlmClient {
    /** The model the API reported answering with, e.g. what `jev-latest` resolved to. */
    var resolvedModel: String? = null
        private set

    override fun choose(prompt: String, options: List<String>): Int {
        val response = Json.parseToJsonElement(post(request(prompt, options).toString())).jsonObject
        resolvedModel = response["model"]?.jsonPrimitive?.content
        val choice = response.getValue("answers").jsonObject[QUESTION]?.jsonObject?.get("choice")?.jsonPrimitive?.content
        return options.indexOf(choice)
    }

    internal fun request(prompt: String, options: List<String>): JsonObject = buildJsonObject {
        put("model", model)
        put("state", prompt)
        putJsonObject("questions") {
            putJsonObject(QUESTION) {
                put("type", "choice")
                put("instructions", "Which option is the next step on this branch of the workflow?")
                // The prompt already describes each option, so the criteria are judged by name.
                putJsonObject("criteria") { options.forEach { put(it, JsonNull) } }
            }
        }
    }

    companion object {
        private const val QUESTION = "next_step"

        fun httpPost(url: String, apiKey: String): (String) -> String {
            val http = HttpClient.newBuilder().connectTimeout(Duration.ofSeconds(10)).build()
            return { body ->
                val request = HttpRequest.newBuilder(URI.create(url))
                    .timeout(Duration.ofMinutes(2))
                    .header("Content-Type", "application/json")
                    .header("Authorization", "Bearer $apiKey")
                    .POST(HttpRequest.BodyPublishers.ofString(body))
                    .build()
                val response = http.send(request, HttpResponse.BodyHandlers.ofString())
                check(response.statusCode() == 200) { "Jev returned ${response.statusCode()}: ${response.body().take(300)}" }
                response.body()
            }
        }
    }
}
