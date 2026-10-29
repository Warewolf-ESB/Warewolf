package warewolf.studio.eval.bakeoff

import kotlinx.serialization.json.Json
import kotlinx.serialization.json.jsonObject
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertNotNull
import kotlin.test.assertNull
import kotlin.test.assertTrue

class WorkflowTextTests {
    @Test
    fun `a workflow is read from a fenced block, from raw JSON, or not at all`() {
        assertNotNull(WorkflowText.extractWorkflow("Here you go:\n```json\n$VALID_WORKFLOW\n```\nDone."))
        assertNotNull(WorkflowText.extractWorkflow("Sure! $VALID_WORKFLOW Hope that helps."))
        assertNull(WorkflowText.extractWorkflow("""{"body":{}}"""))
        assertNull(WorkflowText.extractWorkflow("I cannot do that."))
    }

    @Test
    fun `variables are declared by kind, nested ones included`() {
        val declared = WorkflowText.declarations(listOf("a", "rs(*).f", "rs([[i]]).g", "@person.name", "a")).toString()

        assertEquals(
            """[{"kind":"scalar","name":"a"},{"kind":"scalar","name":"i"},{"kind":"object","name":"person"},""" +
                """{"kind":"recordset","name":"rs","fields":["f","g"]}]""",
            declared,
        )
    }

    @Test
    fun `tool names come from data types, whether short or assembly-qualified`() {
        val body = Json.parseToJsonElement("""{"cells":[
            {"id":"start","shape":"start","data":{"type":"start"}},
            {"id":"n1","shape":"x","data":{"type":"Dev2.Activities.DsfDotNetMultiAssignActivity, Dev2.Activities, Version=0.0.0.0"}},
            {"id":"n2","shape":"x","data":{"type":"flowdecision"}},
            {"id":"n3","shape":"x","data":{"type":"madeup"}},
            {"shape":"edge","source":{"cell":"start"},"target":{"cell":"n1"}}]}""").jsonObject

        assertEquals(listOf("Assign", "Decision", "?madeup"), WorkflowText.toolNames(body, testCatalog))
    }

    @Test
    fun `the judge's summary shows steps, fields and arms but no engine bookkeeping`() {
        val workflow = Json.parseToJsonElement("""{"envelope":{"inputs":[{"kind":"scalar","name":"a"}],"outputs":[]},"body":{"cells":[
            {"id":"s","shape":"start","data":{"type":"start"}},
            {"id":"d","position":{"x":1},"data":{"type":"flowdecision","displayname":"a set?","expression":"[[a]]","properties":{"UniqueID":"x"}}},
            {"id":"n","data":{"type":"dsfdotnetmultiassignactivity","displayname":"Set b","fields":[{"FieldName":"[[b]]","FieldValue":"1","IndexNumber":0,"HasError":false}]}},
            {"shape":"edge","source":{"cell":"s"},"target":{"cell":"d"}},
            {"shape":"edge","source":{"cell":"d"},"target":{"cell":"n"},"data":{"isDecisionArm":true,"isTrue":true}}]}}""").jsonObject

        val summary = WorkflowText.summary(workflow, testCatalog)

        assertTrue("S1: Decision \"a set?\" {\"expression\":\"[[a]]\"}" in summary, summary)
        assertTrue("S2: Assign \"Set b\" {\"fields\":[{\"FieldName\":\"[[b]]\",\"FieldValue\":\"1\"}]}" in summary, summary)
        assertTrue("S1 -> S2 (true)" in summary, summary)
        assertFalse("UniqueID" in summary || "position" in summary || "IndexNumber" in summary, summary)
    }

    @Test
    fun `the prompt carries the rules, every tool with its fields, and the goal`() {
        val prompt = WorkflowText.prompt("Greet someone", testCatalog, "{schema}")

        assertTrue("isDecisionArm" in prompt)
        assertTrue("* SQL Server Database | dsfsqlserverdatabaseactivity | Database | Runs SQL. (needs a source)" in prompt)
        assertTrue("    - expression: string, required — the condition." in prompt)
        assertTrue(prompt.trimEnd().endsWith("Goal: Greet someone"))
    }
}
