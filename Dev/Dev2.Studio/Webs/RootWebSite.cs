
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Web;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Webs.Callbacks;

namespace Dev2.Webs
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
            const int DialogHeight = 85;

            var callBackHandler = new Dev2DecisionCallbackHandler { ModelData = webModel };
            const string RelativeUriString = "switch/drag";
            if(!IsTestMode)
            {
                environment.ShowWebPageDialog(SiteName, RelativeUriString, callBackHandler, DialogWidth, DialogHeight, "Switch Flow");
            }
            else
            {
                TestModeRelativeUri = RelativeUriString;
            }
            return callBackHandler;
        }

        #endregion

        #region ShowSwitchDropDialog

        public static Dev2DecisionCallbackHandler ShowSwitchDropDialog(IEnvironmentModel environment, string webModel)
        {
            const int DialogWidth = 752;
            const int DialogHeight = 161;

            var callBackHandler = new Dev2DecisionCallbackHandler { ModelData = webModel };
            const string RelativeUriString = "switch/drop";
            if(!IsTestMode)
            {
                environment.ShowWebPageDialog(SiteName, RelativeUriString, callBackHandler, DialogWidth, DialogHeight, "Switch Flow");
            }
            else
            {
                TestModeRelativeUri = RelativeUriString;
            }
            return callBackHandler;
        }

        #endregion

        #region ShowDecisionDialog

        public static Dev2DecisionCallbackHandler ShowDecisionDialog(IEnvironmentModel environment, string webModel)
        {
            const int DialogWidth = 824;
            const int DialogHeight = 510;

            var callBackHandler = new Dev2DecisionCallbackHandler { ModelData = webModel };
            const string RelativeUriString = "decisions/wizard";
            if(!IsTestMode)
            {
                environment.ShowWebPageDialog(SiteName, RelativeUriString, callBackHandler, DialogWidth, DialogHeight, "Switch Flow");
            }
            else
            {
                TestModeRelativeUri = RelativeUriString;
            }
            return callBackHandler;
        }

        #endregion

        #region ShowDialog(IContextualResourceModel resourceModel)

        //
        // TWR: 2013.02.14 - refactored to accommodate new requests
        // PBI: 801
        //
        public static bool ShowDialog(IContextualResourceModel resourceModel)
        {
            if(resourceModel == null)
            {
                return false;
            }

            string resourceId = resourceModel.ID.ToString();

            ResourceType resourceType;
            Enum.TryParse(resourceModel.ServerResourceType, out resourceType);

            if(resourceType == ResourceType.WorkflowService && resourceModel.ID == Guid.Empty)
            {
                ShowNewWorkflowSaveDialog(resourceModel);
            }
            else
            {
                // we need to take the SourceID out and pass along ;)
                var srcId = Guid.Empty.ToString();
                if(resourceModel.WorkflowXaml != null)
                {
                    var workflowXaml = resourceModel.WorkflowXaml;

                    try
                    {
                        var xe = workflowXaml.Replace("&", "&amp;").ToXElement();
                        srcId = xe.AttributeSafe("SourceID");
                    }
                    catch
                    {
                        srcId = workflowXaml.ExtractXmlAttributeFromUnsafeXml("SourceID=\"");
                    }
                }

                return ShowDialog(resourceModel.Environment, resourceType, null, resourceModel.Category, resourceId, srcId, resourceModel.ResourceName);
            }
            return true;
        }

        #endregion

        #region ShowDialog(IEnvironmentModel environment, ResourceType resourceType, string resourceID = null)

        public static bool ShowDialog(IEnvironmentModel environment, ResourceType resourceType, string resourcePath, string cateogy, string resourceId = null, string srcId = null, string resourceName = null)
        {
            const int ServiceDialogHeight = 582;
            const int ServiceDialogWidth = 941;
            bool? isSuccessful = true;

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
                var workspaceId = GlobalConstants.ServerWorkspaceID;

                string pageName;
                WebsiteCallbackHandler pageHandler;
                double width;
                double height;
                string leftTitle = string.Empty;
                string rightTitle = environment.Name + " (" + environment.Connection.AppServerUri + ")";
                switch(resourceType)
                {
                    case ResourceType.Server:
                        pageName = "sources/server";
                        pageHandler = new ConnectCallbackHandler();
                        if(!String.IsNullOrEmpty(resourceId) && !String.IsNullOrEmpty(resourceName))
                        {
                            leftTitle = "Edit - " + resourceName;
                        }
                        else
                        {
                            leftTitle = "New Server";
                        }

                        width = 704;
                        height = 520;
                        break;

                    case ResourceType.ServerSource:
                        pageName = "sources/server";
                        pageHandler = new ConnectCallbackHandler();
                        if(!String.IsNullOrEmpty(resourceId) && !String.IsNullOrEmpty(resourceName))
                        {
                            leftTitle = "Edit - " + resourceName;
                        }
                        else
                        {
                            leftTitle = "New Server";
                        }

                        width = 704;
                        height = 520;
                        break;

                    case ResourceType.DbService:
                        pageName = "services/dbservice";
                        pageHandler = new DbServiceCallbackHandler();
                        width = ServiceDialogWidth;
                        height = ServiceDialogHeight;
                        break;

                    case ResourceType.DbSource:
                        pageName = "sources/dbsource";
                        srcId = resourceId;
                        pageHandler = new SourceCallbackHandler();
                        width = 704;
                        height = 517;
                        if(!String.IsNullOrEmpty(resourceId) && !String.IsNullOrEmpty(resourceName))
                        {
                            leftTitle = "Edit - " + resourceName;
                        }
                        else
                        {
                            leftTitle = "New Datbase Source";
                        }
                        break;

                    case ResourceType.PluginService:
                        pageName = "services/pluginservice";
                        pageHandler = new ServiceCallbackHandler();
                        width = ServiceDialogWidth;
                        height = ServiceDialogHeight;
                        break;

                    case ResourceType.PluginSource:
                        pageName = "sources/pluginsource";
                        srcId = resourceId;
                        pageHandler = new SourceCallbackHandler();
                        if(!String.IsNullOrEmpty(resourceId) && !String.IsNullOrEmpty(resourceName))
                        {
                            leftTitle = "Edit - " + resourceName;
                        }
                        else
                        {
                            leftTitle = "New Plugin Source";
                        }
                        width = 700;
                        height = 517;
                        break;

                    case ResourceType.EmailSource:  // PBI 953 - 2013.05.16 - TWR - Added
                        pageName = "sources/emailsource";
                        srcId = resourceId;
                        pageHandler = new SourceCallbackHandler();
                        if(!String.IsNullOrEmpty(resourceId) && !String.IsNullOrEmpty(resourceName))
                        {
                            leftTitle = "Edit - " + resourceName;
                        }
                        else
                        {
                            leftTitle = "New Email Source";
                        }
                        width = 704;
                        height = 488;
                        break;

                    case ResourceType.WebSource:    // PBI 5656 - 2013.05.20 - TWR - Added
                        pageName = "sources/websource";
                        srcId = resourceId;
                        pageHandler = new WebSourceCallbackHandler();
                        if(!String.IsNullOrEmpty(resourceId) && !String.IsNullOrEmpty(resourceName))
                        {
                            leftTitle = "Edit - " + resourceName;
                        }
                        else
                        {
                            leftTitle = "New Web Source";
                        }
                        width = 704;
                        height = 517;
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

                string selectedPath = "";
                if(cateogy != null)
                {
                    selectedPath = cateogy.Equals("Unassigned") || string.IsNullOrEmpty(cateogy) ? "" : cateogy;
                    var lastIndexOf = selectedPath.LastIndexOf("\\", StringComparison.Ordinal);
                    if(lastIndexOf != -1)
                    {
                        selectedPath = selectedPath.Substring(0, lastIndexOf);
                    }
                    selectedPath = selectedPath.Replace("\\", "\\\\");
                }


                string relativeUriString = string.Format("{0}?wid={1}&rid={2}&envir={3}&path={4}&sourceID={5}&category={6}", pageName, workspaceId, resourceId, envirDisplayName, resourcePath, srcId, selectedPath);

                if(!IsTestMode)
                {
                    // this must be a property ;)
                    isSuccessful = environment.ShowWebPageDialog(SiteName, relativeUriString, pageHandler, width, height, leftTitle, rightTitle);
                }
                else
                {
                    TestModeRelativeUri = relativeUriString;
                }
            }
            return isSuccessful.HasValue && isSuccessful.Value;
        }

        #endregion

        #region ShowSaveDialog

        public static void ShowNewWorkflowSaveDialog(IContextualResourceModel resourceModel, string resourceId = null, bool addToTabManager = true)
        {
            ShowSaveDialog(resourceModel, new SaveNewWorkflowCallbackHandler(EventPublishers.Aggregator, EnvironmentRepository.Instance, resourceModel, addToTabManager), "WorkflowService", HttpUtility.UrlEncode("New Workflow"));
        }

        public static void ShowNewOAuthsourceSaveDialog(IContextualResourceModel resourceModel, IEnvironmentModel model, string token, string key)
        {
            ShowSaveDialog(resourceModel, new DropBoxSourceSourceCallbackHandler(EnvironmentRepository.Instance,token??"",key??""), "OauthSource", HttpUtility.UrlEncode("New Dropbox source"));
        }

        static void ShowSaveDialog(IContextualResourceModel resourceModel, WebsiteCallbackHandler callbackHandler, string type, string title, string resourceId = null)
        {
            if(resourceModel == null)
            {
                throw new ArgumentNullException("resourceModel");
            }
            IEnvironmentModel environment = resourceModel.Environment;

            if(environment == null)
            {
                // ReSharper disable NotResolvedInText
                throw new ArgumentNullException("environment");
            }

            EnvironmentRepository.Instance.ActiveEnvironment = environment;

            const string PageName = "dialogs/savedialog";
            const double Width = 604;
            const double Height = 450;
            var workspaceId = GlobalConstants.ServerWorkspaceID;
            const string LeftTitle = "Save";
            string rightTitle = environment.Name + " (" + environment.Connection.AppServerUri + ")";
            var envirDisplayName = FullyEncodeServerDetails(environment.Connection);
            if (resourceModel.Category == null)
            {
                resourceModel.Category = "";
            }
            var selectedPath = resourceModel.Category.Contains("Unassigned") || string.IsNullOrEmpty(resourceModel.Category) ? "" : resourceModel.Category;
            var lastIndexOf = selectedPath.LastIndexOf("\\", StringComparison.Ordinal);
            if(lastIndexOf != -1)
            {
                selectedPath = selectedPath.Substring(0, lastIndexOf);
            }
            selectedPath = selectedPath.Replace("\\", "\\\\");
            var relativeUriString = string.Format("{0}?wid={1}&rid={2}&type={3}&title={4}&envir={5}&category={6}", PageName, workspaceId, resourceId, type, title, envirDisplayName, selectedPath);
            if(!IsTestMode)
            {
                // this must be a property ;)
                environment.ShowWebPageDialog(SiteName, relativeUriString, callbackHandler, Width, Height, LeftTitle, rightTitle);
            }
            else
            {
                // TODO : return the relativeUriString generated ;)
                CallBackHandler = callbackHandler;
                TestModeRelativeUri = relativeUriString;
            }
        }

        public static WebsiteCallbackHandler CallBackHandler { get; set; }

        #endregion

        public static void ShowFileChooser(IEnvironmentModel environment, FileChooserMessage fileChooserMessage)
        {
            VerifyArgument.IsNotNull("environment", environment);

            const string PageName = "dialogs/filechooser";
            const double Width = 704;
            const double Height = 517;
            const string LeftTitle = "Choose File(s)";
            var environmentConnection = environment.Connection;
            if(environmentConnection != null)
            {
                string rightTitle = environment.Name + " (" + environmentConnection.AppServerUri + ")";

            var pageHandler = new FileChooserCallbackHandler(fileChooserMessage);

                var envirDisplayName = FullyEncodeServerDetails(environmentConnection);
                var relativeUriString = string.Format("{0}?envir={1}", PageName, envirDisplayName);
                if(!IsTestMode)
                {
                    environment.ShowWebPageDialog(SiteName, relativeUriString, pageHandler, Width, Height, LeftTitle, rightTitle);
                }
                else
                {
                    CallBackHandler = pageHandler;
                    TestModeRelativeUri = relativeUriString;
                }
            }
        }

        #region Encode Environment Name and Address

        public static string FullyEncodeServerDetails(IEnvironmentConnection allServerDetails)
        {
            return HttpUtility.UrlEncode(allServerDetails.DisplayName + " (" + allServerDetails.AppServerUri.ToString().Replace(".", "%2E") + ")");
        }

        #endregion
    }
}
