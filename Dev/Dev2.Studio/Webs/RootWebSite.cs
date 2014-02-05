using System;
using System.Web;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Webs.Callbacks;
using Dev2.Webs.Callbacks;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Webs
{
    public static class RootWebSite
    {

        public static bool IsTestMode { get; set; }
        public static string TestModeRelativeUri { get; set; }
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
            const int DialogHeight = 517;

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

            if(resourceModel.Category == null)
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
                resourceID = resourceModel.ID.ToString();
                Enum.TryParse(resourceModel.ServerResourceType, out resourceType);
            }

            // we need to take the SourceID out and pass along ;)
            var srcID = Guid.Empty.ToString();
            if(resourceModel.WorkflowXaml != null)
            {
                var xe = resourceModel.WorkflowXaml.Replace("&", "&amp;").ToXElement();
                srcID = xe.AttributeSafe("SourceID");
            }

            return ShowDialog(resourceModel.Environment, resourceType, null, resourceID, srcID);
        }

        #endregion

        #region ShowDialog(IEnvironmentModel environment, ResourceType resourceType, string resourceID = null)

        public static bool ShowDialog(IEnvironmentModel environment, ResourceType resourceType, string resourcePath, string resourceID = null, string srcID = null, Guid? context = null)
        {
            const int ServiceDialogHeight = 557;
            const int ServiceDialogWidth = 941;

            if(environment == null)
            {
                throw new ArgumentNullException("environment");
            }

            // Silly people not checking for nulls on properties that warper other properties?! ;)
            if(environment.Connection == null)
            {
                if(!environment.IsConnected)
                {
                    environment.Connect();
                }

                // server must not be up, just do nothing ;)
                if(!environment.IsConnected)
                {
                    return false;
                }
                // else we managed to connect ;)
            }

            if(environment.Connection != null)
            {
                var workspaceID = environment.Connection.WorkspaceID;

                string pageName;
                WebsiteCallbackHandler pageHandler;
                double width;
                double height;
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
                        pageHandler = new ServiceCallbackHandler();
                        width = ServiceDialogWidth;
                        height = ServiceDialogHeight;
                        break;
                    default:
                        return false;
                }

                var envirDisplayName = FullyEncodeServerDetails(environment.Connection);
                resourcePath = HttpUtility.UrlEncode(resourcePath);
                string relativeUriString = string.Format("{0}?wid={1}&rid={2}&envir={3}&path={4}&sourceID={5}", pageName, workspaceID, resourceID, envirDisplayName, resourcePath, srcID);

                if(!IsTestMode)
                {
                    // this must be a property ;)
                    environment.ShowWebPageDialog(SiteName, relativeUriString, pageHandler, width, height);
                }
                else
                {
                    // TODO : return the relativeUriString generated ;)
                    TestModeRelativeUri = relativeUriString;
                }
            }
            return true;
        }

        #endregion

        #region ShowSaveDialog

        public static void ShowNewWorkflowSaveDialog(IContextualResourceModel resourceModel, string resourceID = null, bool addToTabManager = true)
        {
            ShowSaveDialog(resourceModel, new SaveNewWorkflowCallbackHandler(EnvironmentRepository.Instance, resourceModel), "WorkflowService");
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
                // ReSharper disable once NotResolvedInText
                throw new ArgumentNullException("resourceModel");
            }
            const string PageName = "dialogs/savedialog";
            const double Width = 604;
            const double Height = 460;
            var workspaceID = GlobalConstants.ServerWorkspaceID;

            var envirDisplayName = FullyEncodeServerDetails(environment.Connection);
            environment.ShowWebPageDialog(SiteName, string.Format("{0}?wid={1}&rid={2}&type={3}&title={4}&envir={5}", PageName, workspaceID, resourceID, type, HttpUtility.UrlEncode("New Workflow"), envirDisplayName), callbackHandler, Width, Height);
        }

        #endregion

        public static void ShowFileChooser(IEnvironmentModel environment, FileChooserMessage fileChooserMessage)
        {
            VerifyArgument.IsNotNull("environment", environment);

            const string PageName = "dialogs/filechooser";
            const double Width = 704;
            const double Height = 492;

            var pageHandler = new FileChooserCallbackHandler(fileChooserMessage);

            var envirDisplayName = FullyEncodeServerDetails(environment.Connection);
            environment.ShowWebPageDialog(SiteName, string.Format("{0}?envir={1}", PageName, envirDisplayName), pageHandler, Width, Height);
        }

        #region Encode Environment Name and Address

        public static string FullyEncodeServerDetails(IEnvironmentConnection allServerDetails)
        {
            return HttpUtility.UrlEncode(allServerDetails.DisplayName + " (" + allServerDetails.AppServerUri.ToString().Replace(".", "%2E") + ")");
        }

        #endregion
    }
}