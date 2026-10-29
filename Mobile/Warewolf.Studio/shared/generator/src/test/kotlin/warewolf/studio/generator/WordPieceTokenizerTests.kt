package warewolf.studio.generator

import kotlinx.serialization.json.Json
import kotlinx.serialization.json.int
import kotlinx.serialization.json.jsonArray
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive
import kotlin.test.Test
import kotlin.test.assertContentEquals
import kotlin.test.assertFailsWith

/** Checks against Hugging Face's own output, written by `train/tests/make_tokenizer_golden.py`. */
class WordPieceTokenizerTests {
    private val golden = Json.parseToJsonElement(javaClass.getResource("/wordpiece-golden.json")!!.readText()).jsonObject
    private val vocab = golden.getValue("vocab").jsonArray.map { it.jsonPrimitive.content }
    private val cases = golden.getValue("cases").jsonArray.map { it.jsonObject }

    private fun ids(case: Map<String, kotlinx.serialization.json.JsonElement>, key: String) =
        case.getValue(key).jsonArray.map { it.jsonPrimitive.int }.toIntArray()

    @Test
    fun `token ids match Hugging Face's BertTokenizer`() {
        val tokenizer = WordPieceTokenizer(vocab, lowerCase = true, maxLength = 10_000)

        cases.forEach { case ->
            val text = case.getValue("text").jsonPrimitive.content
            assertContentEquals(ids(case, "ids"), tokenizer.encode(text), "text: $text")
        }
    }

    @Test
    fun `long texts lose tokens from the start, as Hugging Face truncates on the left`() {
        val tokenizer = WordPieceTokenizer(vocab, lowerCase = true, maxLength = golden.getValue("maxLength").jsonPrimitive.int)

        cases.forEach { case ->
            val text = case.getValue("text").jsonPrimitive.content
            assertContentEquals(ids(case, "truncatedIds"), tokenizer.encode(text), "text: $text")
        }
    }

    @Test
    fun `a vocabulary without the special tokens is rejected`() {
        assertFailsWith<IllegalStateException> { WordPieceTokenizer(listOf("goal"), lowerCase = true, maxLength = 8) }
    }
}
