package warewolf.studio.eval.bakeoff

import kotlinx.serialization.json.JsonArray
import kotlinx.serialization.json.JsonObject
import kotlinx.serialization.json.jsonArray
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive
import warewolf.studio.generator.Action
import warewolf.studio.generator.BranchKind
import warewolf.studio.generator.BranchTail
import warewolf.studio.generator.ChoiceContext
import warewolf.studio.generator.NextActionChooser
import warewolf.studio.generator.Tool
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertNotNull
import kotlin.test.assertNull
import kotlin.test.assertTrue

class WholeWorkflowGeneratorTests {
    private val invalid = """{"valid":false,"errors":[{"message":"no start node","path":"/cells","severity":"error"}]}"""

    @Test
    fun `a valid first reply is accepted without repairs`() {
        val model = ScriptedModel(VALID_WORKFLOW)

        val outcome = WholeWorkflowGenerator(model, testCatalog, FakeEngine().client, "{}", constrained = true).generate("w", "goal")

        assertTrue(outcome.validation.valid && outcome.firstTryValid)
        assertEquals(1, outcome.attempts)
        assertNotNull(model.formats.single(), "a local model is held to the workflow format")
    }

    @Test
    fun `validator errors are fed back and the repair loop stops at its budget`() {
        val model = ScriptedModel(VALID_WORKFLOW)

        val outcome = WholeWorkflowGenerator(model, testCatalog, FakeEngine(invalid).client, "{}", constrained = false, repairs = 2).generate("w", "goal")

        assertFalse(outcome.validation.valid)
        assertEquals(3, outcome.attempts)
        assertEquals(3, model.prompts.size)
        assertTrue("- /cells: no start node" in model.prompts[1])
        assertNull(model.formats.first(), "the frontier model gets no format")
    }

    @Test
    fun `a reply that is not a workflow counts as an attempt and is retried`() {
        val model = ScriptedModel("I'm not sure.", VALID_WORKFLOW)

        val outcome = WholeWorkflowGenerator(model, testCatalog, FakeEngine().client, "{}", constrained = true).generate("w", "goal")

        assertTrue(outcome.validation.valid)
        assertFalse(outcome.firstTryValid)
        assertEquals(2, outcome.attempts)
        assertTrue(WholeWorkflowGenerator.NOT_A_WORKFLOW in model.prompts[1])
    }
}

class System1GeneratorTests {
    private val assign = Tool("Assign", "Data", "Assigns.")
    private val decision = Tool("Decision", "Control Flow", "Branches.", BranchKind.DECISION)
    private val switch = Tool("Switch", "Control Flow", "Cases.", BranchKind.SWITCH)

    private class Scripted(vararg actions: Action) : NextActionChooser {
        private val queue = ArrayDeque(actions.toList())
        override fun choose(context: ChoiceContext) = queue.removeFirstOrNull() ?: Action.End()
    }

    private val filler = FieldFiller(ScriptedModel(
        """{"label":"Step","fields":{"expression":"[[a]]","fields":[{"FieldName":"[[a]]","FieldValue":"1"}]},"cases":["gold","silver"]}""",
    ))

    private fun edges(workflow: JsonObject) = workflow.getValue("body").jsonObject.getValue("cells").jsonArray
        .map { it.jsonObject }.filter { it["shape"]?.jsonPrimitive?.content == "edge" }

    private fun describe(edge: JsonObject): String {
        val from = edge.getValue("source").jsonObject.getValue("cell").jsonPrimitive.content
        val to = edge.getValue("target").jsonObject.getValue("cell").jsonPrimitive.content
        val arm = edge["data"]?.jsonObject?.get("isTrue")?.jsonPrimitive?.content ?: edge["label"]?.jsonPrimitive?.content ?: ""
        return "$from->$to $arm".trim()
    }

    @Test
    fun `steps hang off the start node and decision arms are flagged`() {
        val chooser = Scripted(Action.Place(decision), Action.Place(assign), Action.End(), Action.Place(assign), Action.End())

        val outcome = System1Generator(chooser, filler, listOf(assign, decision), testCatalog, FakeEngine().client).generate("w", "goal")

        assertEquals(listOf("start->n1", "n1->n2 true", "n1->n3 false"), edges(outcome.workflow!!).map(::describe))
        assertTrue(outcome.validation.valid && outcome.firstTryValid)
    }

    @Test
    fun `switch cases come from the small model and label their edges`() {
        val chooser = Scripted(Action.Place(switch), Action.Place(assign), Action.End(), Action.Place(assign), Action.End())

        val outcome = System1Generator(chooser, filler, listOf(assign, switch), testCatalog, FakeEngine().client).generate("w", "goal")

        assertEquals(listOf("start->n1", "n1->n2 gold", "n1->n3 silver"), edges(outcome.workflow!!).map(::describe))
    }

    @Test
    fun `a step that brings new validator errors is refilled, then kept and counted`() {
        val engine = FakeEngine("""{"valid":false,"errors":[{"message":"bad field","path":"/cells/1","severity":"error"}]}""")

        val outcome = System1Generator(Scripted(Action.Place(assign)), filler, listOf(assign), testCatalog, engine.client, repairs = 2).generate("w", "goal")

        assertEquals(3, outcome.attempts)
        assertFalse(outcome.firstTryValid)
        assertTrue("steps still failing 1" in outcome.note!!)
    }

    @Test
    fun `an immediate END gives no workflow`() {
        val outcome = System1Generator(Scripted(), filler, listOf(assign), testCatalog, FakeEngine().client).generate("w", "goal")

        assertNull(outcome.workflow)
        assertFalse(outcome.validation.valid)
    }

    @Test
    fun `variables used by the steps are declared as outputs`() {
        val outcome = System1Generator(Scripted(Action.Place(assign)), filler, listOf(assign), testCatalog, FakeEngine().client).generate("w", "goal")

        val outputs = outcome.workflow!!.getValue("envelope").jsonObject.getValue("outputs") as JsonArray
        assertEquals("a", outputs.single().jsonObject.getValue("name").jsonPrimitive.content)
    }
}

class FieldFillerTests {
    @Test
    fun `the format names each field with the type and requirement its prose gives`() {
        val format = FieldFiller(ScriptedModel("{}")).format(testCatalog.named("Decision")!!).toString()

        assertTrue(""""expression":{"type":"string"}""" in format, format)
        assertTrue(""""and":{"type":"boolean"}""" in format, format)
        assertTrue(""""required":["expression"]""" in format, format)
    }

    @Test
    fun `prose types map to JSON types, anything unnamed is a string`() {
        assertEquals("array", FieldFiller.fieldType("array, required — x").getValue("type").jsonPrimitive.content)
        assertEquals("integer", FieldFiller.fieldType("integer, optional").getValue("type").jsonPrimitive.content)
        assertEquals("string", FieldFiller.fieldType("the value to use").getValue("type").jsonPrimitive.content)
        assertTrue(FieldFiller.isRequired("string, required — the value"))
        assertFalse(FieldFiller.isRequired("string, optional — required only when x"))
    }

    @Test
    fun `a reply that does not parse leaves the fields empty and labels the step by its tool`() {
        val fill = FieldFiller(ScriptedModel("no idea")).fill("goal", BranchTail(null), testCatalog.named("Assign")!!, null)

        assertEquals(FieldFiller.Fill("Assign", emptyMap()), fill)
    }

    @Test
    fun `a rejected attempt's reason is shown on the refill`() {
        val model = ScriptedModel("""{"label":"x","fields":{}}""")

        FieldFiller(model).fill("goal", BranchTail(null), testCatalog.named("Assign")!!, "bad field")

        assertTrue("Your previous attempt was rejected: bad field" in model.prompts.single())
    }

    @Test
    fun `switch cases fall back to two generic names when the reply has none`() {
        assertEquals(listOf("Case 1", "Case 2"), FieldFiller(ScriptedModel("{}")).cases("goal", BranchTail(null)))
    }
}
