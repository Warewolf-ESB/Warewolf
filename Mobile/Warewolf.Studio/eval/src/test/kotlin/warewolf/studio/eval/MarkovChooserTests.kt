package warewolf.studio.eval

import warewolf.studio.generator.Action
import warewolf.studio.generator.BranchKind
import warewolf.studio.generator.BranchTail
import warewolf.studio.generator.ChoiceContext
import warewolf.studio.generator.Tool
import kotlin.test.Test
import kotlin.test.assertEquals

class MarkovChooserTests {
    private val assign = Tool("Assign", "Data", "Assigns.")
    private val calculate = Tool("Calculate", "Data", "Evaluates.")
    private val decision = Tool("Decision", "Control Flow", "Branches.", BranchKind.DECISION)
    private val tools = listOf(assign, calculate, decision)

    private fun example(path: List<String>, arm: String?, next: String) =
        CorpusExample("w", "a", "F", "goal", path, arm, next)

    private fun at(path: List<String>, arm: String? = null) = ChoiceContext("goal", BranchTail(null, arm, path), tools)

    private val train = listOf(
        example(emptyList(), null, "Assign"),
        example(emptyList(), null, "Assign"),
        example(listOf("Assign"), null, "Decision"),
        example(listOf("Assign"), null, "Decision"),
        example(listOf("Assign"), null, "END"),
        example(listOf("Decision"), "True", "Calculate"),
        example(listOf("Decision"), "False", "END"),
        example(listOf("Decision"), "False", "END"),
        example(listOf("Assign"), "gold", "Calculate"),
        example(listOf("Assign"), null, "?Unknown"),
        example(listOf("Assign"), null, "?Unknown"),
        example(listOf("Assign"), null, "?Unknown"),
    )
    private val chooser = MarkovChooser(train)

    @Test
    fun `the most frequent next step after the previous tool on that arm is chosen`() {
        assertEquals(Action.Place(assign), chooser.choose(at(emptyList())))
        assertEquals(Action.Place(decision), chooser.choose(at(listOf("Assign"))))
        assertEquals(Action.Place(calculate), chooser.choose(at(listOf("Decision"), "True")))
        assertEquals(Action.End(), chooser.choose(at(listOf("Decision"), "False")))
    }

    @Test
    fun `every switch case counts as the same kind of arm`() {
        assertEquals(Action.Place(calculate), chooser.choose(at(listOf("Assign"), "silver")))
    }

    @Test
    fun `an unseen previous tool falls back to the most frequent answer overall`() {
        // END is the most frequent known answer (3 of 9); the unknown activities are not counted.
        assertEquals(Action.End(), chooser.choose(at(listOf("Calculate"))))
    }

    @Test
    fun `answers that are not offered are skipped`() {
        val withoutDecision = at(listOf("Assign")).copy(tools = listOf(assign, calculate))

        assertEquals(Action.End(), chooser.choose(withoutDecision))
    }

    @Test
    fun `when END is not allowed the most frequent tool after it is placed`() {
        val noEnd = MarkovChooser(train, endAllowed = { false })

        assertEquals(Action.Place(calculate), noEnd.choose(at(listOf("Decision"), "True")))
        // Only END follows a False arm in training, so the fallback is the most frequent tool after any Decision.
        assertEquals(Action.Place(calculate), noEnd.choose(at(listOf("Decision"), "False")))
    }
}
