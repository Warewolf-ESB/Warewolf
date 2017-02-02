using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        IExecuteMessage FetchDependencies(Guid resourceId);
        IExecuteMessage FetchDependants(Guid resourceId);
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        StringBuilder FetchResourceXaml(Guid resourceId);
        Task<IExplorerItem> Load(bool reloadCatalogue = false);
        IList<IToolDescriptor> FetchTools();
        IList<IExchangeSource> FetchExchangeSources();
        IList<string> GetComputerNames();
        IList<IDbSource> FetchDbSources();
        IList<IDbAction> FetchDbActions(IDbSource source);
        IEnumerable<IWebServiceSource> FetchWebServiceSources();
        IList<IPluginSource> FetchPluginSources();
        IList<IComPluginSource> FetchComPluginSources();
        IList<IPluginAction> PluginActions(IPluginSource source, INamespaceItem ns);
        IList<IPluginAction> PluginActionsWithReturns(IPluginSource source, INamespaceItem ns);
        IList<IPluginConstructor> PluginConstructors(IPluginSource source, INamespaceItem ns);
        IList<IPluginAction> PluginActions(IComPluginSource source, INamespaceItem ns);
        List<IFileListing> GetDllListings(IFileListing listing);
        List<IFileListing> GetComDllListings(IFileListing listing);
        ICollection<INamespaceItem> FetchNamespaces(IPluginSource source);
        ICollection<INamespaceItem> FetchNamespacesWithJsonRetunrs(IPluginSource source);
        ICollection<INamespaceItem> FetchNamespaces(IComPluginSource source);
        IList<IFileListing> FetchFiles();
        IList<IFileListing> FetchFiles(IFileListing file);
        IList<Guid> FetchDependenciesOnList(IEnumerable<Guid> values);
        List<IWindowsGroupPermission> FetchPermissions();
        // ReSharper disable once InconsistentNaming
        IEnumerable<IRabbitMQServiceSourceDefinition> FetchRabbitMQServiceSources();
        IList<IWcfServerSource> FetchWcfSources();
        IList<IWcfAction> WcfActions(IWcfServerSource source);
        Task<List<IFileResource>> FetchResourceFileTree();

        Task<List<string>> LoadDuplicates();
    }
}