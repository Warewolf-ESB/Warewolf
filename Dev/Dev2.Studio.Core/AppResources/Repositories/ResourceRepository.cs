#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Composition;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Providers.Errors;
using Dev2.Providers.Logs;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Core.Wizards.Interfaces;
using Dev2.Workspaces;
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

            ExecuteCommand(_environmentModel, package);

            IsLoaded = false;
            Load();
        }

        public List<IResourceModel> ReloadResource(Guid resourceID, Enums.ResourceType resourceType,
                                                   IEqualityComparer<IResourceModel> equalityComparer)
        {
            dynamic reloadPayload = new UnlimitedObject();
            reloadPayload.Service = "ReloadResourceService";
            reloadPayload.ResourceID = resourceID.ToString();
            reloadPayload.ResourceType = Enum.GetName(typeof(Enums.ResourceType), resourceType);

            ExecuteCommand(_environmentModel, reloadPayload);

            dynamic findPayload = new UnlimitedObject();
            findPayload.Service = "FindResourcesByID";
            findPayload.GuidCsv = resourceID.ToString();
            findPayload.Type = Enum.GetName(typeof(Enums.ResourceType), resourceType);

            var findResultObj = ExecuteCommand(_environmentModel, findPayload);

            var effectedResources = new List<IResourceModel>();
            var wfServices = (resourceType == Enums.ResourceType.Source) ? findResultObj.Source : findResultObj.Service;
            if(wfServices is List<UnlimitedObject>)
            {
                foreach(var item in wfServices)
                {
                    IResourceModel resource = HydrateResourceModel(resourceType, item, true);
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
                    return _resourceModels.Find(func.Invoke);
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
            return SaveResource(_environmentModel, instanceObj.ToServiceDefinition(),_securityContext.Roles);
        }

        public void Rename(string resourceID, string newName)
        {
            dynamic package = new UnlimitedObject();
            package.Service = "RenameResourceService";
            package.NewName = newName;
            package.ResourceID = resourceID;
            ExecuteCommand(_environmentModel, package, false);
        }

        public void RenameCategory(string oldCategory, string newCategory, Enums.ResourceType resourceType)
        {
            dynamic package = new UnlimitedObject();
            package.Service = "RenameResourceCategoryService";
            package.OldCategory = oldCategory == StringResources.Navigation_Category_Unassigned ? "" : oldCategory;
            package.NewCategory = newCategory;
            package.ResourceType = resourceType.ToString();
            ExecuteCommand(_environmentModel, package, false);
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

            ExecuteCommand(_environmentModel, package, false);
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

            if(_wizardEngine != null && !_wizardEngine.IsWizard(contextualResource))
            {
                IContextualResourceModel wizard = _wizardEngine.GetWizard(contextualResource);
                if(wizard != null)
                {
                    UnlimitedObject wizardData = ExecuteDeleteResource(wizard);

                    if(wizardData.HasError)
                    {
                        HandleDeleteResourceError(wizardData, contextualResource);
                        return null;
                    }
                }
            }

            UnlimitedObject data = ExecuteDeleteResource(contextualResource);

            if(data.HasError)
            {
                HandleDeleteResourceError(data, contextualResource);
                return null;
            }

            return data;
        }

        public void Add(IResourceModel instanceObj)
        //Ashley Lewis: 15/01/2013 (for ResourceRepositoryTest.WorkFlowService_OnDelete_Expected_NotInRepository())
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

            return ExecuteCommand(resource.Environment, request);
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


        // Make public for testing, should be extracted to a util class for testing....
        public IResourceModel HydrateResourceModel(Enums.ResourceType resourceType, dynamic data, bool forced = false)
        {
            Guid id;

            Guid.TryParse(data.GetValue("ID"), out id);

            //2013.05.15: Ashley Lewis - Bug 9348 updates force hydration, initialization doesn't
            if (!IsInCache(id) || forced)
            {
                // add to cache of services fetched ;)
                _cachedServices.Add(id);

                var resource = ResourceModelFactory.CreateResourceModel(_environmentModel);
                resource.ResourceType = resourceType;

                // TODO : make this property use new fetch definition service ;)

                var xamlDefinition = data.XamlDefinition as string;
                if(!string.IsNullOrEmpty(xamlDefinition))
                {
                    resource.WorkflowXaml = xamlDefinition;
                    resource.ServiceDefinition = data.XmlString;
                }

                resource.DataList = data.GetValue("DataList");
                resource.ID = id;

                Guid serverID;
                Guid.TryParse(data.GetValue("ServerID"), out serverID);
                resource.ServerID = serverID;

                Version version;
                Version.TryParse(data.GetValue("Version"), out version);
                resource.Version = version;

                bool isValid;
                resource.IsValid = !bool.TryParse(data.GetValue("IsValid"), out isValid) || isValid;

                string errorMessagesXml = data.GetValue("ErrorMessages");
                if (!string.IsNullOrEmpty(errorMessagesXml))
                {
                    var errorMessages = XElement.Parse(errorMessagesXml);
                    foreach (var message in errorMessages.Descendants())
                    {
                        Guid instanceID;
                        Guid.TryParse(message.AttributeSafe("InstanceID"), out instanceID);
                        resource.AddError(new ErrorInfo
                        {
                            InstanceID = instanceID,
                            ErrorType = (ErrorType)Enum.Parse(typeof(ErrorType), message.AttributeSafe("ErrorType")),
                            FixType = (FixType)Enum.Parse(typeof(FixType), message.AttributeSafe("FixType")),
                            Message = message.AttributeSafe("Message"),
                            StackTrace = message.AttributeSafe("StackTrace"),
                            FixData = message.Value
                        });
                    }
                }

                if (string.IsNullOrEmpty(resource.ServiceDefinition))
                {
                    resource.ServiceDefinition = data.XmlString;
                }

                var displayName = data.DisplayName as string;
                resource.DisplayName = displayName ?? resourceType.ToString();

                var iconPath = data.IconPath as string;
                if(iconPath != null)
                {
                    resource.IconPath = iconPath;
                }

                var authorRoles = data.AuthorRoles as string;
                if(authorRoles != null)
                {
                    resource.AuthorRoles = authorRoles;
                }

                var category = data.Category as string;
                resource.Category = category ?? string.Empty;

                var tags = data.Tags as string;
                if(tags != null)
                {
                    resource.Tags = tags;
                }

                var comment = data.Comment as string;
                if(comment != null)
                {
                    resource.Comment = comment;
                }

                var serverResourceType = data.ResourceType as string;
                resource.ServerResourceType = serverResourceType ?? string.Empty;

                var connectionString = data.ConnectionString as string;
                resource.ConnectionString = connectionString ?? string.Empty;

                var unitTestTargetWorkflowService = data.UnitTestTargetWorkflowService as string;
                if(unitTestTargetWorkflowService != null)
                {
                    resource.UnitTestTargetWorkflowService = unitTestTargetWorkflowService;
                }

                var helpLink = data.HelpLink as string;
                if(!string.IsNullOrEmpty(helpLink))
                {
                    resource.HelpLink = helpLink;
                }

                var isNewWorkflow = data.IsNewWorkflow as string;
                if(isNewWorkflow != null)
                {
                    resource.IsNewWorkflow = false;
                    if (string.Equals(isNewWorkflow, "true", StringComparison.InvariantCulture))
                    {
                        resource.IsNewWorkflow = true;
                        NewWorkflowNames.Instance.Add(resource.DisplayName);
                    }
                }

                var service = resourceType == Enums.ResourceType.Source ? data.Source : data.Service;
                if (service is List<UnlimitedObject>)
                {
                    foreach (var svc in service)
                    {
                        if (svc.Name is string)
                        {
                            resource.ResourceName = svc.Name;
                        }
                        else
                        {
                            // Travis : if we here it means Name is an element in the datalist
                            var tmpObj = (svc as UnlimitedObject);

                            // ReSharper disable PossibleNullReferenceException
                            var xDoc = new XmlDocument();
                            xDoc.LoadXml(tmpObj.XmlString);
                            var n = xDoc.SelectSingleNode("Service");
                            resource.ResourceName = n.Attributes["Name"].Value;
                            // ReSharper restore PossibleNullReferenceException
                        }
                    }
                }
                return resource;
            }
            return null;
        }


        #endregion Methods

        #region Private Methods

        protected virtual void LoadResources()
        {
            dynamic dataObj = new UnlimitedObject();
            dataObj.Service = "FindAllResourcesService";
            dataObj.Roles = string.Join(",", _securityContext.Roles);

            var resultObj = ExecuteCommand(_environmentModel, dataObj);
            HydrateResourceModels(resultObj.Source as List<UnlimitedObject>);
            HydrateResourceModels(resultObj.Service as List<UnlimitedObject>);

            // Force GC to clear things up a bit ;)
            GC.Collect(2);
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

        void HydrateResourceModels(IEnumerable<UnlimitedObject> wfServices)
        {
            if(wfServices == null)
            {
                return;
            }

            foreach(dynamic item in wfServices)
            {
                try
                {
                    // TODO: Get rid of Enums.ResourceType !
                    //
                    // DO NOT use dynamic properties as these will auto-create non-existing xml elements on the fly!!!!
                    //
                    var resourceTypeStr = item.GetValue("ResourceType");
                    if(string.IsNullOrEmpty(resourceTypeStr))
                    {
                        continue;
                    }
                    var resourceType = (ResourceType)Enum.Parse(typeof(ResourceType), resourceTypeStr);
                    if(resourceType == ResourceType.ReservedService)
                    {
                        _reservedServices.Add(item.Name.ToUpper());
                        continue;
                    }

                    var enumsResourceTypeString = ResourceTypeConverter.ToTypeString(resourceType);
                    var enumsResourceType = enumsResourceTypeString == ResourceTypeConverter.TypeWildcard
                        ? Enums.ResourceType.Unknown
                        : (Enums.ResourceType)Enum.Parse(typeof(Enums.ResourceType), enumsResourceTypeString);

                    IResourceModel resource = HydrateResourceModel(enumsResourceType, item);
                    if(resource != null)
                    {
                        _resourceModels.Add(resource);
                        if(ItemAdded != null)
                        {
                            ItemAdded(resource, null);
                        }
                    }
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch(Exception ex)
                // ReSharper restore EmptyGeneralCatchClause
                {
                    // Ignore malformed resources
                    // TODO Log this
                }
            }
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
            SaveResource(targetEnvironment, sourceDefinition, securityRoles);
        }

        static string SaveResource(IEnvironmentModel targetEnvironment, string resourceDefinition, string[] securityRoles)
        {
            dynamic dataObj = new UnlimitedObject();
            dataObj.Service = "SaveResourceService";
            dataObj.ResourceXml = resourceDefinition;
            dataObj.Roles = string.Join(",", securityRoles);
            return ExecuteCommand(targetEnvironment, dataObj, false) as string;
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

            ExecuteCommand(targetEnvironment, dataObj, false);
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

            var resourcesObj = ExecuteCommand(targetEnvironment, dataObj);

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

            var resourcesObj = ExecuteCommand(targetEnvironment, dataObj);

            var result = new List<UnlimitedObject>();
            AddItems(result, resourcesObj.Source);

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
                    var itemObj = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(item.XmlString);
                    result.Add(itemObj);
                }
            }
        }

        #endregion

        #region ExecuteCommand

        private static dynamic ExecuteCommand(IEnvironmentModel targetEnvironment, UnlimitedObject dataObj,
                                              bool convertResultToUnlimitedObject = true)
        {
            var workspaceID = targetEnvironment.Connection.WorkspaceID;
            var result = targetEnvironment.Connection.ExecuteCommand(dataObj.XmlString, workspaceID,
                                                                     GlobalConstants.NullDataListID);

            if(result == null)
            {
                dynamic tmp = dataObj;
                throw new Exception(string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, tmp.Service));
            }

            if(convertResultToUnlimitedObject)
            {
                // PBI : 7913 -  Travis
                var resultObj = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(result);
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