package warewolf.studio.eval.bakeoff

import kotlinx.serialization.json.Json
import kotlinx.serialization.json.JsonObject
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse

class LmStudioTextTests {
    /** A fake LM Studio that records the request and answers [content] in the OpenAI chat-completion shape. */
    private class FakeLmStudio(private val content: String) {
        val requests = mutableListOf<JsonObject>()
        val post: (String) -> String = { body ->
            requests += Json.parseToJsonElement(body).jsonObject
            """{"choices":[{"index":0,"message":{"role":"assistant","content":${Json.encodeToString(content)}}}]}"""
        }
    }

    private val format = Json.parseToJsonElement("""{"type":"object","required":["a"]}""").jsonObject

    @Test
    fun `the reply is the first choice's message content`() {
        val lmStudio = FakeLmStudio("""{"a":1}""")

        assertEquals("""{"a":1}""", LmStudioText("gemma", post = lmStudio.post).complete("goal", format))
    }

    @Test
    fun `the format goes in as a strict json_schema response_format`() {
        val lmStudio = FakeLmStudio("{}")

        LmStudioText("gemma", post = lmStudio.post).complete("goal", format)
        val request = lmStudio.requests.single()
        val schema = request.getValue("response_format").jsonObject

        assertEquals("gemma", request.getValue("model").jsonPrimitive.content)
        assertEquals("0.2", request.getValue("temperature").jsonPrimitive.content)
        assertEquals("7", request.getValue("seed").jsonPrimitive.content)
        assertEquals("json_schema", schema.getValue("type").jsonPrimitive.content)
        assertEquals(format, schema.getValue("json_schema").jsonObject.getValue("schema"))
    }

    @Test
    fun `a reply left entirely in reasoning_content is still read`() {
        val post: (String) -> String = { """{"choices":[{"message":{"role":"assistant","content":"","reasoning_content":"{\"a\":2}\n"}}]}""" }

        assertEquals("{\"a\":2}\n", LmStudioText("qwen", post = post).complete("goal", format))
    }

    @Test
    fun `content wins over reasoning_content when both are present`() {
        val post: (String) -> String = { """{"choices":[{"message":{"content":"{\"a\":1}","reasoning_content":"thinking"}}]}""" }

        assertEquals("{\"a\":1}", LmStudioText("qwen", post = post).complete("goal", format))
    }

    @Test
    fun `no format sends no response_format`() {
        val lmStudio = FakeLmStudio("text")

        LmStudioText("gemma", post = lmStudio.post).complete("goal", null)

        assertFalse("response_format" in lmStudio.requests.single())
    }
}
