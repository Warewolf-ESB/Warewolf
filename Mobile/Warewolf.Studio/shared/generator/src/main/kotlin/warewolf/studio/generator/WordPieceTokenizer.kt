package warewolf.studio.generator

import java.text.Normalizer

/**
 * BERT's WordPiece tokenizer, matching Hugging Face's `BertTokenizer` for the classifier's encoder: clean and split
 * on whitespace and punctuation (lower-casing and stripping accents when [lowerCase]), then greedy longest-match
 * against [vocab]. Texts longer than [maxLength] lose tokens from the start, as the classifier was trained.
 */
class WordPieceTokenizer(vocab: List<String>, private val lowerCase: Boolean, private val maxLength: Int) {
    private val ids: Map<String, Int> = vocab.withIndex().associate { (i, token) -> token to i }
    private val unk = id("[UNK]")
    private val cls = id("[CLS]")
    private val sep = id("[SEP]")

    init {
        require(maxLength > 2) { "maxLength must leave room for [CLS] and [SEP]" }
    }

    /** `[CLS] … [SEP]` token ids for [text]. */
    fun encode(text: String): IntArray {
        val pieces = basicTokens(text).flatMap(::wordPieces)
        return (listOf(cls) + pieces.takeLast(maxLength - 2) + sep).toIntArray()
    }

    private fun id(token: String) = ids[token] ?: error("vocabulary has no $token")

    private fun basicTokens(text: String): List<String> {
        val cleaned = buildString {
            for (c in text) {
                when {
                    c.code == 0 || c.code == 0xFFFD || isControl(c) -> {}
                    isWhitespace(c) -> append(' ')
                    isCjk(c.code) -> append(' ').append(c).append(' ')
                    else -> append(c)
                }
            }
        }
        return cleaned.split(' ').filter { it.isNotEmpty() }.flatMap { word ->
            val normalized = if (lowerCase) stripAccents(word.lowercase()) else word
            splitPunctuation(normalized)
        }
    }

    private fun stripAccents(word: String) =
        Normalizer.normalize(word, Normalizer.Form.NFD).filter { it.category != CharCategory.NON_SPACING_MARK }

    private fun splitPunctuation(word: String): List<String> {
        val out = mutableListOf<String>()
        val current = StringBuilder()
        for (c in word) {
            if (isPunctuation(c)) {
                if (current.isNotEmpty()) out += current.toString().also { current.clear() }
                out += c.toString()
            } else {
                current.append(c)
            }
        }
        if (current.isNotEmpty()) out += current.toString()
        return out
    }

    private fun wordPieces(word: String): List<Int> {
        if (word.length > MAX_WORD_CHARS) return listOf(unk)
        val pieces = mutableListOf<Int>()
        var start = 0
        while (start < word.length) {
            var end = word.length
            var match: Int? = null
            while (start < end) {
                val piece = (if (start > 0) "##" else "") + word.substring(start, end)
                match = ids[piece]
                if (match != null) break
                end--
            }
            if (match == null) return listOf(unk)
            pieces += match
            start = end
        }
        return pieces
    }

    private companion object {
        const val MAX_WORD_CHARS = 100

        fun isWhitespace(c: Char) = c == ' ' || c == '\t' || c == '\n' || c == '\r' || c.category == CharCategory.SPACE_SEPARATOR

        fun isControl(c: Char) = c != '\t' && c != '\n' && c != '\r' &&
            (c.category == CharCategory.CONTROL || c.category == CharCategory.FORMAT)

        /** BERT treats every non-alphanumeric ASCII character as punctuation, plus Unicode's punctuation classes. */
        fun isPunctuation(c: Char): Boolean {
            val code = c.code
            if (code in 33..47 || code in 58..64 || code in 91..96 || code in 123..126) return true
            return c.category in setOf(
                CharCategory.CONNECTOR_PUNCTUATION, CharCategory.DASH_PUNCTUATION, CharCategory.START_PUNCTUATION,
                CharCategory.END_PUNCTUATION, CharCategory.INITIAL_QUOTE_PUNCTUATION, CharCategory.FINAL_QUOTE_PUNCTUATION,
                CharCategory.OTHER_PUNCTUATION,
            )
        }

        fun isCjk(code: Int) = code in 0x4E00..0x9FFF || code in 0x3400..0x4DBF || code in 0xF900..0xFAFF
    }
}
