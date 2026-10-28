package warewolf.studio.generator

/**
 * The generation loop (Plan.md §2): take one open branch tail, ask System 1 what goes there,
 * apply it, repeat until no tails are left or the step budget runs out.
 *
 * The engine stays authoritative for the graph; this loop only decides placements and hands
 * them to [sink].
 */
class FrontierLoop(
    private val chooser: NextActionChooser,
    private val sink: StepSink,
    private val budget: Budget = Budget(),
) {
    fun run(goal: String, tools: List<Tool>): GenerationResult {
        val placeable = ToolCatalog.placeable(tools)
        val frontier = ArrayDeque(listOf(BranchTail(afterStepId = null)))
        val steps = mutableListOf<PlacedStep>()
        val outputs = linkedSetOf<String>()
        var truncated = 0

        while (frontier.isNotEmpty()) {
            if (steps.size >= budget.maxSteps) {
                return GenerationResult(steps, outputs, truncated, StopReason.STEP_BUDGET)
            }

            val tail = frontier.removeFirst()
            if (tail.path.size >= budget.maxDepth) {
                truncated++
                continue
            }

            when (val action = chooser.choose(ChoiceContext(goal, tail, placeable))) {
                is Action.End -> outputs += action.outputs
                is Action.Place -> {
                    require(action.tool in placeable) { "System 1 chose '${action.tool.name}', which is not placeable" }
                    val id = sink.place(tail, action.tool)
                    steps += PlacedStep(id, action.tool, tail)
                    val path = tail.path + action.tool.name
                    // Depth-first: the new tails go to the front, so one branch finishes before the next starts.
                    armsOf(action).asReversed().forEach { arm -> frontier.addFirst(BranchTail(id, arm, path)) }
                }
            }
        }

        return GenerationResult(steps, outputs, truncated, StopReason.FRONTIER_EMPTY)
    }

    private fun armsOf(place: Action.Place): List<String?> = when (place.tool.branchKind) {
        BranchKind.NONE -> listOf(null)
        BranchKind.DECISION -> listOf("True", "False")
        BranchKind.SWITCH -> {
            require(place.arms.isNotEmpty()) { "A Switch needs at least one case" }
            place.arms
        }
    }
}
