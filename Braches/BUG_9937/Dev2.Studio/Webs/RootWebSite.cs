using System;
using System.Web;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Webs.Callbacks;

namespace Dev2.Studio.Webs
{
    public static class RootWebSite
    {
        public const string SiteName = "wwwroot";

        #region ShowSwitchDragDialog

        public static Dev2DecisionCallbackHandler ShowSwitchDragDialog(IEnvironmentModel environment, string webModel)
        {
            const int DialogWidth = 752;
            const int DialogHeight = 121;

            var callBackHandler = new Dev2DecisionCallbackHandler { ModelData = webModel };
            environment.ShowWebPageDialog(SiteName, "switch/drag", callBackHandler, DialogWidth, DialogHeight);

            return callBackHandler;
        }

        #endregion

        #region ShowSwitchDropDialog

        public static Dev2DecisionCallbackHandler ShowSwitchDropDialog(IEnvironmentModel environment, string webModel)
        {
            const int DialogWidth = 752;
            const int DialogHeight = 171;

            var callBackHandler = new Dev2DecisionCallbackHandler { ModelData = webModel };
            environment.ShowWebPageDialog(SiteName, "switch/drop", callBackHandler, DialogWidth, DialogHeight);

            return callBackHandler;
        }

        #endregion

        #region ShowDecisionDialog

        public static Dev2DecisionCallbackHandler ShowDecisionDialog(IEnvironmentModel environment, string webModel)
        {
            const int DialogWidth = 824;
            const int DialogHeight = 520;

            var callBackHandler = new Dev2DecisionCallbackHandler { ModelData = webModel };
            environment.ShowWebPageDialog(SiteName, "decisions/wizard", callBackHandler, DialogWidth, DialogHeight);

            return callBackHandler;
        }

        #endregion

        #region ShowDialog(IContextualResourceModel resourceModel)

        //
        // TWR: 2013.02.14 - refactored to accommodate new requests
        // PBI: 801
        // BUG: 8477
        //
        public static bool ShowDialog(IContextualResourceModel resourceModel)
        {
            if(resourceModel == null)
            {
                return false;
            }

            string resourceID;
            var resourceType = ResourceType.Unknown;
            //2013.06.04: Ashley Lewis for PBI 9458 - placeholder for adding new resources to a context
            //TODO set this to some folder name (eg: the folder that was right clicked).
            //The wizard save dialog will pre-select this folder name
            string resourcePath = null;

            if(string.IsNullOrEmpty(resourceModel.ServiceDefinition))
            {
                resourceID = Guid.Empty.ToString();
                if(resourceModel.IsDatabaseService)
                {
                    resourceType = ResourceType.DbService;
                }
                else if(resourceModel.IsPluginService)
                {
                    Enum.TryParse(resourceModel.DisplayName, out resourceType);
                }
                else if(resourceModel.IsResourceService)
                {
                    resourceType = ResourceType.PluginSource;
                }
                else switch(resourceModel.DisplayName)  // see ResourceModelFactory.CreateResourceModel()
                    {
                        case "PluginSource":
                            resourceType = ResourceType.PluginSource;
                            break;
                        case "DbSource":
                            resourceType = ResourceType.DbSource;
                            break;
                        case "EmailSource": // PBI 953 - 2013.05.20 - TWR - Added
                            resourceType = ResourceType.EmailSource;
                            break;
                        case "WebSource":   // PBI 5656 - 2013.05.20 - TWR - Added
                            resourceType = ResourceType.WebSource;
                            break;
                        case "WebService":  // PBI 1220 - 2013.05.20 - TWR - Added
                            resourceType = ResourceType.WebService;
                            break;
                        case "PluginService":
                            resourceType = ResourceType.PluginService;
                            break;
                        case "DbService":
                            resourceType = ResourceType.DbService;
                            break;
                        case "RemoteWarewolf":
                        case "Server":
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

            return ShowDialog(resourceModel.Environment, resourceType, resourcePath, resourceID);
        }

        #endregion

        #region ShowDialog(IEnvironmentModel environment, ResourceType resourceType, string resourceID = null)

        public static bool ShowDialog(IEnvironmentModel environment, ResourceType resourceType, string resourcePath, string resourceID = null, Guid? context = null)
        {
            const int ServiceDialogHeight = 557;
            const int ServiceDialogWidth = 941;

            if(environment == null)
            {
                throw new ArgumentNullException("environment");
            }

            string pageName;
            WebsiteCallbackHandler pageHandler;
            double width;
            double height;
            var workspaceID = ((IStudioClientContext)environment.DsfChannel).WorkspaceID;

            switch(resourceType)
            {
                case ResourceType.Server:
                    workspaceID = GlobalConstants.ServerWorkspaceID; // MUST always save to the server!
                    pageName = "sources/server";
                    pageHandler = new ConnectCallbackHandler(context);
                    width = 704;
                    height = 492;
                    break;

                case ResourceType.DbService:
                    pageName = "services/dbservice";
                    pageHandler = new DbServiceCallbackHandler();
                    width = ServiceDialogWidth;
                    height = ServiceDialogHeight;
                    break;

                case ResourceType.DbSource:
                    pageName = "sources/dbsource";
                    pageHandler = new SourceCallbackHandler();
                    width = 704;
                    height = 492;
                    break;

                case ResourceType.PluginService:
                    pageName = "services/pluginservice";
                    pageHandler = new ServiceCallbackHandler();
                    width = ServiceDialogWidth;
                    height = ServiceDialogHeight;
                    break;

                case ResourceType.PluginSource:
                    pageName = "sources/pluginsource";
                    pageHandler = new SourceCallbackHandler();
                    width = 704;
                    height = 492;
                    break;

                case ResourceType.EmailSource:  // PBI 953 - 2013.05.16 - TWR - Added
                    pageName = "sources/emailsource";
                    pageHandler = new SourceCallbackHandler();
                    width = 706;
                    height = 494;
                    break;

                case ResourceType.WebSource:    // PBI 5656 - 2013.05.20 - TWR - Added
                    pageName = "sources/websource";
                    pageHandler = new WebSourceCallbackHandler();
                    width = 704;
                    height = 492;
                    break;

                case ResourceType.WebService:   // PBI 1220 - 2013.05.20 - TWR - Added
                    pageName = "services/webservice";
                    pageHandler = new WebServiceCallbackHandler();
                    width = ServiceDialogWidth;
                    height = ServiceDialogHeight;
                    break;
                default:
                    return false;
            }

            var envirDisplayName = FullyEncodeServerDetails(environment.Connection);
            resourcePath = HttpUtility.UrlEncode(resourcePath);
            environment.ShowWebPageDialog(SiteName, string.Format("{0}?wid={1}&rid={2}&envir={3}&path={4}", pageName, workspaceID, resourceID, envirDisplayName, resourcePath), pageHandler, width, height);
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
            double width = 604;
            double height = 460;
            var workspaceID = GlobalConstants.ServerWorkspaceID;

            var envirDisplayName = FullyEncodeServerDetails(environment.Connection);
            environment.ShowWebPageDialog(SiteName, string.Format("{0}?wid={1}&rid={2}&type={3}&title={4}&envir={5}", pageName, workspaceID, resourceID, type, HttpUtility.UrlEncode("New Workflow"), envirDisplayName), callbackHandler, width, height);
        }

        #endregion

        #region Encode Environment Name and Address

        public static string FullyEncodeServerDetails(IEnvironmentConnection allServerDetails)
        {
            return HttpUtility.UrlEncode(allServerDetails.DisplayName + " (" + allServerDetails.AppServerUri.ToString().Replace(".", "%2E") + ")");
        }

        #endregion
    }
}