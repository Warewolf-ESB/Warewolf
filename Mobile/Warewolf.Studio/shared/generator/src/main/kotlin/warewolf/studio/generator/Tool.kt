package warewolf.studio.generator

/** How placing a tool changes the frontier: most tools continue their branch, Decision and Switch fork it. */
enum class BranchKind { NONE, DECISION, SWITCH }

/**
 * One toolbox entry, as the engine's `list_tools` reports it
 * (`Dev/Warewolf.Execution.Lightweight/Mcp/ToolCatalog.cs`).
 */
data class Tool(
    val name: String,
    val category: String,
    val description: String,
    val branchKind: BranchKind = BranchKind.NONE,
)

object ToolCatalog {
    /**
     * Tools the generator must never place. Once a workflow holds a legacy Decision
     * (`DsfDecision`), the engine reports it `bodyEditable: false` and rejects every later
     * `add_step` (`AddStepTool.cs`, "Known current limitation").
     */
    val excluded: Set<String> = setOf("Decision (legacy)")

    /** The tools System 1 may choose from, in the catalog's own order. */
    fun placeable(tools: List<Tool>): List<Tool> = tools.filter { it.name !in excluded }
}
