package warewolf.studio.eval.bakeoff

import kotlinx.serialization.json.Json
import kotlinx.serialization.json.JsonObject
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive
import com.sun.net.httpserver.HttpExchange
import com.sun.net.httpserver.HttpServer
import java.net.InetSocketAddress
import java.time.Duration
import java.util.concurrent.CountDownLatch
import java.util.concurrent.Executors
import java.util.concurrent.TimeUnit
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFailsWith
import kotlin.test.assertFalse
import kotlin.test.assertTrue

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
    fun `reasoning_content is not the answer when the reply is free-form code`() {
        val post: (String) -> String = { """{"choices":[{"message":{"content":"","reasoning_content":"Wait, the prompt says"}}]}""" }

        assertEquals("", LmStudioText("gemma", post = post).complete("goal", null))
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

class WorkflowFormatTests {
    @Test
    fun `the envelope may declare working variables, shaped like inputs and outputs`() {
        val envelope = workflowFormat(testCatalog).getValue("properties").jsonObject.getValue("envelope").jsonObject
        val properties = envelope.getValue("properties").jsonObject

        assertEquals(properties.getValue("inputs"), properties.getValue("variables"))
        assertFalse("variables" in envelope["required"].toString())
    }

    @Test
    fun `an edge may carry an id, so one opened with an id is not forced into a node`() {
        val edge = workflowFormat(testCatalog).getValue("\$defs").jsonObject.getValue("edge").jsonObject

        assertTrue("id" in edge.getValue("properties").jsonObject)
        assertFalse("id" in edge["required"].toString())
    }
}

class LmStudioHttpPostTests {
    /** A local HTTP server on a free port whose single route is [handle]; stopped by [use]. */
    private fun <T> withServer(handle: (HttpExchange) -> Unit, use: (url: String) -> T): T {
        val server = HttpServer.create(InetSocketAddress("127.0.0.1", 0), 0)
        server.createContext("/v1/chat/completions") { exchange -> handle(exchange) }
        server.executor = Executors.newCachedThreadPool()
        server.start()
        try {
            return use("http://127.0.0.1:${server.address.port}/v1/chat/completions")
        } finally {
            server.stop(0)
        }
    }

    private fun reply(exchange: HttpExchange, status: Int, body: String) {
        val bytes = body.toByteArray()
        exchange.sendResponseHeaders(status, bytes.size.toLong())
        exchange.responseBody.use { it.write(bytes) }
    }

    @Test
    fun `a reply that never comes ends in a timeout instead of hanging`() {
        val release = CountDownLatch(1)
        withServer({ release.await(30, TimeUnit.SECONDS) }) { url ->
            val post = LmStudioText.httpPost(url, "key", Duration.ofSeconds(1))
            val started = System.nanoTime()

            val error = assertFailsWith<IllegalStateException> { post("{}") }

            assertTrue("did not answer" in error.message.orEmpty())
            assertTrue(Duration.ofNanos(System.nanoTime() - started) < Duration.ofSeconds(10))
            release.countDown()
        }
    }

    @Test
    fun `the body comes back and the API key goes out as a bearer token`() {
        var authorization: String? = null
        withServer({ exchange ->
            authorization = exchange.requestHeaders.getFirst("Authorization")
            reply(exchange, 200, """{"ok":true}""")
        }) { url ->
            assertEquals("""{"ok":true}""", LmStudioText.httpPost(url, "secret")("{}"))
        }

        assertEquals("Bearer secret", authorization)
    }

    @Test
    fun `a non-200 status is an error naming the status`() {
        withServer({ exchange -> reply(exchange, 524, "error code: 524") }) { url ->
            val error = assertFailsWith<IllegalStateException> { LmStudioText.httpPost(url, "key")("{}") }

            assertTrue("524" in error.message.orEmpty())
        }
    }
}
