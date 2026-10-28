package warewolf.studio.generator

val assign = Tool("Assign", "Data", "Assigns values to variables.")
val calculate = Tool("Calculate", "Data", "Evaluates an expression.")
val decision = Tool("Decision", "Control Flow", "Branches on a condition.", BranchKind.DECISION)
val switch = Tool("Switch", "Control Flow", "Branches across cases.", BranchKind.SWITCH)
val legacyDecision = Tool("Decision (legacy)", "Control Flow", "Superseded by Decision.", BranchKind.DECISION)

val catalog = listOf(assign, calculate, decision, legacyDecision, switch)

/** Returns [actions] in order, or [fallback] once they run out, and records every context it saw. */
class ScriptedChooser(vararg actions: Action, private val fallback: Action? = null) : NextActionChooser {
    private val queue = ArrayDeque(actions.toList())
    val seen = mutableListOf<ChoiceContext>()

    override fun choose(context: ChoiceContext): Action {
        seen += context
        return queue.removeFirstOrNull() ?: fallback ?: error("ScriptedChooser ran out of actions")
    }
}

/** Numbers placed steps s1, s2, ... */
class RecordingSink : StepSink {
    val placed = mutableListOf<Pair<BranchTail, Tool>>()

    override fun place(tail: BranchTail, tool: Tool): String {
        placed += tail to tool
        return "s${placed.size}"
    }
}
