package warewolf.studio.eval

import kotlinx.serialization.json.Json
import kotlinx.serialization.json.JsonObject
import kotlinx.serialization.json.JsonPrimitive
import kotlinx.serialization.json.buildJsonArray
import kotlinx.serialization.json.buildJsonObject
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive
import kotlinx.serialization.json.put
import kotlinx.serialization.json.putJsonArray
import kotlinx.serialization.json.putJsonObject
import warewolf.studio.generator.LlmClient
import java.net.URI
import java.net.http.HttpClient
import java.net.http.HttpRequest
import java.net.http.HttpResponse
import java.time.Duration

/**
 * The harness's [LlmClient]: Ollama's chat API with a JSON-schema `format` whose only field is a string
 * enumerated over the option *names*, so the model can only answer with a valid option (the phone does the
 * same with a llama.cpp grammar). Names, not indices: a 0.5B model held to a bare number answered option 1
 * for almost every goal (3.2% accuracy against a 20.4% majority floor). Temperature 0 keeps runs repeatable.
 */
class OllamaClient(
    private val model: String,
    /** Posts a chat request body and returns the response body; replaced by a fake in tests. */
    private val post: (String) -> String = httpPost("http://localhost:11434/api/chat"),
) : LlmClient {

    /** The index of the option the model named, or -1 if it named none (the chooser rejects that). */
    override fun choose(prompt: String, options: List<String>): Int {
        val response = Json.parseToJsonElement(post(request(prompt, options).toString())).jsonObject
        val content = response.getValue("message").jsonObject.getValue("content").jsonPrimitive.content
        val choice = Json.parseToJsonElement(content).jsonObject["choice"]?.jsonPrimitive?.content
        return options.indexOf(choice)
    }

    internal fun request(prompt: String, options: List<String>): JsonObject = buildJsonObject {
        put("model", model)
        put("stream", false)
        putJsonArray("messages") {
            add(buildJsonObject {
                put("role", "user")
                put("content", "$prompt\nAnswer with the name of your choice.")
            })
        }
        putJsonObject("format") {
            put("type", "object")
            putJsonObject("properties") {
                putJsonObject("choice") {
                    put("type", "string")
                    put("enum", buildJsonArray { options.forEach { add(JsonPrimitive(it)) } })
                }
            }
            putJsonArray("required") { add(JsonPrimitive("choice")) }
        }
        putJsonObject("options") { put("temperature", 0) }
    }

    companion object {
        fun httpPost(url: String): (String) -> String {
            val http = HttpClient.newBuilder().connectTimeout(Duration.ofSeconds(10)).build()
            return { body ->
                val request = HttpRequest.newBuilder(URI.create(url))
                    .timeout(Duration.ofMinutes(2))
                    .header("Content-Type", "application/json")
                    .POST(HttpRequest.BodyPublishers.ofString(body))
                    .build()
                val response = http.send(request, HttpResponse.BodyHandlers.ofString())
                check(response.statusCode() == 200) { "Ollama returned ${response.statusCode()}: ${response.body().take(300)}" }
                response.body()
            }
        }
    }
}
