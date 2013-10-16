using System;
using System.Linq;
using System.Threading;
using Dev2.CodedUI.Tests.TabManagerUIMapClasses;

namespace Dev2.CodedUI.Tests.UIMaps.ExplorerUIMapClasses
{
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Input;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using MouseButtons = System.Windows.Forms.MouseButtons;


    public partial class ExplorerUIMap
    {
        public UITestControl GetConnectControl(string controlType)
        {
            var uiControl = UIBusinessDesignStudioWindow.GetChildren()
                                                        .First(c => c.FriendlyName == "UI_DocManager_AutoID" && c.ControlType.Name == "Custom")
                                                        .GetChildren()
                                                        .First(c => c.FriendlyName == "Explorer")
                                                        .GetChildren()
                                                        .SelectMany(c => c.GetChildren())
                                                        .SelectMany(c => c.GetChildren())
                                                        .FirstOrDefault(c => c.ControlType.Name == controlType);

            if(uiControl == null)
            {
                string message = string.Format("control with type name  {0} was not found on the connect control", controlType);
                throw new Exception(message);
            }

            return uiControl;
        }

        public void ClickRefreshButton()
        {
            UITestControl window = UIBusinessDesignStudioWindow;
            var findDocManager = window.GetChildren();
            UITestControl docManager = findDocManager[3];
            var findSplitPane = docManager.GetChildren();
            UITestControl splitPane = findSplitPane[3];
            var findExplorerPane = splitPane.GetChildren()[0].GetChildren()[0].GetChildren();
            UITestControl explorerPane = findExplorerPane[3].GetChildren()[6];
            var findRefreshButton = explorerPane.GetChildren()[1].GetChildren();
            UITestControl refreshButton = findRefreshButton[2];
            Mouse.Click(refreshButton, new Point(5, 5));
        }

        public void ClickConnectDropdown()
        {
            UITestControl window = UIBusinessDesignStudioWindow;
            var findDocManager = window.GetChildren();
            UITestControl docManager = findDocManager[3];
            var findSplitPane = docManager.GetChildren();
            UITestControl splitPane = findSplitPane[3];
            var findExplorerPane = splitPane.GetChildren()[0].GetChildren()[0].GetChildren();
            UITestControl explorerPane = findExplorerPane[3].GetChildren()[6];
            var findNewServerButton = explorerPane.GetChildren()[0].GetChildren();
            UITestControl connectDropdown = findNewServerButton[1];
            Mouse.Click(connectDropdown, new Point(5, 5));
        }

        public void DoubleClickOpenProject(string serverName, string serviceType, string folderName, string projectName)
        {
            UITestControl theControl = GetServiceItem(serverName, serviceType, folderName, projectName);
            Point p = new Point(theControl.BoundingRectangle.X + 60, theControl.BoundingRectangle.Y+7);
            Playback.Wait(200);
            Mouse.Click(p);
            Playback.Wait(100);
            Mouse.DoubleClick(p);
            Playback.Wait(1500);
        }

        public bool ValidateServiceExists(string serverName, string serviceType, string folderName, string projectName)
        {
            UITestControl theControl = GetServiceItem(serverName, serviceType, folderName, projectName);
            Point p = new Point(theControl.BoundingRectangle.X + 50, theControl.BoundingRectangle.Y + 5);
            Mouse.Click(p);

            var kids = theControl.GetChildren();

            if (kids != null && kids.Count > 0)
            {
                return kids.Any(kid => kid.Name == projectName);
            }

            return false;
        }

        public void RightClickDeployProject(string serverName, string serviceType, string folderName, string projectName)
        {
            UITestControl theControl = GetServiceItem(serverName, serviceType, folderName, projectName);
            Point p = new Point(theControl.BoundingRectangle.X + 50, theControl.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Thread.Sleep(2500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Thread.Sleep(2500);
            SendKeys.SendWait("{DOWN}");
            Thread.Sleep(100);
            SendKeys.SendWait("{DOWN}");
            Thread.Sleep(100);
            SendKeys.SendWait("{ENTER}");

            // Add some focus, and dleay to make sure everything is fine
            Thread.Sleep(2500);
            Mouse.Click(new Point(Screen.PrimaryScreen.WorkingArea.Width - 10, Screen.PrimaryScreen.WorkingArea.Height / 2));
            Thread.Sleep(2500);
        }

        public void ClickServerInServerDDL(string serverName)
        {
            // Get the base control
            UITestControl ddlBase = GetServerDDL();

            // Click it to expand it
            Mouse.Click(ddlBase, new Point(10, 10));

            // And get the item :D
            WpfComboBox theDDL = new WpfComboBox(ddlBase);
            UITestControlCollection ddlItems = theDDL.Items;
            foreach (WpfListItem item in ddlItems)
            {
                if (item.AutomationId.ToString() == "U_UI_ExplorerServerCbx_AutoID_" + serverName)
                {
                    Mouse.Click(item, new Point(5, 5));
                    break;
                }
            }
        }

        public string SelectedSeverName()
        {
            UITestControl ddlBase = GetServerDDL();
          
            var theDdl = new WpfComboBox(ddlBase);
            var firstItemInDdl = theDdl.Items[theDdl.SelectedIndex] as WpfListItem;

            if(firstItemInDdl == null)
            {
                string message = string.Format("No selected server name where found on the explorer server combo box");
                throw new Exception(message);
            }

            return firstItemInDdl.AutomationId.Replace("U_UI_ExplorerServerCbx_AutoID_", "");
        }

        public int CountServers()
        {
            WpfTree uITvExplorerTree = this.UIBusinessDesignStudioWindow.UIExplorerCustom.UINavigationViewUserCoCustom.UITvExplorerTree;
            return uITvExplorerTree.GetChildren().Count;
        }

        public void RightClickDeleteProject(string serverName, string serviceType, string folderName, string projectName)
        {
            UITestControl theControl = GetServiceItem(serverName, serviceType, folderName, projectName);
            Point p = new Point(theControl.BoundingRectangle.X + 50, theControl.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            Playback.Wait(500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(1000);
            SendKeys.SendWait("{DOWN}");
            Playback.Wait(100);
            SendKeys.SendWait("{DOWN}");
            Playback.Wait(100);
            SendKeys.SendWait("{DOWN}");
            Playback.Wait(100);
            SendKeys.SendWait("{DOWN}");
            Playback.Wait(100);
            SendKeys.SendWait("{ENTER}");
            Playback.Wait(100);
            var confirmationDialog = UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            var yesButton = confirmationDialog.GetChildren().FirstOrDefault(c => c.FriendlyName == "Yes");
            Mouse.Click(yesButton, new Point(10, 10));
        }

        public bool ServiceExists(string serverName, string serviceType, string folderName, string projectName)
        {
            UITestControl theControl = GetServiceItem(serverName, serviceType, folderName, projectName);
            return theControl != null && theControl.Exists;
        }

        /// <summary>
        /// Navigates to a Workflow Item in the Explorer, Right Clicks it, and clicks Properties
        /// </summary>
        /// <param name="serverName">The name of the server (EG: localhost)</param>
        /// <param name="serviceType">The name of the service (EG: WORKFLOWS)</param>
        /// <param name="folderName">The name of the folder (AKA: Category - EG: BARNEY (Or CODEDUITESTCATEGORY for the CodedUI Test Default))</param>
        /// <param name="projectName">The name of the project (EG: MyWorkflow)</param>
        public void RightClickProperties(string serverName, string serviceType, string folderName, string projectName)
        {
            UITestControl theControl = GetServiceItem(serverName, serviceType, folderName, projectName);
            Point p = new Point(theControl.BoundingRectangle.X + 50, theControl.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            Playback.Wait(500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(2500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(2500);
            SendKeys.SendWait("{UP}");
            Playback.Wait(100);
            SendKeys.SendWait("{ENTER}");
            Playback.Wait(100);
        }

        public void Server_RightClick_Disconnect(string serverName)
        {
            UITestControl theServer = GetServer(serverName);
            Mouse.Move(theServer, new Point(50, 5));
            Playback.Wait(500);
            Mouse.Click(MouseButtons.Right);
            Playback.Wait(500);
            Keyboard.SendKeys("{Down}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Enter}");
            Playback.Wait(500);
        }

        public void Server_RightClick_Connect(string serverName)
        {
            UITestControl theServer = GetServer(serverName);
            Point p = new Point(theServer.BoundingRectangle.X + 50, theServer.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            Playback.Wait(500);
            Mouse.Move(theServer, new Point(50, 5));
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(2500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(2500);
            Keyboard.SendKeys("{Down}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Enter}");
            Playback.Wait(500);
        }
        
        public void Server_RightClick_NewWorkflow(string serverName)
        {
            UITestControl theServer = GetServer(serverName);
            Point p = new Point(theServer.BoundingRectangle.X + 50, theServer.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            Playback.Wait(500);
            Mouse.Move(theServer, new Point(50, 5));
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(2500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(2500);
            Keyboard.SendKeys("{Down}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Right}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Enter}");
        }
        public void Server_RightClick_NewDatabaseService(string serverName)
        {
            UITestControl theServer = GetServer(serverName);
            Point p = new Point(theServer.BoundingRectangle.X + 50, theServer.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            Playback.Wait(500);
            Mouse.Move(theServer, new Point(50, 5));
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(2500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(2500);
            Keyboard.SendKeys("{Down}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Right}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Down}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Right}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Enter}");
        }
        
        public void Server_RightClick_NewPluginService(string serverName)
        {
            UITestControl theServer = GetServer(serverName);
            Point p = new Point(theServer.BoundingRectangle.X + 50, theServer.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            Playback.Wait(500);
            Mouse.Move(theServer, new Point(50, 5));
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(2500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(2500);
            Keyboard.SendKeys("{Down}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Right}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Down}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Right}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Down}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Enter}");
        }

        public void Server_RightClick_Delete(string serverName)
        {
            UITestControl theServer = GetServer(serverName);
            Point p = new Point(theServer.BoundingRectangle.X + 50, theServer.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            Playback.Wait(500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(2500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(2500);
            Keyboard.SendKeys("{Down}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Down}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Enter}");
            Playback.Wait(500);
        }

        public void ConnectedServer_RightClick_Delete(string serverName)
        {
            UITestControl theServer = GetServer(serverName);
            Point p = new Point(theServer.BoundingRectangle.X + 50, theServer.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            Playback.Wait(500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(2500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(2500);
            Keyboard.SendKeys("{Down}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Down}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Down}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Enter}");
            Playback.Wait(500);
        }

        public void RightClickShowProjectDependancies(string serverName, string serviceType, string folderName, string projectName)
        {
            UITestControl theControl = GetServiceItem(serverName, serviceType, folderName, projectName);
            Point p = new Point(theControl.BoundingRectangle.X + 50, theControl.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            Thread.Sleep(500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Thread.Sleep(500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Thread.Sleep(500);
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Enter}");
        }

        public void RightClickHelp(string serverName, string serviceType, string folderName, string projectName)
        {
            UITestControl theControl = GetServiceItem(serverName, serviceType, folderName, projectName);
            Point p = new Point(theControl.BoundingRectangle.X + 50, theControl.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            Thread.Sleep(500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Thread.Sleep(500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Thread.Sleep(500);
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Enter}");
        }

        public UITestControl GetLocalServer()
        {
            var explorerTree = GetExplorerTree();
            var firstTreeChild = explorerTree.GetChildren()[0];
            return firstTreeChild.GetChildren()[3];
        }

        /// <summary>
        /// Gets the Service as a UITestControl object
        /// </summary>
        /// <param name="serverName">The name of the server (EG: localhost)</param>
        /// <param name="serviceType">The name of the service (EG: WORKFLOWS)</param>
        /// <param name="folderName">The name of the folder (AKA: Category - EG: BARNEY (Or CODEDUITESTCATEGORY for the CodedUI Test Default))</param>
        /// <param name="projectName">The name of the project (EG: MyWorkflow)</param>
        /// <returns>A UITestControl object</returns>
        public UITestControl GetService(string serverName, string serviceType, string folderName, string serviceName)
        {
            UITestControl service = GetServiceItem(serverName, serviceType, folderName, serviceName);
            return service;
        }

        public UITestControl GetServer(string serverName)
        {
            return GetConnectedServer(serverName);
        }

        public UITestControl GetServiceType(string serverName, string serviceType)
        {
            return GetServiceTypeControl(serverName, serviceType);
        }

        public void DragControlToWorkflowDesigner(UITestControl theControl, Point p)
        {
            Mouse.StartDragging(theControl, MouseButtons.Left);
            Mouse.StopDragging(p);
        }

        public void DragControlToWorkflowDesigner(string serverName, string serviceType, string folderName, string projectName, Point p, bool overrideDblClickBehavior = false)
        {
            UITestControl theControl = GetServiceItem(serverName, serviceType, folderName, projectName,overrideDblClickBehavior);
            Mouse.StartDragging(theControl);
            Mouse.StopDragging(p);
        }

        public UITestControl ReturnCategory(string serverName, string serviceType, string categoryName)
        {
            return GetCategory(serverName, serviceType, categoryName);
        }

        public void RightClickRenameProject(string serverName, string serviceType, string folderName, string projectName)
        {
            UITestControl theControl = GetServiceItem(serverName, serviceType, folderName, projectName);
            Point p = new Point(theControl.BoundingRectangle.X + 50, theControl.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Thread.Sleep(2500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Thread.Sleep(2500);
            SendKeys.SendWait("{DOWN}");
            Thread.Sleep(100);
            SendKeys.SendWait("{DOWN}");
            Thread.Sleep(100);
            SendKeys.SendWait("{DOWN}");
            Thread.Sleep(100);
            SendKeys.SendWait("{ENTER}");
        }

        public static void ClosePane(UITestControl theTab)
        {
            //step into tab to find the far right coordinate
            UITestControl findContentPane = null;
            foreach (var child in theTab.GetChildren())
            {
                if (child.ClassName == "Uia.ContentPane")
                {
                    findContentPane = child;
                    break;
                }
            }
            if (findContentPane != null)
            {
                var tabFarRightCoord = findContentPane.BoundingRectangle.Right;
                var tabYCoord = findContentPane.Top;
                //click relative to the right side of the tab (should be well clear of the explorer tab)
                Mouse.Click(new Point(tabFarRightCoord - 100, tabYCoord + 500));
                Playback.Wait(2500);
            }
        }

        public void ClickNewServerButton()
        {
            var firstConnectControlButton = GetConnectControl("Button");
            var nextConnectControlButtonPosition = new Point(firstConnectControlButton.Left + 35, firstConnectControlButton.Top + 10);
            Mouse.Click(nextConnectControlButtonPosition);
        }
    }
}
