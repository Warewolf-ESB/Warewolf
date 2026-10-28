package warewolf.studio.generator

/** An open branch tail: where the next step on this branch attaches. */
data class BranchTail(
    /** The step this branch continues from; `null` attaches to the workflow's start node. */
    val afterStepId: String?,
    /** The Decision or Switch arm this branch hangs off, when [afterStepId] forks. */
    val arm: String? = null,
    /** The tool names already placed on the path from the start node to this tail. */
    val path: List<String> = emptyList(),
)

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
)

/** System 1: picks the next action for one branch tail. The trained classifier implements this too. */
fun interface NextActionChooser {
    fun choose(context: ChoiceContext): Action
}

/** Applies a placement to the workflow; the engine client implements it with `add_step`. Returns the new step's id. */
fun interface StepSink {
    fun place(tail: BranchTail, tool: Tool): String
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
