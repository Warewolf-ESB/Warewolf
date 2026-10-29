package warewolf.studio.eval

import warewolf.studio.generator.Action
import warewolf.studio.generator.ChoiceContext
import warewolf.studio.generator.LlmChoiceChooser
import warewolf.studio.generator.NextActionChooser

/**
 * No-model baseline: places what authors most often placed after the same previous tool on the same kind of arm,
 * falling back to the previous tool alone, then to the most frequent answer overall. A trained classifier that
 * cannot beat this has learned nothing from the goal or the labels.
 */
class MarkovChooser(
    train: List<CorpusExample>,
    /** Whether END may be chosen here (see [warewolf.studio.generator.ClassifierChooser]). */
    private val endAllowed: (ChoiceContext) -> Boolean = { true },
) : NextActionChooser {
    private val known = train.filter { it.answerIsKnown }
    private val byPreviousAndArm = rank(known) { key(it.path.lastOrNull(), it.arm) }
    private val byPrevious = rank(known) { it.path.lastOrNull() ?: START }
    private val overall = rank(known) { "" }.getValue("")

    override fun choose(context: ChoiceContext): Action {
        val previous = context.tail.path.lastOrNull()
        val candidates = (byPreviousAndArm[key(previous, context.tail.arm)].orEmpty() +
            byPrevious[previous ?: START].orEmpty() + overall)
        val canEnd = endAllowed(context)
        for (name in candidates) {
            if (name == LlmChoiceChooser.END) {
                if (canEnd) return Action.End() else continue
            }
            context.tools.firstOrNull { it.name == name }?.let { return Action.Place(it) }
        }
        return if (canEnd) Action.End() else Action.Place(context.tools.first())
    }

    /** Switch case names are the author's own, so every case counts as one kind of arm. */
    private fun key(previous: String?, arm: String?) = "${previous ?: START}|" + when (arm) {
        null, "True", "False" -> arm.orEmpty()
        else -> "case"
    }

    private companion object {
        const val START = "<start>"

        /** Answers per key, most frequent first (ties by name, so runs are repeatable). */
        fun rank(examples: List<CorpusExample>, keyOf: (CorpusExample) -> String): Map<String, List<String>> =
            examples.groupBy(keyOf).mapValues { (_, group) ->
                group.groupingBy { it.next }.eachCount().entries
                    .sortedWith(compareByDescending<Map.Entry<String, Int>> { it.value }.thenBy { it.key })
                    .map { it.key }
            }
    }
}
