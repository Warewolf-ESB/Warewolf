package warewolf.studio.generator

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFailsWith
import kotlin.test.assertTrue

class ClassifierChooserTests {
    private val labels = listOf("Assign", "Calculate", "Decision", "END")
    private val context = ChoiceContext("Add two numbers", BranchTail("s1", path = listOf("Assign"), steps = listOf(StepSummary("Assign", "Set a"))), listOf(assign, calculate, decision))

    private fun chooser(vararg scores: Float, texts: MutableList<String> = mutableListOf()) =
        ClassifierChooser({ text -> texts += text; floatArrayOf(*scores) }, labels, catalog, recentSteps = 6)

    @Test
    fun `the best-scoring tool is placed`() {
        assertEquals(Action.Place(calculate), chooser(0.1f, 0.9f, 0.3f, 0.2f).choose(context))
    }

    @Test
    fun `a best score on END ends the branch`() {
        assertEquals(Action.End(), chooser(0.1f, 0.2f, 0.3f, 0.9f).choose(context))
    }

    @Test
    fun `tools that are not on offer are passed over`() {
        val withoutCalculate = context.copy(tools = listOf(assign, decision))

        assertEquals(Action.Place(decision), chooser(0.1f, 0.9f, 0.3f, 0.2f).choose(withoutCalculate))
    }

    @Test
    fun `the model reads the state text it was trained on`() {
        val texts = mutableListOf<String>()

        chooser(1f, 0f, 0f, 0f, texts = texts).choose(context)

        assertEquals(context.describeState(6), texts.single())
        assertTrue("""Assign "Set a"""" in texts.single())
    }

    @Test
    fun `labels the catalog does not know are rejected`() {
        assertFailsWith<IllegalArgumentException> {
            ClassifierChooser({ FloatArray(2) }, listOf("Assign", "Sort Records"), catalog, recentSteps = 6)
        }
    }

    @Test
    fun `a score count that does not match the labels is rejected`() {
        assertFailsWith<IllegalStateException> { chooser(0.1f, 0.2f).choose(context) }
    }

    @Test
    fun `when END is not allowed the best tool is placed instead`() {
        val chooser = ClassifierChooser({ floatArrayOf(0.1f, 0.4f, 0.3f, 0.9f) }, labels, catalog, recentSteps = 6, endAllowed = { false })

        assertEquals(Action.Place(calculate), chooser.choose(context))
    }
}
