using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Security;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Data.ServiceModel;
using Dev2.Explorer;
using Dev2.Models;
using Dev2.Providers.Logs;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Utils;
using Dev2.Utils;
using Dev2.Workspaces;
using Newtonsoft.Json;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.AppResources.Repositories
{
    public class ResourceRepository : IResourceRepository
    {
        readonly HashSet<Guid> _cachedServices;
        readonly IEnvironmentModel _environmentModel;
        readonly List<string> _reservedServices;
        protected List<IResourceModel> ResourceModels;
        bool _isLoaded;
        readonly IDeployService _deployService = new DeployService();

        bool _isDisposed;
        public event EventHandler ItemAdded;

        public bool IsLoaded
        {
            get { return _isLoaded; }
            set
            {
                if(_isLoaded == value)
                {
                    return;
                }

                if(!value)
                {
                    _cachedServices.Clear();
                    _reservedServices.Clear();
                }

                _isLoaded = value;
            }
        }

        #region Methods

        /// <summary>
        /// Deploys the resources.
        /// </summary>
        /// <param name="targetEnviroment">The target enviroment.</param>
        /// <param name="sourceEnviroment">The source enviroment.</param>
        /// <param name="dto">The dto.</param>
        /// <param name="eventPublisher">The event publisher.</param>
        public void DeployResources(IEnvironmentModel sourceEnviroment, IEnvironmentModel targetEnviroment, IDeployDto dto, IEventAggregator eventPublisher)
        {
            //Fetch from resource repository to deploy ;)
            var resourceModels = dto.ResourceModels.Where(model =>
            {
                var b = model != null;
                return b;
            });
            var guids = resourceModels.Select(resource => resource.ID);
            IList<IResourceModel> deployableResources = guids.Select(fetchId => sourceEnviroment.ResourceRepository.FindSingle(c => c.ID == fetchId)).ToList();

            // Create the real deployment payload ;)
            IDeployDto trueDto = new DeployDto { ResourceModels = deployableResources };

            // Deploy - Seems a bit silly to go out to another service only to comeback in?
            Dev2Logger.Log.Info(String.Format("Deploy Resources. Source:{0} Destination:{1}", sourceEnviroment.DisplayName, targetEnviroment.Name));
            _deployService.Deploy(trueDto, targetEnviroment);

            var targetResourceRepo = targetEnviroment.ResourceRepository;

            // Inform the repo to reload the deployed resources against the targetEnviroment ;)
            var deployables = deployableResources.GroupBy(a => a.ResourceType);
            foreach(var x in deployables)
            {
                targetResourceRepo.FindAffectedResources(x.Select(a => a.ID).ToList(), x.First().ResourceType, ResourceModelEqualityComparer.Current, true);

            }
        }


        public void Load()
        {
            if(IsLoaded)
            {
                return;
            }

            IsLoaded = true;
            try
            {
                ResourceModels.Clear();
                _reservedServices.Clear();
                LoadResources();
            }
            catch
            {
                IsLoaded = false;
            }
        }

        public void UpdateWorkspace(IList<IWorkspaceItem> workspaceItems)
        {
            IsLoaded = false;
            Load();
        }

        public List<IResourceModel> ReloadResource(Guid resourceId, Enums.ResourceType resourceType, IEqualityComparer<IResourceModel> equalityComparer, bool fetchXaml)
        {
            var comsController = new CommunicationController { ServiceName = "ReloadResourceService" };
            comsController.AddPayloadArgument("ResourceID", resourceId.ToString());
            comsController.AddPayloadArgument("ResourceType", Enum.GetName(typeof(Enums.ResourceType), resourceType));

            var con = _environmentModel.Connection;
            comsController.ExecuteCommand<ExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);

            var effectedResources = FindAffectedResources(new List<Guid> { resourceId }, resourceType, equalityComparer, fetchXaml);

            return effectedResources;
        }

        public List<IResourceModel> FindAffectedResources(IList<Guid> resourceId, Enums.ResourceType resourceType, IEqualityComparer<IResourceModel> equalityComparer, bool fetchXaml)
        {
            CommunicationController comsController = new CommunicationController { ServiceName = "FindResourcesByID" };
            var resourceIds = resourceId.Select(a => a.ToString() + ",").Aggregate((a, b) => a + b);
            resourceIds = resourceIds.EndsWith(",") ? resourceIds.Substring(0, resourceIds.Length - 1) : resourceIds;

            comsController.AddPayloadArgument("GuidCsv", resourceIds);
            comsController.AddPayloadArgument("ResourceType", Enum.GetName(typeof(Enums.ResourceType), resourceType));

            var toReloadResources = comsController.ExecuteCommand<List<SerializableResource>>(_environmentModel.Connection, GlobalConstants.ServerWorkspaceID);
            var effectedResources = new List<IResourceModel>();

            foreach(var serializableResource in toReloadResources)
            {
                IResourceModel resource = HydrateResourceModel(resourceType, serializableResource, _environmentModel.Connection.ServerID, true, fetchXaml);
                var resourceToUpdate = ResourceModels.FirstOrDefault(r => equalityComparer.Equals(r, resource));

                if(resourceToUpdate != null)
                {
                    resourceToUpdate.Update(resource);
                    effectedResources.Add(resourceToUpdate);
                }
                else
                {
                    effectedResources.Add(resource);
                    ResourceModels.Add(resource);
                    AddResourceToStudioResourceRepository(resource, new ExecuteMessage());
                    if(ItemAdded != null)
                    {
                        ItemAdded(resource, null);
                    }
                }
            }
            return effectedResources;
        }

        public void LoadResourceFromWorkspace(Guid resourceId, Guid? workspaceId)
        {
            var con = _environmentModel.Connection;
            var comsController = new CommunicationController { ServiceName = "FindResourcesByID" };
            comsController.AddPayloadArgument("GuidCsv", resourceId.ToString());
            comsController.AddPayloadArgument("ResourceType", Enum.GetName(typeof(Enums.ResourceType), Enums.ResourceType.WorkflowService));
            var workspaceIdToUse = workspaceId.HasValue ? workspaceId.Value : con.WorkspaceID;
            var toReloadResources = comsController.ExecuteCommand<List<SerializableResource>>(con, workspaceIdToUse);
            foreach(var serializableResource in toReloadResources)
            {
                var resource = HydrateResourceModel(Enums.ResourceType.WorkflowService, serializableResource, _environmentModel.Connection.ServerID, true);
                var resourceToUpdate = ResourceModels.FirstOrDefault(r => ResourceModelEqualityComparer.Current.Equals(r, resource));

                if(resourceToUpdate != null)
                {
                    resourceToUpdate.Update(resource);
                }
                else
                {
                    AddResourceToStudioResourceRepository(resource, new ExecuteMessage());
                    ResourceModels.Add(resource);
                    if(ItemAdded != null)
                    {
                        ItemAdded(resource, null);
                    }
                }
            }
        }

        /// <summary>
        ///     Checks if a resources exists and is a workflow.
        /// </summary>
        public bool IsWorkflow(string resourceName)
        {
            IResourceModel match = All().FirstOrDefault(c => c.ResourceName.ToUpper().Equals(resourceName.ToUpper()));
            if(match != null)
            {
                return match.ResourceType == Enums.ResourceType.WorkflowService;
            }

            return false;
        }

        public bool IsReservedService(string resourceName)
        {
            return _reservedServices.Contains(resourceName.ToUpper());
        }

        public ICollection<IResourceModel> All()
        {
            return ResourceModels;
        }

        public ICollection<IResourceModel> Find(Expression<Func<IResourceModel, bool>> expression)
        {
            if(expression == null)
            {
                return null;
            }
            Func<IResourceModel, bool> func = expression.Compile();
            return ResourceModels.FindAll(func.Invoke);
        }

        public IResourceModel FindSingleWithPayLoad(Expression<Func<IResourceModel, bool>> expression)
        {
            return FindSingle(expression, true);
        }

        public IResourceModel FindSingle(Expression<Func<IResourceModel, bool>> expression, bool fetchPayload = false)
        {
            if(expression != null && _reservedServices != null)
            {
                var func = expression.Compile();
                if(func.Method != null)
                {
                    var result = ResourceModels.Find(func.Invoke);

                    // force a payload fetch ;)
                    if(result != null && ((result.ResourceType == Enums.ResourceType.Service && result.WorkflowXaml != null && result.WorkflowXaml.Length > 0) || fetchPayload))
                    {
                        var msg = FetchResourceDefinition(_environmentModel, GlobalConstants.ServerWorkspaceID, result.ID);
                        result.WorkflowXaml = msg.Message;
                    }

                    return result;
                }
            }
            return null;
        }

        public ExecuteMessage Save(IResourceModel instanceObj)
        {
            return Save(instanceObj, true);
        }

        public ExecuteMessage Save(IResourceModel instanceObj, bool addToStudioRespotory)
        {
            Dev2Logger.Log.Info(String.Format("Save Resource: {0}  Environment:{1}", instanceObj.Category, _environmentModel.Name));
            var workflow = FindSingle(c => c.ResourceName.Equals(instanceObj.ResourceName, StringComparison.CurrentCultureIgnoreCase) && c.Category.Equals(instanceObj.Category, StringComparison.CurrentCultureIgnoreCase));

            if(workflow == null)
            {
                ResourceModels.Add(instanceObj);
            }

            var executeMessage = SaveResource(_environmentModel, instanceObj.ToServiceDefinition(), _environmentModel.Connection.WorkspaceID);

            if(addToStudioRespotory)
            {
                AddResourceToStudioResourceRepository(instanceObj, executeMessage);
            }

            return executeMessage;
        }

        public void RenameCategory(string oldCategory, string newCategory, Enums.ResourceType resourceType)
        {

            var comsController = new CommunicationController { ServiceName = "RenameResourceCategoryService" };
            comsController.AddPayloadArgument("OldCategory", oldCategory);
            comsController.AddPayloadArgument("NewCategory", newCategory);
            comsController.AddPayloadArgument("ResourceType", resourceType.ToString());

            var con = _environmentModel.Connection;
            comsController.ExecuteCommand<ExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);

        }

        public ExecuteMessage SaveToServer(IResourceModel instanceObj)
        {
            Dev2Logger.Log.Info(String.Format("Save Resource: {0}  Environment:{1}", instanceObj.Category, _environmentModel.Name));
            var workflow = FindSingle(c => c.ResourceName.Equals(instanceObj.ResourceName, StringComparison.CurrentCultureIgnoreCase));

            if(workflow == null)
            {
                ResourceModels.Add(instanceObj);
            }
            return SaveResource(_environmentModel, instanceObj.ToServiceDefinition(), GlobalConstants.ServerWorkspaceID);
        }

        public void Rename(string resourceId, string newName)
        {
            Guid resId;

            if(Guid.TryParse(resourceId, out resId))
            {

                var comsController = new CommunicationController { ServiceName = "RenameResourceService" };
                comsController.AddPayloadArgument("NewName", newName);
                comsController.AddPayloadArgument("ResourceID", resourceId);

                var con = _environmentModel.Connection;
                var me = comsController.ExecuteCommand<ExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);

                if(me.Message.Contains("Renamed Resource"))
                {
                    var findInLocalRepo = ResourceModels.FirstOrDefault(res => res.ID == Guid.Parse(resourceId));
                    if(findInLocalRepo != null)
                    {
                        findInLocalRepo.ResourceName = newName;
                    }
                }
            }
            else
            {
                throw new ArgumentException(StringResources.Resource_ID_must_be_a_Guid, "resourceId");
            }
        }

        public void DeployResource(IResourceModel resource)
        {

          
            if (resource == null)
            {
                throw new ArgumentNullException("resource");
            }
            Dev2Logger.Log.Info(String.Format("Deploy Resource. Resource:{0} Environment:{1}", resource.DisplayName, _environmentModel.Name));
            var theResource = FindSingle(c => c.ResourceName.Equals(resource.ResourceName, StringComparison.CurrentCultureIgnoreCase), true);

            if(theResource != null)
            {
                ResourceModels.Remove(theResource);
            }
            theResource = new ResourceModel(_environmentModel);
            theResource.Update(resource);
            ResourceModels.Add(theResource);

            var comsController = new CommunicationController { ServiceName = "DeployResourceService" };
            comsController.AddPayloadArgument("ResourceDefinition", resource.ToServiceDefinition());
            comsController.AddPayloadArgument("Roles", "*");

            var con = _environmentModel.Connection;
            comsController.ExecuteCommand<ExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
        }





        public void Remove(IResourceModel instanceObj)
        {
            DeleteResource(instanceObj);
        }

        public void RefreshResource(Guid resourceId)
        {
            _cachedServices.Remove(resourceId);
        }

        public bool ResourceExist(IResourceModel resource)
        {
            int index = ResourceModels.IndexOf(resource);

            if(index != -1)
            {
                return true;
            }

            return false;
        }

        public Func<IStudioResourceRepository> GetStudioResourceRepository = () => Dev2.AppResources.Repositories.StudioResourceRepository.Instance;

        private IStudioResourceRepository StudioResourceRepository
        {
            get
            {
                return GetStudioResourceRepository();
            }
        }

        public ExecuteMessage DeleteResource(IResourceModel resource)
        {
            Dev2Logger.Log.Info(String.Format("DeleteResource Resource: {0}  Environment:{1}", resource.DisplayName, this._environmentModel.Name));
            IResourceModel res = ResourceModels.FirstOrDefault(c => c.ID == resource.ID);

            if(res == null)
            {
                var msg = new ExecuteMessage { HasError = true };
                msg.SetMessage("Failure");
                return msg;
            }

            int index = ResourceModels.IndexOf(res);

            if(index != -1)
            {
                ResourceModels.RemoveAt(index);

            }
            else
            {
                throw new KeyNotFoundException();
            }
            var comsController = new CommunicationController { ServiceName = "DeleteResourceService" };

            if(resource.ResourceName.Contains("Unsaved"))
            {
                comsController.AddPayloadArgument("ResourceID", resource.ID.ToString());
                comsController.AddPayloadArgument("ResourceType", resource.ResourceType.ToString());
                StudioResourceRepository.DeleteItem(_environmentModel.ID, resource.ID);
                return comsController.ExecuteCommand<ExecuteMessage>(_environmentModel.Connection,
                                                              _environmentModel.Connection.WorkspaceID);

            }

            comsController.AddPayloadArgument("ResourceID", resource.ID.ToString());
            comsController.AddPayloadArgument("ResourceType", resource.ResourceType.ToString());

            var result = comsController.ExecuteCommand<ExecuteMessage>(_environmentModel.Connection,
                                                                       GlobalConstants.ServerWorkspaceID);

            if(result.HasError)
            {
                HandleDeleteResourceError(result, resource);
                return null;
            }

            if(!(resource.ResourceName.Contains("Unsaved")))
            {
                StudioResourceRepository.DeleteItem(_environmentModel.ID, resource.ID);
            }
            return result;
        }

        public ExecuteMessage DeleteResourceFromWorkspace(IResourceModel resource)
        {
            if(resource == null)
            {
                var msg = new ExecuteMessage { HasError = true };
                msg.SetMessage("Failure");
                return msg;
            }

            var comsController = new CommunicationController { ServiceName = "DeleteResourceService" };
            if(!String.IsNullOrEmpty(resource.ResourceName) && resource.ResourceName.Contains("Unsaved"))
            {
                comsController.AddPayloadArgument("ResourceID", resource.ID.ToString());
                comsController.AddPayloadArgument("ResourceType", resource.ResourceType.ToString());
                ExecuteMessage deleteResourceFromWorkspace = comsController.ExecuteCommand<ExecuteMessage>(_environmentModel.Connection, _environmentModel.Connection.WorkspaceID);
                StudioResourceRepository.DeleteItem(_environmentModel.ID, resource.ID);
                return deleteResourceFromWorkspace;
            }

            var res = ResourceModels.FirstOrDefault(c => c.ID == resource.ID);

            if(res == null)
            {
                var msg = new ExecuteMessage { HasError = true };
                msg.SetMessage("Failure");
                return msg;
            }

            comsController.AddPayloadArgument("ResourceID", resource.ID.ToString());
            comsController.AddPayloadArgument("ResourceType", resource.ResourceType.ToString());
            return comsController.ExecuteCommand<ExecuteMessage>(_environmentModel.Connection,
                                                              _environmentModel.Connection.WorkspaceID);

        }


        public void Add(IResourceModel instanceObj)
        {
            ResourceModels.Insert(ResourceModels.Count, instanceObj);
        }

        public void ForceLoad()
        {
            IsLoaded = false;
            Load();
        }

        #endregion Methods

        #region Private Methods

        void AddResourceToStudioResourceRepository(IResourceModel instanceObj, ExecuteMessage executeMessage)
        {
            if(executeMessage != null && !executeMessage.HasError)
            {
                if(!String.IsNullOrEmpty(instanceObj.ResourceName) && !instanceObj.ResourceName.Contains("Unsaved"))
                {
                    var resType = ResourceType.WorkflowService;
                    if(instanceObj.ServerResourceType != null)
                    {
                        resType = (ResourceType)Enum.Parse(typeof(ResourceType), instanceObj.ServerResourceType);
                        if(resType == ResourceType.Unknown)
                        {
                            resType = ResourceType.WorkflowService;
                        }
                    }

                    StudioResourceRepository.ItemAddedMessageHandler(new ServerExplorerItem
                    {
                        DisplayName = instanceObj.ResourceName,
                        Permissions = instanceObj.UserPermissions,
                        ResourceId = instanceObj.ID,
                        ResourceType = resType,
                        ResourcePath = instanceObj.Category
                    });
                }
            }
        }

        [ExcludeFromCodeCoverage]
        void HandleDeleteResourceError(ExecuteMessage data, IResourceModel model)
        {
            if(data.HasError)
            {
                MessageBox.Show(Application.Current.MainWindow,
                                model.ResourceType.GetDescription() + " \"" + model.ResourceName +
                                "\" could not be deleted, reason: " + data.Message,
                                model.ResourceType.GetDescription() + " Deletion Failed", MessageBoxButton.OK);
            }
        }

        string GetIconPath(ResourceType type)
        {
            var iconPath = string.Empty;

            switch(type)
            {
                case ResourceType.DbService:
                case ResourceType.DbSource:
                    iconPath = StringResources.Pack_Uri_DatabaseService_Image;
                    break;
                case ResourceType.EmailSource:
                    iconPath = StringResources.Pack_Uri_EmailSource_Image;
                    break;
                case ResourceType.PluginService:
                case ResourceType.PluginSource:
                    iconPath = StringResources.Pack_Uri_PluginService_Image;
                    break;
                case ResourceType.WebService:
                case ResourceType.WebSource:
                    iconPath = StringResources.Pack_Uri_WebService_Image;
                    break;
                case ResourceType.WorkflowService:
                    iconPath = StringResources.Pack_Uri_WorkflowService_Image;
                    break;
                case ResourceType.Server:
                    iconPath = StringResources.Pack_Uri_Server_Image;
                    break;
            }


            return iconPath;
        }

        protected virtual void LoadResources()
        {
            Dev2Logger.Log.Warn("Loading Resources - Start");
            var comsController = new CommunicationController { ServiceName = "FindResourceService" };
            comsController.AddPayloadArgument("ResourceName", "*");
            comsController.AddPayloadArgument("ResourceType", string.Empty);

            var con = _environmentModel.Connection;
            var resourceList = comsController.ExecuteCommand<List<SerializableResource>>(con, GlobalConstants.ServerWorkspaceID);

            if(resourceList == null)
            {
                throw new Exception("Failed to fetch resoure list as JSON model");
            }

            HydrateResourceModels(resourceList, _environmentModel.Connection.ServerID);
            Dev2Logger.Log.Warn("Loading Resources - End");
        }

        public void RemoveFromCache(Guid id)
        {
            if(_cachedServices.Contains(id))
            {
                _cachedServices.Remove(id);
            }
        }

        public bool IsInCache(Guid id)
        {
            return _cachedServices.Contains(id);
        }


        void HydrateResourceModels(IEnumerable<SerializableResource> wfServices, Guid serverId)
        {
            if(wfServices == null)
            {
                return;
            }

            foreach(var item in wfServices)
            {
                try
                {
                    var resourceType = item.ResourceType;

                    if(resourceType == ResourceType.ReservedService)
                    {
                        _reservedServices.Add(item.ResourceName.ToUpper());
                        continue;
                    }

                    var enumsResourceTypeString = ResourceTypeConverter.ToTypeString(resourceType);
                    var enumsResourceType = enumsResourceTypeString == ResourceTypeConverter.TypeWildcard
                                                ? Enums.ResourceType.Unknown
                                                : (Enums.ResourceType)
                                                  Enum.Parse(typeof(Enums.ResourceType), enumsResourceTypeString);

                    IResourceModel resource = HydrateResourceModel(enumsResourceType, item, serverId);
                    if(resource != null)
                    {
                        ResourceModels.Add(resource);
                        if(ItemAdded != null)
                        {
                            ItemAdded(resource, null);
                        }
                    }
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch
                // ReSharper restore EmptyGeneralCatchClause
                {
                    Dev2Logger.Log.Warn(string.Format("Resource Not Loaded - {0} - {1}", item.ResourceName, item.ResourceID));
                    // Ignore malformed resource
                }
            }
        }

        // Make public for testing, should be extracted to a util class for testing....
        public IResourceModel HydrateResourceModel(Enums.ResourceType resourceType, SerializableResource data, Guid serverId, bool forced = false, bool fetchXaml = false)
        {

            Guid id = data.ResourceID;

            if(!IsInCache(id) || forced)
            {
                // add to cache of services fetched ;)
                _cachedServices.Add(id);

                var isNewWorkflow = data.IsNewResource;

                var resource = ResourceModelFactory.CreateResourceModel(_environmentModel);

                resource.Inputs = data.Inputs;
                resource.Outputs = data.Outputs;
                resource.ResourceType = resourceType;
                resource.ID = id;
                resource.ServerID = serverId;
                resource.IsValid = data.IsValid;
                if(data.DataList != null)
                {
                    resource.DataList =
                        data.DataList.Replace(GlobalConstants.SerializableResourceQuote, "\"")
                            .Replace(GlobalConstants.SerializableResourceSingleQuote, "'");
                }
                else
                {
                    resource.DataList = data.DataList;
                }
                resource.ResourceName = data.ResourceName;
                resource.DisplayName = data.ResourceName;
                resource.VersionInfo = data.VersionInfo;
                resource.IconPath = GetIconPath(data.ResourceType);
                resource.Category = data.ResourceCategory;
                resource.Tags = string.Empty;
                resource.Comment = string.Empty;
                resource.ServerResourceType = data.ResourceType.ToString();
                resource.UnitTestTargetWorkflowService = string.Empty;
                resource.HelpLink = string.Empty;
                resource.IsNewWorkflow = isNewWorkflow;

                if(data.Errors != null && data.Errors.Count > 0)
                {
                    // set the errors ;)
                    foreach(var error in data.Errors)
                    {
                        resource.AddError(error);
                    }
                }

                if(fetchXaml)
                {
                    var msg = FetchResourceDefinition(_environmentModel, GlobalConstants.ServerWorkspaceID, id);
                    resource.WorkflowXaml = msg.Message;
                }

                if(isNewWorkflow)
                {
                    NewWorkflowNames.Instance.Add(resource.DisplayName);
                }

                return resource;
            }
            return null;
        }


        #endregion Private Methods

        #region Add/RemoveEnvironment

        public Func<string, ICommunicationController> GetCommunicationController = serviveName => new CommunicationController { ServiceName = serviveName };

        public ExecuteMessage SaveResource(IEnvironmentModel targetEnvironment, StringBuilder resourceDefinition, Guid workspaceId)
        {
            var comsController = GetCommunicationController("SaveResourceService");
            comsController.AddPayloadArgument("ResourceXml", resourceDefinition);
            comsController.AddPayloadArgument("WorkspaceID", workspaceId.ToString());

            var con = targetEnvironment.Connection;
            var result = comsController.ExecuteCommand<ExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);

            return result;
        }

        public void RemoveEnvironment(IEnvironmentModel targetEnvironment, IEnvironmentModel environment)
        {
            if(targetEnvironment == null)
            {
                throw new ArgumentNullException("targetEnvironment");
            }
            if(environment == null)
            {
                throw new ArgumentNullException("environment");
            }


            var comsController = new CommunicationController { ServiceName = "DeleteResourceService" };
            comsController.AddPayloadArgument("ResourceName", environment.Name);
            comsController.AddPayloadArgument("ResourceType", Enums.ResourceType.Source.ToString());
            comsController.AddPayloadArgument("Roles", "*");

            var con = targetEnvironment.Connection;
            comsController.ExecuteCommand<ExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
        }

        #endregion

        #region Stop Execution

        /// <summary>
        /// Stops the execution.
        /// </summary>
        /// <param name="resourceModel">The resource model.</param>
        /// <returns></returns>
        public ExecuteMessage StopExecution(IContextualResourceModel resourceModel)
        {
            if(resourceModel == null)
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

        #endregion

        #region Resource Dependency

        #region GetUniqueDependencies

        //<summary>
        //Gets a list of unique dependencies for the given <see cref="IResourceModel"/>.
        //</summary>
        //<param name="resourceModel">The resource model to be queried.</param>
        //<returns>A list of <see cref="IResourceModel"/>'s.</returns>
        public List<IResourceModel> GetUniqueDependencies(IContextualResourceModel resourceModel)
        {
            if(resourceModel == null || resourceModel.Environment == null || resourceModel.Environment.ResourceRepository == null)
            {
                return new List<IResourceModel>();
            }

            var msg = GetDependenciesXml(resourceModel, true);
            var xml = XElement.Parse(msg.Message.ToString());
            var nodes = from node in xml.DescendantsAndSelf("node") // this is case-sensitive!
                        select node.Attribute("id")
                            into idAttr
                            where idAttr != null
                            select idAttr.Value;

            var resources = from r in resourceModel.Environment.ResourceRepository.All()
                            join n in nodes on r.ResourceName equals n
                            select r;

            var returnList = resources.ToList().Distinct().ToList();
            return returnList;
        }

        public bool HasDependencies(IContextualResourceModel resourceModel)
        {
            var uniqueList = GetUniqueDependencies(resourceModel);
            uniqueList.RemoveAll(res => res.ID == resourceModel.ID);
            return uniqueList.Count > 0;
        }

        #endregion

        public List<string> GetDependanciesOnList(List<IContextualResourceModel> resourceModels, IEnvironmentModel environmentModel, bool getDependsOnMe = false)
        {
            if(!resourceModels.Any() || environmentModel == null)
            {
                return new List<string>();
            }

            List<string> resourceNames = resourceModels.Select(contextualResourceModel => contextualResourceModel.Category).ToList();

            var comsController = new CommunicationController { ServiceName = "GetDependanciesOnListService" };
            comsController.AddPayloadArgument("ResourceNames", JsonConvert.SerializeObject(resourceNames));
            comsController.AddPayloadArgument("GetDependsOnMe", getDependsOnMe.ToString());

            var result = comsController.ExecuteCommand<List<string>>(environmentModel.Connection, GlobalConstants.ServerWorkspaceID);

            if(result == null)
            {
                throw new Exception(string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, "GetDependanciesOnListService"));
            }

            return result;
        }


        public ExecuteMessage GetDependenciesXml(IContextualResourceModel resourceModel, bool getDependsOnMe)
        {
            if(resourceModel == null)
            {
                return new ExecuteMessage { HasError = false };
            }

            var comsController = new CommunicationController { ServiceName = "FindDependencyService" };
            comsController.AddPayloadArgument("ResourceName", resourceModel.Category);
            comsController.AddPayloadArgument("GetDependsOnMe", getDependsOnMe.ToString());

            var workspaceId = (resourceModel.Environment.Connection).WorkspaceID;
            var payload = comsController.ExecuteCommand<ExecuteMessage>(resourceModel.Environment.Connection, workspaceId);

            if(payload == null)
            {
                throw new Exception(string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, "FindDependencyService"));
            }

            return payload;
        }

        #endregion

        #region Read and Write Settings

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

        #endregion

        #region Get Server Tmp Log File

        public string GetServerLogTempPath(IEnvironmentModel environmentModel)
        {
            // PBI 9598 - 2013.06.10 - TWR : environmentModel may be null for disconnected scenario's
            if(environmentModel == null)
            {
                return string.Empty;
            }

            try
            {
                var comController = new CommunicationController { ServiceName = "FetchCurrentServerLogService" };
                ExecuteMessage serverLogData = comController.ExecuteCommand<ExecuteMessage>(environmentModel.Connection, GlobalConstants.ServerWorkspaceID);

                if(serverLogData != null && serverLogData.Message.Length > 0)
                {
                    string uniqueOutputPath = FileHelper.GetUniqueOutputPath(".txt");
                    return FileHelper.CreateATemporaryFile(serverLogData.Message, uniqueOutputPath);
                }
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
                // Server unavailable!
            }

            return null;
        }

        #endregion

        #region DB Resources

        public DbTableList GetDatabaseTables(DbSource dbSource)
        {
            var comController = new CommunicationController { ServiceName = "GetDatabaseTablesService" };

            comController.AddPayloadArgument("Database", JsonConvert.SerializeObject(dbSource));

            var tables = comController.ExecuteCommand<DbTableList>(_environmentModel.Connection, GlobalConstants.ServerWorkspaceID);

            return tables;
        }

        public DbColumnList GetDatabaseTableColumns(DbSource dbSource, DbTable dbTable)
        {
            var comController = new CommunicationController { ServiceName = "GetDatabaseColumnsForTableService" };

            comController.AddPayloadArgument("Database", JsonConvert.SerializeObject(dbSource));
            comController.AddPayloadArgument("TableName", JsonConvert.SerializeObject(dbTable.TableName));
            comController.AddPayloadArgument("Schema", JsonConvert.SerializeObject(dbTable.Schema));

            var columns = comController.ExecuteCommand<DbColumnList>(_environmentModel.Connection, GlobalConstants.ServerWorkspaceID);

            return columns;
        }

        #endregion

        #region FindResourcesByID

        public bool DoesResourceExistInRepo(IResourceModel resource)
        {
            int index = ResourceModels.IndexOf(resource);
            if(index != -1)
            {
                return true;
            }

            return false;
        }

        public List<IResourceModel> FindResourcesByID(IEnvironmentModel targetEnvironment, IEnumerable<string> guids, Enums.ResourceType resourceType)
        {
            if(targetEnvironment == null || guids == null)
            {
                return new List<IResourceModel>();
            }

            var comController = new CommunicationController { ServiceName = "FindResourcesByID" };

            comController.AddPayloadArgument("GuidCsv", string.Join(",", guids));
            comController.AddPayloadArgument("Type", Enum.GetName(typeof(Enums.ResourceType), resourceType));

            var models = comController.ExecuteCommand<List<SerializableResource>>(targetEnvironment.Connection, GlobalConstants.ServerWorkspaceID);
            var serverId = targetEnvironment.Connection.ServerID;

            var result = new List<IResourceModel>();

            if(models != null)
            {
                result.AddRange(models.Select(model => HydrateResourceModel(resourceType, model, serverId)));
            }

            return result;
        }

        #endregion

        #region FindSourcesByType

        public List<T> FindSourcesByType<T>(IEnvironmentModel targetEnvironment, enSourceType sourceType)
        {
            var result = new List<T>();

            if(targetEnvironment == null)
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
        /// <returns></returns>
        public ExecuteMessage FetchResourceDefinition(IEnvironmentModel targetEnv, Guid workspaceId, Guid resourceModelId)
        {
            var comsController = new CommunicationController { ServiceName = "FetchResourceDefinitionService" };
            comsController.AddPayloadArgument("ResourceID", resourceModelId.ToString());

            var result = comsController.ExecuteCommand<ExecuteMessage>(targetEnv.Connection, workspaceId);

            // log the trace for fetch ;)
            Dev2Logger.Log.Debug(string.Format("Fetched Definition For {0} From Workspace {1}", resourceModelId, workspaceId));

            return result;
        }

        #endregion

        #region Implementation of IDisposable

        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);

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
            Dispose(false);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if(!_isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if(disposing)
                {
                }
                // Call the appropriate methods to clean up
                // unmanaged resources here.
                _isDisposed = true;
            }
        }

        #endregion

        #region Constructor

        public ResourceRepository(IEnvironmentModel environmentModel)
        {
            VerifyArgument.IsNotNull("environmentModel", environmentModel);


            _environmentModel = environmentModel;
            _environmentModel.AuthorizationServiceSet += (sender, args) =>
            {
                if(_environmentModel.AuthorizationService != null)
                {
                    _environmentModel.AuthorizationService.PermissionsModified -= AuthorizationServiceOnPermissionsModified;
                    _environmentModel.AuthorizationService.PermissionsModified += AuthorizationServiceOnPermissionsModified;
                }
            };

            _reservedServices = new List<string>();
            ResourceModels = new List<IResourceModel>();
            _cachedServices = new HashSet<Guid>();
        }

        void AuthorizationServiceOnPermissionsModified(object sender, PermissionsModifiedEventArgs permissionsModifiedEventArgs)
        {
            ReceivePermissionsModified(permissionsModifiedEventArgs.ModifiedWindowsGroupPermissions);
        }

        void ReceivePermissionsModified(IEnumerable<WindowsGroupPermission> modifiedPermissions)
        {
            var windowsGroupPermissions = modifiedPermissions as IList<WindowsGroupPermission> ?? modifiedPermissions.ToList();
            var exclusionResourceIds = _environmentModel.AuthorizationService.SecurityService.Permissions.Where(permission => permission.ResourceID != Guid.Empty && !permission.IsServer).Select(permission => permission.ResourceID);

            if(windowsGroupPermissions.Any(permission => permission.ResourceID == Guid.Empty && permission.IsServer))
            {
                var serverPermissions = _environmentModel.AuthorizationService.GetResourcePermissions(Guid.Empty);
                ResourceModels.ForEach(model =>
                    {
                        var resourceId = model.ID;
                        if(exclusionResourceIds.Contains(resourceId))
                        {
                            return;
                        }
                        model.UserPermissions = serverPermissions;
                        StudioResourceRepository.UpdateItem(resourceId, (a =>
                        {
                            a.Permissions = serverPermissions;
                        }), _environmentModel.ID);
                    });
                StudioResourceRepository.UpdateRootAndFoldersPermissions(serverPermissions, _environmentModel.ID, false);
                StudioResourceRepository.UpdateItem(Guid.Empty, (x =>
                {
                    if(serverPermissions != Permissions.None && x.Children.Count == 0 && !x.IsRefreshing)
                    {
                        // This code is meant for auto update should the permissions change on the server.
                        // It is however better to use the ConnectControlSingleton as it will raise events an notify to subscribers i.e. Connect(x.EnvironmentId).
                        // StudioResourceRepository.Load(x.EnvironmentId, x.AsyncWorker);
                    }
                    else if((x.Descendants().Where(z => z.ResourceType != ResourceType.Server).All(a => a.Permissions == Permissions.None) && serverPermissions == Permissions.None) && _environmentModel.AuthorizationService.SecurityService.Permissions.All(permission => permission.Permissions == Permissions.None))
                    {
                        StudioResourceRepository.PerformUpdateOnDispatcher(() => x.Children = new ObservableCollection<IExplorerItemModel>());
                    }
                    x.Permissions = serverPermissions;

                }), _environmentModel.ID);
            }
            foreach(var perm in windowsGroupPermissions.Where(permission => permission.ResourceID != Guid.Empty && !permission.IsServer))
            {
                WindowsGroupPermission permission = perm;
                var resourceModel = FindSingle(model => model.ID == permission.ResourceID);
                if(resourceModel != null)
                {
                    try
                    {
                        var resourceId = resourceModel.ID;
                        var resourcePermissions = _environmentModel.AuthorizationService.GetResourcePermissions(resourceId);
                        resourceModel.UserPermissions = resourcePermissions;
                        StudioResourceRepository.UpdateItem(resourceId, (a =>
                                                {
                                                    a.Permissions = resourcePermissions;
                                                }), _environmentModel.ID);
                    }
                    catch(SystemException exception)
                    {
                        HelperUtils.ShowTrustRelationshipError(exception);
                    }
                }
            }
        }

        #endregion Constructor
    }
}