using System.Collections.Generic;
using Dev2.Studio.UI.Tests.Extensions;
using Microsoft.VisualStudio.TestTools.UITesting;
using System;

namespace Dev2.Studio.UI.Tests.UIMaps
{
    public class PopupDialogUiMap : UIMapBase
    {
        #region AutomationId
        private const string DataListSearchTextBoxId = "UI_DataListSearchtxt_AutoID";
        private const string RefreshButtonId = "UI_SourceServerRefreshbtn_AutoID";
        #endregion

        public UITestControl GetPopupWindow()
        {
            return StudioWindow.GetChildren()[0];
        }

        /// <summary>
        /// Returns true if found in the timeout period.
        /// </summary>
        public void WaitForDialog()
        {
            Playback.Wait(2500);
        }

        public void AddAResource(string serverName, string serviceType, string folderName, string projectName)
        {
            UITestControl addResourcePopupWindow = GetPopupWindow();
            var searchResourceTextBox = addResourcePopupWindow.FindByAutomationId(DataListSearchTextBoxId);
            searchResourceTextBox.EnterText(projectName);
            DoubleClickOpenProject(serverName, serviceType, folderName, projectName);
        }

        public void DoubleClickOpenProject(string serverName, string serviceType, string folderName, string projectName)
        {
            UITestControl control = GetServiceItem(serverName, serviceType, folderName, projectName);
            if (control != null)
            {
                control.DoubleClick();
            }
            else
            {
                var message = string.Format("service with name : [{0}], could not be located", projectName);
                throw new Exception(message);
            }
        }

        /// <summary>
        /// ClickExplorer
        /// </summary>
        /// <param name="serverName">Name of the server.</param>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="folderName">Name of the folder.</param>
        /// <param name="projectName">Name of the project.</param>
        /// <returns></returns>
        private UITestControl GetServiceItem(string serverName, string serviceType, string folderName, string projectName)
        {
            Playback.Wait(200);

            var args = new List<string> { serverName, serviceType, folderName, projectName };
            var parent = StudioWindow.GetChildren()[0].FindByAutomationId("Navigation");

            foreach (var arg in args)
            {
                if (parent != null)
                {
                    var kids = parent.GetChildren();

                    UITestControl canidate = null;
                    foreach (var kid in kids)
                    {
                        if (kid.Exists)
                        {
                            var id = kid.GetProperty("AutomationID").ToString();

                            if (id.ToUpper().Contains(arg.ToUpper()) ||
                                kid.FriendlyName.ToUpper().Contains(arg.ToUpper()) ||
                                kid.ControlType.Name.ToUpper().Contains(arg.ToUpper()) ||
                                kid.ClassName.ToUpper().Contains(arg.ToUpper()))
                            {
                                canidate = kid;
                            }
                        }
                    }

                    parent = canidate;
                }
            }

            return parent;

        }
    }
}
