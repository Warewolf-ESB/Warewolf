using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Patterns;
using Dev2.DynamicServices;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management
{
    /// <summary>
    /// The internal managment interface all Management Methods must implement
    /// </summary>
    public interface IEsbManagementEndpoint : ISpookyLoadable<string>
    {
        /// <summary>
        /// Executes the service
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="theWorkspace">The workspace.</param>
        /// <returns></returns>
        StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace);

        /// <summary>
        /// Creates the service entry.
        /// </summary>
        /// <returns></returns>
        DynamicService CreateServiceEntry();

    }
}
