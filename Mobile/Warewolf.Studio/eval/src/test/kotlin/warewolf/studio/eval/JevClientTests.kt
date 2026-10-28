package warewolf.studio.eval

import kotlinx.serialization.json.Json
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive
import kotlin.test.Test
import kotlin.test.assertEquals

class JevClientTests {
    private val options = listOf("Assign", "Decision", "END")

    /** A fake Jev that records the request and answers [choice] in the documented response shape. */
    private class FakeJev(private val choice: String) {
        val requests = mutableListOf<String>()
        val post: (String) -> String = { body ->
            requests += body
            """{"model":"jev-1.13.0","answers":{"next_step":{"type":"choice","choice":"$choice","confidence":0.9,
               "probabilities":{"$choice":0.9}}},"usage":{"input_tokens":10,"output_tokens":1}}"""
        }
    }

    @Test
    fun `the chosen option maps back to its index`() {
        assertEquals(2, JevClient(post = FakeJev("END").post).choose("goal", options))
    }

    @Test
    fun `a choice outside the options maps to -1 so the chooser rejects it`() {
        assertEquals(-1, JevClient(post = FakeJev("Sort Records").post).choose("goal", options))
    }

    @Test
    fun `the request is one choice question over the option names with the prompt as state`() {
        val jev = FakeJev("Assign")

        JevClient(model = "jev-latest", post = jev.post).choose("the prompt", options)
        val body = Json.parseToJsonElement(jev.requests.single()).jsonObject
        val question = body.getValue("questions").jsonObject.getValue("next_step").jsonObject

        assertEquals("jev-latest", body.getValue("model").jsonPrimitive.content)
        assertEquals("the prompt", body.getValue("state").jsonPrimitive.content)
        assertEquals("choice", question.getValue("type").jsonPrimitive.content)
        assertEquals(options, question.getValue("criteria").jsonObject.keys.toList())
    }

    @Test
    fun `the model the API resolved is recorded`() {
        val client = JevClient(post = FakeJev("Assign").post)

        client.choose("goal", options)

        assertEquals("jev-1.13.0", client.resolvedModel)
    }
}
