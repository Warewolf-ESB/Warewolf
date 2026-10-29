package warewolf.studio.generator

/** An open branch tail: where the next step on this branch attaches. */
data class BranchTail(
    /** The step this branch continues from; `null` attaches to the workflow's start node. */
    val afterStepId: String?,
    /** The Decision or Switch arm this branch hangs off, when [afterStepId] forks. */
    val arm: String? = null,
    /** The tool names already placed on the path from the start node to this tail. */
    val path: List<String> = emptyList(),
    /** The same steps as [path] in more detail, when known; empty means System 1 sees tool names only. */
    val steps: List<StepSummary> = emptyList(),
) {
    /**
     * The steps so far for a System 1 prompt: `Assign "Set defaults" [[name]], [[age]] -> Decision`, or
     * `(none)`. Labels and variable lists are capped so small models keep a short prompt. Only the last [recent]
     * steps are shown; earlier ones are counted, as `(3 earlier steps) -> …`.
     */
    fun describePath(recent: Int = Int.MAX_VALUE): String {
        val items = if (steps.isEmpty()) path else steps.map { it.describe() }
        if (items.isEmpty()) return "(none)"
        val shown = items.takeLast(recent)
        val earlier = items.size - shown.size
        return (if (earlier > 0) listOf("($earlier earlier steps)") + shown else shown).joinToString(" -> ")
    }
}

/** One step already on a branch: its tool, the label it was given, and the variables its fields mention. */
data class StepSummary(
    val tool: String,
    val label: String = "",
    val variables: List<String> = emptyList(),
) {
    fun describe(): String = buildString {
        append(tool)
        if (label.isNotBlank()) append(" \"").append(label.trim().take(MAX_LABEL)).append('"')
        if (variables.isNotEmpty()) {
            append(' ').append(variables.take(MAX_VARIABLES).joinToString(", ") { "[[$it]]" })
            if (variables.size > MAX_VARIABLES) append(", …")
        }
    }

    companion object {
        const val MAX_LABEL = 60
        const val MAX_VARIABLES = 8
    }
}

/** What System 1 decides for one branch tail. */
sealed interface Action {
    /** Place [tool] at the tail. For a Switch, [arms] names its cases; Decision always has True and False. */
    data class Place(val tool: Tool, val arms: List<String> = emptyList()) : Action

    /** Close the branch and mark [outputs] as workflow outputs. */
    data class End(val outputs: List<String> = emptyList()) : Action
}

/** Everything System 1 sees when it decides for one tail. */
data class ChoiceContext(
    val goal: String,
    val tail: BranchTail,
    val tools: List<Tool>,
) {
    /**
     * The goal, the steps so far and the arm, one per line. Every chooser describes the state with this, and the
     * trained classifier is trained on exactly this text, so the wording must only change with a retrain.
     * [recentSteps] caps the steps shown (see [BranchTail.describePath]).
     */
    fun describeState(recentSteps: Int = Int.MAX_VALUE): String = buildString {
        appendLine("Goal: $goal")
        appendLine("Steps so far on this branch: ${tail.describePath(recentSteps)}")
        tail.arm?.let { appendLine("This branch is the '$it' arm of the previous step.") }
    }
}

/** System 1: picks the next action for one branch tail. The trained classifier implements this too. */
fun interface NextActionChooser {
    fun choose(context: ChoiceContext): Action
}

/** Applies a placement to the workflow; the engine client implements it with `add_step`. Returns the new step's id. */
fun interface StepSink {
    fun place(tail: BranchTail, tool: Tool): String

    /** What System 1 sees of a placed step on later decisions: its label and variables, once fields are filled. */
    fun summary(stepId: String, tool: Tool): StepSummary = StepSummary(tool.name)
}

data class Budget(val maxSteps: Int = 50, val maxDepth: Int = 20)

enum class StopReason { FRONTIER_EMPTY, STEP_BUDGET }

data class PlacedStep(val id: String, val tool: Tool, val tail: BranchTail)

data class GenerationResult(
    val steps: List<PlacedStep>,
    val outputs: Set<String>,
    /** Branches closed by the depth budget rather than by System 1. */
    val truncatedBranches: Int,
    val stopReason: StopReason,
)
