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

    @Test
    fun `prompt shows each step's label and variables when the tail has them`() {
        val capture = mutableListOf<Pair<String, List<String>>>()
        val steps = listOf(StepSummary("Assign", "Set defaults", listOf("name", "rs(*).f")), StepSummary("Decision", "Is valid?"))
        val rich = context.copy(tail = BranchTail("s2", "True", listOf("Assign", "Decision"), steps))

        chooserReturning(0, capture).choose(rich)

        assertTrue("""Steps so far on this branch: Assign "Set defaults" [[name]], [[rs(*).f]] -> Decision "Is valid?"""" in capture.single().first)
    }

    @Test
    fun `prompt without steps lists tool names only`() {
        val capture = mutableListOf<Pair<String, List<String>>>()

        chooserReturning(0, capture).choose(context.copy(tail = BranchTail("s1", path = listOf("Assign", "Decision"))))

        assertTrue(capture.single().first.lines().contains("Steps so far on this branch: Assign -> Decision"))
    }

    @Test
    fun `step labels and variable lists are capped`() {
        val step = StepSummary("Assign", "x".repeat(StepSummary.MAX_LABEL + 10), (1..StepSummary.MAX_VARIABLES + 2).map { "v$it" })

        val described = step.describe()

        assertTrue("\"${"x".repeat(StepSummary.MAX_LABEL)}\" " in described)
        assertTrue("[[v${StepSummary.MAX_VARIABLES}]], …" in described)
        assertTrue("[[v${StepSummary.MAX_VARIABLES + 1}]]" !in described)
    }

    @Test
    fun `an empty branch is described as none`() {
        assertEquals("(none)", BranchTail(afterStepId = null).describePath())
    }

    @Test
    fun `only the most recent steps are shown when capped, with earlier ones counted`() {
        val tail = BranchTail("s4", path = listOf("Assign", "Calculate", "Decision", "Assign"))

        assertEquals("(2 earlier steps) -> Decision -> Assign", tail.describePath(recent = 2))
        assertEquals("Assign -> Calculate -> Decision -> Assign", tail.describePath(recent = 4))
    }
}
