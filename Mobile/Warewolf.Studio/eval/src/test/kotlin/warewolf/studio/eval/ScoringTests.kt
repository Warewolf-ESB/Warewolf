package warewolf.studio.eval

import kotlin.test.Test
import kotlin.test.assertEquals

class ScoringTests {
    private val predictions = listOf(
        Prediction("Assign", "Assign"),
        Prediction("Assign", "Decision"),
        Prediction("END", "END"),
        Prediction("END", "Assign"),
        Prediction("Decision", "Decision"),
        Prediction("Assign", "Decision"),
    )

    @Test
    fun `accuracy counts exact matches over all examples`() {
        val report = score(predictions, end = "END")

        assertEquals(6, report.total)
        assertEquals(3, report.correct)
        assertEquals(0.5, report.accuracy)
    }

    @Test
    fun `END accuracy only counts examples where the author ended the branch`() {
        val report = score(predictions, end = "END")

        assertEquals(2, report.endTotal)
        assertEquals(1, report.endCorrect)
    }

    @Test
    fun `confusions are counted and ordered most frequent first`() {
        val report = score(predictions, end = "END")

        assertEquals(listOf(("Assign" to "Decision") to 2, ("END" to "Assign") to 1), report.topConfusions)
    }

    @Test
    fun `an empty run scores zero rather than dividing by zero`() {
        assertEquals(0.0, score(emptyList(), end = "END").accuracy)
    }
}
