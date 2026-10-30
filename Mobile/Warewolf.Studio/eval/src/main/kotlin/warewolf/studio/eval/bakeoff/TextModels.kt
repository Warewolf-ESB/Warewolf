package warewolf.studio.eval.bakeoff

import kotlinx.serialization.json.Json
import kotlinx.serialization.json.JsonObject
import kotlinx.serialization.json.buildJsonObject
import kotlinx.serialization.json.contentOrNull
import kotlinx.serialization.json.jsonArray
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive
import kotlinx.serialization.json.put
import kotlinx.serialization.json.putJsonArray
import kotlinx.serialization.json.putJsonObject
import java.net.URI
import java.net.http.HttpClient
import java.net.http.HttpRequest
import java.net.http.HttpResponse
import java.time.Duration
import java.util.concurrent.TimeUnit

/** A model that answers one prompt with text. Every attempt is a fresh, single prompt, so runs stay comparable. */
fun interface TextModel {
    fun complete(prompt: String, format: JsonObject?): String
}

/**
 * A local model through Ollama's chat API. A JSON-schema [format] holds the reply to that shape (grammar-constrained
 * decoding, as llama.cpp does on the phone). Temperature 0 keeps runs repeatable.
 */
class OllamaText(
    private val model: String,
    private val contextTokens: Int = 32_768,
    private val maxReplyTokens: Int = 8_192,
    private val post: (String) -> String = httpPost("http://localhost:11434/api/chat"),
) : TextModel {
    override fun complete(prompt: String, format: JsonObject?): String {
        val request = buildJsonObject {
            put("model", model)
            put("stream", false)
            putJsonArray("messages") { add(buildJsonObject { put("role", "user"); put("content", prompt) }) }
            if (format != null) put("format", format)
            putJsonObject("options") {
                put("temperature", 0)
                put("num_ctx", contextTokens)
                put("num_predict", maxReplyTokens)
            }
        }
        val response = Json.parseToJsonElement(post(request.toString())).jsonObject
        return response.getValue("message").jsonObject.getValue("content").jsonPrimitive.content
    }

    companion object {
        fun httpPost(url: String): (String) -> String {
            val http = HttpClient.newBuilder().connectTimeout(Duration.ofSeconds(10)).build()
            return { body ->
                val request = HttpRequest.newBuilder(URI.create(url))
                    .timeout(Duration.ofMinutes(15))
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

/**
 * A larger local model served by LM Studio's OpenAI-compatible chat API (ask.leylew.is by default), authorised with
 * the `LLM_API_KEY` bearer token. A JSON-schema [format] goes in as `response_format`, which LM Studio enforces with
 * grammar-constrained decoding like [OllamaText]. Gemma 4 loops on one token under the grammar at temperature 0, so
 * this samples at a low temperature with a fixed seed to keep runs repeatable.
 */
class LmStudioText(
    private val model: String,
    private val maxReplyTokens: Int = 8_192,
    private val temperature: Double = 0.2,
    private val seed: Int = 7,
    private val post: (String) -> String = httpPost(
        (System.getenv("LLM_BASE_URL") ?: "https://ask.leylew.is/v1") + "/chat/completions",
        System.getenv("LLM_API_KEY") ?: error("LLM_API_KEY is not set"),
    ),
) : TextModel {
    override fun complete(prompt: String, format: JsonObject?): String {
        val request = buildJsonObject {
            put("model", model)
            put("stream", false)
            put("temperature", temperature)
            put("seed", seed)
            put("max_tokens", maxReplyTokens)
            putJsonArray("messages") { add(buildJsonObject { put("role", "user"); put("content", prompt) }) }
            if (format != null) putJsonObject("response_format") {
                put("type", "json_schema")
                putJsonObject("json_schema") { put("name", "reply"); put("strict", true); put("schema", format) }
            }
        }
        val response = Json.parseToJsonElement(post(request.toString())).jsonObject
        val message = response.getValue("choices").jsonArray.first().jsonObject.getValue("message").jsonObject
        // A thinking model (Qwen 3.x) under a grammar can leave the whole reply in reasoning_content.
        return message["content"]?.jsonPrimitive?.contentOrNull?.takeIf { it.isNotBlank() }
            ?: message["reasoning_content"]?.jsonPrimitive?.contentOrNull.orEmpty()
    }

    companion object {
        fun httpPost(url: String, apiKey: String): (String) -> String {
            val http = HttpClient.newBuilder().connectTimeout(Duration.ofSeconds(10)).build()
            return { body ->
                val request = HttpRequest.newBuilder(URI.create(url))
                    .timeout(Duration.ofMinutes(15))
                    .header("Content-Type", "application/json")
                    .header("Authorization", "Bearer $apiKey")
                    .POST(HttpRequest.BodyPublishers.ofString(body))
                    .build()
                val response = http.send(request, HttpResponse.BodyHandlers.ofString())
                check(response.statusCode() == 200) { "LM Studio returned ${response.statusCode()}: ${response.body().take(300)}" }
                response.body()
            }
        }
    }
}

/**
 * A Claude model through the `claude` CLI, the way `corpus/generate_goals.py` calls it: print mode, no tools, no
 * MCP servers, no settings, no saved session, and a one-line system prompt in place of Claude Code's own. The
 * prompt goes in on stdin. [format] is ignored: the frontier model is held to the shape by the prompt alone.
 */
class ClaudeCli(private val model: String, private val timeoutMinutes: Long = 15) : TextModel {
    override fun complete(prompt: String, format: JsonObject?): String {
        val command = listOf(
            System.getenv("CLAUDE_CLI") ?: "claude", "-p", "--model", model, "--tools", "", "--strict-mcp-config",
            "--setting-sources", "", "--no-session-persistence", "--system-prompt", SYSTEM_PROMPT,
        )
        val process = ProcessBuilder(command).redirectErrorStream(false).start()
        process.outputStream.bufferedWriter().use { it.write(prompt) }
        val output = process.inputStream.bufferedReader().readText()
        val error = process.errorStream.bufferedReader().readText()
        check(process.waitFor(timeoutMinutes, TimeUnit.MINUTES)) { "claude timed out" }
        check(process.exitValue() == 0) { "claude exited ${process.exitValue()}: ${error.take(500)}" }
        return output
    }

    private companion object {
        const val SYSTEM_PROMPT = "You build Warewolf workflows and answer with JSON only."
    }
}

/**
 * The JSON-schema constraint for a whole workflow: an envelope and a body whose node shapes and types come from the
 * catalog (so a node can never be mistaken for an edge), with each tool's own fields allowed inside `data`.
 */
fun workflowFormat(catalog: Catalog): JsonObject {
    val types = (listOf("start") + catalog.tools.map { it.dataType }).joinToString(",") { "\"$it\"" }
    return workflowFormatJson(types)
}

private fun workflowFormatJson(types: String): JsonObject = Json.parseToJsonElement(
    """
    {"type":"object","required":["envelope","body"],"properties":{
      "envelope":{"type":"object","required":["name","inputs","outputs"],"properties":{
        "name":{"type":"string"},"description":{"type":"string"},
        "inputs":{"type":"array","items":{"${'$'}ref":"#/${'$'}defs/variable"}},
        "outputs":{"type":"array","items":{"${'$'}ref":"#/${'$'}defs/variable"}}}},
      "body":{"type":"object","required":["resourcename","cells"],"properties":{
        "resourcename":{"type":"string"},
        "cells":{"type":"array","items":{"anyOf":[{"${'$'}ref":"#/${'$'}defs/node"},{"${'$'}ref":"#/${'$'}defs/edge"}]}}}}},
     "${'$'}defs":{
      "variable":{"type":"object","required":["kind","name"],"properties":{
        "kind":{"enum":["scalar","object","recordset"]},"name":{"type":"string"},"fields":{"type":"array","items":{"type":"string"}}}},
      "node":{"type":"object","required":["id","shape","data"],"properties":{
        "id":{"type":"string"},"shape":{"enum":[$types]},
        "data":{"type":"object","required":["type"],"additionalProperties":true,"properties":{
          "type":{"enum":[$types]},
          "displayname":{"type":"string"}}}}},
      "edge":{"type":"object","required":["shape","source","target"],"properties":{
        "shape":{"const":"edge"},
        "source":{"type":"object","required":["cell"],"properties":{"cell":{"type":"string"}}},
        "target":{"type":"object","required":["cell"],"properties":{"cell":{"type":"string"}}},
        "label":{"type":"string"},
        "data":{"type":"object","properties":{"isDecisionArm":{"type":"boolean"},"isTrue":{"type":"boolean"}}}}}}}
    """.trimIndent(),
).jsonObject
