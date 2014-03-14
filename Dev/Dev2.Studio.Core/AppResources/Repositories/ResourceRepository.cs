using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.ExtMethods;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Providers.Logs;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Utils;
using Dev2.Utils;
using Dev2.Workspaces;
using Newtonsoft.Json;


// ReSharper disable once CheckNamespace
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
            IList<IResourceModel> deployableResources =
            dto.ResourceModels.Select(resource => resource.ID)
            .Select(fetchID => sourceEnviroment.ResourceRepository
            .FindSingle(c => c.ID == fetchID)).ToList();

            // Create the real deployment payload ;)
            IDeployDto trueDto = new DeployDto { ResourceModels = deployableResources };


            // Deploy - Seems a bit silly to go out to another service only to comeback in?
            _deployService.Deploy(trueDto, targetEnviroment);

            var targetResourceRepo = targetEnviroment.ResourceRepository;

            // Inform the repo to reload the deployed resources against the targetEnviroment ;)
            foreach(var resourceToReload in deployableResources)
            {
                var effectedResources = targetResourceRepo.ReloadResource(resourceToReload.ID, resourceToReload.ResourceType, ResourceModelEqualityComparer.Current, true);
                if(effectedResources != null)
                {
                    foreach(IResourceModel resource in effectedResources)
                    {
                        var theID = resource.ID;
                        var resourceToUpdate = deployableResources.FirstOrDefault(res => res.ID == theID);

                        if(resourceToUpdate != null)
                        {
                            resourceToUpdate.Update(resource);
                            Logger.foobar(); // force vs to stop removing the damn references ;)
                            this.TraceInfo("Publish message of type - " + typeof(UpdateResourceMessage));
                            // For some daft reason we have two model versions?!
                            var resourceWithContext = new ResourceModel(targetEnviroment);
                            resourceWithContext.Update(resource);
                            eventPublisher.Publish(new UpdateResourceMessage(resourceWithContext));
                        }

                    }
                }
            }

            this.TraceInfo("Publish message of type - " + typeof(RefreshExplorerMessage));
            eventPublisher.Publish(new RefreshExplorerMessage());
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
            IList<IWorkspaceItem> applicableWorkspaceItems = workspaceItems
                .Where(w => w.ServerID == _environmentModel.Connection.ServerID &&
                            w.WorkspaceID == _environmentModel.Connection.WorkspaceID)
                .ToList();

            var rootElement = new XElement("WorkspaceItems");
            rootElement.Add(applicableWorkspaceItems.Select(w => w.ToXml()));

            var comsController = new CommunicationController { ServiceName = "GetLatestService" };
            comsController.AddPayloadArgument("EditedItemsXml", rootElement.ToString());
            comsController.AddPayloadArgument("Roles", "*");

            comsController.ExecuteCommand<ExecuteMessage>(_environmentModel.Connection, _environmentModel.Connection.WorkspaceID);

            IsLoaded = false;
            Load();
        }

        // TODO : Refactor this ;)
        public List<IResourceModel> ReloadResource(Guid resourceID, Enums.ResourceType resourceType, IEqualityComparer<IResourceModel> equalityComparer, bool fetchXaml)
        {
            var comsController = new CommunicationController { ServiceName = "ReloadResourceService" };
            comsController.AddPayloadArgument("ResourceID", resourceID.ToString());
            comsController.AddPayloadArgument("ResourceType", Enum.GetName(typeof(Enums.ResourceType), resourceType));

            var con = _environmentModel.Connection;
            comsController.ExecuteCommand<ExecuteMessage>(con, con.WorkspaceID);

            comsController = new CommunicationController { ServiceName = "FindResourcesByID" };
            comsController.AddPayloadArgument("GuidCsv", resourceID.ToString());
            comsController.AddPayloadArgument("ResourceType", Enum.GetName(typeof(Enums.ResourceType), resourceType));

            var toReloadResources = comsController.ExecuteCommand<List<SerializableResource>>(con, con.WorkspaceID);
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
                    if(ItemAdded != null)
                    {
                        ItemAdded(resource, null);
                    }
                }
            }

            return effectedResources;
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
            var workflow = FindSingle(c => c.ResourceName.Equals(instanceObj.ResourceName, StringComparison.CurrentCultureIgnoreCase));

            if(workflow == null)
            {
                ResourceModels.Add(instanceObj);
            }
            return SaveResource(_environmentModel, instanceObj.ToServiceDefinition(), _environmentModel.Connection.WorkspaceID);
        }

        public void RenameCategory(string oldCategory, string newCategory, Enums.ResourceType resourceType)
        {

            var comsController = new CommunicationController { ServiceName = "RenameResourceCategoryService" };
            comsController.AddPayloadArgument("OldCategory", oldCategory);
            comsController.AddPayloadArgument("NewCategory", newCategory);
            comsController.AddPayloadArgument("ResourceType", resourceType.ToString());

            var con = _environmentModel.Connection;
            comsController.ExecuteCommand<ExecuteMessage>(con, con.WorkspaceID);

        }

        public ExecuteMessage SaveToServer(IResourceModel instanceObj)
        {
            var workflow = FindSingle(c => c.ResourceName.Equals(instanceObj.ResourceName, StringComparison.CurrentCultureIgnoreCase));

            if(workflow == null)
            {
                ResourceModels.Add(instanceObj);
            }
            return SaveResource(_environmentModel, instanceObj.ToServiceDefinition(), GlobalConstants.ServerWorkspaceID);
        }

        public void Rename(string resourceID, string newName)
        {
            Guid resID;

            if(Guid.TryParse(resourceID, out resID))
            {

                var comsController = new CommunicationController { ServiceName = "RenameResourceService" };
                comsController.AddPayloadArgument("NewName", newName);
                comsController.AddPayloadArgument("ResourceID", resourceID);

                var con = _environmentModel.Connection;
                var me = comsController.ExecuteCommand<ExecuteMessage>(con, con.WorkspaceID);

                if(me.Message.Contains("Renamed Resource"))
                {
                    var findInLocalRepo = ResourceModels.FirstOrDefault(res => res.ID == Guid.Parse(resourceID));
                    if(findInLocalRepo != null)
                    {
                        findInLocalRepo.ResourceName = newName;
                    }
                }
            }
            else
            {
                throw new ArgumentException(StringResources.Resource_ID_must_be_a_Guid, "resourceID");
            }
        }

        public void RenameCategory(string oldCategory, string newCategory, ResourceType resourceType)
        {

            var comsController = new CommunicationController { ServiceName = "RenameResourceCategoryService" };
            comsController.AddPayloadArgument("OldCategory", oldCategory);
            comsController.AddPayloadArgument("NewCategory", newCategory);
            comsController.AddPayloadArgument("ResourceType", resourceType.ToString());

            var con = _environmentModel.Connection;
            comsController.ExecuteCommand<ExecuteMessage>(con, con.WorkspaceID);
        }

        public void DeployResource(IResourceModel resource)
        {
            if(resource == null)
            {
                throw new ArgumentNullException("resource");
            }

            var theResource = FindSingle(c => c.ResourceName.Equals(resource.ResourceName, StringComparison.CurrentCultureIgnoreCase));

            // BUG 9703 - 2013.06.21 - TWR - refactored to make sure we always create a new one
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
            comsController.ExecuteCommand<ExecuteMessage>(con, con.WorkspaceID);
        }

        public void Save(ICollection<IResourceModel> instanceObjs)
        {
            throw new NotImplementedException();
        }

        public void Remove(ICollection<IResourceModel> instanceObjs)
        {
            throw new NotImplementedException();
        }

        public void Remove(IResourceModel instanceObj)
        {
            DeleteResource(instanceObj);
        }

        public void RefreshResource(Guid resourceID)
        {
            _cachedServices.Remove(resourceID);
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

        public ExecuteMessage DeleteResource(IResourceModel resource)
        {
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
            comsController.AddPayloadArgument("ResourceName", resource.ResourceName);
            comsController.AddPayloadArgument("ResourceType", resource.ResourceType.ToString());

            var result = comsController.ExecuteCommand<ExecuteMessage>(_environmentModel.Connection, _environmentModel.Connection.WorkspaceID);

            if(result.HasError)
            {
                HandleDeleteResourceError(result, resource);
                return null;
            }

            return result;
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

        //Juries TODO - Refactor to popupProvider
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

            var comsController = new CommunicationController { ServiceName = "FindResourceService" };
            comsController.AddPayloadArgument("ResourceName", "*");
            comsController.AddPayloadArgument("ResourceType", string.Empty);

            var con = _environmentModel.Connection;
            var resourceList = comsController.ExecuteCommand<List<SerializableResource>>(con, con.WorkspaceID);

            if(resourceList == null)
            {
                throw new Exception("Failed to fetch resoure list as JSON model");
            }

            HydrateResourceModels(resourceList, _environmentModel.Connection.ServerID);
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


        void HydrateResourceModels(IEnumerable<SerializableResource> wfServices, Guid serverID)
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

                    IResourceModel resource = HydrateResourceModel(enumsResourceType, item, serverID);
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
                    // Ignore malformed resource
                }
            }
        }

        // Make public for testing, should be extracted to a util class for testing....
        public IResourceModel HydrateResourceModel(Enums.ResourceType resourceType, SerializableResource data, Guid serverID, bool forced = false, bool fetchXaml = false)
        {

            Guid id = data.ResourceID;

            //2013.05.15: Ashley Lewis - Bug 9348 updates force hydration, initialization doesn't
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
                resource.ServerID = serverID;
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
                resource.IconPath = GetIconPath(data.ResourceType);
                resource.Category = data.ResourceCategory;
                resource.Tags = string.Empty;
                resource.Comment = string.Empty;
                resource.ServerResourceType = data.ResourceType.ToString();
                resource.UnitTestTargetWorkflowService = string.Empty;
                resource.HelpLink = string.Empty;
                resource.IsNewWorkflow = isNewWorkflow;
                try
                {
                resource.UserPermissions = _environmentModel.AuthorizationService.GetResourcePermissions(resource.ID);
                }
                catch(SystemException exception)
                {
                    HelperUtils.ShowTrustRelationshipError(exception);
                }

                if(data.Errors != null)
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

        public void AddEnvironment(IEnvironmentModel targetEnvironment, IEnvironmentModel environment)
        {
            if(targetEnvironment == null)
            {
                throw new ArgumentNullException("targetEnvironment");
            }
            if(environment == null)
            {
                throw new ArgumentNullException("environment");
            }

            var sourceDefinition = environment.ToSourceDefinition();

            SaveResource(targetEnvironment, sourceDefinition, targetEnvironment.Connection.WorkspaceID);
        }

        public ExecuteMessage SaveResource(IEnvironmentModel targetEnvironment, StringBuilder resourceDefinition, Guid workspaceID)
        {
            var comsController = new CommunicationController { ServiceName = "SaveResourceService" };
            comsController.AddPayloadArgument("ResourceXml", resourceDefinition);
            comsController.AddPayloadArgument("WorkspaceID", workspaceID.ToString());

            var con = targetEnvironment.Connection;
            var result = comsController.ExecuteCommand<ExecuteMessage>(con, con.WorkspaceID);

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
            comsController.ExecuteCommand<ExecuteMessage>(con, con.WorkspaceID);
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

            Guid workspaceID = con.WorkspaceID;

            var comsController = new CommunicationController { ServiceName = "TerminateExecutionService" };
            comsController.AddPayloadArgument("Roles", "*");
            comsController.AddPayloadArgument("ResourceID", resourceModel.ID.ToString());

            var result = comsController.ExecuteCommand<ExecuteMessage>(con, workspaceID);

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

            List<string> resourceNames = resourceModels.Select(contextualResourceModel => contextualResourceModel.ResourceName).ToList();

            var comsController = new CommunicationController { ServiceName = "GetDependanciesOnListService" };
            comsController.AddPayloadArgument("ResourceNames", JsonConvert.SerializeObject(resourceNames));
            comsController.AddPayloadArgument("GetDependsOnMe", getDependsOnMe.ToString());

            var workspaceID = (environmentModel.Connection).WorkspaceID;
            var result = comsController.ExecuteCommand<List<string>>(environmentModel.Connection, workspaceID);

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
            comsController.AddPayloadArgument("ResourceName", resourceModel.ResourceName);
            comsController.AddPayloadArgument("GetDependsOnMe", getDependsOnMe.ToString());

            var workspaceID = (resourceModel.Environment.Connection).WorkspaceID;
            var payload = comsController.ExecuteCommand<ExecuteMessage>(resourceModel.Environment.Connection, workspaceID);

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

            return comController.ExecuteCommand<Data.Settings.Settings>(currentEnv.Connection, currentEnv.Connection.WorkspaceID);
        }

        public ExecuteMessage WriteSettings(IEnvironmentModel currentEnv, Data.Settings.Settings settings)
        {
            var comController = new CommunicationController { ServiceName = "SettingsWriteService" };
            comController.AddPayloadArgument("Settings", settings.ToString());

            return comController.ExecuteCommand<ExecuteMessage>(currentEnv.Connection, currentEnv.Connection.WorkspaceID);
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
                ExecuteMessage serverLogData = comController.ExecuteCommand<ExecuteMessage>(environmentModel.Connection, environmentModel.Connection.WorkspaceID);

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

            var workspaceID = _environmentModel.Connection.WorkspaceID;
            var tables = comController.ExecuteCommand<DbTableList>(_environmentModel.Connection, workspaceID);

            return tables;
        }

        public DbColumnList GetDatabaseTableColumns(DbSource dbSource, DbTable dbTable)
        {
            var comController = new CommunicationController { ServiceName = "GetDatabaseColumnsForTableService" };

            comController.AddPayloadArgument("Database", JsonConvert.SerializeObject(dbSource));
            comController.AddPayloadArgument("TableName", JsonConvert.SerializeObject(dbTable.TableName));

            var workspaceID = _environmentModel.Connection.WorkspaceID;
            var columns = comController.ExecuteCommand<DbColumnList>(_environmentModel.Connection, workspaceID);

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

            var models = comController.ExecuteCommand<List<SerializableResource>>(targetEnvironment.Connection, targetEnvironment.Connection.WorkspaceID);
            var serverID = targetEnvironment.Connection.ServerID;

            var result = new List<IResourceModel>();

            if(models != null)
            {
                result.AddRange(models.Select(model => HydrateResourceModel(resourceType, model, serverID)));
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

            result = comsController.ExecuteCommand<List<T>>(targetEnvironment.Connection, targetEnvironment.Connection.WorkspaceID);

            return result;
        }

        /// <summary>
        /// Fetches the resource definition.
        /// </summary>
        /// <param name="targetEnv">The target env.</param>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="resourceModelID">The resource model unique identifier.</param>
        /// <returns></returns>
        public ExecuteMessage FetchResourceDefinition(IEnvironmentModel targetEnv, Guid workspaceID, Guid resourceModelID)
        {
            var comsController = new CommunicationController { ServiceName = "FetchResourceDefinitionService" };
            comsController.AddPayloadArgument("ResourceID", resourceModelID.ToString());

            var result = comsController.ExecuteCommand<ExecuteMessage>(targetEnv.Connection, targetEnv.Connection.WorkspaceID);

            // log the trace for fetch ;)
            this.TraceInfo(string.Format("Fetched Definition For {0} From Workspace {1}", resourceModelID, workspaceID));

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
                    // TODO 
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

            _reservedServices = new List<string>();
            ResourceModels = new List<IResourceModel>();
            _cachedServices = new HashSet<Guid>();
        }


        #endregion Constructor
    }
}