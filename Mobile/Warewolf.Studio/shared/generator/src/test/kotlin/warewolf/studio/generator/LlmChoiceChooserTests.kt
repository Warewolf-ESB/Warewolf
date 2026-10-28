package warewolf.studio.generator

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFailsWith
import kotlin.test.assertTrue

class LlmChoiceChooserTests {
    private val tools = listOf(assign, calculate, decision)
    private val context = ChoiceContext("Add two numbers", BranchTail(afterStepId = null), tools)

    private fun chooserReturning(index: Int, capture: MutableList<Pair<String, List<String>>> = mutableListOf()) =
        LlmChoiceChooser { prompt, options -> capture += prompt to options; index }

    @Test
    fun `a tool index places that tool`() {
        assertEquals(Action.Place(calculate), chooserReturning(1).choose(context))
    }

    @Test
    fun `the last option is END`() {
        assertEquals(Action.End(), chooserReturning(tools.size).choose(context))
    }

    @Test
    fun `an index past END is rejected`() {
        assertFailsWith<InvalidChoiceException> { chooserReturning(tools.size + 1).choose(context) }
    }

    @Test
    fun `a negative index is rejected`() {
        assertFailsWith<InvalidChoiceException> { chooserReturning(-1).choose(context) }
    }

    @Test
    fun `options are every tool in catalog order then END`() {
        val capture = mutableListOf<Pair<String, List<String>>>()

        chooserReturning(0, capture).choose(context)

        assertEquals(listOf("Assign", "Calculate", "Decision", "END"), capture.single().second)
    }

    @Test
    fun `prompt numbers each option and carries the goal, path and arm`() {
        val capture = mutableListOf<Pair<String, List<String>>>()
        val onArm = context.copy(tail = BranchTail("s1", "True", listOf("Assign", "Decision")))

        chooserReturning(0, capture).choose(onArm)
        val prompt = capture.single().first

        assertTrue("Goal: Add two numbers" in prompt)
        assertTrue("Assign -> Decision" in prompt)
        assertTrue("'True' arm" in prompt)
        listOf("0. Assign", "1. Calculate", "2. Decision", "3. END").forEach { assertTrue(it in prompt, "missing '$it'") }
    }
}
