using System.Linq;
using System.Threading;

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
        public void ClickNewServerButton()
        {
            UITestControl window = UIBusinessDesignStudioWindow.UIExplorerCustom;
            var findDocManager = window.GetChildren();
            UITestControl docManager = findDocManager.FirstOrDefault(c=>c.FriendlyName == "Explorer" &&c.ControlType.Name == "Custom");
            if(docManager != null)
            {
                var findSplitPane = docManager.GetChildren();
                UITestControl splitPane = findSplitPane.FirstOrDefault(c=>c.FriendlyName=="ConnectUserControl");
                if(splitPane != null)
                {
                    var findExplorerPane = splitPane.GetChildren();
            
                    UITestControl explorerPane = findExplorerPane.FirstOrDefault(c=>c.ControlType.Name=="Button");
                    
                    //var findNewServerButton = explorerPane.GetChildren()[0].GetChildren();
                    //UITestControl browseButton = findNewServerButton[2];
                    Mouse.Click(explorerPane, new Point(5, 5));
                }
            }
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
            Point p = new Point(theControl.BoundingRectangle.X + 60, theControl.BoundingRectangle.Y+5);
            Mouse.Click(p);
            Mouse.DoubleClick(p);
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
            int j = ddlItems.Count;
            foreach (WpfListItem item in ddlItems)
            {
                if (item.AutomationId.ToString() == "U_UI_ExplorerServerCbx_AutoID_" + serverName)
                {
                    Mouse.Click(item, new Point(5, 5));
                    break;
                }
            }
        }

        public int CountServers()
        {
            UITestControl returnControl = new UITestControl();
            WpfTree uITvExplorerTree = this.UIBusinessDesignStudioWindow.UINavigationViewUserCoCustom.UITvExplorerTree;
            return uITvExplorerTree.GetChildren().Count;
        }

        public void RightClickDeleteProject(string serverName, string serviceType, string folderName, string projectName)
        {
            UITestControl theControl = GetServiceItem(serverName, serviceType, folderName, projectName);
            Point p = new Point(theControl.BoundingRectangle.X + 50, theControl.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            System.Threading.Thread.Sleep(500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            System.Threading.Thread.Sleep(1000);
            SendKeys.SendWait("{DOWN}");
            System.Threading.Thread.Sleep(100);
            SendKeys.SendWait("{DOWN}");
            System.Threading.Thread.Sleep(100);
            SendKeys.SendWait("{DOWN}");
            System.Threading.Thread.Sleep(100);
            SendKeys.SendWait("{DOWN}");
            System.Threading.Thread.Sleep(100);
            SendKeys.SendWait("{DOWN}");
            System.Threading.Thread.Sleep(100);
            SendKeys.SendWait("{DOWN}");
            System.Threading.Thread.Sleep(100);
            SendKeys.SendWait("{DOWN}");
            System.Threading.Thread.Sleep(100);
            SendKeys.SendWait("{ENTER}");
            System.Threading.Thread.Sleep(100);
            SendKeys.SendWait("{ENTER}");
            System.Threading.Thread.Sleep(100);
            SendKeys.SendWait("{ENTER}");
            System.Threading.Thread.Sleep(100);
        }

        public bool ServiceExists(string serverName, string serviceType, string folderName, string projectName)
        {
            UITestControl theControl = GetServiceItem(serverName, serviceType, folderName, projectName);
            return theControl.Exists;
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
            System.Threading.Thread.Sleep(500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            System.Threading.Thread.Sleep(2500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            System.Threading.Thread.Sleep(2500);
            SendKeys.SendWait("{UP}");
            System.Threading.Thread.Sleep(100);
            SendKeys.SendWait("{ENTER}");
            System.Threading.Thread.Sleep(100);
        }

        public void Server_RightClick_Disconnect(string serverName)
        {
            UITestControl theServer = GetServer(serverName);
            Mouse.Move(theServer, new Point(50, 5));
            System.Threading.Thread.Sleep(500);
            Mouse.Click(MouseButtons.Right);
            System.Threading.Thread.Sleep(500);
            Keyboard.SendKeys("{Down}");
            System.Threading.Thread.Sleep(500);
            Keyboard.SendKeys("{Enter}");
            System.Threading.Thread.Sleep(500);
        }

        public void Server_RightClick_Connect(string serverName)
        {
            UITestControl theServer = GetServer(serverName);
            Point p = new Point(theServer.BoundingRectangle.X + 50, theServer.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            System.Threading.Thread.Sleep(500);
            Mouse.Move(theServer, new Point(50, 5));
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            System.Threading.Thread.Sleep(2500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            System.Threading.Thread.Sleep(2500);
            Keyboard.SendKeys("{Down}");
            System.Threading.Thread.Sleep(500);
            Keyboard.SendKeys("{Enter}");
            System.Threading.Thread.Sleep(500);
        }
        
        public void Server_RightClick_NewWorkflow(string serverName)
        {
            UITestControl theServer = GetServer(serverName);
            Point p = new Point(theServer.BoundingRectangle.X + 50, theServer.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            System.Threading.Thread.Sleep(500);
            Mouse.Move(theServer, new Point(50, 5));
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            System.Threading.Thread.Sleep(2500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            System.Threading.Thread.Sleep(2500);
            Keyboard.SendKeys("{Down}");
            System.Threading.Thread.Sleep(500);
            Keyboard.SendKeys("{Right}");
            System.Threading.Thread.Sleep(500);
            Keyboard.SendKeys("{Enter}");
        }
        public void Server_RightClick_NewDatabaseService(string serverName)
        {
            UITestControl theServer = GetServer(serverName);
            Point p = new Point(theServer.BoundingRectangle.X + 50, theServer.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            System.Threading.Thread.Sleep(500);
            Mouse.Move(theServer, new Point(50, 5));
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            System.Threading.Thread.Sleep(2500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            System.Threading.Thread.Sleep(2500);
            Keyboard.SendKeys("{Down}");
            System.Threading.Thread.Sleep(500);
            Keyboard.SendKeys("{Right}");
            System.Threading.Thread.Sleep(500);
            Keyboard.SendKeys("{Down}");
            System.Threading.Thread.Sleep(500);
            Keyboard.SendKeys("{Right}");
            System.Threading.Thread.Sleep(500);
            Keyboard.SendKeys("{Enter}");
        }
        
        public void Server_RightClick_NewPluginService(string serverName)
        {
            UITestControl theServer = GetServer(serverName);
            Point p = new Point(theServer.BoundingRectangle.X + 50, theServer.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            System.Threading.Thread.Sleep(500);
            Mouse.Move(theServer, new Point(50, 5));
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            System.Threading.Thread.Sleep(2500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            System.Threading.Thread.Sleep(2500);
            Keyboard.SendKeys("{Down}");
            System.Threading.Thread.Sleep(500);
            Keyboard.SendKeys("{Right}");
            System.Threading.Thread.Sleep(500);
            Keyboard.SendKeys("{Down}");
            System.Threading.Thread.Sleep(500);
            Keyboard.SendKeys("{Right}");
            System.Threading.Thread.Sleep(500);
            Keyboard.SendKeys("{Down}");
            System.Threading.Thread.Sleep(500);
            Keyboard.SendKeys("{Enter}");
        }

        public void Server_RightClick_Delete(string serverName)
        {
            UITestControl theServer = GetServer(serverName);
            Point p = new Point(theServer.BoundingRectangle.X + 50, theServer.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            System.Threading.Thread.Sleep(500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            System.Threading.Thread.Sleep(2500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            System.Threading.Thread.Sleep(2500);
            Keyboard.SendKeys("{Down}");
            System.Threading.Thread.Sleep(500);
            Keyboard.SendKeys("{Down}");
            System.Threading.Thread.Sleep(500);
            Keyboard.SendKeys("{Enter}");
            System.Threading.Thread.Sleep(500);
        }

        public void ConnectedServer_RightClick_Delete(string serverName)
        {
            UITestControl theServer = GetServer(serverName);
            Point p = new Point(theServer.BoundingRectangle.X + 50, theServer.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            System.Threading.Thread.Sleep(500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            System.Threading.Thread.Sleep(2500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            System.Threading.Thread.Sleep(2500);
            Keyboard.SendKeys("{Down}");
            System.Threading.Thread.Sleep(500);
            Keyboard.SendKeys("{Down}");
            System.Threading.Thread.Sleep(500);
            Keyboard.SendKeys("{Down}");
            System.Threading.Thread.Sleep(500);
            Keyboard.SendKeys("{Enter}");
            System.Threading.Thread.Sleep(500);
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
            return GetExplorerTree().GetChildren()[0].GetChildren()[3];
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

        public void DragControlToWorkflowDesigner(string serverName, string serviceType, string folderName, string projectName, Point p)
        {
            UITestControl theControl = GetServiceItem(serverName, serviceType, folderName, projectName);
            Mouse.StartDragging(theControl);
            Mouse.StopDragging(p);
        }

        public UITestControl ReturnCategory(string serverName, string serviceType, string categoryName)
        {
            return GetCategory(serverName, serviceType, categoryName);
        }
    }
}
