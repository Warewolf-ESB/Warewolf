package warewolf.studio.eval.bakeoff

import kotlinx.serialization.json.Json
import kotlinx.serialization.json.JsonObject
import kotlinx.serialization.json.boolean
import kotlinx.serialization.json.int
import kotlinx.serialization.json.jsonArray
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertNotEquals
import kotlin.test.assertNull
import kotlin.test.assertTrue

private fun obj(json: String) = Json.parseToJsonElement(json).jsonObject

/** A workflow with inputs `id` (scalar) and `rows` (recordset of name, qty), output `total`, and one SQL step. */
private val subject = Subject(
    "abc12345-0000", "Total the quantities for an order", "Bakeoff/abc12345",
    obj(
        """{"envelope":{"name":"Totals","inputs":[{"kind":"scalar","name":"id"},{"kind":"recordset","name":"rows","fields":["name","qty"]}],
           "outputs":[{"kind":"scalar","name":"total"}]},
           "body":{"resourcename":"Totals","cells":[
             {"id":"s","shape":"start","data":{"type":"start"}},
             {"id":"11111111-2222-3333-4444-555555555555","shape":"dsfsqlserverdatabaseactivity","data":{"type":"dsfsqlserverdatabaseactivity","displayname":"Load order"}},
             {"shape":"edge","source":{"cell":"s"},"target":{"cell":"11111111-2222-3333-4444-555555555555"}}]}}""",
    ),
)

/** An engine that answers create_test with [createReplies] in order (repeating the last) and execute_test with [executeReply]. */
private class FakeTestEngine(vararg createReplies: Pair<Int, String>, val executeReply: String = """{"result":"TestPassed","message":""}""") {
    private val creates = ArrayDeque(createReplies.toList())
    val calls = mutableListOf<Pair<String, JsonObject>>()
    val client = EngineClient { tool, body ->
        calls += tool to Json.parseToJsonElement(body).jsonObject
        when (tool) {
            "create_test" -> if (creates.size > 1) creates.removeFirst() else creates.first()
            "execute_test" -> 200 to executeReply
            else -> 400 to """{"message":"not faked: $tool"}"""
        }
    }
}

private const val STORED = """{"created":true}"""
private const val GOOD_TEST = """{"testName":"x","inputs":[{"variable":"[[id]]","value":"7"}],
    "outputs":[{"variable":"[[total]]","assertOp":"=","value":"12"}],
    "testSteps":[{"activityId":"11111111-2222-3333-4444-555555555555","activityType":"dsfsqlserverdatabaseactivity","type":"Mock",
      "stepOutputs":[{"variable":"[[rows(1).qty]]","assertOp":"=","value":"12"}]}]}"""

class ServiceTestGeneratorTests {
    private fun generator(model: TextModel, engine: FakeTestEngine, constrained: Boolean = false) =
        ServiceTestGenerator(model, engine.client, testCatalog, """{"testName":"string"}""", constrained, "lmstudio_gemma_run1")

    @Test
    fun `the prompt names steps by node id and asks for external steps to be mocked`() {
        val prompt = generator(ScriptedModel(GOOD_TEST), FakeTestEngine(200 to STORED)).prompt(subject)

        assertTrue("Total the quantities for an order" in prompt)
        assertTrue("11111111-2222-3333-4444-555555555555: SQL Server Database \"Load order\"" in prompt)
        assertTrue("""{"testName":"string"}""" in prompt)
        assertTrue("type Mock" in prompt)
    }

    @Test
    fun `a stored test is run and scored under the harness's own test name`() {
        val engine = FakeTestEngine(200 to STORED)

        val outcome = generator(ScriptedModel(GOOD_TEST), engine).generate(subject)

        assertTrue(outcome.accepted)
        assertTrue(outcome.firstTryAccepted)
        assertEquals(1, outcome.attempts)
        assertEquals(listOf("create_test", "execute_test"), engine.calls.map { it.first })
        assertEquals("lmstudio_gemma_run1_1", engine.calls[0].second.getValue("test").jsonObject.getValue("testName").jsonPrimitive.content)
        assertEquals("lmstudio_gemma_run1_1", engine.calls[1].second.getValue("testName").jsonPrimitive.content)
        assertEquals("Bakeoff/abc12345", engine.calls[1].second.getValue("name").jsonPrimitive.content)
        assertEquals("TestPassed", outcome.scores.getValue("result").jsonPrimitive.content)
        assertEquals(1, outcome.scores.getValue("outputsAsserted").jsonPrimitive.int)
        assertEquals(1, outcome.scores.getValue("mocks").jsonPrimitive.int)
    }

    @Test
    fun `a rejected test goes back to the model with the engine's reason`() {
        val engine = FakeTestEngine(400 to """{"message":"activityId 'n9' is not a node"}""", 200 to STORED)
        val model = ScriptedModel(GOOD_TEST)

        val outcome = generator(model, engine).generate(subject)

        assertTrue(outcome.accepted)
        assertFalse(outcome.firstTryAccepted)
        assertEquals(2, outcome.attempts)
        assertTrue("activityId 'n9' is not a node" in model.prompts[1])
        assertEquals("lmstudio_gemma_run1_2", engine.calls[1].second.getValue("test").jsonObject.getValue("testName").jsonPrimitive.content)
    }

    @Test
    fun `replies that never parse are not accepted and never reach the engine`() {
        val engine = FakeTestEngine(200 to STORED)

        val outcome = generator(ScriptedModel("no json here"), engine).generate(subject)

        assertFalse(outcome.accepted)
        assertEquals(3, outcome.attempts)
        assertEquals(listOf(ServiceTestGenerator.NOT_A_TEST), outcome.errors)
        assertTrue(engine.calls.isEmpty())
    }

    @Test
    fun `asserting a recordset field counts as asserting the recordset output`() {
        val rsSubject = subject.copy(definition = obj(
            """{"envelope":{"inputs":[],"outputs":[{"kind":"recordset","name":"lines","fields":["a"]},{"kind":"scalar","name":"total"}]},"body":{"cells":[]}}""",
        ))
        val test = obj("""{"outputs":[{"variable":"[[lines(*).a]]","assertOp":"Is Not NULL","value":""}],"testSteps":[{"type":"Assert"}]}""")

        val coverage = ServiceTestGenerator.coverage(rsSubject, test)

        assertEquals(2, coverage.getValue("outputs").jsonPrimitive.int)
        assertEquals(1, coverage.getValue("outputsAsserted").jsonPrimitive.int)
        assertEquals(0, coverage.getValue("mocks").jsonPrimitive.int)
        assertEquals(1, coverage.getValue("stepAsserts").jsonPrimitive.int)
    }

    @Test
    fun `a local model is held to the assert operators and step types the engine knows`() {
        val model = ScriptedModel(GOOD_TEST)

        generator(model, FakeTestEngine(200 to STORED), constrained = true).generate(subject)
        val format = model.formats.single()!!
        val ops = format.getValue("\$defs").jsonObject.getValue("assertion").jsonObject.getValue("properties").jsonObject
            .getValue("assertOp").jsonObject.getValue("enum").jsonArray.map { it.jsonPrimitive.content }

        assertEquals(ServiceTestGenerator.ASSERT_OPS, ops)
        assertTrue("There is No Error" in ops)
    }

    @Test
    fun `Claude gets no format`() {
        val model = ScriptedModel(GOOD_TEST)

        generator(model, FakeTestEngine(200 to STORED)).generate(subject)

        assertNull(model.formats.single())
    }
}

class FormCheckTests {
    private val goodForm = obj(
        """{"type":"object","required":["id"],"properties":{
             "id":{"type":"integer","title":"Order id","description":"The order to total","minimum":1},
             "rows":{"type":"array","title":"Lines","items":{"type":"object","properties":{"name":{"type":"string"},"qty":{"type":"integer"}}}}}}""",
    )

    @Test
    fun `a form with every input in the right shape passes`() {
        val result = FormCheck.check(goodForm, subject.inputs)

        assertTrue(result.ok, result.errors.toString())
        assertEquals(2, result.right)
    }

    @Test
    fun `a missing input and an extra property are both reported`() {
        val form = obj("""{"type":"object","properties":{"id":{"type":"string"},"notes":{"type":"string"}}}""")

        val result = FormCheck.check(form, subject.inputs)

        assertEquals(listOf("rows"), result.missing)
        assertEquals(listOf("notes"), result.extra)
        assertFalse(result.ok)
    }

    @Test
    fun `wrong shapes are reported per input`() {
        val form = obj(
            """{"type":"object","properties":{"id":{"type":"object"},
                 "rows":{"type":"array","items":{"type":"object","properties":{"name":{"type":"string"}}}}}}""",
        )

        val result = FormCheck.check(form, subject.inputs)

        assertEquals(0, result.right)
        assertTrue(result.wrongShape.any { "input 'id'" in it && "scalar" in it })
        assertTrue(result.wrongShape.any { "input 'rows'" in it && "qty" in it })
    }

    @Test
    fun `a recordset that is not an array is the wrong shape`() {
        val form = obj("""{"type":"object","properties":{"id":{"type":"string"},"rows":{"type":"string"}}}""")

        assertTrue(FormCheck.check(form, subject.inputs).wrongShape.single().startsWith("input 'rows': a recordset must be an array"))
    }

    @Test
    fun `a document that is not a valid JSON Schema fails the meta-schema`() {
        val form = obj("""{"type":"object","properties":{"id":{"type":5},"rows":{"type":"array","items":{"type":"object","properties":{"name":{},"qty":{}}}}}}""")

        val result = FormCheck.check(form, subject.inputs)

        assertTrue(result.metaErrors.isNotEmpty())
        assertFalse(result.ok)
        assertFalse(result.scores(2).getValue("metaValid").jsonPrimitive.boolean)
    }
}

class InputFormGeneratorTests {
    private val missingRows = """{"type":"object","properties":{"id":{"type":"string","title":"Id"}}}"""
    private val complete = """{"type":"object","properties":{"id":{"type":"string","title":"Id"},
        "rows":{"type":"array","items":{"type":"object","properties":{"name":{"type":"string"},"qty":{"type":"number"}}}}}}"""

    @Test
    fun `the prompt lists each input with its kind and fields`() {
        val prompt = InputFormGenerator(ScriptedModel(complete), testCatalog, constrained = false).prompt(subject)

        assertTrue("- id (scalar)" in prompt)
        assertTrue("- rows (recordset, fields: name, qty)" in prompt)
        assertTrue("JSON Schema" in prompt)
    }

    @Test
    fun `a failing form is repaired with the check's problems`() {
        val model = ScriptedModel(missingRows, complete)

        val outcome = InputFormGenerator(model, testCatalog, constrained = true).generate(subject)

        assertTrue(outcome.accepted)
        assertFalse(outcome.firstTryAccepted)
        assertEquals(2, outcome.attempts)
        assertTrue("no property for input 'rows'" in model.prompts[1])
        assertEquals(InputFormGenerator.FORM_FORMAT, model.formats[0])
        assertEquals(2, outcome.scores.getValue("inputsCovered").jsonPrimitive.int)
    }

    @Test
    fun `a form still failing after every repair keeps its scores and problems`() {
        val outcome = InputFormGenerator(ScriptedModel(missingRows), testCatalog, constrained = false).generate(subject)

        assertFalse(outcome.accepted)
        assertEquals(3, outcome.attempts)
        assertEquals(1, outcome.scores.getValue("inputsCovered").jsonPrimitive.int)
        assertEquals(listOf("no property for input 'rows'"), outcome.errors)
    }
}

class SubjectsTests {
    private val workflow = obj(
        """{"envelope":{},"body":{"cells":[
             {"id":"start","shape":"start","data":{"type":"start"}},
             {"id":"loop","shape":"dsfforeachactivity","data":{"type":"dsfforeachactivity"}},
             {"id":"inner","shape":"dsfdotnetmultiassignactivity","data":{"type":"dsfdotnetmultiassignactivity","isNested":true,"parentId":"loop","UniqueID":"inner"}},
             {"id":"11111111-2222-3333-4444-555555555555","shape":"flowdecision","data":{"type":"flowdecision"}},
             {"shape":"edge","source":{"cell":"start"},"target":{"cell":"loop"}},
             {"shape":"edge","source":{"cell":"loop"},"target":{"cell":"11111111-2222-3333-4444-555555555555"}}]}}""",
    )

    @Test
    fun `non-GUID node ids become GUIDs everywhere they are referenced, and only there`() {
        val cells = Subjects.withGuidIds(workflow).getValue("body").jsonObject.getValue("cells").jsonArray.map { it.jsonObject }
        val id = { i: Int -> cells[i].getValue("id").jsonPrimitive.content }
        val cell = { i: Int, end: String -> cells[i].getValue(end).jsonObject.getValue("cell").jsonPrimitive.content }

        assertTrue(listOf(0, 1, 2).all { Regex("^[0-9a-f-]{36}$").matches(id(it)) })
        assertEquals("11111111-2222-3333-4444-555555555555", id(3))
        assertEquals(id(0), cell(4, "source"))
        assertEquals(id(1), cell(4, "target"))
        assertEquals(id(1), cell(5, "source"))
        assertEquals("11111111-2222-3333-4444-555555555555", cell(5, "target"))
        assertEquals(id(1), cells[2].getValue("data").jsonObject.getValue("parentId").jsonPrimitive.content)
        assertEquals(id(2), cells[2].getValue("data").jsonObject.getValue("UniqueID").jsonPrimitive.content)
        assertEquals("start", cells[0].getValue("shape").jsonPrimitive.content)
        assertEquals("start", cells[0].getValue("data").jsonObject.getValue("type").jsonPrimitive.content)
    }

    @Test
    fun `the same id always maps to the same GUID, and different ids to different ones`() {
        val first = Subjects.withGuidIds(workflow).toString()

        assertEquals(first, Subjects.withGuidIds(workflow).toString())
        val ids = Subjects.withGuidIds(workflow).getValue("body").jsonObject.getValue("cells").jsonArray.take(3).map { it.jsonObject.getValue("id") }
        assertNotEquals(ids[0], ids[1])
        assertNotEquals(ids[1], ids[2])
    }

    @Test
    fun `a workflow whose ids are all GUIDs is returned unchanged`() {
        val guids = obj("""{"envelope":{},"body":{"cells":[{"id":"11111111-2222-3333-4444-555555555555","shape":"start","data":{"type":"start"}}]}}""")

        assertEquals(guids, Subjects.withGuidIds(guids))
    }
}

class ArtifactJudgeTests {
    @Test
    fun `the judge sees the goal, the workflow, the artifact and the task's rubric`() {
        val prompt = ArtifactJudge(ScriptedModel("{}"), ArtifactJudge.Rubric.FORM)
            .prompt("Total an order", "Inputs: id", obj("""{"type":"object"}"""))

        assertTrue("Goal: Total an order" in prompt)
        assertTrue("Inputs: id" in prompt)
        assertTrue("""{"type":"object"}""" in prompt)
        assertTrue(ArtifactJudge.Rubric.FORM.scale in prompt)
        assertFalse(ArtifactJudge.Rubric.TEST.scale in prompt)
    }

    @Test
    fun `a verdict is parsed like the workflow judge's`() {
        val verdict = ArtifactJudge(ScriptedModel("""{"score": 4, "reason": "good"}"""), ArtifactJudge.Rubric.TEST)
            .rate("goal", "summary", obj("{}"))

        assertEquals(4, verdict?.score)
    }

    @Test
    fun `the harness's test name, which names the contender, is hidden from the judge`() {
        val shown = ArtifactBakeOff.artifactForJudge(obj("""{"testName":"claude_opus_x_1","inputs":[]}"""))

        assertEquals(setOf("inputs"), shown.keys)
    }
}
