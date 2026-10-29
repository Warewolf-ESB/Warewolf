package warewolf.studio.generator

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFailsWith
import kotlin.test.assertTrue

class FrontierLoopTests {
    private val sink = RecordingSink()

    @Test
    fun `place continues the branch from the new step`() {
        val chooser = ScriptedChooser(Action.Place(assign), Action.End())

        val result = FrontierLoop(chooser, sink).run("goal", catalog)

        assertEquals(listOf("Assign"), result.steps.map { it.tool.name })
        assertEquals(BranchTail(afterStepId = "s1", arm = null, path = listOf("Assign"), steps = listOf(StepSummary("Assign"))), chooser.seen[1].tail)
    }

    @Test
    fun `end closes the branch and marks its outputs`() {
        val chooser = ScriptedChooser(Action.Place(assign), Action.End(listOf("result", "error")))

        val result = FrontierLoop(chooser, sink).run("goal", catalog)

        assertEquals(setOf("result", "error"), result.outputs)
        assertEquals(StopReason.FRONTIER_EMPTY, result.stopReason)
        assertEquals(2, chooser.seen.size)
    }

    @Test
    fun `decision opens a True and a False branch`() {
        val chooser = ScriptedChooser(Action.Place(decision), Action.End(), Action.End())

        FrontierLoop(chooser, sink).run("goal", catalog)

        assertEquals(listOf("True", "False"), chooser.seen.drop(1).map { it.tail.arm })
        assertTrue(chooser.seen.drop(1).all { it.tail.afterStepId == "s1" })
    }

    @Test
    fun `each arm of a decision carries the steps placed before it`() {
        val chooser = ScriptedChooser(Action.Place(assign), Action.Place(decision), Action.End(), Action.End())

        FrontierLoop(chooser, sink).run("goal", catalog)

        val expected = listOf(StepSummary("Assign"), StepSummary("Decision"))
        assertEquals(listOf(expected, expected), chooser.seen.drop(2).map { it.tail.steps })
    }

    @Test
    fun `switch opens one branch per case`() {
        val chooser = ScriptedChooser(Action.Place(switch, listOf("a", "b", "c")), fallback = Action.End())

        FrontierLoop(chooser, sink).run("goal", catalog)

        assertEquals(listOf("a", "b", "c"), chooser.seen.drop(1).map { it.tail.arm })
    }

    @Test
    fun `switch without cases is rejected`() {
        val chooser = ScriptedChooser(Action.Place(switch))

        assertFailsWith<IllegalArgumentException> { FrontierLoop(chooser, sink).run("goal", catalog) }
    }

    @Test
    fun `one branch finishes before the next starts`() {
        val chooser = ScriptedChooser(Action.Place(decision), Action.Place(assign), Action.End(), Action.End())

        FrontierLoop(chooser, sink).run("goal", catalog)

        assertEquals(listOf(null, "True", null, "False"), chooser.seen.map { it.tail.arm })
    }

    @Test
    fun `step budget stops the loop`() {
        val chooser = ScriptedChooser(fallback = Action.Place(assign))

        val result = FrontierLoop(chooser, sink, Budget(maxSteps = 3)).run("goal", catalog)

        assertEquals(3, result.steps.size)
        assertEquals(StopReason.STEP_BUDGET, result.stopReason)
    }

    @Test
    fun `depth budget closes a branch without asking System 1`() {
        val chooser = ScriptedChooser(fallback = Action.Place(assign))

        val result = FrontierLoop(chooser, sink, Budget(maxDepth = 2)).run("goal", catalog)

        assertEquals(2, result.steps.size)
        assertEquals(1, result.truncatedBranches)
        assertEquals(2, chooser.seen.size)
        assertEquals(StopReason.FRONTIER_EMPTY, result.stopReason)
    }

    @Test
    fun `immediate end places nothing`() {
        val result = FrontierLoop(ScriptedChooser(Action.End()), sink).run("goal", catalog)

        assertTrue(result.steps.isEmpty())
        assertTrue(sink.placed.isEmpty())
        assertEquals(StopReason.FRONTIER_EMPTY, result.stopReason)
    }

    @Test
    fun `system 1 is never offered the legacy decision`() {
        val chooser = ScriptedChooser(Action.End())

        FrontierLoop(chooser, sink).run("goal", catalog)

        assertTrue(legacyDecision !in chooser.seen.single().tools)
    }

    @Test
    fun `choosing a tool that is not placeable is rejected`() {
        val chooser = ScriptedChooser(Action.Place(legacyDecision))

        assertFailsWith<IllegalArgumentException> { FrontierLoop(chooser, sink).run("goal", catalog) }
        assertTrue(sink.placed.isEmpty())
    }
}
