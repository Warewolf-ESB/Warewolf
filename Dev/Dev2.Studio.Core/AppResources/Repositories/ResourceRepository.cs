/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Data;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Utils;
using Dev2.Utils;
using Warewolf.Resource.Errors;
// ReSharper disable NonLocalizedString
// ReSharper disable ParameterTypeCanBeEnumerable.Local
// ReSharper disable RedundantNameQualifier
// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Global

namespace Dev2.Studio.Core.AppResources.Repositories
{
    public class ResourceRepository : IResourceRepository
    {
        readonly HashSet<Guid> _cachedServices;
        readonly IEnvironmentModel _environmentModel;
        private readonly List<IResourceModel> _resourceModels;
        bool _isLoaded;
        readonly IDeployService _deployService = new DeployService();
        readonly object _updatingPermissions = new object();

        public bool IsLoaded
        {
            get { return _isLoaded; }
            set
            {
                if (!value)
                {
                    _cachedServices.Clear();
                }

                _isLoaded = value;
            }
        }

        /// <summary>
        /// Deploys the resources.
        /// </summary>
        /// <param name="targetEnviroment">The target enviroment.</param>
        /// <param name="sourceEnviroment">The source enviroment.</param>
        /// <param name="dto">The dto.</param>
        public void DeployResources(IEnvironmentModel sourceEnviroment, IEnvironmentModel targetEnviroment, IDeployDto dto)
        {
            Dev2Logger.Info($"Deploy Resources. Source:{sourceEnviroment.DisplayName} Destination:{targetEnviroment.Name}");
            _deployService.Deploy(dto, sourceEnviroment, targetEnviroment);

        }

        public void Load()
        {
            if (IsLoaded)
            {
                return;
            }

            IsLoaded = true;
            try
            {
                _resourceModels.Clear();
                LoadResources();
            }
            catch
            {
                IsLoaded = false;
            }
        }

        public void UpdateWorkspace()
        {
            IsLoaded = false;
            Load();
        }

        private void ShowServerDisconnectedPopup()
        {
            var controller = CustomContainer.Get<IPopupController>();
            controller?.Show(string.Format(ErrorResource.ServerDisconnected, _environmentModel.Connection.DisplayName.Replace("(Connected)", "")) + Environment.NewLine +
                             ErrorResource.ServerReconnectForActions, ErrorResource.ServerDisconnectedHeader, MessageBoxButton.OK,
                MessageBoxImage.Error, "", false, true, false, false, false, false);
        }

        public IResourceModel LoadResourceFromWorkspace(Guid resourceId, Guid? workspaceId)
        {
            if (!_environmentModel.Connection.IsConnected)
            {
                _environmentModel.Connection.Connect(_environmentModel.ID);
                if (!_environmentModel.Connection.IsConnected)
                {
                    ShowServerDisconnectedPopup();
                    return null;
                }
            }
            var con = _environmentModel.Connection;
            var comsController = new CommunicationController { ServiceName = "FindResourcesByID" };
            comsController.AddPayloadArgument("GuidCsv", resourceId.ToString());
            comsController.AddPayloadArgument("ResourceType", Enum.GetName(typeof(ResourceType), ResourceType.WorkflowService));
            var workspaceIdToUse = workspaceId ?? con.WorkspaceID;
            var toReloadResources = comsController.ExecuteCompressedCommand<List<SerializableResource>>(con, workspaceIdToUse);
            if (toReloadResources == null && !_environmentModel.Connection.IsConnected)
            {
                if (!_environmentModel.Connection.IsConnected)
                {
                    _environmentModel.Connection.Connect(_environmentModel.ID);
                    if (!_environmentModel.Connection.IsConnected)
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
                    var resource = HydrateResourceModel(serializableResource, _environmentModel.Connection.ServerID, true);
                    var resourceToUpdate = _resourceModels.FirstOrDefault(r => ResourceModelEqualityComparer.Current.Equals(r, resource));

                    if (resourceToUpdate != null)
                    {
                        resourceToUpdate.Update(resource);
                    }
                    else
                    {
                        _resourceModels.Add(resource);
                    }
                    return resource;
                }
            }
            return null;
        }

        public ICollection<IResourceModel> All()
        {
            return _resourceModels;
        }

        public ICollection<IResourceModel> Find(Expression<Func<IResourceModel, bool>> expression)
        {
            if (expression == null)
            {
                return null;
            }

            Func<IResourceModel, bool> func = expression.Compile();
            return _resourceModels.FindAll(func.Invoke);
        }

        public IContextualResourceModel LoadContextualResourceModel(Guid resourceId)
        {
            if (!_environmentModel.Connection.IsConnected)
            {
                _environmentModel.Connection.Connect(_environmentModel.ID);
                if (!_environmentModel.Connection.IsConnected)
                {
                    ShowServerDisconnectedPopup();
                    return null;
                }
            }
            var con = _environmentModel.Connection;
            var comsController = new CommunicationController { ServiceName = "FindResourcesByID" };
            comsController.AddPayloadArgument("GuidCsv", resourceId.ToString());
            var toReloadResources = comsController.ExecuteCompressedCommand<List<SerializableResource>>(con, GlobalConstants.ServerWorkspaceID);
            return GetContextualResourceModel(resourceId, toReloadResources);
        }

        public async Task<IContextualResourceModel> LoadContextualResourceModelAsync(Guid resourceId)
        {
            var con = _environmentModel.Connection;
            var comsController = new CommunicationController { ServiceName = "FindResourcesByID" };
            comsController.AddPayloadArgument("GuidCsv", resourceId.ToString());
            var toReloadResources = await comsController.ExecuteCompressedCommandAsync<List<SerializableResource>>(con, GlobalConstants.ServerWorkspaceID);
            return GetContextualResourceModel(resourceId, toReloadResources);
        }

        private IContextualResourceModel GetContextualResourceModel(Guid resourceId, List<SerializableResource> toReloadResources)
        {
            if (toReloadResources != null && toReloadResources.Count == 1)
            {
                var serializableResource = toReloadResources[0];
                var resource = HydrateResourceModel(serializableResource, _environmentModel.Connection.ServerID, true, true, true);
                if (resource != null)
                {
                    var contextualResourceModel = new ResourceModel(_environmentModel);
                    contextualResourceModel.Update(resource);
                    return contextualResourceModel;
                }
            }
            Dev2Logger.Error(string.Format(ErrorResource.MultipleResourcesFound, resourceId));
            return null;
        }

        public IResourceModel FindSingle(Expression<Func<IResourceModel, bool>> expression, bool fetchPayload = false, bool prepairForDeployment = false)
        {
            var func = expression?.Compile();
            if (func?.Method != null)
            {
                var result = _resourceModels.Find(func.Invoke);

                if (result != null && (result.ResourceType == ResourceType.Service && result.WorkflowXaml != null && result.WorkflowXaml.Length > 0 || fetchPayload))
                {
                    var msg = FetchResourceDefinition(_environmentModel, GlobalConstants.ServerWorkspaceID, result.ID, prepairForDeployment);
                    if (msg != null && !msg.HasError)
                    {
                        result.WorkflowXaml = msg.Message;
                    }
                }

                return result;
            }
            return null;
        }

        public ExecuteMessage Save(IResourceModel instanceObj)
        {
            AddResourceIfNotExist(instanceObj);

            var executeMessage = SaveResource(_environmentModel, instanceObj.ToServiceDefinition(), _environmentModel.Connection.WorkspaceID, instanceObj.GetSavePath());

            return executeMessage;
        }

        private void AddResourceIfNotExist(IResourceModel instanceObj)
        {
            Dev2Logger.Info($"Save Resource: {instanceObj.ResourceName}  Environment:{_environmentModel.Name}");
            var workflow = FindSingle(c => c.ID == instanceObj.ID);

            if (workflow == null)
            {
                _resourceModels.Add(instanceObj);
            }
        }

        public ExecuteMessage SaveToServer(IResourceModel instanceObj)
        {
            AddResourceIfNotExist(instanceObj);
            var saveResource = SaveResource(_environmentModel, instanceObj.ToServiceDefinition(), GlobalConstants.ServerWorkspaceID, instanceObj.GetSavePath());
            if (saveResource != null && !saveResource.HasError)
            {
                var connection = _environmentModel.Connection;
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
            Dev2Logger.Info($"Deploy Resource. Resource:{resource.DisplayName} Environment:{_environmentModel.Name}");
            var theResource = FindSingle(c => c.ResourceName.Equals(resource.ResourceName, StringComparison.CurrentCultureIgnoreCase));

            if (theResource != null)
            {
                _resourceModels.Remove(theResource);
            }
            theResource = new ResourceModel(_environmentModel);
            theResource.Update(resource);
            _resourceModels.Add(theResource);

            var comsController = new CommunicationController { ServiceName = "DeployResourceService" };
            comsController.AddPayloadArgument("savePath", savePath);
            comsController.AddPayloadArgument("ResourceDefinition", resource.ToServiceDefinition(true));
            comsController.AddPayloadArgument("Roles", "*");

            var con = _environmentModel.Connection;
            var executeCommand = comsController.ExecuteCommand<ExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (executeCommand != null && executeCommand.HasError)
            {
                throw new Exception(executeCommand.Message.ToString());
            }
        }


        public ExecuteMessage DeleteResource(IResourceModel resource)
        {
            Dev2Logger.Info($"DeleteResource Resource: {resource.DisplayName}  Environment:{_environmentModel.Name}");
            IResourceModel res = _resourceModels.FirstOrDefault(c => c.ID == resource.ID);

            if (res == null)
            {
                var msg = new ExecuteMessage { HasError = true };
                msg.SetMessage("Failure");
                return msg;
            }

            int index = _resourceModels.IndexOf(res);

            if (index != -1)
            {
                _resourceModels.RemoveAt(index);
            }
            else
            {
                throw new KeyNotFoundException();
            }
            var comsController = new CommunicationController { ServiceName = "DeleteResourceService" };

            if (resource.ResourceName.Contains("Unsaved"))
            {
                comsController.AddPayloadArgument("ResourceID", resource.ID.ToString());
                comsController.AddPayloadArgument("ResourceType", resource.ResourceType.ToString());
                return comsController.ExecuteCommand<ExecuteMessage>(_environmentModel.Connection, _environmentModel.Connection.WorkspaceID);
            }

            comsController.AddPayloadArgument("ResourceID", resource.ID.ToString());
            comsController.AddPayloadArgument("ResourceType", resource.ResourceType.ToString());

            var result = comsController.ExecuteCommand<ExecuteMessage>(_environmentModel.Connection, GlobalConstants.ServerWorkspaceID);

            if (result.HasError)
            {
                HandleDeleteResourceError(result, resource);
                return null;
            }
            return result;
        }

        public async Task<ExecuteMessage> DeleteResourceFromWorkspaceAsync(IContextualResourceModel resourceModel)
        {
            if (resourceModel == null)
            {
                var msg = new ExecuteMessage { HasError = true };
                msg.SetMessage("Failure");
                return msg;
            }

            var comsController = new CommunicationController { ServiceName = "DeleteResourceService" };
            if (!string.IsNullOrEmpty(resourceModel.ResourceName) && resourceModel.ResourceName.Contains("Unsaved"))
            {
                comsController.AddPayloadArgument("ResourceID", resourceModel.ID.ToString());
                comsController.AddPayloadArgument("ResourceType", resourceModel.ResourceType.ToString());
                ExecuteMessage deleteResourceFromWorkspace = await comsController.ExecuteCommandAsync<ExecuteMessage>(_environmentModel.Connection, _environmentModel.Connection.WorkspaceID);
                return deleteResourceFromWorkspace;
            }

            var res = _resourceModels.FirstOrDefault(c => c.ID == resourceModel.ID);

            if (res == null)
            {
                var msg = new ExecuteMessage { HasError = true };
                msg.SetMessage("Failure");
                return msg;
            }

            comsController.AddPayloadArgument("ResourceID", resourceModel.ID.ToString());
            comsController.AddPayloadArgument("ResourceType", resourceModel.ResourceType.ToString());
            return await comsController.ExecuteCommandAsync<ExecuteMessage>(_environmentModel.Connection, _environmentModel.Connection.WorkspaceID);
        }

        public ExecuteMessage DeleteResourceFromWorkspace(IResourceModel resource)
        {
            if (resource == null)
            {
                var msg = new ExecuteMessage { HasError = true };
                msg.SetMessage("Failure");
                return msg;
            }

            var comsController = new CommunicationController { ServiceName = "DeleteResourceService" };
            if (!string.IsNullOrEmpty(resource.ResourceName) && resource.ResourceName.Contains("Unsaved"))
            {
                comsController.AddPayloadArgument("ResourceID", resource.ID.ToString());
                comsController.AddPayloadArgument("ResourceType", resource.ResourceType.ToString());
                ExecuteMessage deleteResourceFromWorkspace = comsController.ExecuteCommand<ExecuteMessage>(_environmentModel.Connection, _environmentModel.Connection.WorkspaceID);
                return deleteResourceFromWorkspace;
            }

            var res = _resourceModels.FirstOrDefault(c => c.ID == resource.ID);

            if (res == null)
            {
                var msg = new ExecuteMessage { HasError = true };
                msg.SetMessage("Failure");
                return msg;
            }

            comsController.AddPayloadArgument("ResourceID", resource.ID.ToString());
            comsController.AddPayloadArgument("ResourceType", resource.ResourceType.ToString());
            return comsController.ExecuteCommand<ExecuteMessage>(_environmentModel.Connection, _environmentModel.Connection.WorkspaceID);
        }

        public void Add(IResourceModel instanceObj)
        {
            _resourceModels.Insert(_resourceModels.Count, instanceObj);
        }

        public void ForceLoad()
        {
            IsLoaded = false;
            Load();
        }

        void HandleDeleteResourceError(ExecuteMessage data, IResourceModel model)
        {
            if (data.HasError)
            {
                MessageBox.Show(Application.Current.MainWindow, model.ResourceType.GetDescription() + " \"" + model.ResourceName + "\" could not be deleted, reason: " + data.Message, model.ResourceType.GetDescription() + " Deletion Failed", MessageBoxButton.OK);
            }
        }

        protected virtual void LoadResources()
        {
            var comsController = GetCommunicationControllerForLoadResources();

            var con = _environmentModel.Connection;
            var resourceList = comsController.ExecuteCommand<List<SerializableResource>>(con, GlobalConstants.ServerWorkspaceID);

            if (resourceList == null)
            {
                throw new Exception(ErrorResource.FailedToFetchResoureListAsJSONModel);
            }

            HydrateResourceModels(resourceList, _environmentModel.Connection.ServerID);
            Dev2Logger.Warn("Loading Resources - End");
        }

        private static CommunicationController GetCommunicationControllerForLoadResources()
        {
            Dev2Logger.Warn("Loading Resources - Start");
            var comsController = new CommunicationController { ServiceName = "FindResourceService" };
            comsController.AddPayloadArgument("ResourceName", "*");
            comsController.AddPayloadArgument("ResourceId", "*");
            comsController.AddPayloadArgument("ResourceType", string.Empty);
            return comsController;
        }

        public bool IsInCache(Guid id)
        {
            return _cachedServices.Contains(id);
        }

        void HydrateResourceModels(IEnumerable<SerializableResource> wfServices, Guid serverId)
        {
            if (wfServices == null)
            {
                return;
            }

            foreach (var item in wfServices)
            {
                try
                {
                    var resourceType = item.ResourceType;

                    if (resourceType == "ReservedService")
                    {
                        continue;
                    }

                    IResourceModel resource = HydrateResourceModel(item, serverId);
                    if (resource != null)
                    {
                        _resourceModels.Add(resource);
                    }
                }
                catch
                {
                    Dev2Logger.Warn($"Resource Not Loaded - {item.ResourceName} - {item.ResourceID}");
                }
            }
        }

        public IResourceModel HydrateResourceModel(SerializableResource data, Guid serverId, bool forced = false, bool fetchXaml = false, bool prepairForDeployment = false)
        {
            if (!_environmentModel.Connection.IsConnected)
            {
                _environmentModel.Connection.Connect(_environmentModel.ID);
                if (!_environmentModel.Connection.IsConnected)
                {
                    ShowServerDisconnectedPopup();
                    return null;
                }
            }

            Guid id = data.ResourceID;

            if (!IsInCache(id) || forced)
            {
                _cachedServices.Add(id);

                var isNewWorkflow = data.IsNewResource;

                var resource = ResourceModelFactory.CreateResourceModel(_environmentModel);

                resource.Inputs = data.Inputs;
                resource.Outputs = data.Outputs;
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



        public Func<string, ICommunicationController> GetCommunicationController = serviveName => new CommunicationController { ServiceName = serviveName };

        private ExecuteMessage SaveResource(IEnvironmentModel targetEnvironment, StringBuilder resourceDefinition, Guid workspaceId, string savePath)
        {
            var comsController = GetCommunicationController("SaveResourceService");
            CompressedExecuteMessage message = new CompressedExecuteMessage();
            message.SetMessage(resourceDefinition.ToString());
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("savePath", savePath);
            comsController.AddPayloadArgument("ResourceXml", ser.SerializeToBuilder(message));
            comsController.AddPayloadArgument("WorkspaceID", workspaceId.ToString());

            var con = targetEnvironment.Connection;
            var result = comsController.ExecuteCommand<ExecuteMessage>(con, con.WorkspaceID);

            return result;
        }

        public TestSaveResult SaveTests(IResourceModel resource, List<IServiceTestModelTO> tests)
        {
            var comsController = GetCommunicationController("SaveTests");
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            CompressedExecuteMessage message = new CompressedExecuteMessage();
            message.SetMessage(serializer.Serialize(tests));
            comsController.AddPayloadArgument("resourceID", resource.ID.ToString());
            comsController.AddPayloadArgument("resourcePath", string.IsNullOrEmpty(resource.Category) ? resource.ResourceName : resource.Category);
            comsController.AddPayloadArgument("testDefinitions", serializer.SerializeToBuilder(message));

            var result = comsController.ExecuteCommand<ExecuteMessage>(_environmentModel.Connection, GlobalConstants.ServerWorkspaceID);
            var res = serializer.Deserialize<TestSaveResult>(result?.Message);
            return res;
        }


        public IServiceTestModelTO ExecuteTest(IContextualResourceModel resourceModel, string testName)
        {
            if (resourceModel?.Environment == null || !resourceModel.Environment.IsConnected)
            {
                var testRunReuslt = new ServiceTestModelTO { TestFailing = true };
                return testRunReuslt;
            }

            var clientContext = resourceModel.Environment.Connection;
            if (clientContext == null)
            {
                var testRunReuslt = new ServiceTestModelTO { TestFailing = true };
                return testRunReuslt;
            }
            var controller = new CommunicationController
            {
                ServiceName = string.IsNullOrEmpty(resourceModel.Category) ? resourceModel.ResourceName : resourceModel.Category,
                ServicePayload = { ResourceID = resourceModel.ID }
            };
            controller.AddPayloadArgument("ResourceID", resourceModel.ID.ToString());
            controller.AddPayloadArgument("IsDebug", true.ToString());
            controller.ServicePayload.TestName = testName;
            var res = controller.ExecuteCommand<IServiceTestModelTO>(clientContext, GlobalConstants.ServerWorkspaceID);
            return res;

        }
        public void ReloadResourceTests()
        {
            var comsController = GetCommunicationController("ReloadTestsService");
            var executeCommand = comsController.ExecuteCommand<CompressedExecuteMessage>(_environmentModel.Connection, GlobalConstants.ServerWorkspaceID);
            var serializer = new Dev2JsonSerializer();
            var message = executeCommand.GetDecompressedMessage();
            if (executeCommand.HasError)
            {
                var msg = serializer.Deserialize<StringBuilder>(message);
                throw new Exception(msg.ToString());
            }
        }

        public void DeleteAlltests(List<string> ignoreList)
        {
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var serializeToBuilder = serializer.SerializeToBuilder(ignoreList);
            var comsController = GetCommunicationController("DeleteAllTestsService");
            comsController.AddPayloadArgument("excludeList", serializeToBuilder);
            var executeCommand = comsController.ExecuteCommand<CompressedExecuteMessage>(_environmentModel.Connection, GlobalConstants.ServerWorkspaceID);
            var message = executeCommand.GetDecompressedMessage();
            if (executeCommand.HasError)
            {
                throw new Exception(message);
            }
        }
        public List<IServiceTestModelTO> LoadResourceTests(Guid resourceId)
        {
            var comsController = GetCommunicationController("FetchTests");
            comsController.AddPayloadArgument("resourceID", resourceId.ToString());
            var executeCommand = comsController.ExecuteCommand<CompressedExecuteMessage>(_environmentModel.Connection, GlobalConstants.ServerWorkspaceID);
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
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

        public List<IServiceTestModelTO> LoadResourceTestsForDeploy(Guid resourceId)
        {
            var comsController = GetCommunicationController("FetchTestsForDeploy");
            comsController.AddPayloadArgument("resourceID", resourceId.ToString());
            var executeCommand = comsController.ExecuteCommand<CompressedExecuteMessage>(_environmentModel.Connection, GlobalConstants.ServerWorkspaceID);
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
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
        public void DeleteResourceTest(Guid resourceId, string testName)
        {
            var comsController = GetCommunicationController("DeleteTest");
            comsController.AddPayloadArgument("resourceID", resourceId.ToString());
            comsController.AddPayloadArgument("testName", testName);
            if (string.IsNullOrEmpty(testName))
            {
                throw new ArgumentNullException(nameof(testName));
            }

            if (resourceId == Guid.Empty || resourceId == null)
            {
                throw new ArgumentNullException(nameof(resourceId));
            }
            var executeCommand = comsController.ExecuteCommand<CompressedExecuteMessage>(_environmentModel.Connection, GlobalConstants.ServerWorkspaceID);
            var serializer = new Dev2JsonSerializer();
            if (executeCommand.HasError)
            {
                var message = executeCommand.GetDecompressedMessage();
                var msg = serializer.Deserialize<StringBuilder>(message);
                throw new Exception(msg.ToString());
            }
        }



        /// <summary>
        /// Stops the execution.
        /// </summary>
        /// <param name="resourceModel">The resource model.</param>
        /// <returns></returns>
        public ExecuteMessage StopExecution(IContextualResourceModel resourceModel)
        {
            if (resourceModel == null)
            {
                var msg = new ExecuteMessage { HasError = true };
                msg.SetMessage(string.Empty);
                return msg;
            }

            var con = resourceModel.Environment.Connection;
            Guid workspaceId = con.WorkspaceID;
            var comsController = new CommunicationController { ServiceName = "TerminateExecutionService" };
            comsController.AddPayloadArgument("Roles", "*");
            comsController.AddPayloadArgument("ResourceID", resourceModel.ID.ToString());

            var result = comsController.ExecuteCommand<ExecuteMessage>(con, workspaceId);

            return result;
        }


        //<summary>
        //Gets a list of unique dependencies for the given <see cref="IResourceModel"/>.
        //</summary>
        //<param name="resourceModel">The resource model to be queried.</param>
        //<returns>A list of <see cref="IResourceModel"/>'s.</returns>
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
                return new ExecuteMessage { HasError = false };
            }

            var comsController = new CommunicationController { ServiceName = "FindDependencyService" };
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
                return new ExecuteMessage { HasError = false };
            }

            var comsController = new CommunicationController { ServiceName = "FindDependencyService" };
            comsController.AddPayloadArgument("ResourceId", resourceModel.ID.ToString());
            comsController.AddPayloadArgument("GetDependsOnMe", getDependsOnMe.ToString());

            var workspaceId = resourceModel.Environment.Connection.WorkspaceID;
            var payload = await comsController.ExecuteCommandAsync<ExecuteMessage>(resourceModel.Environment.Connection, workspaceId);

            if (payload == null)
            {
                throw new Exception(string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, "FindDependencyService"));
            }

            return payload;
        }


        public Data.Settings.Settings ReadSettings(IEnvironmentModel currentEnv)
        {
            var comController = new CommunicationController { ServiceName = "SettingsReadService" };

            return comController.ExecuteCommand<Data.Settings.Settings>(currentEnv.Connection, GlobalConstants.ServerWorkspaceID);
        }

        public ExecuteMessage WriteSettings(IEnvironmentModel currentEnv, Data.Settings.Settings settings)
        {
            var comController = new CommunicationController { ServiceName = "SettingsWriteService" };
            comController.AddPayloadArgument("Settings", settings.ToString());

            return comController.ExecuteCommand<ExecuteMessage>(currentEnv.Connection, GlobalConstants.ServerWorkspaceID);
        }

        readonly Dev2JsonSerializer _serializer = new Dev2JsonSerializer();

        public DbTableList GetDatabaseTables(DbSource dbSource)
        {
            var comController = new CommunicationController { ServiceName = "GetDatabaseTablesService" };

            comController.AddPayloadArgument("Database", _serializer.Serialize(dbSource));

            var tables = comController.ExecuteCommand<DbTableList>(_environmentModel.Connection, GlobalConstants.ServerWorkspaceID);

            return tables;
        }

        public DbColumnList GetDatabaseTableColumns(DbSource dbSource, DbTable dbTable)
        {
            var comController = new CommunicationController { ServiceName = "GetDatabaseColumnsForTableService" };
            comController.AddPayloadArgument("Database", _serializer.Serialize(dbSource));
            comController.AddPayloadArgument("TableName", _serializer.Serialize(dbTable.TableName));
            comController.AddPayloadArgument("Schema", _serializer.Serialize(dbTable.Schema));

            var columns = comController.ExecuteCommand<DbColumnList>(_environmentModel.Connection, GlobalConstants.ServerWorkspaceID);

            return columns;
        }

        public List<SharepointListTo> GetSharepointLists(SharepointSource source)
        {
            var comController = new CommunicationController { ServiceName = "GetSharepointListService" };

            comController.AddPayloadArgument("SharepointServer", _serializer.Serialize(source));

            var lists = comController.ExecuteCommand<List<SharepointListTo>>(_environmentModel.Connection, GlobalConstants.ServerWorkspaceID);

            return lists;
        }
        private string CreateServiceName(Type type)
        {
            var serviceName = $"Fetch{type.Name}s";
            return serviceName;
        }
        public IList<T> GetResourceList<T>(IEnvironmentModel targetEnvironment) where T : new()
        {
            var comController = new CommunicationController { ServiceName = CreateServiceName(typeof(T)) };
            var sources = comController.ExecuteCommand<List<T>>(targetEnvironment.Connection, GlobalConstants.ServerWorkspaceID);
            return sources;
        }
        public List<ISharepointFieldTo> GetSharepointListFields(ISharepointSource source, SharepointListTo list, bool onlyEditableFields)
        {
            var comController = new CommunicationController { ServiceName = "GetSharepointListFields" };
            comController.AddPayloadArgument("SharepointServer", _serializer.Serialize(source));
            comController.AddPayloadArgument("ListName", _serializer.Serialize(list.FullName));
            comController.AddPayloadArgument("OnlyEditable", _serializer.Serialize(onlyEditableFields));

            var fields = comController.ExecuteCommand<List<ISharepointFieldTo>>(_environmentModel.Connection, GlobalConstants.ServerWorkspaceID);

            return fields;
        }

        public bool DoesResourceExistInRepo(IResourceModel resource)
        {
            int index = _resourceModels.IndexOf(resource);
            if (index != -1)
            {
                return true;
            }

            return false;
        }

        public List<IResourceModel> FindResourcesByID(IEnvironmentModel targetEnvironment, IEnumerable<string> guids, ResourceType resourceType)
        {
            if (targetEnvironment == null || guids == null)
            {
                return new List<IResourceModel>();
            }

            var comController = new CommunicationController { ServiceName = "FindResourcesByID" };

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

        public List<T> FindSourcesByType<T>(IEnvironmentModel targetEnvironment, enSourceType sourceType)
        {
            var result = new List<T>();

            if (targetEnvironment == null)
            {
                return result;
            }

            var comsController = new CommunicationController { ServiceName = "FindSourcesByType" };

            comsController.AddPayloadArgument("Type", Enum.GetName(typeof(enSourceType), sourceType));

            result = comsController.ExecuteCommand<List<T>>(targetEnvironment.Connection, GlobalConstants.ServerWorkspaceID);

            return result;
        }

        /// <summary>
        /// Fetches the resource definition.
        /// </summary>
        /// <param name="targetEnv">The target env.</param>
        /// <param name="workspaceId">The workspace unique identifier.</param>
        /// <param name="resourceModelId">The resource model unique identifier.</param>
        /// <param name="prepaireForDeployment"></param>
        /// <returns></returns>
        public ExecuteMessage FetchResourceDefinition(IEnvironmentModel targetEnv, Guid workspaceId, Guid resourceModelId, bool prepaireForDeployment)
        {
            var comsController = new CommunicationController { ServiceName = "FetchResourceDefinitionService" };
            comsController.AddPayloadArgument("ResourceID", resourceModelId.ToString());
            comsController.AddPayloadArgument("PrepairForDeployment", prepaireForDeployment.ToString());

            var result = comsController.ExecuteCommand<ExecuteMessage>(targetEnv.Connection, workspaceId);

            // log the trace for fetch ;)
            if (result != null)
                Dev2Logger.Debug($"Fetched Definition For {resourceModelId} From Workspace {workspaceId}");

            return result;
        }

        /// <summary>
        /// Fetches the resource definition.
        /// </summary>
        /// <param name="targetEnv">The target env.</param>
        /// <param name="workspaceId">The workspace unique identifier.</param>
        /// <param name="resourceModelId">The resource model unique identifier.</param>
        /// <param name="prepaireForDeployment"></param>
        /// <returns></returns>
        public async Task<ExecuteMessage> FetchResourceDefinitionAsyn(IEnvironmentModel targetEnv, Guid workspaceId, Guid resourceModelId, bool prepaireForDeployment)
        {
            var comsController = new CommunicationController { ServiceName = "FetchResourceDefinitionService" };
            comsController.AddPayloadArgument("ResourceID", resourceModelId.ToString());
            comsController.AddPayloadArgument("PrepairForDeployment", prepaireForDeployment.ToString());

            var result = await comsController.ExecuteCommandAsync<ExecuteMessage>(targetEnv.Connection, workspaceId);

            // log the trace for fetch ;)
            if (result != null)
                Dev2Logger.Debug($"Fetched Definition For {resourceModelId} From Workspace {workspaceId}");

            return result;
        }

        /// <summary>
        /// Fetches the resource definition Asyncronouly.
        /// </summary>
        /// <param name="targetEnv">The target env.</param>
        /// <param name="workspaceId">The workspace unique identifier.</param>
        /// <param name="resourceModelId">The resource model unique identifier.</param>
        /// <param name="prepaireForDeployment"></param>
        /// <returns></returns>
        public async Task<ExecuteMessage> FetchResourceDefinitionAsync(IEnvironmentModel targetEnv, Guid workspaceId, Guid resourceModelId, bool prepaireForDeployment)
        {
            var comsController = new CommunicationController { ServiceName = "FetchResourceDefinitionService" };
            comsController.AddPayloadArgument("ResourceID", resourceModelId.ToString());
            comsController.AddPayloadArgument("PrepairForDeployment", prepaireForDeployment.ToString());

            var result = await comsController.ExecuteCommandAsync<ExecuteMessage>(targetEnv.Connection, workspaceId);

            // log the trace for fetch ;)
            if (result != null)
                Dev2Logger.Debug($"Fetched Definition For {resourceModelId} From Workspace {workspaceId}");

            return result;
        }

        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        ~ResourceRepository()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose();
        }




        public ResourceRepository(IEnvironmentModel environmentModel)
        {
            VerifyArgument.IsNotNull("environmentModel", environmentModel);

            _environmentModel = environmentModel;
            _environmentModel.AuthorizationServiceSet += (sender, args) =>
            {
                if (_environmentModel.AuthorizationService != null)
                {
                    _environmentModel.Connection.PermissionsModified += AuthorizationServiceOnPermissionsModified;
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
            var serverPermissions = _environmentModel.AuthorizationService.GetResourcePermissions(Guid.Empty);

            _resourceModels.ForEach(model =>
            {
                model.UserPermissions = serverPermissions;
            });

            foreach (var perm in windowsGroupPermissions.Where(permission => permission.ResourceID != Guid.Empty && !permission.IsServer))
            {
                WindowsGroupPermission permission = perm;
                var resourceModel = FindSingle(model => model.ID == permission.ResourceID);
                if (resourceModel != null)
                {
                    try
                    {
                        var resourceId = resourceModel.ID;
                        var resourcePermissions = _environmentModel.AuthorizationService.GetResourcePermissions(resourceId);
                        resourceModel.UserPermissions = resourcePermissions;
                    }
                    catch (SystemException exception)
                    {
                        HelperUtils.ShowTrustRelationshipError(exception);
                    }
                }
            }
        }

    }
}
