package warewolf.studio.eval.bakeoff

import kotlinx.serialization.json.JsonObject
import kotlinx.serialization.json.buildJsonObject
import kotlinx.serialization.json.put
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertTrue

class EngineClientTests {
    private val empty = JsonObject(emptyMap())

    @Test
    fun `validation keeps errors with their path and leaves warnings out`() {
        val engine = FakeEngine("""{"valid":false,"errors":[
            {"message":"no start node","path":"/cells","severity":"error"},
            {"message":"unused variable","path":"/envelope","severity":"warning"}]}""")

        val result = engine.client.validate(empty, empty)

        assertFalse(result.valid)
        assertEquals(listOf("/cells: no start node"), result.errors)
        assertEquals("validate_workflow", engine.calls.single().first)
    }

    @Test
    fun `a rejected request is an invalid result carrying the engine's message`() {
        val client = EngineClient { _, _ -> 400 to """{"error":"bad_request","message":"body must be a JSON object"}""" }

        val result = client.validate(empty, empty)

        assertEquals(EngineClient.Validation(false, listOf("body must be a JSON object")), result)
    }

    @Test
    fun `only undeclared-variable errors count as valid ignoring declarations`() {
        val undeclared = "cell 'n1' references undeclared variable '[[a]]' — 'a' ${EngineClient.UNDECLARED}.inputs/outputs."

        assertTrue(EngineClient.Validation(false, listOf(undeclared)).validIgnoringDeclarations)
        assertFalse(EngineClient.Validation(false, listOf(undeclared, "no start node")).validIgnoringDeclarations)
    }

    @Test
    fun `add step returns the new step id or the engine's reason`() {
        val ok = EngineClient { _, _ -> 200 to """{"name":"w","stepId":"s9","updated":true}""" }
        val rejected = EngineClient { _, _ -> 400 to """{"message":"afterStepId x was not found"}""" }
        val step = buildJsonObject { put("shape", "assign") }

        assertEquals("s9", ok.addStep("w", step, null, null).getOrThrow())
        assertEquals("afterStepId x was not found", rejected.addStep("w", step, "x", null).exceptionOrNull()?.message)
    }
}
