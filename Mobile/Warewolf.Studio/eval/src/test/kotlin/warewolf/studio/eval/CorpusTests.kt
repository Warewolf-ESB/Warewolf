package warewolf.studio.eval

import warewolf.studio.generator.StepSummary
import java.nio.file.Files
import kotlin.io.path.writeText
import kotlin.test.Test
import kotlin.test.assertEquals

class CorpusTests {
    @Test
    fun `examples load with their steps, and without them from an older extraction`() {
        val dir = Files.createTempDirectory("corpus")
        dir.resolve("goals.jsonl").writeText("""{"id":"w1","goal":"Greet someone"}""")
        dir.resolve("examples.jsonl").writeText(
            """
            {"set":"a","id":"w1","family":"F","Path":["Assign"],"Arm":null,"Next":"END","Steps":[{"tool":"Assign","label":"Set name","variables":["name"]}]}
            {"set":"a","id":"w1","family":"F","Path":[],"Arm":null,"Next":"Assign"}
            {"set":"a","id":"w2","family":"F","Path":[],"Arm":null,"Next":"Assign","Steps":[]}
            """.trimIndent(),
        )

        val examples = Corpus.loadExamples(dir)

        assertEquals(2, examples.size, "w2 has no goal, so it is left out")
        assertEquals(listOf(StepSummary("Assign", "Set name", listOf("name"))), examples[0].steps)
        assertEquals(emptyList(), examples[1].steps)
    }
}
