using System;

namespace Warewolf.Execution.Lightweight
{
    /// <summary>
    /// Generates an apis.json discovery document listing all available workflows,
    /// mirroring the behavior of <c>GetApisJsonServiceHandler</c> and
    /// <c>ApisJsonBuilder</c> in the full Warewolf server — but without requiring
    /// a <c>ResourceCatalog</c> or <c>IAuthorizationService</c>.
    /// The list is built by scanning the configured <c>WorkflowsDirectory</c>.
    /// </summary>
    public interface IApisJsonGenerator
    {
        /// <summary>
        /// Produces an apis.json JSON string for the given path filter and request URI.
        /// </summary>
        /// <param name="pathFilter">
        /// Optional sub-folder path (relative to WorkflowsDirectory) to scope the listing.
        /// Pass <c>null</c> or empty to return all workflows.
        /// </param>
        /// <param name="requestUri">
        /// The incoming request URI — used to derive the base URL embedded in each entry.
        /// </param>
        /// <param name="isPublic">
        /// When <c>true</c>, workflow entries use the <c>/Public/</c> route prefix.
        /// When <c>false</c>, the <c>/Secure/</c> prefix is used.
        /// </param>
        /// <param name="workflowFilter">
        /// Optional predicate applied to each workflow's bare name before it is included
        /// in the output.  Pass <c>null</c> to include every discovered workflow.
        /// Used to enforce permission-based visibility: public vs. JWT-user access.
        /// </param>
        string Generate(string? pathFilter, Uri requestUri, bool isPublic = false, Func<string, bool>? workflowFilter = null);
    }
}
