#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Composition;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Providers.Errors;
using Dev2.Providers.Events;
using Dev2.Providers.Logs;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Core.Wizards.Interfaces;
using Dev2.Workspaces;
using Infragistics.Shared;
using Newtonsoft.Json;
using Unlimited.Framework;

#endregion

namespace Dev2.Studio.Core.AppResources.Repositories
{
    public class ResourceRepository : IResourceRepository
    {
        private HashSet<Guid> _cachedServices;
        private IEnvironmentModel _environmentModel;
        private List<string> _reservedServices;
        protected List<IResourceModel> _resourceModels;
        private IFrameworkSecurityContext _securityContext;
        private IWizardEngine _wizardEngine;
        private bool _isLoaded;

        private IDeployService _deployService = new DeployService();

        private bool _isDisposed;
        Guid _updateWorkflowServerMessageID;
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

        public IWizardEngine WizardEngine
        {
            get { return _wizardEngine; }
        }

        #region Methods

        /// <summary>
        /// Deploys the resources.
        /// </summary>
        /// <param name="targetEnviroment">The target enviroment.</param>
        /// <param name="sourceEnviroment">The source enviroment.</param>
        /// <param name="dto">The dto.</param>
        /// <param name="eventPublisher">The event publisher.</param>
        public void DeployResources(IEnvironmentModel sourceEnviroment, IEnvironmentModel targetEnviroment, IDeployDTO dto, IEventAggregator eventPublisher)
        {

            //Fetch from resource repository to deploy ;)
            IList<IResourceModel> deployableResources =
            dto.ResourceModels.Select(resource => resource.ID)
            .Select(fetchID => sourceEnviroment.ResourceRepository
            .FindSingle(c => c.ID == fetchID)).ToList();

            // Create the real deployment payload ;)
            IDeployDTO trueDto = new DeployDTO { ResourceModels = deployableResources };


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
                            Logger.TraceInfo("Publish message of type - " + typeof(UpdateResourceMessage));
                            // For some daft reason we have two model versions?!
                            var resourceWithContext = new ResourceModel(targetEnviroment);
                            resourceWithContext.Update(resource);
                            eventPublisher.Publish(new UpdateResourceMessage(resourceWithContext));
                        }

                    }
                }
            }

            Logger.TraceInfo("Publish message of type - " + typeof(RefreshExplorerMessage));
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
                _resourceModels.Clear();
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
                .Where(w => w.ServerID == ((IStudioClientContext)_environmentModel.DsfChannel).ServerID &&
                            w.WorkspaceID == ((IStudioClientContext)_environmentModel.DsfChannel).WorkspaceID)
                .ToList();

            var rootElement = new XElement("WorkspaceItems");
            rootElement.Add(applicableWorkspaceItems.Select(w => w.ToXml()));

            dynamic package = new UnlimitedObject();
            package.Service = "GetLatestService";
            package.EditedItemsXml = rootElement.ToString();
            package.Roles = string.Join(",", _securityContext.Roles);

            ExecuteCommand(_environmentModel, package, _environmentModel.Connection.WorkspaceID);

            IsLoaded = false;
            Load();
        }

        // TODO : Refactor this ;)
        public List<IResourceModel> ReloadResource(Guid resourceID, Enums.ResourceType resourceType, IEqualityComparer<IResourceModel> equalityComparer, bool fetchXAML)
        {
            dynamic reloadPayload = new UnlimitedObject();
            reloadPayload.Service = "ReloadResourceService";
            reloadPayload.ResourceID = resourceID.ToString();
            reloadPayload.ResourceType = Enum.GetName(typeof(Enums.ResourceType), resourceType);

            ExecuteCommand(_environmentModel, reloadPayload, _environmentModel.Connection.WorkspaceID);

            dynamic findPayload = new UnlimitedObject();
            findPayload.Service = "FindResourcesByID";
            findPayload.GuidCsv = resourceID.ToString();
            findPayload.Type = Enum.GetName(typeof(Enums.ResourceType), resourceType);

            var findResultObj = ExecuteCommand(_environmentModel, findPayload, _environmentModel.Connection.WorkspaceID, false);

            List<SerializableResource> toReloadResources = JsonConvert.DeserializeObject<List<SerializableResource>>(findResultObj);
            var effectedResources = new List<IResourceModel>();

            foreach(var serializableResource in toReloadResources)
            {
                IResourceModel resource = HydrateResourceModel(resourceType, serializableResource, _environmentModel.Connection.ServerID, true, fetchXAML);
                var resourceToUpdate = _resourceModels.FirstOrDefault(r => equalityComparer.Equals(r, resource));

                if(resourceToUpdate != null)
                {
                    resourceToUpdate.Update(resource);
                    effectedResources.Add(resourceToUpdate);
                }
                else
                {
                    effectedResources.Add(resource);
                    _resourceModels.Add(resource);
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
            return _resourceModels;
        }

        public ICollection<IResourceModel> Find(Expression<Func<IResourceModel, bool>> expression)
        {
            if(expression == null) return null;
            Func<IResourceModel, bool> func = expression.Compile();
            return _resourceModels.FindAll(func.Invoke);
        }

        public IResourceModel FindSingle(Expression<Func<IResourceModel, bool>> expression)
        {
            if(expression != null && _reservedServices != null)
            {
                var func = expression.Compile();
                if(func.Method != null)
                {
                    var result = _resourceModels.Find(func.Invoke);

                    // force a payload fetch ;)
                    if(result != null && result.ResourceType == Enums.ResourceType.Service && string.IsNullOrEmpty(result.WorkflowXaml))
                    {
                        result.WorkflowXaml = FetchResourceDefinition(_environmentModel, GlobalConstants.ServerWorkspaceID, result.ID);
                    }

                    return result;
                }
            }
            return null;
        }

        public string Save(IResourceModel instanceObj)
        {
            var workflow = FindSingle(c => c.ResourceName.Equals(instanceObj.ResourceName, StringComparison.CurrentCultureIgnoreCase));
            if(workflow == null)
            {
                _resourceModels.Add(instanceObj);
            }
            return SaveResource(_environmentModel, instanceObj.ToServiceDefinition(), _securityContext.Roles, _environmentModel.Connection.WorkspaceID);
        }

        public string SaveToServer(IResourceModel instanceObj)
        {
            var workflow = FindSingle(c => c.ResourceName.Equals(instanceObj.ResourceName, StringComparison.CurrentCultureIgnoreCase));
            if(workflow == null)
            {
                _resourceModels.Add(instanceObj);
            }
            return SaveResource(_environmentModel, instanceObj.ToServiceDefinition(), _securityContext.Roles, GlobalConstants.ServerWorkspaceID);
        }

        public void Rename(string resourceID, string newName)
        {
            Guid resID;
            if(Guid.TryParse(resourceID, out resID))
            {
                dynamic package = new UnlimitedObject();
                package.Service = "RenameResourceService";
                package.NewName = newName;
                package.ResourceID = resourceID;
                var executeCommand = ExecuteCommand(_environmentModel, package, _environmentModel.Connection.WorkspaceID, false);
                if(executeCommand.Contains("Renamed Resource"))
                {
                    var findInLocalRepo = _resourceModels.FirstOrDefault(res => res.ID == Guid.Parse(resourceID));
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

        public void RenameCategory(string oldCategory, string newCategory, Enums.ResourceType resourceType)
        {
            dynamic package = new UnlimitedObject();
            package.Service = "RenameResourceCategoryService";
            package.OldCategory = oldCategory == StringResources.Navigation_Category_Unassigned ? "" : oldCategory;
            package.NewCategory = newCategory;
            package.ResourceType = resourceType.ToString();
            ExecuteCommand(_environmentModel, package, _environmentModel.Connection.WorkspaceID, false);
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
                _resourceModels.Remove(theResource);
            }
            theResource = new ResourceModel(_environmentModel);
            theResource.Update(resource);
            _resourceModels.Add(theResource);

            var package = BuildUnlimitedPackage(resource);

            ExecuteCommand(_environmentModel, package, _environmentModel.Connection.WorkspaceID, false);
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
            int index = _resourceModels.IndexOf(resource);

            if(index != -1)
            {
                return true;
            }

            return false;
        }

        public UnlimitedObject DeleteResource(IResourceModel resource)
        {
            IResourceModel res = _resourceModels.FirstOrDefault(c => c.ID == resource.ID);
            if(res == null)
            {
                return new UnlimitedObject("<Result>Failure</Result>");
            }
            int index = _resourceModels.IndexOf(res);
            if(index != -1)
                _resourceModels.RemoveAt(index);
            else throw new KeyNotFoundException();

            var contextualResource = resource as IContextualResourceModel;
            if(contextualResource == null) return null;

            UnlimitedObject data = ExecuteDeleteResource(contextualResource);

            if(data.HasError)
            {
                HandleDeleteResourceError(data, contextualResource);
                return null;
            }

            return data;
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

        public dynamic BuildUnlimitedPackage(IResourceModel resource)
        {
            if(resource == null)
            {
                throw new ArgumentNullException("resource");
            }
            dynamic package = new UnlimitedObject();
            package.Service = "DeployResourceService";
            package.ResourceDefinition = resource.ToServiceDefinition();
            package.Roles = string.Join(",", _securityContext.Roles ?? new string[0]);
            return package;
        }

        private UnlimitedObject ExecuteDeleteResource(IContextualResourceModel resource)
        {
            dynamic request = new UnlimitedObject();
            request.Service = "DeleteResourceService";
            request.ResourceName = resource.ResourceName;
            request.ResourceType = resource.ResourceType.ToString();
            request.Roles = String.Join(",", _securityContext.Roles ?? new string[0]);

            IEnvironmentModel targetEnvironment = resource.Environment;
            return ExecuteCommand(targetEnvironment, request, targetEnvironment.Connection.WorkspaceID);
        }

        //Juries TODO - Refactor to popupProvider
        private void HandleDeleteResourceError(dynamic data, IContextualResourceModel model)
        {
            if(data.HasError)
            {
                MessageBox.Show(Application.Current.MainWindow,
                                model.ResourceType.GetDescription() + " \"" + model.ResourceName +
                                "\" could not be deleted, reason: " + data.Error,
                                model.ResourceType.GetDescription() + " Deletion Failed", MessageBoxButton.OK);
            }
        }

        private string GetIconPath(ResourceType type)
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

        #endregion Methods

        #region Private Methods

        protected virtual void LoadResources()
        {
            dynamic dataObj = new UnlimitedObject();
            dataObj.Service = "FindResourceService";
            dataObj.ResourceName = "*";
            dataObj.ResourceType = string.Empty;
            dataObj.Roles = string.Join(",", _securityContext.Roles);

            var resultObj = ExecuteCommand(_environmentModel, dataObj, _environmentModel.Connection.WorkspaceID, false);

            IList<SerializableResource> resourceList = JsonConvert.DeserializeObject<List<SerializableResource>>(resultObj);

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
                        _resourceModels.Add(resource);
                        if(ItemAdded != null)
                        {
                            ItemAdded(resource, null);
                        }
                    }
                }
                catch(Exception)
                {
                    // Ignore malformed resource

                }
            }
        }

        // Make public for testing, should be extracted to a util class for testing....
        public IResourceModel HydrateResourceModel(Enums.ResourceType resourceType, SerializableResource data, Guid serverID, bool forced = false, bool fetchXAML = false)
        {

            Guid id = data.ResourceID;

            //2013.05.15: Ashley Lewis - Bug 9348 updates force hydration, initialization doesn't
            if(!IsInCache(id) || forced)
            {
                // add to cache of services fetched ;)
                _cachedServices.Add(id);

                var isNewWorkflow = data.IsNewResource;

                var resource = ResourceModelFactory.CreateResourceModel(_environmentModel);
                resource.ResourceType = resourceType;
                resource.ID = id;
                resource.ServerID = serverID;
                resource.IsValid = data.IsValid;
                resource.DataList = data.DataList.Replace(GlobalConstants.SerializableResourceQuote, "\"").Replace(GlobalConstants.SerializableResourceSingleQuote, "'");
                resource.ResourceName = data.ResourceName;
                resource.DisplayName = data.ResourceName;
                resource.IconPath = GetIconPath(data.ResourceType);
                resource.AuthorRoles = string.Empty;
                resource.Category = data.ResourceCategory;
                resource.Tags = string.Empty;
                resource.Comment = string.Empty;
                resource.ServerResourceType = data.ResourceType.ToString();
                resource.UnitTestTargetWorkflowService = string.Empty;
                resource.HelpLink = string.Empty;
                resource.IsNewWorkflow = isNewWorkflow;

                // set the errors ;)
                foreach(var error in data.Errors)
                {
                    resource.AddError(error);
                }

                if(fetchXAML)
                {
                    resource.WorkflowXaml = FetchResourceDefinition(_environmentModel, GlobalConstants.ServerWorkspaceID, id);
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

        public static void AddEnvironment(IEnvironmentModel targetEnvironment, IEnvironmentModel environment)
        {
            if(targetEnvironment == null)
            {
                throw new ArgumentNullException("targetEnvironment");
            }
            if(environment == null)
            {
                throw new ArgumentNullException("environment");
            }

            string sourceDefinition = environment.ToSourceDefinition();
            string[] securityRoles = environment.Connection.SecurityContext.Roles;
            SaveResource(targetEnvironment, sourceDefinition, securityRoles, targetEnvironment.Connection.WorkspaceID);
        }

        public static string SaveResource(IEnvironmentModel targetEnvironment, string resourceDefinition, string[] securityRoles, Guid workspaceID)
        {
            dynamic dataObj = new UnlimitedObject();
            dataObj.Service = "SaveResourceService";
            dataObj.ResourceXml = resourceDefinition;
            dataObj.WorkspaceID = workspaceID;
            dataObj.Roles = string.Join(",", securityRoles);
            return ExecuteCommand(targetEnvironment, dataObj, workspaceID, false) as string;
        }

        public static void RemoveEnvironment(IEnvironmentModel targetEnvironment, IEnvironmentModel environment)
        {
            if(targetEnvironment == null)
            {
                throw new ArgumentNullException("targetEnvironment");
            }
            if(environment == null)
            {
                throw new ArgumentNullException("environment");
            }

            dynamic dataObj = new UnlimitedObject();
            dataObj.Service = "DeleteResourceService";
            dataObj.ResourceName = environment.Name;
            dataObj.ResourceType = Enums.ResourceType.Source.ToString();
            dataObj.Roles = string.Join(",", environment.Connection.SecurityContext.Roles);

            ExecuteCommand(targetEnvironment, dataObj, targetEnvironment.Connection.WorkspaceID, false);
        }

        #endregion

        #region FindResourcesByID

        public bool DoesResourceExistInRepo(IResourceModel resource)
        {
            int index = _resourceModels.IndexOf(resource);
            if(index != -1)
            {
                return true;
            }

            return false;
        }

        public static List<UnlimitedObject> FindResourcesByID(IEnvironmentModel targetEnvironment,
                                                              IEnumerable<string> guids, Enums.ResourceType resourceType)
        {
            if(targetEnvironment == null || guids == null)
            {
                return new List<UnlimitedObject>();
            }

            dynamic dataObj = new UnlimitedObject();
            dataObj.Service = "FindResourcesByID";
            dataObj.GuidCsv = string.Join(",", guids); // BUG 9276 : TWR : 2013.04.19 - reintroduced to all filtering
            dataObj.Type = Enum.GetName(typeof(Enums.ResourceType), resourceType);

            var resourcesObj = ExecuteCommand(targetEnvironment, dataObj, targetEnvironment.Connection.WorkspaceID);

            var result = new List<UnlimitedObject>();
            AddItems(result, resourceType == Enums.ResourceType.Source ? resourcesObj.Source : resourcesObj.Service);

            return result;
        }

        #endregion

        #region FindSourcesByType

        public static List<UnlimitedObject> FindSourcesByType(IEnvironmentModel targetEnvironment,
                                                              enSourceType sourceType)
        {
            if(targetEnvironment == null)
            {
                return new List<UnlimitedObject>();
            }

            dynamic dataObj = new UnlimitedObject();
            dataObj.Service = "FindSourcesByType";
            dataObj.Type = Enum.GetName(typeof(enSourceType), sourceType);

            var resourcesObj = ExecuteCommand(targetEnvironment, dataObj, targetEnvironment.Connection.WorkspaceID);

            var result = new List<UnlimitedObject>();
            AddItems(result, resourcesObj.Source);

            return result;
        }

        /// <summary>
        /// Fetches the resource definition.
        /// </summary>
        /// <param name="targetEnv">The target env.</param>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="resourceModelID">The resource model unique identifier.</param>
        /// <returns></returns>
        public string FetchResourceDefinition(IEnvironmentModel targetEnv, Guid workspaceID, Guid resourceModelID)
        {
            dynamic dataObj = new UnlimitedObject();
            dataObj.Service = "FetchResourceDefinitionService";
            dataObj.ResourceID = resourceModelID;

            // log the trace for fetch ;)
            Logger.TraceInfo(string.Format("Fetched Definition For {0} From Workspace {1}", resourceModelID, workspaceID));

            var result = ExecuteCommand(targetEnv, dataObj, workspaceID, false) as string;

            if(!string.IsNullOrEmpty(result))
            {
                result = result.Unescape();
            }

            return result;
        }

        #endregion

        #region AddItems

        private static void AddItems(ICollection<UnlimitedObject> result, dynamic items)
        {
            if(items is IEnumerable<UnlimitedObject>)
            {
                foreach(var item in items)
                {
                    var itemObj = new UnlimitedObject().GetStringXmlDataAsUnlimitedObject(item.XmlString);
                    result.Add(itemObj);
                }
            }
        }

        #endregion

        #region ExecuteCommand

        private static dynamic ExecuteCommand(IEnvironmentModel targetEnvironment, UnlimitedObject dataObj, Guid workspaceID, bool convertResultToUnlimitedObject = true)
        {
            var result = targetEnvironment.Connection.ExecuteCommand(dataObj.XmlString, workspaceID, GlobalConstants.NullDataListID);

            if(result == null)
            {
                dynamic tmp = dataObj;
                throw new Exception(string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, tmp.Service));
            }

            if(convertResultToUnlimitedObject)
            {
                // PBI : 7913 -  Travis
                var resultObj = new UnlimitedObject().GetStringXmlDataAsUnlimitedObject(result);
                if(resultObj.HasError)
                {
                    throw new Exception(resultObj.Error);
                }
                return resultObj;
            }
            return result as string;
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
        private void Dispose(bool disposing)
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

        public ResourceRepository(IEnvironmentModel environmentModel, IWizardEngine wizardEngine, IFrameworkSecurityContext securityContext)
        {
            VerifyArgument.IsNotNull("environmentModel", environmentModel);
            VerifyArgument.IsNotNull("wizardEngine", wizardEngine);
            VerifyArgument.IsNotNull("securityContext", securityContext);

            _environmentModel = environmentModel;
            _wizardEngine = wizardEngine;
            _securityContext = securityContext;

            _reservedServices = new List<string>();
            _resourceModels = new List<IResourceModel>();
            _cachedServices = new HashSet<Guid>();
        }


        #endregion Constructor
    }
}