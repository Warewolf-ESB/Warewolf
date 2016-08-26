using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Infrastructure.Communication;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Studio.Core.Interfaces;
using Warewolf.Resource.Errors;

namespace Warewolf.Studio.ServerProxyLayer
{
    public class QueryManagerProxy : ProxyBase, IQueryManager
    {

        public QueryManagerProxy(ICommunicationControllerFactory communicationControllerFactory, IEnvironmentConnection connection) : base(communicationControllerFactory, connection)
        {

        }

        #region Implementation of IQueryManager

        /// <summary>
        /// Gets the dependencies of a resource. a dependency referes to a nested resource
        /// </summary>
        /// <param name="resourceId">the resource</param>
        /// <returns>a list of tree dependencies</returns>
        public IExecuteMessage FetchDependencies(Guid resourceId)
        {
            return FetchDependantsFromServerService(resourceId, false);
        }

        ExecuteMessage FetchDependantsFromServerService(Guid resourceId, bool getDependsOnMe)
        {
            var comsController = CommunicationControllerFactory.CreateController("FindDependencyService");
            comsController.AddPayloadArgument("ResourceId", resourceId.ToString());
            comsController.AddPayloadArgument("GetDependsOnMe", getDependsOnMe.ToString());

            var workspaceId = Connection.WorkspaceID;
            var payload = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);

            return payload;
        }

        /// <summary>
        /// Get the list of items that use this resource a nested resource
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        public IExecuteMessage FetchDependants(Guid resourceId)
        {
            return FetchDependantsFromServerService(resourceId, true);
        }

        /// <summary>
        /// Fetch a heavy weight reource
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        public StringBuilder FetchResourceXaml(Guid resourceId)
        {
            var comsController = CommunicationControllerFactory.CreateController("FetchResourceDefinitionService");
            comsController.AddPayloadArgument("ResourceID", resourceId.ToString());

            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, Connection.WorkspaceID);
            return result.Message;
        }

        /// <summary>
        /// Loads the Tree.
        /// </summary>
        /// <returns></returns>
        public async Task<IExplorerItem> Load(bool reloadCatalogue=false)
        {
            var comsController = CommunicationControllerFactory.CreateController("FetchExplorerItemsService");

            comsController.AddPayloadArgument("ReloadResourceCatalogue",reloadCatalogue.ToString());
            var result = await comsController.ExecuteCompressedCommandAsync<IExplorerItem>(Connection, GlobalConstants.ServerWorkspaceID);
            return result;
        }        
                
        #endregion

        public IList<IToolDescriptor> FetchTools()
        {
            var comsController = CommunicationControllerFactory.CreateController("FetchToolsService");

            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<IList<IToolDescriptor>>(Connection, workspaceId) ?? new List<IToolDescriptor>();
            return result;
        }

        public IList<string> GetComputerNames()
        {
            var comsController = CommunicationControllerFactory.CreateController("GetComputerNamesService");

            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (result == null || result.HasError)
            {
                if (result != null)
                {
                    throw new WarewolfSupportServiceException(result.Message.ToString(), null);
                }
                throw new WarewolfSupportServiceException(ErrorResource.ServiceDoesNotExist, null);
            }
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.Deserialize<IList<string>>(result.Message.ToString());
        }

        public IList<IDbSource> FetchDbSources()
        {
            var comsController = CommunicationControllerFactory.CreateController("FetchDbSources");

            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (result == null || result.HasError)
            {
                if (result != null)
                {
                    throw new WarewolfSupportServiceException(result.Message.ToString(), null);
                }
                throw new WarewolfSupportServiceException(ErrorResource.ServiceDoesNotExist, null);
            }
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.Deserialize<IList<IDbSource>>(result.Message.ToString());
        }

        public async Task<List<IFileResource>> FetchResourceFileTree()
        {
            var comsController = CommunicationControllerFactory.CreateController("FileResourceBuilder");

            var workspaceId = Connection.WorkspaceID;
            var result = await comsController.ExecuteCommandAsync<ExecuteMessage>(Connection, workspaceId);
            if (result == null || result.HasError)
            {
                if (result != null)
                {
                    throw new WarewolfSupportServiceException(result.Message.ToString(), null);
                }
                throw new WarewolfSupportServiceException(ErrorResource.ServiceDoesNotExist, null);
            }
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.Deserialize<List<IFileResource>>(result.Message.ToString());
        }

        public IList<IExchangeSource> FetchExchangeSources()
        {
            var comsController = CommunicationControllerFactory.CreateController("FetchExchangeSources");

            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (result == null || result.HasError)
            {
                if (result != null)
                {
                    throw new WarewolfSupportServiceException(result.Message.ToString(), null);
                }
                throw new WarewolfSupportServiceException(ErrorResource.ServiceDoesNotExist, null);
            }
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.Deserialize<IList<IExchangeSource>>(result.Message.ToString());
        }

        public IList<IDbAction> FetchDbActions(IDbSource source)
        {
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var comsController = CommunicationControllerFactory.CreateController("FetchDbActions");
            comsController.AddPayloadArgument("source", serializer.SerializeToBuilder(source));
            var workspaceId = Connection.WorkspaceID;
            var payload = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (payload == null || payload.HasError)
            {
                if (payload != null)
                {
                    throw new WarewolfSupportServiceException(payload.Message.ToString(), null);
                }
                throw new WarewolfSupportServiceException(ErrorResource.ServiceDoesNotExist, null);
            }
            return serializer.Deserialize<IList<IDbAction>>(payload.Message);
        }

        public IEnumerable<IWebServiceSource> FetchWebServiceSources()
        {
            var comsController = CommunicationControllerFactory.CreateController("FetchWebServiceSources");

            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (result == null || result.HasError)
            {
                if (result != null)
                {
                    throw new WarewolfSupportServiceException(result.Message.ToString(), null);
                }
                throw new WarewolfSupportServiceException(ErrorResource.ServiceDoesNotExist, null);
            }
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            List<IWebServiceSource> fetchWebServiceSources = serializer.Deserialize<List<IWebServiceSource>>(result.Message.ToString());
            return fetchWebServiceSources;
        }

        //public ObservableCollection<IWebServiceSource> WebSources { get; set; }

        public List<IFileListing> GetDllListings(IFileListing listing)
        {
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var comsController = CommunicationControllerFactory.CreateController("GetDllListingsService");
            comsController.AddPayloadArgument("currentDllListing", serializer.Serialize(listing));
            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (result == null || result.HasError)
            {
                if (result != null)
                {
                    throw new WarewolfSupportServiceException(result.Message.ToString(), null);
                }
                throw new WarewolfSupportServiceException(ErrorResource.ServiceDoesNotExist, null);
            }
            var dllListings = serializer.Deserialize<List<IFileListing>>(result.Message.ToString());
            return dllListings;
        }

        public List<IFileListing> GetComDllListings(IFileListing listing)
        {
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var comsController = CommunicationControllerFactory.CreateController("GetComDllListingsService");
            comsController.AddPayloadArgument("currentDllListing", serializer.Serialize(listing));
            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (result == null || result.HasError)
            {
                if (result != null)
                {
                    throw new WarewolfSupportServiceException(result.Message.ToString(), null);
                }
                throw new WarewolfSupportServiceException(ErrorResource.ServiceDoesNotExist, null);
            }
            var dllListings = serializer.Deserialize<List<IFileListing>>(result.Message.ToString());
            return dllListings;
        }

        public ICollection<INamespaceItem> FetchNamespaces(IPluginSource source)
        {
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var comsController = CommunicationControllerFactory.CreateController("FetchPluginNameSpaces");
            comsController.AddPayloadArgument("source", serializer.SerializeToBuilder(source));
            var workspaceId = Connection.WorkspaceID;
            var payload = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (payload == null || payload.HasError)
            {
                if (payload != null)
                {
                    throw new WarewolfSupportServiceException(payload.Message.ToString(), null);
                }
                throw new WarewolfSupportServiceException(ErrorResource.ServiceDoesNotExist, null);
            }
            return serializer.Deserialize<List<INamespaceItem>>(payload.Message);
        }
        public ICollection<INamespaceItem> FetchNamespaces(IComPluginSource source)
        {
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var comsController = CommunicationControllerFactory.CreateController("FetchComPluginNameSpaces");
            comsController.AddPayloadArgument("source", serializer.SerializeToBuilder(source));
            var workspaceId = Connection.WorkspaceID;
            var payload = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (payload == null || payload.HasError)
            {
                if (payload != null)
                {
                    throw new WarewolfSupportServiceException(payload.Message.ToString(), null);
                }
                throw new WarewolfSupportServiceException(ErrorResource.ServiceDoesNotExist, null);
            }
            return serializer.Deserialize<List<INamespaceItem>>(payload.Message);
        }

        public IList<IFileListing> FetchFiles()
        {
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var comsController = CommunicationControllerFactory.CreateController("GetFiles");

            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (result.HasError)
            {
                throw new WarewolfSupportServiceException(result.Message.ToString(), null);
            }
            var fileListings = serializer.Deserialize<List<IFileListing>>(result.Message.ToString());
            return fileListings;
        }


        public IList<IFileListing> FetchFiles(IFileListing root)
        {
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var comsController = CommunicationControllerFactory.CreateController("GetFiles");
            comsController.AddPayloadArgument("fileListing", serializer.Serialize(root));
            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (result.HasError)
            {
                throw new WarewolfSupportServiceException(result.Message.ToString(), null);
            }
            var fileListings = serializer.Deserialize<List<IFileListing>>(result.Message.ToString());
            return fileListings;
        }

        /// <summary>
        /// Get the list of dependencies for the deploy screen
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public IList<Guid> FetchDependenciesOnList(IEnumerable<Guid> values)
        {
            var enumerable = values as Guid[] ?? values.ToArray();
            if (!enumerable.Any())
            {
                return new List<Guid>();
            }


            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var comsController = CommunicationControllerFactory.CreateController("GetDependanciesOnListService");
            comsController.AddPayloadArgument("ResourceIds", serializer.SerializeToBuilder(enumerable.Select(a => a.ToString()).ToList()));
            comsController.AddPayloadArgument("GetDependsOnMe", "false");
            var res = comsController.ExecuteCommand<List<string>>(Connection, GlobalConstants.ServerWorkspaceID).Where(a =>
            {
                Guid b;
                Guid.TryParse(a, out b);
                return b != Guid.Empty;
            });
            var result = res.Select(Guid.Parse).ToList();

            if (result == null)
            {
                throw new Exception(string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, "GetDependanciesOnListService"));
            }

            return result;
        }

        public List<IWindowsGroupPermission> FetchPermissions()
        {

            var comsController = CommunicationControllerFactory.CreateController("FetchServerPermissions");
            var result = comsController.ExecuteCommand<PermissionsModifiedMemo>(Connection, GlobalConstants.ServerWorkspaceID);
            return result.ModifiedPermissions.Cast<IWindowsGroupPermission>().ToList();
        }

        public IList<IPluginSource> FetchPluginSources()
        {
            var comsController = CommunicationControllerFactory.CreateController("FetchPluginSources");

            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (result == null || result.HasError)
            {
                if (result != null)
                {
                    throw new WarewolfSupportServiceException(result.Message.ToString(), null);
                }
                throw new WarewolfSupportServiceException(ErrorResource.ServiceDoesNotExist, null);
            }
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.Deserialize<List<IPluginSource>>(result.Message.ToString());
        }

        public IList<IComPluginSource> FetchComPluginSources()
        {
            var comsController = CommunicationControllerFactory.CreateController("FetchComPluginSources");

            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (result == null || result.HasError)
            {
                if (result != null)
                {
                    throw new WarewolfSupportServiceException(result.Message.ToString(), null);
                }
                throw new WarewolfSupportServiceException(ErrorResource.ServiceDoesNotExist, null);
            }
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.Deserialize<List<IComPluginSource>>(result.Message.ToString());
        }

        public IList<IPluginAction> PluginActions(IPluginSource source, INamespaceItem ns)
        {
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var comsController = CommunicationControllerFactory.CreateController("FetchPluginActions");

            comsController.AddPayloadArgument("source", serializer.SerializeToBuilder(source));
            comsController.AddPayloadArgument("namespace", serializer.SerializeToBuilder(ns));
            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (result == null || result.HasError)
            {
                if (result != null)
                {
                    throw new WarewolfSupportServiceException(result.Message.ToString(), null);
                }
                throw new WarewolfSupportServiceException(ErrorResource.ServiceDoesNotExist, null);
            }


            return serializer.Deserialize<List<IPluginAction>>(result.Message.ToString());
        }

        public IList<IPluginAction> PluginActions(IComPluginSource source, INamespaceItem ns)
        {
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var comsController = CommunicationControllerFactory.CreateController("FetchComPluginActions");

            comsController.AddPayloadArgument("source", serializer.SerializeToBuilder(source));
            comsController.AddPayloadArgument("namespace", serializer.SerializeToBuilder(ns));
            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (result == null || result.HasError)
            {
                if (result != null)
                {
                    throw new WarewolfSupportServiceException(result.Message.ToString(), null);
                }
                throw new WarewolfSupportServiceException(ErrorResource.ServiceDoesNotExist, null);
            }


            return serializer.Deserialize<List<IPluginAction>>(result.Message.ToString());
        }

        public IEnumerable<IRabbitMQServiceSourceDefinition> FetchRabbitMQServiceSources()
        {
            var comsController = CommunicationControllerFactory.CreateController("FetchRabbitMQServiceSources");

            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (result == null || result.HasError)
            {
                if (result != null)
                {
                    throw new WarewolfSupportServiceException(result.Message.ToString(), null);
                }
                throw new WarewolfSupportServiceException(ErrorResource.ServiceDoesNotExist, null);
            }
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            // ReSharper disable once InconsistentNaming
            List<IRabbitMQServiceSourceDefinition> rabbitMQServiceSources = serializer.Deserialize<List<IRabbitMQServiceSourceDefinition>>(result.Message.ToString());
            return rabbitMQServiceSources;
        }

        public IList<IWcfServerSource> FetchWcfSources()
        {
            var comsController = CommunicationControllerFactory.CreateController("FetchWcfSources");

            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (result == null || result.HasError)
            {
                if (result != null)
                {
                    throw new WarewolfSupportServiceException(result.Message.ToString(), null);
                }
                throw new WarewolfSupportServiceException(ErrorResource.ServiceDoesNotExist, null);
            }
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.Deserialize<List<IWcfServerSource>>(result.Message.ToString());
        }

        public IList<IWcfAction> WcfActions(IWcfServerSource wcfSource)
        {
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var comsController = CommunicationControllerFactory.CreateController("FetchWcfAction");
            comsController.AddPayloadArgument("WcfSource", serializer.SerializeToBuilder(wcfSource));
            var workspaceId = Connection.WorkspaceID;
            var payload = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (payload == null || payload.HasError)
            {
                if (payload != null)
                {
                    throw new WarewolfSupportServiceException(payload.Message.ToString(), null);
                }
                throw new WarewolfSupportServiceException(ErrorResource.ServiceDoesNotExist, null);
            }

            return serializer.Deserialize<IList<IWcfAction>>(payload.Message);
        }

        public Task<List<string>> LoadDuplicates()
        {
            var comsController = CommunicationControllerFactory.CreateController("FetchResourceDuplicates");
            var result = comsController.ExecuteCompressedCommandAsync<List<string>>(Connection, GlobalConstants.ServerWorkspaceID);     
            return result;
        }
    }
}