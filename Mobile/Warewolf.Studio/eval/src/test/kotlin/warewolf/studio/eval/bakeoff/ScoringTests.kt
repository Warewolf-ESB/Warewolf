package warewolf.studio.eval.bakeoff

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertNull
import kotlin.test.assertTrue

class StructureTests {
    @Test
    fun `tool overlap counts repeats`() {
        assertEquals(1.0, Structure.toolF1(listOf("Assign", "Assign", "Decision"), listOf("Decision", "Assign", "Assign")))
        assertEquals(2.0 * 2 / 6, Structure.toolF1(listOf("Assign", "Assign", "Assign", "Assign"), listOf("Assign", "Assign")))
        assertEquals(0.0, Structure.toolF1(listOf("Replace"), listOf("Assign")))
        assertEquals(0.0, Structure.toolF1(emptyList(), listOf("Assign")))
    }

    @Test
    fun `decisions and switches count as branching`() {
        assertTrue(Structure.branches(listOf("Assign", "Decision")))
        assertTrue(Structure.branches(listOf("Switch")))
        assertFalse(Structure.branches(listOf("Assign")))
    }
}

class JudgeTests {
    @Test
    fun `the prompt holds only the goal and the summary`() {
        val model = ScriptedModel("""{"score":4,"reason":"fine"}""")

        val verdict = Judge(model).rate("Greet someone", "Steps:\n  S1: Assign")

        assertEquals(Judge.Verdict(4, "fine"), verdict)
        val prompt = model.prompts.single()
        assertTrue("Goal: Greet someone" in prompt && "S1: Assign" in prompt)
        listOf("qwen", "claude", "system1", "ollama", "markov", "classifier").forEach { assertFalse(it in prompt.lowercase(), it) }
    }

    @Test
    fun `malformed or out-of-range verdicts are rejected`() {
        assertNull(Judge.parse("great workflow!"))
        assertNull(Judge.parse("""{"score":7,"reason":"x"}"""))
        assertNull(Judge.parse("""{"score":"high"}"""))
        assertEquals(Judge.Verdict(2, ""), Judge.parse("""Here: {"score":2}"""))
    }
}
