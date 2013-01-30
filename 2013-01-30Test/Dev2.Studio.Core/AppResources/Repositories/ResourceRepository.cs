using Dev2.Common;
using Dev2.DynamicServices;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Workspaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Unlimited.Framework;

namespace Dev2.Studio.Core.AppResources.Repositories
{
    public class ResourceRepository : IResourceRepository
    {
        #region Class Members

        private readonly List<IResourceModel> _workflowDb;
        private readonly List<string> _reservedServices;

        public event EventHandler ItemAdded;

        #endregion Class Members

        #region Properties

        public IEnvironmentModel Environment { get; private set; }

        [Import]
        public IFrameworkSecurityContext SecurityContext { get; set; }

        Guid WorkspaceID
        {
            get
            {
                if (Environment.DsfChannel != null)
                {
                    return ((IStudioClientContext)Environment.DsfChannel).AccountID;
                }
                return Guid.Empty;
            }
        }

        #endregion Properties

        #region Constructor

        public ResourceRepository(IEnvironmentModel environment)
        {
            _reservedServices = new List<string>();
            _workflowDb = new List<IResourceModel>();
            Environment = environment;
        }

        #endregion Constructor

        #region Methods

        public void Load()
        {
            AddResources(ResourceType.WorkflowService);
            AddResources(ResourceType.Service);
            AddResources(ResourceType.Source);
            AddResources("ReservedService");
        }

        public void UpdateWorkspace(IList<IWorkspaceItem> workspaceItems)
        {
            IList<IWorkspaceItem> applicableWorkspaceItems = workspaceItems
                .Where(w => w.ServerID == ((IStudioClientContext)Environment.DsfChannel).ServerID &&
                    w.WorkspaceID == ((IStudioClientContext)Environment.DsfChannel).AccountID)
                .ToList();

            XElement rootElement = new XElement("WorkspaceItems");
            rootElement.Add(applicableWorkspaceItems.Select(w => w.ToXml()));

            dynamic package = new UnlimitedObject();
            package.Service = "GetLatestService";
            package.EditedItemsXml = rootElement.ToString();
            package.Roles = string.Join(",", SecurityContext.Roles);

            string updateResult = Environment.DsfChannel.ExecuteCommand(package.XmlString, WorkspaceID, GlobalConstants.NullDataListID);

            dynamic reloadResultObj = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(updateResult);
            if (reloadResultObj.HasError)
            {
                throw new Exception(reloadResultObj.Error);
            }

            _workflowDb.Clear();
            Load();
        }

        public List<IResourceModel> ReloadResource(string resourceName, ResourceType resourceType, IEqualityComparer<IResourceModel> equalityComparer)
        {
            dynamic reloadPayload = new UnlimitedObject();
            reloadPayload.Service = "ReloadResourceService";
            reloadPayload.ResourceName = resourceName;
            reloadPayload.ResourceType = Enum.GetName(typeof(ResourceType), resourceType);
            ;

            string reloadResult = Environment.DsfChannel.ExecuteCommand(reloadPayload.XmlString, WorkspaceID, GlobalConstants.NullDataListID);

            // Debug statements do not belong in product code ;)
            //Debug.WriteLine(string.Format("Outputting service data for resource type '{0}'", resourceType.ToString()));
            //Debug.WriteLine(reloadResult);

            dynamic reloadResultObj = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(reloadResult);
            if (reloadResultObj.HasError)
            {
                throw new Exception(reloadResultObj.Error);
            }

            dynamic findPayload = new UnlimitedObject();
            findPayload.Service = "GetResourceService";
            findPayload.ResourceName = resourceName;
            findPayload.ResourceType = Enum.GetName(typeof(ResourceType), resourceType);
            ;
            findPayload.Roles = string.Join(",", SecurityContext.Roles);

            string findResult = Environment.DsfChannel.ExecuteCommand(findPayload.XmlString, WorkspaceID, GlobalConstants.NullDataListID);

            dynamic findResultObj = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(findResult);
            if (findResultObj.HasError)
            {
                throw new Exception(findResultObj.Error);
            }

            List<IResourceModel> effectedResources = new List<IResourceModel>();
            dynamic wfServices = (resourceType == ResourceType.Source) ? findResultObj.Source : findResultObj.Service;
            if (wfServices is List<UnlimitedObject>)
            {
                foreach (dynamic item in wfServices)
                {
                    IResourceModel resource = HydrateResourceModel(resourceType, item);
                    IResourceModel resourceToUpdate = _workflowDb.FirstOrDefault(r => equalityComparer.Equals(r, resource));

                    if (resourceToUpdate != null)
                    {
                        resourceToUpdate.Update(resource);
                        effectedResources.Add(resourceToUpdate);
                    }
                    else
                    {
                        effectedResources.Add(resourceToUpdate);
                        _workflowDb.Add(resource);
                        if (ItemAdded != null)
                        {
                            ItemAdded(resource, null);
                        }
                    }
                }
            }

            return effectedResources;
        }

        /// <summary>
        /// Checks if a resources exists and is a workflow.
        /// </summary>
        public bool IsWorkflow(string resourceName)
        {
            IResourceModel match = All().FirstOrDefault(c => c.ResourceName.ToUpper().Equals(resourceName.ToUpper()));
            if (match != null)
            {
                return match.ResourceType == ResourceType.WorkflowService;
            }

            return false;
        }

        public bool IsReservedService(string resourceName)
        {
            return _reservedServices.Contains(resourceName.ToUpper());
        }

        public ICollection<IResourceModel> All()
        {
            return _workflowDb;
        }

        public ICollection<IResourceModel> Find(System.Linq.Expressions.Expression<Func<IResourceModel, bool>> expression)
        {
            Func<IResourceModel, bool> func = expression.Compile();
            return _workflowDb.FindAll(func.Invoke);
        }

        public IResourceModel FindSingle(System.Linq.Expressions.Expression<Func<IResourceModel, bool>> expression)
        {
            Func<IResourceModel, bool> func = expression.Compile();


            return _workflowDb.Find(func.Invoke);
        }

        public void Save(IResourceModel instanceObj)
        {
            IResourceModel workflow = this.FindSingle(c => c.ResourceName.Equals(instanceObj.ResourceName, StringComparison.CurrentCultureIgnoreCase));
            if (workflow == null)
            {
                _workflowDb.Add(instanceObj);
            }

            dynamic package = new UnlimitedObject();
            package.Service = "AddResourceService";
            package.ResourceXml = instanceObj.ToServiceDefinition();
            package.Roles = string.Join(",", SecurityContext.Roles);
            Environment.DsfChannel.ExecuteCommand(package.XmlString, WorkspaceID, GlobalConstants.NullDataListID);
        }

        public void DeployResource(IResourceModel resource)
        {
            IResourceModel workflow = this.FindSingle(c => c.ResourceName.Equals(resource.ResourceName, StringComparison.CurrentCultureIgnoreCase));
            if (workflow == null)
            {
                _workflowDb.Add(resource);
            }
            else
            {
                workflow.Update(resource);
            }

            dynamic package = new UnlimitedObject();
            package.Service = "DeployResourceService";
            package.ResourceXml = resource.ToServiceDefinition();
            package.Roles = string.Join(",", SecurityContext.Roles);
            Environment.DsfChannel.ExecuteCommand(package.XmlString, WorkspaceID, GlobalConstants.NullDataListID);
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
            int index = _workflowDb.IndexOf(instanceObj);
            if (index != -1)
                _workflowDb.RemoveAt(index);
            else throw new KeyNotFoundException();
        }

        public void Add(IResourceModel instanceObj)//Ashley Lewis: 15/01/2013 (for ResourceRepositoryTest.WorkFlowService_OnDelete_Expected_NotInRepository())
        {
            _workflowDb.Insert(_workflowDb.Count, instanceObj);
        }

        #endregion Methods

        #region Private Methods

        private void AddResources(string resourceType)
        {
            dynamic dataObj = new UnlimitedObject();
            dataObj.Service = "FindResourceService";
            dataObj.ResourceName = "*";
            dataObj.ResourceType = resourceType;
            dataObj.Roles = string.Join(",", SecurityContext.Roles);

            string result = Environment.DsfChannel.ExecuteCommand(dataObj.XmlString, WorkspaceID, GlobalConstants.NullDataListID);

            // Debug statements do not belong in product code ;)
            //Debug.WriteLine(string.Format("Outputting service data for resource type '{0}'", resourceType.ToString()));
            //Debug.WriteLine(result);

            dynamic resultObj = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(result);
            if (resultObj.HasError)
            {
                throw new Exception(resultObj.Error);
            }

            string xml = resultObj.XmlString;
            int index = 0;

            while ((index = xml.IndexOf("<ReservedName>", index)) != -1)
            {
                int start = index + 14;
                if ((index = xml.IndexOf("</ReservedName>", start)) == -1)
                    break;

                int length = index - start;
                string name = xml.Substring(start, length);
                _reservedServices.Add(name.ToUpper());
            }
        }

        private void AddResources(ResourceType resourceType)
        {
            dynamic dataObj = new UnlimitedObject();
            dataObj.Service = "FindResourceService";
            dataObj.ResourceName = "*";
            dataObj.ResourceType = Enum.GetName(typeof(ResourceType), resourceType);
            dataObj.Roles = string.Join(",", SecurityContext.Roles);

            string result = Environment.DsfChannel.ExecuteCommand(dataObj.XmlString, WorkspaceID, GlobalConstants.NullDataListID);

            // Debug statements do not belong in product code ;)
            //Debug.WriteLine(string.Format("Outputting service data for resource type '{0}'", resourceType.ToString()));
            //Debug.WriteLine(result);

            dynamic resultObj = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(result);
            if (resultObj.HasError)
            {
                throw new Exception(resultObj.Error);
            }

            dynamic wfServices = (resourceType == ResourceType.Source) ? resultObj.Source : resultObj.Service;
            if (wfServices is List<UnlimitedObject>)
            {
                foreach (dynamic item in wfServices)
                {
                    IResourceModel resource = HydrateResourceModel(resourceType, item);
                    _workflowDb.Add(resource);
                    if (ItemAdded != null)
                    {
                        ItemAdded(resource, null);
                    }
                }
            }
        }

        private IResourceModel HydrateResourceModel(ResourceType resourceType, dynamic data)
        {
            IResourceModel resource = ResourceModelFactory.CreateResourceModel(Environment);
            resource.ResourceType = resourceType;
            if (data.XamlDefinition is string)
            {
                if (!string.IsNullOrEmpty(data.XamlDefinition))
                {
                    resource.WorkflowXaml = data.XamlDefinition;
                    resource.ServiceDefinition = data.XmlString;
                }
            }
            resource.DataList = data.GetValue("DataList");

            if (string.IsNullOrEmpty(resource.ServiceDefinition))
            {
                resource.ServiceDefinition = data.XmlString;
            }

            if (data.DisplayName is string)
            {
                resource.DisplayName = data.DisplayName;
            }
            else
            {
                resource.DisplayName = resourceType.ToString();
            }

            if (data.IconPath is string)
            {
                resource.IconPath = data.IconPath;
            }

            if (data.AuthorRoles is string)
            {
                resource.AuthorRoles = data.AuthorRoles;
            }

            if (data.Category is string)
            {
                resource.Category = data.Category;
            }
            else
            {
                resource.Category = string.Empty;
            }

            if (data.Tags is string)
            {
                resource.Tags = data.Tags;
            }

            if (data.Comment is string)
            {
                resource.Comment = data.Comment;
            }

            if (data.UnitTestTargetWorkflowService is string)
            {
                resource.UnitTestTargetWorkflowService = data.UnitTestTargetWorkflowService;
            }

            if (data.HelpLink is string)
            {
                if (!string.IsNullOrEmpty(data.HelpLink))
                {
                    resource.HelpLink = data.HelpLink;
                }
            }

            var service = resourceType == ResourceType.Source ? data.Source : data.Service;
            if (service is List<UnlimitedObject>)
            {
                foreach (dynamic svc in service)
                {

                    if (svc.Name is string)
                    {
                        resource.ResourceName = svc.Name;
                    }
                    else
                    {
                        // Travis : if we here it means Name is an element in the datalist
                        try
                        {
                            UnlimitedObject tmpObj = (svc as UnlimitedObject);

                            XmlDocument xDoc = new XmlDocument();
                            xDoc.LoadXml(tmpObj.XmlString);
                            XmlNode n = xDoc.SelectSingleNode("Service");
                            resource.ResourceName = n.Attributes["Name"].Value;
                        }
                        catch (Exception e1)
                        {
                            throw e1;
                        }
                    }
                }
            }

            return resource;
        }

        #endregion Private Methods

        #region Add/RemoveEnvironment

        public static void AddEnvironment(IEnvironmentModel targetEnvironment, IEnvironmentModel environment)
        {
            if (targetEnvironment == null)
            {
                throw new ArgumentNullException("targetEnvironment");
            }
            if (environment == null)
            {
                throw new ArgumentNullException("environment");
            }

            dynamic dataObj = new UnlimitedObject();
            dataObj.Service = "AddResourceService";
            dataObj.ResourceXml = environment.ToSourceDefinition();
            dataObj.Roles = string.Join(",", environment.EnvironmentConnection.SecurityContext.Roles);

            ExecuteCommand(targetEnvironment, dataObj);
        }

        public static void RemoveEnvironment(IEnvironmentModel targetEnvironment, IEnvironmentModel environment)
        {
            if (targetEnvironment == null)
            {
                throw new ArgumentNullException("targetEnvironment");
            }
            if (environment == null)
            {
                throw new ArgumentNullException("environment");
            }

            dynamic dataObj = new UnlimitedObject();
            dataObj.Service = "DeleteResourceService";
            dataObj.ResourceName = environment.Name;
            dataObj.ResourceType = ResourceType.Source.ToString();
            dataObj.Roles = string.Join(",", environment.EnvironmentConnection.SecurityContext.Roles);

            ExecuteCommand(targetEnvironment, dataObj);
        }

        #endregion

        #region FindResourcesByID

        public static List<UnlimitedObject> FindResourcesByID(IEnvironmentModel targetEnvironment, IEnumerable<string> guids, ResourceType resourceType)
        {
            if (targetEnvironment == null || guids == null)
            {
                return new List<UnlimitedObject>();
            }

            dynamic dataObj = new UnlimitedObject();
            dataObj.Service = "FindResourcesByID";
            //dataObj.GuidCsv = string.Join(",", guids);
            dataObj.Type = Enum.GetName(typeof(ResourceType), resourceType);

            var resourcesObj = ExecuteCommand(targetEnvironment, dataObj);

            var result = new List<UnlimitedObject>();
            AddItems(result, resourceType == ResourceType.Source ? resourcesObj.Source : resourcesObj.Service);

            return result;
        }

        #endregion

        #region FindSourcesByType

        public static List<UnlimitedObject> FindSourcesByType(IEnvironmentModel targetEnvironment, enSourceType sourceType)
        {
            if (targetEnvironment == null)
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

        #region ExecuteCommand

        static dynamic ExecuteCommand(IEnvironmentModel targetEnvironment, UnlimitedObject dataObj)
        {
            var workspaceID = ((IStudioClientContext)targetEnvironment.DsfChannel).AccountID;
            var result = targetEnvironment.DsfChannel.ExecuteCommand(dataObj.XmlString, workspaceID, GlobalConstants.NullDataListID);
            var resultObj = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(result);
            if (resultObj.HasError)
            {
                throw new Exception(resultObj.Error);
            }
            return resultObj;
        }

        #endregion

        #region AddItems

        static void AddItems(ICollection<UnlimitedObject> result, dynamic items)
        {
            if (items is IEnumerable<UnlimitedObject>)
            {
                foreach (var item in items)
                {
                    var itemObj = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(item.XmlString);
                    result.Add(itemObj);
                }
            }
        }

        #endregion
    }
}
