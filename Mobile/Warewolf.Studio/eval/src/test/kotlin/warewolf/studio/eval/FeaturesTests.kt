package warewolf.studio.eval

import kotlinx.serialization.json.Json
import kotlinx.serialization.json.jsonArray
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive
import warewolf.studio.generator.StepSummary
import warewolf.studio.generator.Tool
import java.nio.file.Files
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertTrue

class FeaturesTests {
    private val tools = listOf(Tool("Assign", "Data", "Assigns."), Tool("Decision", "Control Flow", "Branches."))

    private fun example(family: String, next: String, set: String = "a") = CorpusExample(
        workflowId = "w-$family", set = set, family = family, goal = "Greet someone",
        path = listOf("Assign"), arm = null, next = next,
        steps = listOf(StepSummary("Assign", "Set name", listOf("name"))),
    )

    @Test
    fun `a row's text is the choosers' own state description with rich steps`() {
        val example = example("AA0000000000", "END")

        val row = Features.row(example, tools)

        assertEquals(example.context(tools, rich = true).describeState(Features.RECENT_STEPS), row.getValue("text").jsonPrimitive.content)
        assertTrue("""Assign "Set name" [[name]]""" in row.getValue("text").jsonPrimitive.content)
        assertEquals("END", row.getValue("label").jsonPrimitive.content)
    }

    @Test
    fun `validation holds out whole families by their hash`() {
        assertTrue(Features.isValidation("000000000000"))
        assertTrue(Features.isValidation("25FFFFFFFFFF"))
        assertFalse(Features.isValidation("260000000000"))
        assertFalse(Features.isValidation("FF0000000000"))
    }

    @Test
    fun `export splits train from validation by family and leaves out answers the classifier cannot give`() {
        val out = Files.createTempDirectory("features")
        val split = Split(
            train = listOf(example("00AAAAAAAAAA", "Assign"), example("00AAAAAAAAAA", "END"), example("F0BBBBBBBBBB", "Decision"), example("F0BBBBBBBBBB", "?SomeLegacyType")),
            eval = listOf(example("F1CCCCCCCCCC", "Assign", set = "held")),
            droppedForLeakage = 0,
        )

        val counts = Features.export(split, tools, out)

        assertEquals(mapOf("train" to 1, "val" to 2, "eval" to 1), counts)
        val families = { name: String -> Files.readAllLines(out.resolve("$name.jsonl")).map { Json.parseToJsonElement(it).jsonObject.getValue("family").jsonPrimitive.content }.toSet() }
        assertEquals(setOf("F0BBBBBBBBBB"), families("train"))
        assertEquals(setOf("00AAAAAAAAAA"), families("val"))
        val labels = Json.parseToJsonElement(Files.readString(out.resolve("labels.json"))).jsonArray.map { it.jsonPrimitive.content }
        assertEquals(listOf("Assign", "Decision", "END"), labels)
    }
}
