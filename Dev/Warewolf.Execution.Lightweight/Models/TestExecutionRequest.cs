using Dev2.Common.Interfaces;
using System.Security.Principal;

namespace Warewolf.Execution.Lightweight.Models
{
    /// <summary>
    /// Request to run a persisted workflow test (<c>execute_test</c>) — the test-execution
    /// counterpart of <see cref="WorkflowExecutionRequest"/>.
    /// </summary>
    public class TestExecutionRequest
    {
        /// <summary>
        /// Root directory that was used to resolve <see cref="WorkflowFilePath"/> from a name.
        /// Mirrors <see cref="WorkflowExecutionRequest.WorkflowsDirectory"/>.
        /// </summary>
        public string WorkflowsDirectory { get; set; }

        /// <summary>
        /// Full path to the owning workflow's resource XML file on disk.
        /// </summary>
        public string WorkflowFilePath { get; set; }

        /// <summary>
        /// The owning workflow's display name (used in logging and error-identity extraction).
        /// </summary>
        public string WorkflowName { get; set; }

        /// <summary>
        /// The test definition to run, mapped onto the concrete <c>Dev2.Data</c> types the shared
        /// <c>Dev2.Activities</c> assertion engine expects — see
        /// <c>Mcp.Execution.TestDefinitionMapper</c>.
        /// </summary>
        public IServiceTestModelTO ServiceTest { get; set; }

        /// <summary>
        /// The authenticated caller, taken from the auth middleware — never from the request
        /// payload. Mirrors <see cref="WorkflowExecutionRequest.ExecutingPrincipal"/>.
        /// </summary>
        public IPrincipal ExecutingPrincipal { get; set; }

        /// <summary>
        /// Validates that the request has the minimum required information.
        /// </summary>
        public bool IsValid => !string.IsNullOrWhiteSpace(WorkflowFilePath) && ServiceTest is not null;
    }
}
