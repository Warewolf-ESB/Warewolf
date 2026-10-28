package warewolf.studio.eval

import kotlinx.serialization.json.Json
import kotlinx.serialization.json.jsonArray
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive
import kotlin.test.Test
import kotlin.test.assertEquals

class OllamaClientTests {
    private val options = listOf("Assign", "Decision", "END")

    /** A fake Ollama that records the request and answers [choice] in the chat response shape. */
    private class FakeOllama(private val choice: String) {
        val requests = mutableListOf<String>()
        val post: (String) -> String = { body ->
            requests += body
            """{"message":{"role":"assistant","content":"{\"choice\":\"$choice\"}"}}"""
        }
    }

    @Test
    fun `the named option maps back to its index`() {
        val ollama = FakeOllama("Decision")

        assertEquals(1, OllamaClient("m", ollama.post).choose("goal", options))
    }

    @Test
    fun `a name outside the options maps to -1 so the chooser rejects it`() {
        assertEquals(-1, OllamaClient("m", FakeOllama("Sort Records").post).choose("goal", options))
    }

    @Test
    fun `the answer is constrained to the option names`() {
        val ollama = FakeOllama("END")

        OllamaClient("m", ollama.post).choose("goal", options)
        val choice = Json.parseToJsonElement(ollama.requests.single()).jsonObject
            .getValue("format").jsonObject.getValue("properties").jsonObject.getValue("choice").jsonObject

        assertEquals("string", choice.getValue("type").jsonPrimitive.content)
        assertEquals(options, choice.getValue("enum").jsonArray.map { it.jsonPrimitive.content })
    }
}
