using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Infrastructure.Communication;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;

namespace Dev2.Common.Interfaces
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
        IExecuteMessage FetchDependencies(Guid resourceId);
        /// <summary>
        /// Get the list of items that use this resource a nested resource
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        IExecuteMessage FetchDependants(Guid resourceId);

        /// <summary>
        /// Fetch a heavy weight reource
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        StringBuilder FetchResourceXaml(Guid resourceId);


        /// <summary>
        /// Loads the Tree.
        /// </summary>
        /// <returns></returns>
        Task<IExplorerItem> Load();

        IList<IToolDescriptor> FetchTools();

        IList<IExchangeSource> FetchExchangeSources();
        IList<string> GetComputerNames();

        IList<IDbSource> FetchDbSources();

        IList<IDbAction> FetchDbActions(IDbSource source);

        IEnumerable<IWebServiceSource> FetchWebServiceSources();


        /// <summary>
        /// Get the list of plugin sources
        /// </summary>
        /// <returns></returns>
        IList<IPluginSource> FetchPluginSources();

        /// <summary>
        /// get the available methods of a class
        /// </summary>
        /// <param name="source"></param>
        /// <param name="ns"></param>
        /// <returns></returns>
        IList<IPluginAction> PluginActions(IPluginSource source, INamespaceItem ns);

        /// <summary>
        /// Get the list of dlls for plugin source
        /// </summary>
        /// <param name="listing"></param>
        /// <returns></returns>
        List<IFileListing> GetDllListings(IFileListing listing);
        
        /// <summary>
        /// Get the list of dlls for plugin source
        /// </summary>
        /// <param name="listing"></param>
        /// <returns></returns>
        List<IFileListing> GetComDllListings(IFileListing listing);

        /// <summary>
        /// Fetch namespaces for a plugin source
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        ICollection<INamespaceItem> FetchNamespaces(IPluginSource source);

        /// <summary>
        /// fetch files for plugin source
        /// </summary>
        /// <returns></returns>

        IList<IFileListing> FetchFiles();

        /// <summary>
        /// fetch files for new plugin service
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        IList<IFileListing> FetchFiles(IFileListing file);

        /// <summary>
        /// Get the list of dependencies for the deploy screen
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        IList<Guid> FetchDependenciesOnList(IEnumerable<Guid> values);

        List<IWindowsGroupPermission> FetchPermissions();

        // ReSharper disable once InconsistentNaming
        IEnumerable<IRabbitMQServiceSourceDefinition> FetchRabbitMQServiceSources();

        IList<IWcfServerSource> FetchWcfSources();

        IList<IWcfAction> WcfActions(IWcfServerSource source);
    }
}