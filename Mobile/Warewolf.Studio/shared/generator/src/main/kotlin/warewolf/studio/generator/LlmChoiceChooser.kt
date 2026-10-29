package warewolf.studio.generator

/**
 * A local LLM that can be held to one choice from a fixed list (grammar- or enum-constrained
 * decoding). llama.cpp implements it on the phone; the evaluation harness uses Ollama.
 */
fun interface LlmClient {
    /** Returns the index into [options] the model chose. */
    fun choose(prompt: String, options: List<String>): Int
}

class InvalidChoiceException(message: String) : RuntimeException(message)

/**
 * System 1 baseline (Plan.md §2, option b): the small LLM picks one tool name or END. The
 * trained classifier has to beat this. Switch cases and END outputs are left to the
 * field-filling step, so this chooser always returns them empty.
 */
class LlmChoiceChooser(private val llm: LlmClient) : NextActionChooser {
    override fun choose(context: ChoiceContext): Action {
        val options = context.tools.map { it.name } + END
        val index = llm.choose(prompt(context, options), options)
        if (index !in options.indices) {
            throw InvalidChoiceException("LLM chose option $index; valid options are 0..${options.lastIndex}")
        }
        return if (index == options.lastIndex) Action.End() else Action.Place(context.tools[index])
    }

    internal fun prompt(context: ChoiceContext, options: List<String>): String = buildString {
        appendLine("You are building a Warewolf workflow one step at a time.")
        append(context.describeState())
        appendLine("Choose the next step. Pick $END if this branch is finished.")
        options.forEachIndexed { i, name ->
            val description = context.tools.getOrNull(i)?.description ?: "Finish this branch."
            appendLine("$i. $name - $description")
        }
    }

    companion object {
        const val END = "END"
    }
}
