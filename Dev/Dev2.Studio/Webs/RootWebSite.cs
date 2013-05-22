using System;
using System.Web;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Webs.Callbacks;

namespace Dev2.Studio.Webs
{
    public static class RootWebSite
    {
        public const string SiteName = "wwwroot";

        #region ShowDialog(IContextualResourceModel resourceModel)

        //
        // TWR: 2013.02.14 - refactored to accommodate new requests
        // PBI: 801
        // BUG: 8477
        //
        public static bool ShowDialog(IContextualResourceModel resourceModel, bool isSaveDialogStandAlone)
        {
            if(resourceModel == null)
            {
                return false;
            }

            string resourceID = null;
            var resourceType = ResourceType.Unknown;

            if(string.IsNullOrEmpty(resourceModel.ServiceDefinition) || resourceModel.IsDuplicate)
            {
                if(resourceModel.IsDuplicate)
                {
                    resourceID = resourceModel.ID.ToString();
                }
                else
                {
                    resourceID = Guid.Empty.ToString();
                }
                if(resourceModel.IsDatabaseService || resourceModel.DisplayName == "DbService")
                {
                    resourceType = ResourceType.DbService;
                }
                else if(resourceModel.IsPluginService || resourceModel.DisplayName == "PluginService")
                {
                    if(!Enum.TryParse(resourceModel.DisplayName, out resourceType))
                    {
                        resourceType = ResourceType.PluginService;
                    }
                }
                else if(resourceModel.IsResourceService || resourceModel.DisplayName == "PluginSource")
                {
                    if(!Enum.TryParse(resourceModel.DisplayName, out resourceType))
                    {
                        resourceType = ResourceType.PluginSource;
                    }
                }else switch(resourceModel.DisplayName)  // see ResourceModelFactory.CreateResourceModel()
                    {
                        case "EmailSource": // PBI 953 - 2013.05.20 - TWR - Added
                            resourceType = ResourceType.EmailSource;
                            break;
                        case "WebSource":   // PBI 5656 - 2013.05.20 - TWR - Added
                            resourceType = ResourceType.WebSource;
                            break;
                        case "WebService":  // PBI 1220 - 2013.05.20 - TWR - Added
                            resourceType = ResourceType.WebService;
                            break;
                        case "RemoteWarewolf":
                            resourceType = ResourceType.Server;
                            break;
                    }
            }
            else
            {
                var resourceXml = XElement.Parse(resourceModel.ServiceDefinition);

                resourceID = resourceXml.AttributeSafe("ID");
                if(string.IsNullOrEmpty(resourceID))
                {
                    resourceID = resourceXml.AttributeSafe("Name");
                }
                Enum.TryParse(resourceXml.AttributeSafe("ResourceType"), out resourceType);

                if(resourceType == ResourceType.Unknown)
                {
                    #region Try determine resourceType of 'old' resources

                    if(resourceXml.Name.LocalName.Equals("Source", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if(resourceXml.AttributeSafe("Type").Equals(enSourceType.Dev2Server.ToString(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            resourceType = ResourceType.Server;
                        }
                        else if(resourceXml.AttributeSafe("Type").Equals(enSourceType.SqlDatabase.ToString(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            // Since the resource is outdated we use the name instead                            
                            resourceID = resourceXml.AttributeSafe("Name");
                            resourceType = ResourceType.DbSource;
                        }
                    }
                    else if(resourceXml.Name.LocalName.Equals("Service", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if(resourceXml.ElementSafe("TypeOf").Equals(enActionType.InvokeStoredProc.ToString(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            // DynamicServicesHost auto-adds an ID to each service if it doesn't exist in the persisted XML
                            // Since the resource is read from disk rather than DynamicServicesHost we use the name instead                            
                            resourceID = resourceXml.AttributeSafe("Name");
                            resourceType = ResourceType.DbService;
                        }
                    }

                    #endregion
                }
            }

            //2013.05.18: Ashley Lewis for PBI 8858 - add contextual resource
            return ShowDialog(resourceModel.Environment, resourceType, isSaveDialogStandAlone, resourceModel.IsDuplicate, resourceID, null, resourceModel);
        }

        #endregion

        #region ShowDialog(IEnvironmentModel environment, ResourceType resourceType, string resourceID = null)

        public static bool ShowDialog(IEnvironmentModel environment, ResourceType resourceType, bool isSaveDialogStandAlone, bool isDuplicate, string resourceID = null, Guid? context = null, IContextualResourceModel resourceModel = null)
        {
            if(isSaveDialogStandAlone)
            {
                //resourceType = ResourceType.Unknown;
            }

            if (environment == null)
            {
                throw new ArgumentNullException("environment");
            }

            string pageName;
            WebsiteCallbackHandler pageHandler;
            double width;
            double height;
            var workspaceID = ((IStudioClientContext)environment.DsfChannel).WorkspaceID;
            string type = null;
            string path = null;
            string name = null;
            if(resourceModel != null)
            {
                if(!string.IsNullOrEmpty(resourceModel.Category))
                {
                    path = resourceModel.Category.Replace(" ", "%20");
                }
                if(!string.IsNullOrEmpty(resourceModel.ResourceName))
                {
                    name = resourceModel.ResourceName.Replace(" ", "%20").Replace(".", "%2E");
                }
            }

            switch(resourceType)
            {
                case ResourceType.Server:
                    workspaceID = GlobalConstants.ServerWorkspaceID; // MUST always save to the server!
                    pageName = "sources/server";
                    pageHandler = new ConnectCallbackHandler(context);
                    width = 705;
                    height = 492;
                    break;

                case ResourceType.DbService:
                    pageName = "services/dbservice";
                    pageHandler = new ServiceCallbackHandler();
                    width = 941;
                    height = 562;
                    type = ResourceType.DbService.ToString();
                    break;

                case ResourceType.DbSource:
                    pageName = "sources/dbsource";
                    pageHandler = new SourceCallbackHandler();
                    width = 705;
                    height = 460;
                    break;

                case ResourceType.PluginService:
                    pageName = "services/pluginservice";
                    pageHandler = new ServiceCallbackHandler();
                    width = 941;
                    height = 562;
                    break;

                case ResourceType.PluginSource:
                    pageName = "sources/pluginsource";
                    pageHandler = new SourceCallbackHandler();
                    width = 743;
                    height = 474;
                    break;

                case ResourceType.EmailSource:  // PBI 953 - 2013.05.16 - TWR - Added
                    pageName = "sources/emailsource";
                    pageHandler = new SourceCallbackHandler();
                    width = 705;
                    height = 492;
                    break;
                default:
                    return false;
            }

            var args = string.Format("{0}?wid={1}&rid={2}&type={3}&path={4}&name={5}&isDuplicate={6}", pageName, workspaceID, (resourceID != Guid.Empty.ToString()) ? resourceID : null, type, string.IsNullOrEmpty(path) ? null : path.ToUpper(), name, isDuplicate);
            environment.ShowWebPageDialog(SiteName, args, pageHandler, width, height);
            return true;
        }

        #endregion

        #region ShowSaveDialog

        public static void ShowNewWorkflowSaveDialog(IContextualResourceModel resourceModel, string resourceID = null, bool addToTabManager = true)
        {
            ShowSaveDialog(resourceModel, new SaveNewWorkflowCallbackHandler(EnvironmentRepository.Instance, resourceModel, addToTabManager), "WorkflowService", resourceID = null);
        }

        static void ShowSaveDialog(IContextualResourceModel resourceModel, WebsiteCallbackHandler callbackHandler, string type, string resourceID = null)
        {
            if(resourceModel == null)
            {
                throw new ArgumentNullException("resourceModel");
            }
            IEnvironmentModel environment = resourceModel.Environment;

            if(environment == null)
            {
                throw new ArgumentNullException("environment");
            }
            string pageName = "dialogs/savedialog";
            double width = 612;
            double height = 459;
            var workspaceID = GlobalConstants.ServerWorkspaceID;

            environment.ShowWebPageDialog(SiteName, string.Format("{0}?wid={1}&rid={2}&type={3}", pageName, workspaceID, resourceID, type), callbackHandler, width, height);
        }

        #endregion
    }
}