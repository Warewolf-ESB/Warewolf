#pragma warning disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure.Communication;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Explorer;
using Dev2.Studio.Interfaces;
using Warewolf.Resource.Errors;
using System.Diagnostics.CodeAnalysis;

namespace Dev2.Studio.Core
{
    public class QueryManagerProxy : ProxyBase, IQueryManager
    {
        public QueryManagerProxy(ICommunicationControllerFactory communicationControllerFactory, IEnvironmentConnection connection) : base(communicationControllerFactory, connection)
        {

        }

        #region Implementation of IQueryManager
        
        public IExecuteMessage FetchDependencies(Guid resourceId)
        {
            if (!Connection.IsConnected)
            {
                ShowServerDisconnectedPopup();
                return new CompressedExecuteMessage();
            }

            return FetchDependantsFromServerService(resourceId, false);
        }

        ExecuteMessage FetchDependantsFromServerService(Guid resourceId, bool getDependsOnMe)
        {
            var comsController = CommunicationControllerFactory.CreateController("FindDependencyService");
            comsController.AddPayloadArgument("ResourceId", resourceId.ToString());
            comsController.AddPayloadArgument("GetDependsOnMe", getDependsOnMe.ToString());

            var payload = comsController.ExecuteCommand<ExecuteMessage>(Connection, GlobalConstants.ServerWorkspaceID);

            return payload;
        }
        
        public IExecuteMessage FetchDependants(Guid resourceId)
        {
            if (!Connection.IsConnected)
            {
                ShowServerDisconnectedPopup();
                return new CompressedExecuteMessage();
            }

            return FetchDependantsFromServerService(resourceId, true);
        }
        
        public StringBuilder FetchResourceXaml(Guid resourceId)
        {
            if (!Connection.IsConnected)
            {
                ShowServerDisconnectedPopup();
                return new StringBuilder();
            }

            var comsController = CommunicationControllerFactory.CreateController("FetchResourceDefinitionService");
            comsController.AddPayloadArgument("ResourceID", resourceId.ToString());
            comsController.AddPayloadArgument("PrepairForDeployment",true.ToString());

            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, Connection.WorkspaceID);
            return result.Message;
        }
        
        public async Task<IExplorerItem> Load(bool reloadCatalogue, IPopupController popupController)
        {
            if (!Connection.IsConnected)
            {
                ShowServerDisconnectedPopup();
                return new ServerExplorerItem();
            }

            var comsController = CommunicationControllerFactory.CreateController("FetchExplorerItemsService");

            comsController.AddPayloadArgument("ReloadResourceCatalogue", reloadCatalogue.ToString());

            if (Connection.IsLocalHost)
            {
                var result = await comsController.ExecuteCompressedCommandAsync<IExplorerItem>(Connection, GlobalConstants.ServerWorkspaceID).ConfigureAwait(true);
                return result;
            }
            else
            {

                var fetchExplorerTask = comsController.ExecuteCompressedCommandAsync<IExplorerItem>(Connection, GlobalConstants.ServerWorkspaceID);
                var delayTask = Task.Delay(60000).ContinueWith((t) =>
                {
                    if (fetchExplorerTask.Status != TaskStatus.RanToCompletion)
                    {
                        popupController?.Show(string.Format(ErrorResource.ServerBusyError, Connection.DisplayName), ErrorResource.ServerBusyHeader, MessageBoxButton.OK,
                                              MessageBoxImage.Warning, "", false, false, true, false, false, false);
                    }
                },TaskScheduler.FromCurrentSynchronizationContext());
                var result = await fetchExplorerTask.ConfigureAwait(true);
                return result;
            }                        
        }

        #endregion

        void ShowServerDisconnectedPopup()
        {
            var controller = CustomContainer.Get<IPopupController>();
            controller?.Show(string.Format(ErrorResource.ServerDisconnected, Connection.DisplayName.Replace("(Connected)", "")) + Environment.NewLine +
                             ErrorResource.ServerReconnectForActions, ErrorResource.ServerDisconnectedHeader, MessageBoxButton.OK,
                MessageBoxImage.Error, "", false, true, false, false, false, false);
        }

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
                if (!Connection.IsConnected)
                {
                    var application = Application.Current;
                    application?.Dispatcher?.BeginInvoke(new Action(ShowServerDisconnectedPopup));
                    return new List<string>();
                }
                if (result != null)
                {
                    throw new WarewolfSupportServiceException(result.Message.ToString(), null);
                }
                throw new WarewolfSupportServiceException(ErrorResource.ServiceDoesNotExist, null);
            }
            var serializer = new Dev2JsonSerializer();
            return serializer.Deserialize<IList<string>>(result.Message.ToString());
        }

        public IList<IDbSource> FetchDbSources()
        {
            var comsController = CommunicationControllerFactory.CreateController(nameof(FetchDbSources));

            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (result == null || result.HasError)
            {
                if (!Connection.IsConnected)
                {
                    ShowServerDisconnectedPopup();
                    return new List<IDbSource>();
                }
                if (result != null)
                {
                    throw new WarewolfSupportServiceException(result.Message.ToString(), null);
                }
                throw new WarewolfSupportServiceException(ErrorResource.ServiceDoesNotExist, null);
            }
            var serializer = new Dev2JsonSerializer();
            return serializer.Deserialize<IList<IDbSource>>(result.Message.ToString());
        }

        public IList<IExchangeSource> FetchExchangeSources()
        {
            var comsController = CommunicationControllerFactory.CreateController(nameof(FetchExchangeSources));

            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (result == null || result.HasError)
            {
                if (!Connection.IsConnected)
                {
                    ShowServerDisconnectedPopup();
                    return new List<IExchangeSource>();
                }
                if (result != null)
                {
                    throw new WarewolfSupportServiceException(result.Message.ToString(), null);
                }
                throw new WarewolfSupportServiceException(ErrorResource.ServiceDoesNotExist, null);
            }
            var serializer = new Dev2JsonSerializer();
            return serializer.Deserialize<IList<IExchangeSource>>(result.Message.ToString());
        }

        public IList<IDbAction> FetchDbActions(IDbSource source)
        {
            var serializer = new Dev2JsonSerializer();
            var comsController = CommunicationControllerFactory.CreateController(nameof(FetchDbActions));
            comsController.AddPayloadArgument(nameof(source), serializer.SerializeToBuilder(source));
            var workspaceId = Connection.WorkspaceID;
            var payload = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (payload == null || payload.HasError)
            {
                if (!Connection.IsConnected)
                {
                    ShowServerDisconnectedPopup();
                    return new List<IDbAction>();
                }
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
            var comsController = CommunicationControllerFactory.CreateController(nameof(FetchWebServiceSources));

            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (result == null || result.HasError)
            {
                if (!Connection.IsConnected)
                {
                    ShowServerDisconnectedPopup();
                    return new List<IWebServiceSource>();
                }
                if (result != null)
                {
                    throw new WarewolfSupportServiceException(result.Message.ToString(), null);
                }
                throw new WarewolfSupportServiceException(ErrorResource.ServiceDoesNotExist, null);
            }
            var serializer = new Dev2JsonSerializer();
            var fetchWebServiceSources = serializer.Deserialize<List<IWebServiceSource>>(result.Message.ToString());
            return fetchWebServiceSources;
        }

        public List<IFileListing> GetDllListings(IFileListing listing)
        {
            var serializer = new Dev2JsonSerializer();
            var comsController = CommunicationControllerFactory.CreateController("GetDllListingsService");
            comsController.AddPayloadArgument("currentDllListing", serializer.Serialize(listing));
            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (result == null || result.HasError)
            {
                if (!Connection.IsConnected)
                {
                    ShowServerDisconnectedPopup();
                    return new List<IFileListing>();
                }
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
            var serializer = new Dev2JsonSerializer();
            var comsController = CommunicationControllerFactory.CreateController("GetComDllListingsService");
            comsController.AddPayloadArgument("currentDllListing", serializer.Serialize(listing));
            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (result == null || result.HasError)
            {
                if (!Connection.IsConnected)
                {
                    ShowServerDisconnectedPopup();
                    return new List<IFileListing>();
                }
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
            var serializer = new Dev2JsonSerializer();
            var comsController = CommunicationControllerFactory.CreateController("FetchPluginNameSpaces");
            comsController.AddPayloadArgument(nameof(source), serializer.SerializeToBuilder(source));
            var workspaceId = Connection.WorkspaceID;
            var payload = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (payload == null || payload.HasError)
            {
                if (!Connection.IsConnected)
                {
                    ShowServerDisconnectedPopup();
                    return new List<INamespaceItem>();
                }
                if (payload != null)
                {
                    throw new WarewolfSupportServiceException(payload.Message.ToString(), null);
                }
                throw new WarewolfSupportServiceException(ErrorResource.ServiceDoesNotExist, null);
            }
            return serializer.Deserialize<List<INamespaceItem>>(payload.Message);
        }

        public ICollection<INamespaceItem> FetchNamespacesWithJsonRetunrs(IPluginSource source)
        {
            var serializer = new Dev2JsonSerializer();
            var comsController = CommunicationControllerFactory.CreateController("FetchPluginNameSpaces");
            comsController.AddPayloadArgument(nameof(source), serializer.SerializeToBuilder(source));
            comsController.AddPayloadArgument("fetchJson", new StringBuilder(true.ToString()));
            var workspaceId = Connection.WorkspaceID;
            var payload = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (payload == null || payload.HasError)
            {
                if (!Connection.IsConnected)
                {
                    ShowServerDisconnectedPopup();
                    return new List<INamespaceItem>();
                }
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
            var serializer = new Dev2JsonSerializer();
            var comsController = CommunicationControllerFactory.CreateController("FetchComPluginNameSpaces");
            comsController.AddPayloadArgument(nameof(source), serializer.SerializeToBuilder(source));
            var workspaceId = Connection.WorkspaceID;
            var payload = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (payload == null || payload.HasError)
            {
                if (!Connection.IsConnected)
                {
                    ShowServerDisconnectedPopup();
                    return new List<INamespaceItem>();
                }
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
            var serializer = new Dev2JsonSerializer();
            var comsController = CommunicationControllerFactory.CreateController("GetFiles");

            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (result.HasError)
            {
                if (!Connection.IsConnected)
                {
                    ShowServerDisconnectedPopup();
                    return new List<IFileListing>();
                }
                throw new WarewolfSupportServiceException(result.Message.ToString(), null);
            }
            var fileListings = serializer.Deserialize<List<IFileListing>>(result.Message.ToString());
            return fileListings;
        }

        public IList<IFileListing> FetchFiles(IFileListing file)
        {
            var serializer = new Dev2JsonSerializer();
            var comsController = CommunicationControllerFactory.CreateController("GetFiles");
            comsController.AddPayloadArgument("fileListing", serializer.Serialize(file));
            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (result.HasError)
            {
                if (!Connection.IsConnected)
                {
                    ShowServerDisconnectedPopup();
                    return new List<IFileListing>();
                }
                throw new WarewolfSupportServiceException(result.Message.ToString(), null);
            }
            var fileListings = serializer.Deserialize<List<IFileListing>>(result.Message.ToString());
            return fileListings;
        }
        
        public IList<Guid> FetchDependenciesOnList(IEnumerable<Guid> values)
        {
            if (!Connection.IsConnected)
            {
                ShowServerDisconnectedPopup();
                return new List<Guid>();
            }

            var enumerable = values as Guid[] ?? values.ToArray();
            if (!enumerable.Any())
            {
                return new List<Guid>();
            }
            
            var serializer = new Dev2JsonSerializer();
            var comsController = CommunicationControllerFactory.CreateController("GetDependanciesOnListService");
            comsController.AddPayloadArgument("ResourceIds", serializer.SerializeToBuilder(enumerable.Select(a => a.ToString()).ToList()));
            comsController.AddPayloadArgument("GetDependsOnMe", "false");
            var res = comsController.ExecuteCommand<List<string>>(Connection, GlobalConstants.ServerWorkspaceID).Where(a =>
            {
                Guid.TryParse(a, out Guid b);
                return b != Guid.Empty;
            });
            var result = res.Select(Guid.Parse).ToList();

            if (result == null)
            {
                throw new Exception(string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, "GetDependanciesOnListService"));
            }

            return result;
        }

        public IList<IPluginSource> FetchPluginSources()
        {
            var comsController = CommunicationControllerFactory.CreateController(nameof(FetchPluginSources));

            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (result == null || result.HasError)
            {
                if (!Connection.IsConnected)
                {
                    ShowServerDisconnectedPopup();
                    return new List<IPluginSource>();
                }
                if (result != null)
                {
                    throw new WarewolfSupportServiceException(result.Message.ToString(), null);
                }
                throw new WarewolfSupportServiceException(ErrorResource.ServiceDoesNotExist, null);
            }
            var serializer = new Dev2JsonSerializer();
            return serializer.Deserialize<List<IPluginSource>>(result.Message.ToString());
        }

        public IList<IComPluginSource> FetchComPluginSources()
        {
            var comsController = CommunicationControllerFactory.CreateController(nameof(FetchComPluginSources));

            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (result == null || result.HasError)
            {
                if (!Connection.IsConnected)
                {
                    ShowServerDisconnectedPopup();
                    return new List<IComPluginSource>();
                }
                if (result != null)
                {
                    throw new WarewolfSupportServiceException(result.Message.ToString(), null);
                }
                throw new WarewolfSupportServiceException(ErrorResource.ServiceDoesNotExist, null);
            }
            var serializer = new Dev2JsonSerializer();
            return serializer.Deserialize<List<IComPluginSource>>(result.Message.ToString());
        }

        [ExcludeFromCodeCoverage]
        public IList<IPluginAction> PluginActions(IPluginSource source, INamespaceItem ns)
        {
            var serializer = new Dev2JsonSerializer();
            var comsController = CommunicationControllerFactory.CreateController("FetchPluginActions");

            var pluginActions = GetPluginActions(source, ns, comsController, serializer);
            return pluginActions;
        }

        [ExcludeFromCodeCoverage]
        IList<IPluginAction> GetPluginActions(IPluginSource source, INamespaceItem ns, ICommunicationController comsController, Dev2JsonSerializer serializer)
        {
            comsController.AddPayloadArgument(nameof(source), serializer.SerializeToBuilder(source));
            comsController.AddPayloadArgument("namespace", serializer.SerializeToBuilder(ns));
            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (result == null || result.HasError)
            {
                if (!Connection.IsConnected)
                {
                    ShowServerDisconnectedPopup();
                    return new List<IPluginAction>();
                }
                if (result != null)
                {
                    throw new WarewolfSupportServiceException(result.Message.ToString(), null);
                }
                throw new WarewolfSupportServiceException(ErrorResource.ServiceDoesNotExist, null);
            }

            return serializer.Deserialize<List<IPluginAction>>(result.Message.ToString());
        }

        [ExcludeFromCodeCoverage]
        public IList<IPluginAction> PluginActionsWithReturns(IPluginSource source, INamespaceItem ns)
        {
            var serializer = new Dev2JsonSerializer();
            var comsController = CommunicationControllerFactory.CreateController("FetchPluginActionsWithReturnsTypes");
            var pluginActions = GetPluginActions(source, ns, comsController, serializer);
            return pluginActions;
        }

        [ExcludeFromCodeCoverage]
        public IList<IPluginConstructor> PluginConstructors(IPluginSource source, INamespaceItem ns)
        {
            var serializer = new Dev2JsonSerializer();
            var comsController = CommunicationControllerFactory.CreateController("FetchPluginConstructors");

            comsController.AddPayloadArgument(nameof(source), serializer.SerializeToBuilder(source));
            comsController.AddPayloadArgument("namespace", serializer.SerializeToBuilder(ns));
            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (result == null || result.HasError)
            {
                if (!Connection.IsConnected)
                {
                    ShowServerDisconnectedPopup();
                    return new List<IPluginConstructor>();
                }
                if (result != null)
                {
                    throw new WarewolfSupportServiceException(result.Message.ToString(), null);
                }
                throw new WarewolfSupportServiceException(ErrorResource.ServiceDoesNotExist, null);
            }
            var pluginConstructors = serializer.Deserialize<List<IPluginConstructor>>(result.Message.ToString());

            if (DataListSingleton.ActiveDataList != null && DataListSingleton.ActiveDataList.ComplexObjectCollection != null)
            {
                var objectCollection = DataListSingleton.ActiveDataList.ComplexObjectCollection;
                pluginConstructors.AddRange(objectCollection.Select(objectItemModel => new PluginConstructor
                {
                    ConstructorName = objectItemModel.Name,
                    IsExistingObject = true
                }));
            }


            return pluginConstructors;
        }

        [ExcludeFromCodeCoverage]
        public IList<IPluginAction> PluginActions(IComPluginSource source, INamespaceItem ns)
        {
            var serializer = new Dev2JsonSerializer();
            var comsController = CommunicationControllerFactory.CreateController("FetchComPluginActions");

            comsController.AddPayloadArgument(nameof(source), serializer.SerializeToBuilder(source));
            comsController.AddPayloadArgument("namespace", serializer.SerializeToBuilder(ns));
            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (result == null || result.HasError)
            {
                if (!Connection.IsConnected)
                {
                    ShowServerDisconnectedPopup();
                    return new List<IPluginAction>();
                }
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
            var comsController = CommunicationControllerFactory.CreateController(nameof(FetchRabbitMQServiceSources));

            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (result == null || result.HasError)
            {
                if (!Connection.IsConnected)
                {
                    ShowServerDisconnectedPopup();
                    return new List<IRabbitMQServiceSourceDefinition>();
                }
                if (result != null)
                {
                    throw new WarewolfSupportServiceException(result.Message.ToString(), null);
                }
                throw new WarewolfSupportServiceException(ErrorResource.ServiceDoesNotExist, null);
            }
            var serializer = new Dev2JsonSerializer();
    
            var rabbitMQServiceSources = serializer.Deserialize<List<IRabbitMQServiceSourceDefinition>>(result.Message.ToString());
            return rabbitMQServiceSources;
        }

        public IEnumerable<IElasticsearchSourceDefinition> FetchElasticsearchServiceSources()
        {
            var comsController = CommunicationControllerFactory.CreateController(nameof(FetchElasticsearchServiceSources));

            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (result == null || result.HasError)
            {
                if (!Connection.IsConnected)
                {
                    return new List<IElasticsearchSourceDefinition>();
                }
                if (result != null)
                {
                    throw new WarewolfSupportServiceException(result.Message.ToString(), null);
                }
                throw new WarewolfSupportServiceException(ErrorResource.ServiceDoesNotExist, null);
            }
            var serializer = new Dev2JsonSerializer();
    
            var elasticsearchServiceSources = serializer.Deserialize<List<IElasticsearchSourceDefinition>>(result.Message.ToString());
            return elasticsearchServiceSources;
        }
        public IList<IWcfServerSource> FetchWcfSources()
        {
            var comsController = CommunicationControllerFactory.CreateController(nameof(FetchWcfSources));

            var workspaceId = Connection.WorkspaceID;
            var result = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (result == null || result.HasError)
            {
                if (!Connection.IsConnected)
                {
                    ShowServerDisconnectedPopup();
                    return new List<IWcfServerSource>();
                }
                if (result != null)
                {
                    throw new WarewolfSupportServiceException(result.Message.ToString(), null);
                }
                throw new WarewolfSupportServiceException(ErrorResource.ServiceDoesNotExist, null);
            }
            var serializer = new Dev2JsonSerializer();
            return serializer.Deserialize<List<IWcfServerSource>>(result.Message.ToString());
        }

        public IList<IWcfAction> WcfActions(IWcfServerSource source)
        {
            var serializer = new Dev2JsonSerializer();
            var comsController = CommunicationControllerFactory.CreateController("FetchWcfAction");
            comsController.AddPayloadArgument("WcfSource", serializer.SerializeToBuilder(source));
            var workspaceId = Connection.WorkspaceID;
            var payload = comsController.ExecuteCommand<ExecuteMessage>(Connection, workspaceId);
            if (payload == null || payload.HasError)
            {
                if (!Connection.IsConnected)
                {
                    ShowServerDisconnectedPopup();
                    return new List<IWcfAction>();
                }
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