package warewolf.studio.eval

import kotlinx.serialization.json.Json
import kotlinx.serialization.json.buildJsonArray
import kotlinx.serialization.json.buildJsonObject
import kotlinx.serialization.json.int
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
 * The harness's [LlmClient]: Ollama's chat API with a JSON-schema `format` whose only field is an integer
 * enumerated over the option indices, so the model can only answer with a valid option (the phone does the
 * same with a llama.cpp grammar). Temperature 0 keeps runs repeatable.
 */
class OllamaClient(
    private val model: String,
    private val baseUrl: String = "http://localhost:11434",
) : LlmClient {
    private val http = HttpClient.newBuilder().connectTimeout(Duration.ofSeconds(10)).build()

    override fun choose(prompt: String, options: List<String>): Int {
        val body = buildJsonObject {
            put("model", model)
            put("stream", false)
            putJsonArray("messages") {
                add(buildJsonObject {
                    put("role", "user")
                    put("content", "$prompt\nAnswer with the number of your choice.")
                })
            }
            putJsonObject("format") {
                put("type", "object")
                putJsonObject("properties") {
                    putJsonObject("choice") {
                        put("type", "integer")
                        put("enum", buildJsonArray { options.indices.forEach { add(kotlinx.serialization.json.JsonPrimitive(it)) } })
                    }
                }
                putJsonArray("required") { add(kotlinx.serialization.json.JsonPrimitive("choice")) }
            }
            putJsonObject("options") { put("temperature", 0) }
        }

        val request = HttpRequest.newBuilder(URI.create("$baseUrl/api/chat"))
            .timeout(Duration.ofMinutes(2))
            .header("Content-Type", "application/json")
            .POST(HttpRequest.BodyPublishers.ofString(body.toString()))
            .build()
        val response = http.send(request, HttpResponse.BodyHandlers.ofString())
        check(response.statusCode() == 200) { "Ollama returned ${response.statusCode()}: ${response.body().take(300)}" }

        val content = Json.parseToJsonElement(response.body()).jsonObject.getValue("message").jsonObject.getValue("content").jsonPrimitive.content
        return Json.parseToJsonElement(content).jsonObject.getValue("choice").jsonPrimitive.int
    }
}
