#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Common.Interfaces.Search;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Data;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Enums;
using Dev2.Utils;
using Warewolf.Auditing;
using Warewolf.Configuration;
using Warewolf.Data;
using Warewolf.Options;
using Warewolf.Resource.Errors;
using Warewolf.Service;
using Warewolf.Triggers;

namespace Dev2.Studio.Core.AppResources.Repositories
{
    public class ResourceRepository : IResourceRepository
    {
        readonly HashSet<Guid> _cachedServices;
        IServer _server;
        readonly List<IResourceModel> _resourceModels;
        bool _isLoaded;
        readonly IDeployService _deployService = new DeployService();
        readonly object _updatingPermissions = new object();

        public bool IsLoaded
        {
            get => _isLoaded;
            set
            {
                if (!value)
                {
                    _cachedServices.Clear();
                }

                _isLoaded = value;
            }
        }

        public void DeployResources(IServer targetEnviroment, IServer sourceEnviroment, IDeployDto dto)
        {
            Dev2Logger.Info($"Deploy Resources. Source:{sourceEnviroment.DisplayName} Destination:{targetEnviroment.Name}", GlobalConstants.WarewolfInfo);
            _deployService.Deploy(dto, targetEnviroment, sourceEnviroment);
        }

        public void Load(bool force)
        {
            if (IsLoaded && !force)
            {
                return;
            }

            IsLoaded = true;
            try
            {
                _resourceModels.Clear();
                LoadResources(force);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error("Resource Load error", ex, GlobalConstants.WarewolfError);
                IsLoaded = false;
            }
        }

        public void UpdateWorkspace()
        {
            Load(true);
        }

        void ShowServerDisconnectedPopup()
        {
            var controller = CustomContainer.Get<IPopupController>();
            controller?.Show(string.Format(ErrorResource.ServerDisconnected, _server.Connection.DisplayName.Replace("(Connected)", "")) + Environment.NewLine +
                             ErrorResource.ServerReconnectForActions, ErrorResource.ServerDisconnectedHeader, MessageBoxButton.OK,
                MessageBoxImage.Error, "", false, true, false, false, false, false);
        }

        public IResourceModel LoadResourceFromWorkspace(Guid resourceId, Guid? workspaceId)
        {
            if (!_server.Connection.IsConnected)
            {
                _server.Connection.Connect(_server.EnvironmentID);
                if (!_server.Connection.IsConnected)
                {
                    ShowServerDisconnectedPopup();
                    return null;
                }
            }

            var con = _server.Connection;
            var comsController = new CommunicationController {ServiceName = "FindResourcesByID"};
            comsController.AddPayloadArgument("GuidCsv", resourceId.ToString());
            comsController.AddPayloadArgument("ResourceType", Enum.GetName(typeof(ResourceType), ResourceType.WorkflowService));
            var workspaceIdToUse = workspaceId ?? con.WorkspaceID;
            var toReloadResources = comsController.ExecuteCompressedCommand<List<SerializableResource>>(con, workspaceIdToUse);
            if (toReloadResources == null && !_server.Connection.IsConnected)
            {
                if (!_server.Connection.IsConnected)
                {
                    _server.Connection.Connect(_server.EnvironmentID);
                    if (!_server.Connection.IsConnected)
                    {
                        ShowServerDisconnectedPopup();
                        return null;
                    }
                }
                else
                {
                    toReloadResources = comsController.ExecuteCompressedCommand<List<SerializableResource>>(con, workspaceIdToUse);
                }
            }

            if (toReloadResources != null)
            {
                foreach (var serializableResource in toReloadResources)
                {
                    var resource = HydrateResourceModel(serializableResource, _server.Connection.ServerID, true);
                    _resourceModels.Add(resource);
                    return resource;
                }
            }

            return null;
        }

        public ICollection<IResourceModel> All() => _resourceModels;

        public ICollection<IResourceModel> Find(Expression<Func<IResourceModel, bool>> expression)
        {
            if (expression == null)
            {
                return null;
            }

            var func = expression.Compile();
            return _resourceModels.FindAll(func.Invoke);
        }

        public void UpdateServer(IServer server)
        {
            _server = server;
        }

        public IContextualResourceModel LoadContextualResourceModel(Guid resourceId)
        {
            if (!_server.Connection.IsConnected)
            {
                _server.Connection.Connect(_server.EnvironmentID);
                if (!_server.Connection.IsConnected)
                {
                    ShowServerDisconnectedPopup();
                    return null;
                }
            }

            var con = _server.Connection;
            var comsController = new CommunicationController {ServiceName = "FindResourcesByID"};
            comsController.AddPayloadArgument("GuidCsv", resourceId.ToString());
            var toReloadResources = comsController.ExecuteCompressedCommand<List<SerializableResource>>(con, GlobalConstants.ServerWorkspaceID);
            return GetContextualResourceModel(resourceId, toReloadResources);
        }

        public async Task<IContextualResourceModel> LoadContextualResourceModelAsync(Guid resourceId)
        {
            var con = _server.Connection;
            var comsController = new CommunicationController {ServiceName = "FindResourcesByID"};
            comsController.AddPayloadArgument("GuidCsv", resourceId.ToString());
            var toReloadResources = await comsController.ExecuteCompressedCommandAsync<List<SerializableResource>>(con, GlobalConstants.ServerWorkspaceID).ConfigureAwait(false);
            return GetContextualResourceModel(resourceId, toReloadResources);
        }

        IContextualResourceModel GetContextualResourceModel(Guid resourceId, List<SerializableResource> toReloadResources)
        {
            if (toReloadResources != null && toReloadResources.Count == 0)
            {
                Dev2Logger.Error(string.Format(ErrorResource.NoResourcesFound, resourceId), "Warewolf Error");
                return null;
            }

            if (toReloadResources != null && toReloadResources.Count == 1)
            {
                var serializableResource = toReloadResources[0];
                var resource = HydrateResourceModel(serializableResource, _server.Connection.ServerID, true, true, true);
                if (resource != null)
                {
                    var contextualResourceModel = new ResourceModel(_server);
                    contextualResourceModel.Update(resource);
                    return contextualResourceModel;
                }
            }

            Dev2Logger.Error(string.Format(ErrorResource.MultipleResourcesFound, resourceId), GlobalConstants.WarewolfError);
            return null;
        }

        public IResourceModel FindSingle(Expression<Func<IResourceModel, bool>> expression) => FindSingle(expression, false, false);

        public IResourceModel FindSingle(Expression<Func<IResourceModel, bool>> expression, bool fetchDefinition) => FindSingle(expression, fetchDefinition, false);

        public IResourceModel FindSingle(Expression<Func<IResourceModel, bool>> expression, bool fetchDefinition, bool prepairForDeployment)
        {
            var func = expression?.Compile();
            if (func?.Method == null)
            {
                return null;
            }

            if (!IsLoaded)
            {
                Load(false);
            }

            var result = _resourceModels.Find(func.Invoke);

            if (result != null && fetchDefinition)
            {
                var contextualResourceModel = result as IContextualResourceModel;
                result.WorkflowXaml = contextualResourceModel.GetWorkflowXaml();
            }

            return result;
        }

        public ExecuteMessage Save(IResourceModel instanceObj)
        {
            AddResourceIfNotExist(instanceObj);
            var executeMessage = SaveResource(_server, instanceObj.ToServiceDefinition(), _server.Connection.WorkspaceID, instanceObj.GetSavePath(), "Save");
            return executeMessage;
        }

        void AddResourceIfNotExist(IResourceModel instanceObj)
        {
            Dev2Logger.Info($"Save Resource: {instanceObj.ResourceName}  Environment:{_server.Name}", GlobalConstants.WarewolfInfo);
            var workflow = FindSingle(c => c.ID == instanceObj.ID);
            if (workflow == null)
            {
                _resourceModels.Add(instanceObj);
            }
        }

        public ExecuteMessage SaveToServer(IResourceModel instanceObj) => SaveToServer(instanceObj, "Save");

        public ExecuteMessage SaveToServer(IResourceModel instanceObj, string reason)
        {
            AddResourceIfNotExist(instanceObj);
            var saveResource = SaveResource(_server, instanceObj.ToServiceDefinition(), GlobalConstants.ServerWorkspaceID, instanceObj.GetSavePath(), reason);
            if (saveResource != null && !saveResource.HasError)
            {
                var connection = _server.Connection;
                var comsController = new CommunicationController();
                comsController.FetchResourceAffectedMessages(connection, instanceObj.ID);
            }

            return saveResource;
        }

        public void DeployResource(IResourceModel resource, string savePath)
        {
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            Dev2Logger.Info($"Deploy Resource. Resource:{resource.DisplayName} Environment:{_server.Name}", GlobalConstants.WarewolfInfo);
            var theResource = FindSingle(c => c.ResourceName.Equals(resource.ResourceName, StringComparison.CurrentCultureIgnoreCase));

            if (theResource != null)
            {
                _resourceModels.Remove(theResource);
            }

            theResource = new ResourceModel(_server);
            theResource.Update(resource);
            _resourceModels.Add(theResource);

            var comsController = new CommunicationController {ServiceName = "DeployResourceService"};
            comsController.AddPayloadArgument("savePath", savePath);
            comsController.AddPayloadArgument("ResourceDefinition", resource.ToServiceDefinition(true));
            comsController.AddPayloadArgument("Roles", "*");

            var con = _server.Connection;
            var executeCommand = comsController.ExecuteCommand<ExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (executeCommand != null && executeCommand.HasError)
            {
                throw new Exception(executeCommand.Message.ToString());
            }
        }

        public ExecuteMessage DeleteResource(IResourceModel resource)
        {
            try
            {
                Dev2Logger.Info($"DeleteResource Resource: {resource.DisplayName}  Environment:{_server.Name}", GlobalConstants.WarewolfInfo);
                var res = _resourceModels.FirstOrDefault(c => c.ID == resource.ID);

                if (res == null)
                {
                    var msg = new ExecuteMessage {HasError = true};
                    msg.SetMessage("Failure");
                    return msg;
                }

                var index = _resourceModels.IndexOf(res);
                if (index != -1)
                {
                    _resourceModels.RemoveAt(index);
                }
                else
                {
                    throw new KeyNotFoundException();
                }

                var comsController = new CommunicationController {ServiceName = "DeleteResourceService"};

                if (resource.ResourceName.Contains("Unsaved"))
                {
                    comsController.AddPayloadArgument("ResourceID", resource.ID.ToString());
                    comsController.AddPayloadArgument("ResourceType", resource.ResourceType.ToString());
                    return comsController.ExecuteCommand<ExecuteMessage>(_server.Connection, _server.Connection.WorkspaceID);
                }

                comsController.AddPayloadArgument("ResourceID", resource.ID.ToString());
                comsController.AddPayloadArgument("ResourceType", resource.ResourceType.ToString());

                var result = comsController.ExecuteCommand<ExecuteMessage>(_server.Connection, GlobalConstants.ServerWorkspaceID);
                if (result.HasError)
                {
                    HandleDeleteResourceError(result, resource);
                    return null;
                }

                return result;
            }
            catch (Exception ex)
            {
                Dev2Logger.Error($"DeleteResource Resource: {resource.DisplayName}  Environment:{_server.Name} " + ex.Message, GlobalConstants.WarewolfError);
                return null;
            }
        }

        public async Task<ExecuteMessage> DeleteResourceFromWorkspaceAsync(IContextualResourceModel resourceModel)
        {
            if (resourceModel == null)
            {
                var msg = new ExecuteMessage {HasError = true};
                msg.SetMessage("Failure");
                return msg;
            }

            var comsController = new CommunicationController {ServiceName = "DeleteResourceService"};
            if (!string.IsNullOrEmpty(resourceModel.ResourceName) && resourceModel.ResourceName.Contains("Unsaved"))
            {
                comsController.AddPayloadArgument("ResourceID", resourceModel.ID.ToString());
                comsController.AddPayloadArgument("ResourceType", resourceModel.ResourceType.ToString());
                var deleteResourceFromWorkspace = await comsController.ExecuteCommandAsync<ExecuteMessage>(_server.Connection, _server.Connection.WorkspaceID).ConfigureAwait(false);
                return deleteResourceFromWorkspace;
            }

            var res = _resourceModels.FirstOrDefault(c => c.ID == resourceModel.ID);
            if (res == null)
            {
                var msg = new ExecuteMessage {HasError = true};
                msg.SetMessage("Failure");
                return msg;
            }

            comsController.AddPayloadArgument("ResourceID", resourceModel.ID.ToString());
            comsController.AddPayloadArgument("ResourceType", resourceModel.ResourceType.ToString());
            return await comsController.ExecuteCommandAsync<ExecuteMessage>(_server.Connection, _server.Connection.WorkspaceID).ConfigureAwait(false);
        }

        public ExecuteMessage DeleteResourceFromWorkspace(IResourceModel resource)
        {
            if (resource == null)
            {
                var msg = new ExecuteMessage {HasError = true};
                msg.SetMessage("Failure");
                return msg;
            }

            var comsController = new CommunicationController {ServiceName = "DeleteResourceService"};
            if (!string.IsNullOrEmpty(resource.ResourceName) && resource.ResourceName.Contains("Unsaved"))
            {
                comsController.AddPayloadArgument("ResourceID", resource.ID.ToString());
                comsController.AddPayloadArgument("ResourceType", resource.ResourceType.ToString());
                var deleteResourceFromWorkspace = comsController.ExecuteCommand<ExecuteMessage>(_server.Connection, _server.Connection.WorkspaceID);
                return deleteResourceFromWorkspace;
            }

            var res = _resourceModels.FirstOrDefault(c => c.ID == resource.ID);
            if (res == null)
            {
                var msg = new ExecuteMessage {HasError = true};
                msg.SetMessage("Failure");
                return msg;
            }

            comsController.AddPayloadArgument("ResourceID", resource.ID.ToString());
            comsController.AddPayloadArgument("ResourceType", resource.ResourceType.ToString());
            return comsController.ExecuteCommand<ExecuteMessage>(_server.Connection, _server.Connection.WorkspaceID);
        }

        public void Add(IResourceModel resource)
        {
            _resourceModels.Insert(_resourceModels.Count, resource);
        }

        static void HandleDeleteResourceError(ExecuteMessage data, IResourceModel model)
        {
            if (data.HasError)
            {
                MessageBox.Show(Application.Current.MainWindow, model.ResourceType.GetDescription() + " \"" + model.ResourceName + "\" could not be deleted, reason: " + data.Message, model.ResourceType.GetDescription() + " Deletion Failed", MessageBoxButton.OK);
            }
        }

        public void ReLoadResources()
        {
            Dev2Logger.Warn("Loading Resources - Start", GlobalConstants.WarewolfWarn);
            var comsController = new CommunicationController {ServiceName = "ReloadResourceService"};
            comsController.AddPayloadArgument("ResourceName", "*");
            comsController.AddPayloadArgument("ResourceID", "*");
            comsController.AddPayloadArgument("ResourceType", string.Empty);
            var con = _server.Connection;
            comsController.ExecuteCommand<List<SerializableResource>>(con, GlobalConstants.ServerWorkspaceID);
            Load(true);
            Dev2Logger.Warn("Loading Resources - End", GlobalConstants.WarewolfWarn);
        }

        protected virtual void LoadResources()
        {
            var comsController = GetCommunicationControllerForLoadResources();
            var con = _server.Connection;
            var resourceList = comsController.ExecuteCommand<List<SerializableResource>>(con, GlobalConstants.ServerWorkspaceID);
            if (resourceList == null)
            {
                throw new Exception(ErrorResource.FailedToFetchResoureListAsJSONModel);
            }

            HydrateResourceModels(resourceList, _server.Connection.ServerID);
            Dev2Logger.Warn("Loading Resources - End", GlobalConstants.WarewolfWarn);
        }

        protected virtual void LoadResources(bool force)
        {
            var comsController = GetCommunicationControllerForLoadResources();
            var con = _server.Connection;
            var resourceList = comsController.ExecuteCommand<List<SerializableResource>>(con, GlobalConstants.ServerWorkspaceID);
            if (resourceList == null)
            {
                throw new Exception(ErrorResource.FailedToFetchResoureListAsJSONModel);
            }

            HydrateResourceModels(resourceList, _server.Connection.ServerID, force);
            Dev2Logger.Warn("Loading Resources - End", GlobalConstants.WarewolfWarn);
        }

        static CommunicationController GetCommunicationControllerForLoadResources()
        {
            Dev2Logger.Warn("Loading Resources - Start", GlobalConstants.WarewolfWarn);
            var comsController = new CommunicationController {ServiceName = "FindResourceService"};
            comsController.AddPayloadArgument("ResourceName", "*");
            comsController.AddPayloadArgument("ResourceId", "*");
            comsController.AddPayloadArgument("ResourceType", string.Empty);
            return comsController;
        }

        public bool IsInCache(Guid id) => _cachedServices.Contains(id);

        void HydrateResourceModels(IEnumerable<SerializableResource> wfServices, Guid serverId, bool force = false)
        {
            if (wfServices == null)
            {
                return;
            }

            foreach (var item in wfServices.Where(p => p.ResourceType != "ReservedService"))
            {
                try
                {
                    var resource = HydrateResourceModel(item, serverId, force);
                    if (resource != null)
                    {
                        _resourceModels.Add(resource);
                    }
                }
                catch (Exception ex)
                {
                    Dev2Logger.Warn($"Resource Not Loaded - {item.ResourceName} - {item.ResourceID}", ex, GlobalConstants.WarewolfWarn);
                }
            }
        }

        public IResourceModel HydrateResourceModel(SerializableResource data, Guid serverId) => HydrateResourceModel(data, serverId, false, false, false);
        public IResourceModel HydrateResourceModel(SerializableResource data, Guid serverId, bool forced) => HydrateResourceModel(data, serverId, forced, false, false);

        public IResourceModel HydrateResourceModel(SerializableResource data, Guid serverId, bool forced, bool fetchXaml, bool prepairForDeployment)
        {
            if (!_server.Connection.IsConnected)
            {
                _server.Connection.Connect(_server.EnvironmentID);
                if (!_server.Connection.IsConnected)
                {
                    ShowServerDisconnectedPopup();
                    return null;
                }
            }

            var id = data.ResourceID;

            if (!IsInCache(id) || forced)
            {
                _cachedServices.Add(id);
                var isNewWorkflow = data.IsNewResource;
                var resource = ResourceModelFactory.CreateResourceModel(_server);

                resource.Inputs = data.Inputs;
                resource.Outputs = data.Outputs;
                SetResourceType(data, resource);
                resource.ID = id;
                resource.ServerID = serverId;
                resource.IsValid = data.IsValid;
                resource.DataList = data.DataList?.Replace(GlobalConstants.SerializableResourceQuote, "\"").Replace(GlobalConstants.SerializableResourceSingleQuote, "'") ?? data.DataList;
                resource.ResourceName = data.ResourceName;
                resource.DisplayName = data.ResourceName;
                resource.VersionInfo = data.VersionInfo;
                resource.Category = data.ResourceCategory;
                resource.UserPermissions = data.Permissions;
                resource.Tags = string.Empty;
                resource.Comment = string.Empty;
                resource.ServerResourceType = data.ResourceType;
                resource.UnitTestTargetWorkflowService = string.Empty;
                resource.HelpLink = string.Empty;
                resource.IsNewWorkflow = isNewWorkflow;

                if (data.Errors != null)
                {
                    foreach (var error in data.Errors)
                    {
                        resource.AddError(error);
                    }
                }

                if (isNewWorkflow)
                {
                    NewWorkflowNames.Instance.Add(resource.DisplayName);
                }

                return resource;
            }

            return null;
        }

        private static void SetResourceType(SerializableResource data, IContextualResourceModel resource)
        {
            if (data.IsSource)
            {
                resource.ResourceType = ResourceType.Source;
            }
            else if (data.IsService)
            {
                resource.ResourceType = ResourceType.WorkflowService;
            }
            else if (data.IsReservedService)
            {
                resource.ResourceType = ResourceType.Service;
            }
            else if (data.IsServer)
            {
                resource.ResourceType = ResourceType.Server;
            }
            else
            {
                resource.ResourceType = ResourceType.Unknown;
            }
        }

        internal Func<string, ICommunicationController> GetCommunicationController = serviveName => new CommunicationController {ServiceName = serviveName};

        ExecuteMessage SaveResource(IServer targetEnvironment, StringBuilder resourceDefinition, Guid workspaceId, string savePath, string reason)
        {
            if (GetCommunicationController != null)
            {
                var comsController = GetCommunicationController.Invoke("SaveResourceService");
                var message = new CompressedExecuteMessage();
                message.SetMessage(resourceDefinition.ToString());
                var ser = new Dev2JsonSerializer();
                comsController.AddPayloadArgument("savePath", savePath);
                comsController.AddPayloadArgument("ResourceXml", ser.SerializeToBuilder(message));
                comsController.AddPayloadArgument("WorkspaceID", workspaceId.ToString());
                comsController.AddPayloadArgument("Reason", reason);
                var con = targetEnvironment.Connection;
                var result = comsController.ExecuteCommand<ExecuteMessage>(con, con.WorkspaceID);
                return result;
            }
            else
            {
                throw new NullReferenceException("Cannot save resource. Cannot get Communication Controller.");
            }
        }

        public List<IExecutionHistory> GetTriggerQueueHistory(Guid resourceId)
        {
            if (GetCommunicationController == null)
            {
                throw new NullReferenceException("Cannot get Queue history. Cannot get Communication Controller.");
            }

            var serializer = new Dev2JsonSerializer();
            var comsController = GetCommunicationController.Invoke("GetExecutionHistoryService");
            comsController.AddPayloadArgument("ResourceId", resourceId.ToString());
            var message = new CompressedExecuteMessage();
            message.SetMessage(serializer.Serialize(resourceId.ToString()));
            var result = comsController.ExecuteCommand<List<IExecutionHistory>>(_server.Connection, GlobalConstants.ServerWorkspaceID);
            return result;
        }

        public List<ITriggerQueue> FetchTriggerQueues()
        {
            var comsController = new CommunicationController {ServiceName = "FetchTriggerQueues"};
            var serializer = new Dev2JsonSerializer();
            var result = comsController.ExecuteCompressedCommand<List<ITriggerQueue>>(_server.Connection, GlobalConstants.ServerWorkspaceID);
            return result;
        }

        public Guid SaveQueue(ITriggerQueue triggerQueue)
        {
            if (GetCommunicationController == null)
            {
                throw new NullReferenceException("Cannot save Queue. Cannot get Communication Controller.");
            }

            var comsController = GetCommunicationController.Invoke("SaveTriggerQueueService");
            var serializer = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("TriggerQueue", serializer.SerializeToBuilder(triggerQueue));
            var result = comsController.ExecuteCommand<ExecuteMessage>(_server.Connection, GlobalConstants.ServerWorkspaceID);
            return Guid.Parse(result.Message.ToString());
        }

        public ExecuteMessage DeleteQueue(ITriggerQueue triggerQueue)
        {
            var comsController = new CommunicationController {ServiceName = "DeleteTriggerQueueService"};
            var serializer = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("TriggerQueue", serializer.SerializeToBuilder(triggerQueue));
            var result = comsController.ExecuteCommand<ExecuteMessage>(_server.Connection, GlobalConstants.ServerWorkspaceID);
            return result;
        }

        public TestSaveResult SaveTests(IResourceModel resourceId, List<IServiceTestModelTO> tests)
        {
            if (GetCommunicationController != null)
            {
                var comsController = GetCommunicationController.Invoke("SaveTests");
                var serializer = new Dev2JsonSerializer();
                var message = new CompressedExecuteMessage();
                message.SetMessage(serializer.Serialize(tests));
                comsController.AddPayloadArgument("resourceID", resourceId.ID.ToString());
                comsController.AddPayloadArgument("resourcePath", string.IsNullOrEmpty(resourceId.Category) ? resourceId.ResourceName : resourceId.Category);
                comsController.AddPayloadArgument("testDefinitions", serializer.SerializeToBuilder(message));
                var result = comsController.ExecuteCommand<ExecuteMessage>(_server.Connection, GlobalConstants.ServerWorkspaceID);
                var res = serializer.Deserialize<TestSaveResult>(result?.Message);
                return res;
            }
            else
            {
                throw new NullReferenceException("Cannot save tests. Cannot get Communication Controller.");
            }
        }

        public IServiceTestModelTO ExecuteTest(IContextualResourceModel resourceModel, string testName)
        {
            if (resourceModel?.Environment == null || !resourceModel.Environment.IsConnected)
            {
                var testRunReuslt = new ServiceTestModelTO {TestFailing = true};
                return testRunReuslt;
            }

            var clientContext = resourceModel.Environment.Connection;
            if (clientContext == null)
            {
                var testRunReuslt = new ServiceTestModelTO {TestFailing = true};
                return testRunReuslt;
            }

            var controller = new CommunicationController
            {
                ServiceName = string.IsNullOrEmpty(resourceModel.Category) ? resourceModel.ResourceName : resourceModel.Category,
                ServicePayload = {ResourceID = resourceModel.ID}
            };
            controller.AddPayloadArgument("ResourceID", resourceModel.ID.ToString());
            controller.AddPayloadArgument("IsDebug", true.ToString());
            controller.ServicePayload.TestName = testName;
            var res = controller.ExecuteCommand<IServiceTestModelTO>(clientContext, GlobalConstants.ServerWorkspaceID);
            return res;
        }

        public void ReloadResourceTests()
        {
            if (GetCommunicationController != null)
            {
                var comsController = GetCommunicationController.Invoke("ReloadTestsService");
                var executeCommand = comsController.ExecuteCommand<CompressedExecuteMessage>(_server.Connection, GlobalConstants.ServerWorkspaceID);
                var serializer = new Dev2JsonSerializer();
                var message = executeCommand.GetDecompressedMessage();
                if (executeCommand.HasError)
                {
                    var msg = serializer.Deserialize<StringBuilder>(message);
                    throw new Exception(msg.ToString());
                }
            }
            else
            {
                throw new NullReferenceException("Cannot reload resource tests. Cannot get Communication Controller.");
            }
        }

        public void DeleteAlltests(List<string> ignoreList)
        {
            if (GetCommunicationController != null)
            {
                var serializer = new Dev2JsonSerializer();
                var serializeToBuilder = serializer.SerializeToBuilder(ignoreList);
                var comsController = GetCommunicationController.Invoke("DeleteAllTestsService");
                comsController.AddPayloadArgument("excludeList", serializeToBuilder);
                var executeCommand = comsController.ExecuteCommand<CompressedExecuteMessage>(_server.Connection, GlobalConstants.ServerWorkspaceID);
                var message = executeCommand.GetDecompressedMessage();
                if (executeCommand.HasError)
                {
                    throw new Exception(message);
                }
            }
            else
            {
                throw new NullReferenceException("Cannot delete all tests. Cannot get Communication Controller.");
            }
        }

        public List<IServiceTestModelTO> LoadResourceTests(Guid resourceId)
        {
            if (GetCommunicationController != null)
            {
                var comsController = GetCommunicationController?.Invoke("FetchTests");
                comsController.AddPayloadArgument("resourceID", resourceId.ToString());
                var executeCommand = comsController.ExecuteCommand<CompressedExecuteMessage>(_server.Connection, GlobalConstants.ServerWorkspaceID);
                var serializer = new Dev2JsonSerializer();
                var message = executeCommand.GetDecompressedMessage();
                if (executeCommand.HasError)
                {
                    var msg = serializer.Deserialize<StringBuilder>(message);
                    throw new Exception(msg.ToString());
                }

                var testsTO = serializer.Deserialize<List<IServiceTestModelTO>>(message);
                if (testsTO != null)
                {
                    return testsTO;
                }

                return new List<IServiceTestModelTO>();
            }
            else
            {
                throw new NullReferenceException("Cannot load resource tests. Cannot get Communication Controller.");
            }
        }

        public List<IServiceTestModelTO> LoadAllTests()
        {
            if (GetCommunicationController != null)
            {
                var comsController = GetCommunicationController?.Invoke("FetchAllTests");
                var executeCommand = comsController.ExecuteCommand<CompressedExecuteMessage>(_server.Connection, GlobalConstants.ServerWorkspaceID);
                var serializer = new Dev2JsonSerializer();
                var message = executeCommand.GetDecompressedMessage();
                if (executeCommand.HasError)
                {
                    var msg = serializer.Deserialize<StringBuilder>(message);
                    throw new Exception(msg.ToString());
                }

                var testsTO = serializer.Deserialize<List<IServiceTestModelTO>>(message);
                if (testsTO != null)
                {
                    return testsTO;
                }

                return new List<IServiceTestModelTO>();
            }

            throw new NullReferenceException("Cannot load resource tests. Cannot get Communication Controller.");
        }

        public List<IServiceTestModelTO> LoadResourceTestsForDeploy(Guid resourceId)
        {
            if (GetCommunicationController != null)
            {
                var comsController = GetCommunicationController?.Invoke("FetchTestsForDeploy");
                comsController.AddPayloadArgument("resourceID", resourceId.ToString());
                var executeCommand = comsController.ExecuteCommand<CompressedExecuteMessage>(_server.Connection, GlobalConstants.ServerWorkspaceID);
                var serializer = new Dev2JsonSerializer();
                var message = executeCommand.GetDecompressedMessage();
                if (executeCommand.HasError)
                {
                    var msg = serializer.Deserialize<StringBuilder>(message);
                    throw new Exception(msg.ToString());
                }

                var testsTO = serializer.Deserialize<List<IServiceTestModelTO>>(message);
                if (testsTO != null)
                {
                    return testsTO;
                }

                return new List<IServiceTestModelTO>();
            }
            else
            {
                throw new NullReferenceException("Cannot load resource tests for deploy. Cannot get Communication Controller.");
            }
        }

        public void DeleteResourceTest(Guid resourceId, string testName)
        {
            if (GetCommunicationController != null)
            {
                var comsController = GetCommunicationController?.Invoke("DeleteTest");
                comsController.AddPayloadArgument("resourceID", resourceId.ToString());
                comsController.AddPayloadArgument("testName", testName);
                if (string.IsNullOrEmpty(testName))
                {
                    throw new ArgumentNullException(nameof(testName));
                }

                if (resourceId == Guid.Empty)
                {
                    throw new ArgumentNullException(nameof(resourceId));
                }

                var executeCommand = comsController.ExecuteCommand<CompressedExecuteMessage>(_server.Connection, GlobalConstants.ServerWorkspaceID);
                var serializer = new Dev2JsonSerializer();
                if (executeCommand.HasError)
                {
                    var message = executeCommand.GetDecompressedMessage();
                    var msg = serializer.Deserialize<StringBuilder>(message);
                    throw new Exception(msg.ToString());
                }
            }
            else
            {
                throw new NullReferenceException("Cannot delete resource test. Cannot get Communication Controller.");
            }
        }

        public ExecuteMessage StopExecution(IContextualResourceModel resourceModel)
        {
            if (resourceModel == null)
            {
                var msg = new ExecuteMessage {HasError = true};
                msg.SetMessage(string.Empty);
                return msg;
            }

            var con = resourceModel.Environment.Connection;
            var workspaceId = con.WorkspaceID;
            var comsController = new CommunicationController {ServiceName = "TerminateExecutionService"};
            comsController.AddPayloadArgument("Roles", "*");
            comsController.AddPayloadArgument("ResourceID", resourceModel.ID.ToString());
            var result = comsController.ExecuteCommand<ExecuteMessage>(con, workspaceId);
            return result;
        }

        public List<IResourceModel> GetUniqueDependencies(IContextualResourceModel resourceModel)
        {
            if (resourceModel?.Environment?.ResourceRepository == null)
            {
                return new List<IResourceModel>();
            }

            var msg = GetDependenciesXml(resourceModel, true);
            var xml = XElement.Parse(msg.Message.ToString());
            var nodes = xml.DescendantsAndSelf("node").Select(node => node.Attribute("id")).Where(idAttr => idAttr != null).Select(idAttr => idAttr.Value);
            var resources = resourceModel.Environment.ResourceRepository.All().Join(nodes, r => r.ID.ToString(), n => n, (r, n) => r);
            var returnList = resources.ToList().Distinct().ToList();
            return returnList;
        }

        public bool HasDependencies(IContextualResourceModel resourceModel)
        {
            var uniqueList = GetUniqueDependencies(resourceModel);
            uniqueList.RemoveAll(res => res.ID == resourceModel.ID);
            return uniqueList.Count > 0;
        }

        public ExecuteMessage GetDependenciesXml(IContextualResourceModel resourceModel, bool getDependsOnMe)
        {
            if (resourceModel == null)
            {
                return new ExecuteMessage {HasError = false};
            }

            var comsController = new CommunicationController {ServiceName = "FindDependencyService"};
            comsController.AddPayloadArgument("ResourceId", resourceModel.ID.ToString());
            comsController.AddPayloadArgument("GetDependsOnMe", getDependsOnMe.ToString());
            var workspaceId = resourceModel.Environment.Connection.WorkspaceID;
            var payload = comsController.ExecuteCommand<ExecuteMessage>(resourceModel.Environment.Connection, workspaceId);
            if (payload == null)
            {
                throw new Exception(string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, "FindDependencyService"));
            }

            return payload;
        }

        public async Task<ExecuteMessage> GetDependenciesXmlAsync(IContextualResourceModel resourceModel, bool getDependsOnMe)
        {
            if (resourceModel == null)
            {
                return new ExecuteMessage {HasError = false};
            }

            var comsController = new CommunicationController {ServiceName = "FindDependencyService"};
            comsController.AddPayloadArgument("ResourceId", resourceModel.ID.ToString());
            comsController.AddPayloadArgument("GetDependsOnMe", getDependsOnMe.ToString());
            var workspaceId = resourceModel.Environment.Connection.WorkspaceID;
            var payload = await comsController.ExecuteCommandAsync<ExecuteMessage>(resourceModel.Environment.Connection, workspaceId).ConfigureAwait(false);
            if (payload == null)
            {
                throw new Exception(string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, "FindDependencyService"));
            }

            return payload;
        }

        public Data.Settings.Settings ReadSettings(IServer currentEnv)
        {
            var comController = new CommunicationController {ServiceName = "SettingsReadService"};
            return comController.ExecuteCommand<Data.Settings.Settings>(currentEnv.Connection, GlobalConstants.ServerWorkspaceID);
        }

        public ExecuteMessage WriteSettings(IServer currentEnv, Data.Settings.Settings settings)
        {
            var comController = new CommunicationController {ServiceName = "SettingsWriteService"};
            comController.AddPayloadArgument("Settings", settings.ToString());
            return comController.ExecuteCommand<ExecuteMessage>(currentEnv.Connection, GlobalConstants.ServerWorkspaceID);
        }

        public ExecuteMessage SaveServerSettings(IServer currentEnv, ServerSettingsData serverSettingsData)
        {
            var comController = new CommunicationController {ServiceName = "SaveServerSettings"};
            comController.AddPayloadArgument("ServerSettings", _serializer.Serialize(serverSettingsData));
            var output = comController.ExecuteCommand<ExecuteMessage>(currentEnv.Connection, GlobalConstants.ServerWorkspaceID);

            if (output == null)
            {
                throw new WarewolfSaveException(ErrorResource.UnableToContactServer, null);
            }

            if (output.HasError)
            {
                throw new WarewolfSaveException(output.Message.ToString(), null);
            }

            return output;
        }

        public ServerSettingsData GetServerSettings(IServer currentEnv)
        {
            var comController = new CommunicationController {ServiceName = "GetServerSettings"};
            var output = comController.ExecuteCommand<ServerSettingsData>(currentEnv.Connection, GlobalConstants.ServerWorkspaceID);

            if (output == null)
            {
                throw new WarewolfSaveException(ErrorResource.UnableToContactServer, null);
            }

            return output;
        }
        
        public ExecuteMessage SaveAuditingSettings(IServer currentEnv, AuditSettingsDataBase auditingSettingsData)
        {
            var comController = new CommunicationController {ServiceName = nameof(Warewolf.Service.SaveAuditingSettings)};
            comController.AddPayloadArgument(Warewolf.Service.SaveAuditingSettings.AuditingSettings, _serializer.Serialize(auditingSettingsData));
            comController.AddPayloadArgument(Warewolf.Service.SaveAuditingSettings.SinkType, _serializer.Serialize(auditingSettingsData.GetType().Name));
            
            var output = comController.ExecuteCommand<ExecuteMessage>(currentEnv.Connection, GlobalConstants.ServerWorkspaceID);
            if (output == null)
            {
                throw new WarewolfSaveException(ErrorResource.UnableToContactServer, null);
            }

            if (output.HasError)
            {
                throw new WarewolfSaveException(output.Message.ToString(), null);
            }

            return output;
        }

        public T GetAuditingSettings<T>(IServer currentEnv) where T : AuditSettingsDataBase, new()
        {
            var comController = new CommunicationController {ServiceName =nameof(Warewolf.Service.GetAuditingSettings)};
            var output = comController.ExecuteCommand<T>(currentEnv.Connection, GlobalConstants.ServerWorkspaceID);

            if (output == null)
            {
                throw new WarewolfSaveException(ErrorResource.UnableToContactServer, null);
            }
            return output;
        }

        readonly Dev2JsonSerializer _serializer = new Dev2JsonSerializer();

        public DbTableList GetDatabaseTables(DbSource dbSource)
        {
            var comController = new CommunicationController {ServiceName = "GetDatabaseTablesService"};
            comController.AddPayloadArgument("Database", _serializer.Serialize(dbSource));
            var tables = comController.ExecuteCommand<DbTableList>(_server.Connection, GlobalConstants.ServerWorkspaceID);
            return tables;
        }

        public DbColumnList GetDatabaseTableColumns(DbSource dbSource, DbTable dbTable)
        {
            var comController = new CommunicationController {ServiceName = "GetDatabaseColumnsForTableService"};
            comController.AddPayloadArgument("Database", _serializer.Serialize(dbSource));
            comController.AddPayloadArgument("TableName", _serializer.Serialize(dbTable.TableName));
            comController.AddPayloadArgument("Schema", _serializer.Serialize(dbTable.Schema));
            var columns = comController.ExecuteCommand<DbColumnList>(_server.Connection, GlobalConstants.ServerWorkspaceID);
            return columns;
        }

        public List<SharepointListTo> GetSharepointLists(SharepointSource source)
        {
            var comController = new CommunicationController {ServiceName = "GetSharepointListService"};
            comController.AddPayloadArgument("SharepointServer", _serializer.Serialize(source));
            var lists = comController.ExecuteCommand<List<SharepointListTo>>(_server.Connection, GlobalConstants.ServerWorkspaceID);
            return lists;
        }

        string CreateServiceName(Type type)
        {
            var serviceName = $"Fetch{type.Name}s";
            return serviceName;
        }

        public IList<T> GetResourceList<T>(IServer targetEnvironment) where T : new()
        {
            var comController = new CommunicationController {ServiceName = CreateServiceName(typeof(T))};
            var sources = comController.ExecuteCommand<List<T>>(targetEnvironment.Connection, GlobalConstants.ServerWorkspaceID);
            return sources;
        }

        public List<ISharepointFieldTo> GetSharepointListFields(ISharepointSource source, SharepointListTo list, bool onlyEditable)
        {
            var comController = new CommunicationController {ServiceName = "GetSharepointListFields"};
            comController.AddPayloadArgument("SharepointServer", _serializer.Serialize(source));
            comController.AddPayloadArgument("ListName", _serializer.Serialize(list.FullName));
            comController.AddPayloadArgument("OnlyEditable", _serializer.Serialize(onlyEditable));
            var fields = comController.ExecuteCommand<List<ISharepointFieldTo>>(_server.Connection, GlobalConstants.ServerWorkspaceID);
            return fields;
        }

        public bool DoesResourceExistInRepo(IResourceModel resource)
        {
            var index = _resourceModels.IndexOf(resource);
            if (index != -1)
            {
                return true;
            }

            return false;
        }

        public List<IResourceModel> FindResourcesByID(IServer targetEnvironment, IEnumerable<string> guids, ResourceType resourceType)
        {
            if (targetEnvironment == null || guids == null)
            {
                return new List<IResourceModel>();
            }

            var comController = new CommunicationController {ServiceName = "FindResourcesByID"};
            comController.AddPayloadArgument("GuidCsv", string.Join(",", guids));
            comController.AddPayloadArgument("Type", Enum.GetName(typeof(ResourceType), resourceType));
            var models = comController.ExecuteCompressedCommand<List<SerializableResource>>(targetEnvironment.Connection, GlobalConstants.ServerWorkspaceID);
            var serverId = targetEnvironment.Connection.ServerID;
            var result = new List<IResourceModel>();
            if (models != null)
            {
                result.AddRange(models.Select(model => HydrateResourceModel(model, serverId)));
            }

            return result;
        }

        public List<T> FindSourcesByType<T>(IServer targetEnvironment, enSourceType sourceType)
        {
            var result = new List<T>();
            if (targetEnvironment == null)
            {
                return result;
            }

            var comsController = new CommunicationController {ServiceName = "FindSourcesByType"};
            comsController.AddPayloadArgument("Type", Enum.GetName(typeof(enSourceType), sourceType));
            result = comsController.ExecuteCommand<List<T>>(targetEnvironment.Connection, GlobalConstants.ServerWorkspaceID);
            return result;
        }

        public List<IResource> FindResourcesByType<T>(IServer targetEnvironment)
        {
            var result = new List<IResource>();
            if (targetEnvironment == null)
            {
                return result;
            }

            var comsController = new CommunicationController {ServiceName = "FindResourcesByType"};
            comsController.AddPayloadArgument("Type", typeof(T).FullName);
            result = comsController.ExecuteCommand<List<IResource>>(targetEnvironment.Connection, GlobalConstants.ServerWorkspaceID);
            return result;
        }

        public Dictionary<string, string[]> FindAutocompleteOptions(IServer targetEnvironment, IResource selectedSource)
        {
            var result = new Dictionary<string, string[]>();
            if (targetEnvironment == null)
            {
                return result;
            }

            var comsController = new CommunicationController {ServiceName = "FindAutocompleteOptions"};
            comsController.AddPayloadArgument("SelectedSource", selectedSource.ToString());
            result = comsController.ExecuteCommand<Dictionary<string, string[]>>(targetEnvironment.Connection, GlobalConstants.ServerWorkspaceID);
            return result;
        }

        public List<IOption> FindOptions(IServer targetEnvironment, IResource selectedSource)
        {
            var result = new List<IOption>();
            if (targetEnvironment == null)
            {
                return result;
            }

            var comsController = new CommunicationController {ServiceName = "FindOptions"};
            comsController.AddPayloadArgument("SelectedSourceId", selectedSource.ResourceID.ToString());
            result = comsController.ExecuteCommand<List<IOption>>(targetEnvironment.Connection, GlobalConstants.ServerWorkspaceID);
            return result;
        }

        public List<IOption> FindOptionsBy(IServer targetEnvironment, string name)
        {
            if (GetCommunicationController != null)
            {
                var result = new List<IOption>();
                if (targetEnvironment == null)
                {
                    return result;
                }

                var comsController = GetCommunicationController.Invoke("FindOptionsBy");
                comsController.AddPayloadArgument(OptionsService.ParameterName, name);
                result = comsController.ExecuteCommand<List<IOption>>(targetEnvironment.Connection, GlobalConstants.ServerWorkspaceID);
                return result;
            }
            else
            {
                throw new NullReferenceException("Cannot Find Options By. Cannot get Communication Controller.");
            }
        }

        public ExecuteMessage FetchResourceDefinition(IServer targetEnv, Guid workspaceId, Guid resourceModelId, bool prepaireForDeployment)
        {
            var comsController = new CommunicationController {ServiceName = "FetchResourceDefinitionService"};
            comsController.AddPayloadArgument("ResourceID", resourceModelId.ToString());
            comsController.AddPayloadArgument("PrepairForDeployment", prepaireForDeployment.ToString());
            var result = comsController.ExecuteCommand<ExecuteMessage>(targetEnv.Connection, workspaceId);
            if (result != null)
            {
                Dev2Logger.Debug($"Fetched Definition For {resourceModelId} From Workspace {workspaceId}", GlobalConstants.WarewolfDebug);
            }

            return result;
        }

        public async Task<ExecuteMessage> FetchResourceDefinitionAsyn(IServer targetEnv, Guid workspaceId, Guid resourceModelId, bool prepaireForDeployment)
        {
            var comsController = new CommunicationController {ServiceName = "FetchResourceDefinitionService"};
            comsController.AddPayloadArgument("ResourceID", resourceModelId.ToString());
            comsController.AddPayloadArgument("PrepairForDeployment", prepaireForDeployment.ToString());
            var result = await comsController.ExecuteCommandAsync<ExecuteMessage>(targetEnv.Connection, workspaceId).ConfigureAwait(false);
            if (result != null)
            {
                Dev2Logger.Debug($"Fetched Definition For {resourceModelId} From Workspace {workspaceId}", GlobalConstants.WarewolfDebug);
            }

            return result;
        }

        public async Task<ExecuteMessage> FetchResourceDefinitionAsync(IServer targetEnv, Guid workspaceId, Guid resourceModelId, bool prepaireForDeployment)
        {
            var comsController = new CommunicationController {ServiceName = "FetchResourceDefinitionService"};
            comsController.AddPayloadArgument("ResourceID", resourceModelId.ToString());
            comsController.AddPayloadArgument("PrepairForDeployment", prepaireForDeployment.ToString());
            var result = await comsController.ExecuteCommandAsync<ExecuteMessage>(targetEnv.Connection, workspaceId).ConfigureAwait(false);
            if (result != null)
            {
                Dev2Logger.Debug($"Fetched Definition For {resourceModelId} From Workspace {workspaceId}", GlobalConstants.WarewolfDebug);
            }

            return result;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        ~ResourceRepository()
        {
            Dispose();
        }

        public ResourceRepository(IServer server)
        {
            VerifyArgument.IsNotNull("environmentModel", server);
            _server = server;
            _server.AuthorizationServiceSet += (sender, args) =>
            {
                if (_server.AuthorizationService != null)
                {
                    _server.Connection.PermissionsModified += AuthorizationServiceOnPermissionsModified;
                }
            };
            _resourceModels = new List<IResourceModel>();
            _cachedServices = new HashSet<Guid>();
        }

        void AuthorizationServiceOnPermissionsModified(object sender, List<WindowsGroupPermission> windowsGroupPermissions)
        {
            lock (_updatingPermissions)
            {
                ReceivePermissionsModified(windowsGroupPermissions);
            }
        }

        void ReceivePermissionsModified(List<WindowsGroupPermission> modifiedPermissions)
        {
            var windowsGroupPermissions = modifiedPermissions as IList<WindowsGroupPermission> ?? modifiedPermissions.ToList();
            UpdateResourcesBasedOnPermissions(windowsGroupPermissions);
        }

        void UpdateResourcesBasedOnPermissions(IList<WindowsGroupPermission> windowsGroupPermissions)
        {
            var serverPermissions = _server.AuthorizationService.GetResourcePermissions(Guid.Empty);
            _resourceModels.ForEach(model => { model.UserPermissions = serverPermissions; });
            foreach (var perm in windowsGroupPermissions.Where(permission => permission.ResourceID != Guid.Empty && !permission.IsServer))
            {
                var permission = perm;
                var resourceModel = FindSingle(model => model.ID == permission.ResourceID);
                if (resourceModel != null)
                {
                    try
                    {
                        var resourceId = resourceModel.ID;
                        var resourcePermissions = _server.AuthorizationService.GetResourcePermissions(resourceId);
                        resourceModel.UserPermissions = resourcePermissions;
                    }
                    catch (SystemException exception)
                    {
                        HelperUtils.ShowTrustRelationshipError(CustomContainer.Get<IPopupController>(), exception);
                    }
                }
            }
        }

        public List<ISearchResult> Filter(ISearch searchValue)
        {
            var comController = new CommunicationController {ServiceName = "GetFilterListService"};
            comController.AddPayloadArgument("Search", _serializer.Serialize(searchValue));
            var lists = comController.ExecuteCommand<List<ISearchResult>>(_server.Connection, GlobalConstants.ServerWorkspaceID);
            return lists;
        }

        public ExecuteMessage ResumeWorkflowExecution(IResourceModel resource, string environment, Guid startActivityId, string versionNumber)
        {
            var comController = new CommunicationController {ServiceName = "WorkflowResume"};
            comController.AddPayloadArgument("resourceID", resource.ID.ToString());
            comController.AddPayloadArgument("environment", environment);
            comController.AddPayloadArgument("startActivityId", startActivityId.ToString());
            comController.AddPayloadArgument("versionNumber", versionNumber);
            return comController.ExecuteCommand<ExecuteMessage>(_server.Connection, GlobalConstants.ServerWorkspaceID);
        }
    }
}