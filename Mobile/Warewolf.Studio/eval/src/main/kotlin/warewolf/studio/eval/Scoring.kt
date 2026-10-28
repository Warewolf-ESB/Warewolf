package warewolf.studio.eval

/** What System 1 chose for one corpus example, against what the workflow's author placed. */
data class Prediction(val gold: String, val predicted: String) {
    val correct: Boolean get() = gold == predicted
}

data class Report(
    val total: Int,
    val correct: Int,
    /** Examples where the author ended the branch, and how many of those System 1 also ended. */
    val endTotal: Int,
    val endCorrect: Int,
    /** The most frequent (gold, predicted) mistakes, most frequent first. */
    val topConfusions: List<Pair<Pair<String, String>, Int>>,
) {
    val accuracy: Double get() = if (total == 0) 0.0 else correct.toDouble() / total
    val endAccuracy: Double get() = if (endTotal == 0) 0.0 else endCorrect.toDouble() / endTotal
}

fun score(predictions: List<Prediction>, end: String, confusions: Int = 10): Report {
    val ends = predictions.filter { it.gold == end }
    return Report(
        total = predictions.size,
        correct = predictions.count { it.correct },
        endTotal = ends.size,
        endCorrect = ends.count { it.correct },
        topConfusions = predictions.filterNot { it.correct }
            .groupingBy { it.gold to it.predicted }.eachCount()
            .entries.sortedWith(compareByDescending<Map.Entry<Pair<String, String>, Int>> { it.value }.thenBy { it.key.first }.thenBy { it.key.second })
            .take(confusions).map { it.key to it.value },
    )
}
