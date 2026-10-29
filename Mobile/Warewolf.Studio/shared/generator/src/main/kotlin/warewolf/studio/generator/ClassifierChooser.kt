package warewolf.studio.generator

/** The trained System 1 model: one score per label for a state text. ONNX Runtime implements it on each platform. */
fun interface Classifier {
    fun scores(text: String): FloatArray
}

/**
 * System 1, option a (Plan.md §2): the trained classifier scores every tool and END for the state text it was
 * trained on ([ChoiceContext.describeState] with the last [recentSteps] steps), and the best-scoring one that is
 * on offer wins. Like the baseline, Switch cases and END outputs are left to field filling.
 */
class ClassifierChooser(
    private val classifier: Classifier,
    /** The model's outputs in order: tool names, then END. */
    private val labels: List<String>,
    catalog: List<Tool>,
    private val recentSteps: Int,
    /** Whether END may be chosen here; a validator that wants both Decision arms wired rules it out on a fresh arm. */
    private val endAllowed: (ChoiceContext) -> Boolean = { true },
) : NextActionChooser {
    init {
        val known = catalog.mapTo(hashSetOf()) { it.name } + END
        val unknown = labels.filter { it !in known }
        require(unknown.isEmpty()) { "the model knows labels the catalog does not: $unknown" }
    }

    override fun choose(context: ChoiceContext): Action {
        val scores = classifier.scores(context.describeState(recentSteps))
        check(scores.size == labels.size) { "the model returned ${scores.size} scores for ${labels.size} labels" }
        val offered = context.tools.associateBy { it.name }
        val canEnd = endAllowed(context)
        val best = labels.indices
            .filter { (labels[it] == END && canEnd) || labels[it] in offered }
            .maxByOrNull { scores[it] }
            ?: return Action.End()
        return offered[labels[best]]?.let { Action.Place(it) } ?: Action.End()
    }

    companion object {
        const val END = "END"
    }
}
