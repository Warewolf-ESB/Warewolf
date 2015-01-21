using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;

namespace Dev2.Common.Interfaces.ServerProxyLayer
{
    /// <summary>
    /// Common interface for server queries
    /// </summary>
    public interface IQueryManager
    {
        /// <summary>
        /// Gets the dependencies of a resource. a dependency referes to a nested resource
        /// </summary>
        /// <param name="resourceId">the resource</param>
        /// <returns>a list of tree dependencies</returns>
        IList<IResource> FetchDependencies(Guid resourceId);
        /// <summary>
        /// Get the list of items that use this resource a nested resource
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        IList<IResource> FetchDependants(Guid resourceId);

        /// <summary>
        /// Fetch a heavy weight reource
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        StringBuilder FetchResource(Guid resourceId);
        /// <summary>
        /// Get a list of tables froma db source
        /// </summary>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        IList<IDbTable> FetchTables(Guid sourceId);



        /// <summary>
        /// Loads the Tree.
        /// </summary>
        /// <returns></returns>
        IExplorerItem Load();

    }
}