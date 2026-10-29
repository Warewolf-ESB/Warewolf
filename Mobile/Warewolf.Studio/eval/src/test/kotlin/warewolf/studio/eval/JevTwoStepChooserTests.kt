package warewolf.studio.eval

import kotlinx.serialization.json.Json
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive
import warewolf.studio.generator.Action
import warewolf.studio.generator.BranchKind
import warewolf.studio.generator.BranchTail
import warewolf.studio.generator.ChoiceContext
import warewolf.studio.generator.InvalidChoiceException
import warewolf.studio.generator.StepSummary
import warewolf.studio.generator.Tool
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFailsWith
import kotlin.test.assertTrue

class JevTwoStepChooserTests {
    private val assign = Tool("Assign", "Data", "Assigns values to variables.")
    private val decision = Tool("Decision", "Control Flow", "Branches on a condition.", BranchKind.DECISION)
    private val context = ChoiceContext("Add two numbers", BranchTail(afterStepId = null, path = listOf("Assign")), listOf(assign, decision))

    /** A fake Jev answering both questions in the documented response shape. */
    private class FakeJev(private val finished: Double, private val tool: String) {
        val requests = mutableListOf<String>()
        val post: (String) -> String = { body ->
            requests += body
            """{"model":"jev-1.14.0","answers":{
                 "finished":{"type":"noul","noul":$finished},
                 "next_tool":{"type":"choice","choice":"$tool","confidence":0.7,"probabilities":{"$tool":0.7}}}}"""
        }
    }

    @Test
    fun `one request asks whether the branch is finished and which tool comes next, without END among the tools`() {
        val jev = FakeJev(finished = 0.1, tool = "Assign")

        JevTwoStepChooser(post = jev.post).choose(context)
        val questions = Json.parseToJsonElement(jev.requests.single()).jsonObject.getValue("questions").jsonObject

        assertEquals("noul", questions.getValue("finished").jsonObject.getValue("type").jsonPrimitive.content)
        val nextTool = questions.getValue("next_tool").jsonObject
        assertEquals("choice", nextTool.getValue("type").jsonPrimitive.content)
        assertEquals(listOf("Assign", "Decision"), nextTool.getValue("criteria").jsonObject.keys.toList())
    }

    @Test
    fun `a finished probability at the threshold ends the branch and ignores the tool`() {
        val chooser = JevTwoStepChooser(endThreshold = 0.6, post = FakeJev(finished = 0.6, tool = "Decision").post)

        assertEquals(Action.End(), chooser.choose(context))
        assertEquals(0.6, chooser.lastFinished)
        assertEquals("Decision", chooser.lastTool)
    }

    @Test
    fun `a finished probability below the threshold places the chosen tool`() {
        val chooser = JevTwoStepChooser(endThreshold = 0.6, post = FakeJev(finished = 0.59, tool = "Decision").post)

        assertEquals(Action.Place(decision), chooser.choose(context))
    }

    @Test
    fun `a tool outside the catalog is rejected`() {
        val chooser = JevTwoStepChooser(post = FakeJev(finished = 0.1, tool = "Sort Records").post)

        assertFailsWith<InvalidChoiceException> { chooser.choose(context) }
    }

    @Test
    fun `the state shows each step's label and variables when the tail has them`() {
        val jev = FakeJev(finished = 0.1, tool = "Assign")
        val rich = context.copy(tail = BranchTail(null, path = listOf("Assign"), steps = listOf(StepSummary("Assign", "Set defaults", listOf("name")))))

        JevTwoStepChooser(post = jev.post).choose(rich)
        val state = Json.parseToJsonElement(jev.requests.single()).jsonObject.getValue("state").jsonPrimitive.content

        assertTrue("""Steps so far on this branch: Assign "Set defaults" [[name]]""" in state)
    }

    @Test
    fun `the state lists tool names only when the tail has no steps`() {
        val jev = FakeJev(finished = 0.1, tool = "Assign")

        JevTwoStepChooser(post = jev.post).choose(context)
        val state = Json.parseToJsonElement(jev.requests.single()).jsonObject.getValue("state").jsonPrimitive.content

        assertTrue(state.lines().contains("Steps so far on this branch: Assign"))
    }
}
