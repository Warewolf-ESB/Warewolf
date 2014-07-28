using System;
using System.Linq;
using System.Threading;
using Dev2.Studio.UI.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UITest.Extension;

namespace Dev2.CodedUI.Tests.UIMaps.ExplorerUIMapClasses
{
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Input;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using MouseButtons = System.Windows.Forms.MouseButtons;

    // ReSharper disable InconsistentNaming
    public partial class ExplorerUIMap
    {
        private UITestControl _explorerTree;
        private UITestControl _explorerSearch;
        private UITestControl _explorerNewConnectionControl;
        UITestControl _explorerRefresh;

        public ExplorerUIMap()
        {
            //Ashley: Ideally these should have no dynamic IDs:
            GetExplorerTree();
            try
            {
                _explorerSearch = VisualTreeWalker.GetControl(
                    "UI_DocManager_AutoID",
                    "Z91e998e0ee8c4f069cdab9d2cd3f544e",
                    "Z306a8df47193448e9dd2228d905d49cd",
                    "UI_ExplorerPane_AutoID",
                    "Uia.ExplorerView",
                    "TheNavigationView",
                    "FilterTextBox",
                    "UI_DataListSearchtxt_AutoID");
            }
            catch(UITestControlNotFoundException)
            //_explorerSearch = VisualTreeWalker.GetControlFromRoot(0, false, 0, "Uia.SplitPane", "Z306a8df47193448e9dd2228d905d49cd", "UI_ExplorerPane_AutoID", "Explorer", "TheNavigationView", "FilterTextBox", "UI_DataListSearchtxt_AutoID");
            //if(_explorerSearch == null)
            {
                _explorerSearch = VisualTreeWalker.GetControl(
                    "UI_DocManager_AutoID",
                    "Zc30a7af8e0c54bb5bccfbea116f8ab0d",
                    "Zf1166e575b5d43bb89f15f346eccb7b1",
                    "Z3d0e8544bdbd4fbc8b0369ecfce4e928",
                    "Explorer",
                    "UI_ExplorerPane_AutoID",
                    "Uia.ExplorerView",
                    "TheNavigationView",
                    "FilterTextBox",
                    "UI_DataListSearchtxt_AutoID");
                //    _explorerSearch = VisualTreeWalker.GetControlFromRoot(0, false, 1, "Uia.SplitPane", "Zf1166e575b5d43bb89f15f346eccb7b1", "Z3d0e8544bdbd4fbc8b0369ecfce4e928", "Explorer", "UI_ExplorerPane_AutoID", "Explorer", "TheNavigationView", "FilterTextBox", "UI_DataListSearchtxt_AutoID");
            }
            try
            {
                _explorerNewConnectionControl = VisualTreeWalker.GetControl(
                    "UI_DocManager_AutoID",
                    "Z91e998e0ee8c4f069cdab9d2cd3f544e",
                    "Z306a8df47193448e9dd2228d905d49cd",
                    "UI_ExplorerPane_AutoID",
                    "Uia.ExplorerView",
                    "ConnectUserControl");
            }
            catch(UITestControlNotFoundException)
            //_explorerNewConnectionControl = VisualTreeWalker.GetControlFromRoot(0, false, 0, "Uia.SplitPane", "Z306a8df47193448e9dd2228d905d49cd", "UI_ExplorerPane_AutoID", "Explorer", "ConnectUserControl");
            //if(_explorerNewConnectionControl == null)
            {
                _explorerNewConnectionControl = VisualTreeWalker.GetControl(
                    "UI_DocManager_AutoID",
                    "Zc30a7af8e0c54bb5bccfbea116f8ab0d",
                    "Zf1166e575b5d43bb89f15f346eccb7b1",
                    "Z3d0e8544bdbd4fbc8b0369ecfce4e928",
                    "Explorer",
                    "UI_ExplorerPane_AutoID",
                    "Uia.ExplorerView",
                    "ConnectUserControl");
                //_explorerNewConnectionControl = VisualTreeWalker.GetControlFromRoot(0, false, 1, "Uia.SplitPane", "Zf1166e575b5d43bb89f15f346eccb7b1", "Z3d0e8544bdbd4fbc8b0369ecfce4e928", "Explorer", "UI_ExplorerPane_AutoID", "Explorer", "ConnectUserControl");
            }
            try
            {
                _explorerRefresh = VisualTreeWalker.GetControl(
                    "UI_DocManager_AutoID",
                    "Z91e998e0ee8c4f069cdab9d2cd3f544e",
                    "Z306a8df47193448e9dd2228d905d49cd",
                    "UI_ExplorerPane_AutoID",
                    "Uia.ExplorerView",
                    "TheNavigationView",
                    "UI_SourceServerRefreshbtn_AutoID");
            }
            catch(UITestControlNotFoundException)
            //_explorerRefresh = VisualTreeWalker.GetControlFromRoot(0, false, 0, "Uia.SplitPane", "Z306a8df47193448e9dd2228d905d49cd", "UI_ExplorerPane_AutoID", "Explorer", "TheNavigationView", "UI_SourceServerRefreshbtn_AutoID");
            //if(_explorerRefresh == null)
            {
                _explorerRefresh = VisualTreeWalker.GetControl(
                    "UI_DocManager_AutoID",
                    "Zc30a7af8e0c54bb5bccfbea116f8ab0d",
                    "Zf1166e575b5d43bb89f15f346eccb7b1",
                    "Z3d0e8544bdbd4fbc8b0369ecfce4e928",
                    "Explorer",
                    "UI_ExplorerPane_AutoID",
                    "Uia.ExplorerView",
                    "TheNavigationView",
                    "UI_SourceServerRefreshbtn_AutoID");
                //_explorerRefresh = VisualTreeWalker.GetControlFromRoot(0, false, 1, "Uia.SplitPane", "Zf1166e575b5d43bb89f15f346eccb7b1", "Z3d0e8544bdbd4fbc8b0369ecfce4e928", "Explorer", "UI_ExplorerPane_AutoID", "Explorer", "TheNavigationView", "UI_SourceServerRefreshbtn_AutoID");
            }
        }

        public UITestControl ExplorerRefresh
        {
            get
            {
                return _explorerRefresh;
            }
        }

        private void GetExplorerTree()
        {
            try
            {
                _explorerTree = VisualTreeWalker.GetControl(
                    "UI_DocManager_AutoID",
                    "Z91e998e0ee8c4f069cdab9d2cd3f544e",
                    "Z306a8df47193448e9dd2228d905d49cd",
                    "UI_ExplorerPane_AutoID",
                    "Uia.ExplorerView",
                    "TheNavigationView",
                    "Navigation");
            }
            catch(UITestControlNotFoundException)
            //_explorerTree = VisualTreeWalker.GetControlFromRoot(0, false, 0, "Uia.SplitPane", "Z306a8df47193448e9dd2228d905d49cd", "UI_ExplorerPane_AutoID", "Explorer", "TheNavigationView", "Navigation");
            //if(_explorerTree == null)
            {
                _explorerTree = VisualTreeWalker.GetControl(
                    "UI_DocManager_AutoID",
                    "Zc30a7af8e0c54bb5bccfbea116f8ab0d",
                    "Zf1166e575b5d43bb89f15f346eccb7b1",
                    "Z3d0e8544bdbd4fbc8b0369ecfce4e928",
                    "Explorer",
                    "UI_ExplorerPane_AutoID",
                    "Uia.ExplorerView",
                    "TheNavigationView",
                    "Navigation");
                //_explorerTree = VisualTreeWalker.GetControlFromRoot(0, false, 1, "Uia.SplitPane", "Zf1166e575b5d43bb89f15f346eccb7b1", "Z3d0e8544bdbd4fbc8b0369ecfce4e928", "Explorer", "UI_ExplorerPane_AutoID", "Explorer", "TheNavigationView", "Navigation");
            }
        }

        public UITestControlCollection GetCategoryItems()
        {
            return GetNavigationItemCategories();
        }

        /// <summary>
        /// Gets the navigation item categories.
        /// </summary>
        /// <returns></returns>
        public UITestControlCollection GetNavigationItemCategories()
        {
            UITestControlCollection categories = _explorerTree.GetChildren();

            UITestControlCollection categoryCollection = new UITestControlCollection();

            foreach(UITestControl category in categories)
            {
                if(category.ControlType.ToString() == "TreeItem")
                {
                    categoryCollection.Add(category);
                }
            }

            return categoryCollection;
        }

        /// <summary>
        /// Gets the navigation items.
        /// </summary>
        /// <returns></returns>
        public UITestControlCollection GetServiceItems()
        {
            UITestControlCollection categories = _explorerTree.GetChildren();

            UITestControlCollection categoryCollection = new UITestControlCollection();

            foreach(UITestControl category in categories)
            {
                if(category.ControlType.ToString() == "TreeItem")
                {
                    var kids = category.GetChildren();
                    foreach(var kid in kids)
                    {
                        if(kid.ControlType.ToString() == "TreeItem")
                        {
                            categoryCollection.Add(kid);
                        }
                    }

                }
            }

            return categoryCollection;
        }


        public UITestControl GetConnectControl(string controlType)
        {
            var kids = _explorerNewConnectionControl.GetChildren();

            if(kids != null)
            {
                return kids.FirstOrDefault(c => c.ControlType.Name == controlType);
            }

            return null;
        }

        public UITestControl GetConnectControl(string controlType, string friendlyName)
        {
            var kids = _explorerNewConnectionControl.GetChildren();

            if(kids != null)
            {
                return kids.FirstOrDefault(c => c.ControlType.Name == controlType && c.FriendlyName == friendlyName);
            }

            return null;
        }

        public UITestControl GetLocalServer()
        {
            var firstTreeChild = _explorerTree.GetChildren()[0];
            return firstTreeChild.GetChildren().FirstOrDefault(c => c.ControlType == ControlType.Text);
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

        public void DoubleClickOpenProject(string serverName, string folderName, string projectName, int waitAmt = 0)
        {
            UITestControl theControl = null;
            if(folderName != "Unassigned" && folderName != string.Empty)
            {
                theControl = VisualTreeWalker.GetChildByAutomationIDPath(_explorerTree, serverName, folderName, projectName);
            }
            else
            {
                theControl = VisualTreeWalker.GetChildByAutomationIDPath(_explorerTree, serverName, projectName);
            }
            if(theControl != null)
            {
                Point p = new Point(theControl.BoundingRectangle.X + 60, theControl.BoundingRectangle.Y + 7);
                Playback.Wait(50);
                Mouse.Click(p);
                Playback.Wait(150);
                try
                {
                    Mouse.DoubleClick(p);
                    // ReSharper disable EmptyGeneralCatchClause
                }
                catch
                // ReSharper restore EmptyGeneralCatchClause
                {
                    // this darn double click keeps bombing out ;)
                }
                Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.AllThreads;
                theControl.WaitForControlReady();
                Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.UIThreadOnly;
                Playback.Wait(waitAmt);
            }
        }

        public bool ValidateServiceExists(string serviceName, string folderName, string serverName = "localhost")
        {
            return ValidateResourceExists(serviceName, folderName, serverName);
        }

        public bool ValidateSourceExists(string sourceName, string folderName, string serverName = "localhost")
        {
            return ValidateResourceExists(sourceName, folderName, serverName);
        }

        public bool ValidateWorkflowExists(string workflowName, string folderName, string serverName = "localhost")
        {
            return ValidateResourceExists(workflowName, folderName, serverName);
        }

        public bool ValidateHasResource(string workflowName, string serverName = "localhost")
        {
            return ValidateResourceExists(workflowName, serverName);
        }

        public void RightClickDeployProject(string folderName, string projectName, string serverName = "localhost")
        {
            UITestControl theControl = null;
            if(folderName != "Unassigned" && folderName != string.Empty)
            {
                theControl = VisualTreeWalker.GetChildByAutomationIDPath(_explorerTree, serverName, folderName, projectName);
            }
            else
            {
                theControl = VisualTreeWalker.GetChildByAutomationIDPath(_explorerTree, serverName, projectName);
            }
            Point p = new Point(theControl.BoundingRectangle.X + 50, theControl.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Thread.Sleep(1500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Thread.Sleep(1500);
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

        /// <summary>
        /// Clicks the server information server DDL.
        /// </summary>
        /// <param name="serverName">Name of the server.</param>
        public void ClickServerInServerDDL(string serverName, bool ingoredEnabled = true)
        {
            // Get the base control
            UITestControl ddlBase = GetServerDDL();

            //Wait for the connect control to be ready
            int counter = 0;
            while(!ddlBase.Enabled && counter < 30)
            {
                Playback.Wait(2000);
                counter++;
            }

            if(!ddlBase.Enabled)
            {
                throw new Exception("The connect control drop down is still disabled after 1 minute wait.");
            }

            // Click it to expand it
            Mouse.Click(ddlBase, new Point(10, 10));
            Playback.Wait(500);

            var item = VisualTreeWalker.GetChildByAutomationIDPath(ddlBase, "U_UI_ExplorerServerCbx_AutoID_" + serverName);

            Mouse.Click(item, new Point(5, 5));

            if(ingoredEnabled)
            {
                //Wait for the connect control to be ready
                int afterCounter = 0;
                while(!ddlBase.Enabled && afterCounter < 10)
                {
                    Playback.Wait(2000);
                    afterCounter++;
                }
                if(!ddlBase.Enabled)
                {
                    throw new Exception("The connect control drop down is still disabled after 10 sec wait.");
                }
            }
        }

        /// <summary>
        /// Selecteds the name of the sever.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public string GetSelectedSeverName()
        {
            UITestControl ddlBase = GetServerDDL();

            int counter = 0;
            while(!ddlBase.Enabled && counter < 15)
            {
                Playback.Wait(2000);
                counter++;
            }
            if(!ddlBase.Enabled)
            {
                throw new Exception("The connect control drop down is still disabled after 30 sec wait.");
            }

            var theDdl = ddlBase as WpfComboBox;
            if(theDdl != null)
            {
                var firstItemInDdl = theDdl.Items[theDdl.SelectedIndex] as WpfListItem;

                if(firstItemInDdl == null)
                {
                    string message = string.Format("No selected server name where found on the explorer server combo box");
                    throw new Exception(message);
                }

                return firstItemInDdl.AutomationId.Replace("U_UI_ExplorerServerCbx_AutoID_", "");
            }
            return string.Empty;
        }

        /// <summary>
        /// Counts the servers.
        /// </summary>
        /// <returns></returns>
        public int CountServers()
        {
            WpfTree uITvExplorerTree = UIBusinessDesignStudioWindow.UIExplorerCustom.UINavigationViewUserCoCustom.UITvExplorerTree;
            return uITvExplorerTree.GetChildren().Count;
        }

        public void RightClickDeleteResource(string resourceName, string categoryName, string serverName)
        {
            EnterExplorerSearchText(resourceName);
            UITestControl theControl = null;
            if(categoryName != "Unassigned" && categoryName != string.Empty)
            {
                theControl = VisualTreeWalker.GetChildByAutomationIDPath(_explorerTree, serverName, categoryName, resourceName);
            }
            else
            {
                theControl = VisualTreeWalker.GetChildByAutomationIDPath(_explorerTree, serverName, resourceName);
            }
            Point p = new Point(theControl.BoundingRectangle.X + 100, theControl.BoundingRectangle.Y + 5);
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
            PopupDialogUIMap.WaitForDialog();
            Playback.Wait(100);
            var confirmationDialog = UIBusinessDesignStudioWindow.GetChildren()[0];
            var yesButton = confirmationDialog.GetChildren().FirstOrDefault(c => c.FriendlyName == "Yes");
            Mouse.Click(yesButton, new Point(10, 10));
        }

        public void RightClickRemoveResource(string resourceName, string categoryName, string serverName)
        {
            EnterExplorerSearchText(resourceName);
            UITestControl theControl = GetServiceItem(serverName, categoryName, resourceName);
            Point p = new Point(theControl.BoundingRectangle.X + 100, theControl.BoundingRectangle.Y + 5);
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
            SendKeys.SendWait("{ENTER}");
            PopupDialogUIMap.WaitForDialog();
            Playback.Wait(100);
            var confirmationDialog = UIBusinessDesignStudioWindow.GetChildren()[0];
            var yesButton = confirmationDialog.GetChildren().FirstOrDefault(c => c.FriendlyName == "Yes");
            Mouse.Click(yesButton, new Point(10, 10));
        }


        public bool ServiceExists(string serverName, string folderName, string projectName)
        {
            UITestControl theControl = null;
            if(folderName != "Unassigned" && folderName != string.Empty)
            {
                theControl = VisualTreeWalker.GetChildByAutomationIDPath(_explorerTree, serverName, folderName, projectName);
            }
            else
            {
                theControl = VisualTreeWalker.GetChildByAutomationIDPath(_explorerTree, serverName, projectName);
            }
            return theControl != null && theControl.Exists;
        }

        /// <summary>
        /// Navigates to a Workflow Item in the Explorer, Right Clicks it, and clicks Properties
        /// </summary>
        /// <param name="serverName">The name of the server (EG: localhost)</param>
        /// <param name="serviceType">The name of the service (EG: WORKFLOWS)</param>
        /// <param name="folderName">The name of the folder (AKA: Category - EG: BARNEY (Or CODEDUITESTCATEGORY for the CodedUI Test Default))</param>
        /// <param name="projectName">The name of the project (EG: MyWorkflow)</param>
        public void RightClickProperties(string serverName, string folderName, string projectName)
        {
            UITestControl theControl = null;
            if(folderName != "Unassigned" && folderName != string.Empty)
            {
                theControl = VisualTreeWalker.GetChildByAutomationIDPath(_explorerTree, serverName, folderName, projectName);
            }
            else
            {
                theControl = VisualTreeWalker.GetChildByAutomationIDPath(_explorerTree, serverName, projectName);
            }
            Point p = new Point(theControl.BoundingRectangle.X + 50, theControl.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            Playback.Wait(500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(1500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(1500);
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

        public void Server_Click_WarewolfIcon(string serverName)
        {
            UITestControl theServer = GetServer(serverName);
            Point p = new Point(theServer.BoundingRectangle.X + 50, theServer.BoundingRectangle.Y + 5);
            Mouse.Click(theServer, new Point(30, 5));
            Playback.Wait(15000);
        }

        public void Server_RightClick_NewWorkflow(string serverName)
        {
            UITestControl theServer = GetServer(serverName);
            Point p = new Point(theServer.BoundingRectangle.X + 50, theServer.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            Playback.Wait(500);
            Mouse.Move(theServer, new Point(50, 5));
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(1500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(1500);
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
            Playback.Wait(1500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(1500);
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
            Playback.Wait(1500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(1500);
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
            Playback.Wait(1500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(1500);
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

        // Not functioning right now ;)
        public void RightClickShowProjectDependancies(string serverName, string folderName, string projectName)
        {
            UITestControl theControl = null;
            if(folderName != "Unassigned" && folderName != string.Empty)
            {
                theControl = VisualTreeWalker.GetChildByAutomationIDPath(_explorerTree, serverName, folderName, projectName);
            }
            else
            {
                theControl = VisualTreeWalker.GetChildByAutomationIDPath(_explorerTree, serverName, projectName);
            }
            theControl.WaitForControlEnabled();
            Point p = new Point(theControl.BoundingRectangle.X + 50, theControl.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            Playback.Wait(500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(500);
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Enter}");
            Playback.Wait(100);
        }

        public void RightClickHelp(string serverName, string folderName, string projectName)
        {
            UITestControl theControl = null;
            if(folderName != "Unassigned" && folderName != string.Empty)
            {
                theControl = VisualTreeWalker.GetChildByAutomationIDPath(_explorerTree, serverName, folderName, projectName);
            }
            else
            {
                theControl = VisualTreeWalker.GetChildByAutomationIDPath(_explorerTree, serverName, projectName);
            }
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

        public UITestControl GetServer(string serverName)
        {
            return GetConnectedServer(serverName);
        }

        public UITestControl GetServiceType(string serverName, string serviceType)
        {
            return GetServiceTypeControl(serverName, serviceType);
        }

        /// <summary>
        /// Drags the resource from the explorer to the active tab.
        /// </summary>
        /// <param name="resourceName">The name of the resource.</param>
        /// <param name="categoryName">The name of the category.</param>
        /// <param name="serverName">Name of the server (Will default to "localhost").</param>      
        /// <returns></returns>
        public UITestControl DoubleClickWorkflow(string resourceName, string categoryName, string serverName = "localhost")
        {
            DoubleClickResource(resourceName, categoryName, serverName);
            UITestControl newTab = TabManagerUIMap.GetActiveTab();

            int counter = 0;
            while(newTab == null || !TabManagerUIMap.GetActiveTabName().Contains(resourceName) && counter < 7)
            {
                Playback.Wait(500);
                newTab = TabManagerUIMap.GetActiveTab();
                counter++;
            }
            return newTab;
        }

        /// <summary>
        /// Drags the resource from the explorer to the active tab.
        /// </summary>
        /// <param name="tabToDropOnto">The tab to drop the resource onto.</param>
        /// <param name="resourceName">The name of the resource.</param>
        /// <param name="categoryName">The name of the category.</param>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="serverName">Name of the server (Will default to "localhost").</param>
        /// <param name="pointToDragTo">The point to drop the resource on the designer (Will default to just below the start node).</param>
        /// <param name="overrideDblClickBehavior">if set to <c>true</c> [override double click behavior].</param>        
        public UITestControl DragResourceOntoWorkflowDesigner(UITestControl tabToDropOnto, string resourceName, string categoryName, string serverName = "localhost", Point pointToDragTo = new Point())
        {
            if(pointToDragTo.X == 0 && pointToDragTo.Y == 0)
            {
                pointToDragTo = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint(tabToDropOnto);
            }
            EnterExplorerSearchText(resourceName);
            UITestControl theControl = null;
            if(categoryName != "Unassigned" && categoryName != string.Empty)
            {
                theControl = VisualTreeWalker.GetChildByAutomationIDPath(_explorerTree, serverName, categoryName, resourceName);
            }
            else
            {
                theControl = VisualTreeWalker.GetChildByAutomationIDPath(_explorerTree, serverName, resourceName);
            }
            Mouse.StartDragging(theControl);
            Playback.Wait(20);
            Mouse.StopDragging(pointToDragTo);
            Playback.Wait(100);
            UITestControl resourceOnDesignSurface = WorkflowDesignerUIMap.FindControlByAutomationId(tabToDropOnto, resourceName);
            int counter = 0;
            while(resourceOnDesignSurface == null && counter < 5)
            {
                Playback.Wait(1000);
                resourceOnDesignSurface = WorkflowDesignerUIMap.FindControlByAutomationId(tabToDropOnto, resourceName);
                Playback.Wait(500);
                counter++;
            }

            return resourceOnDesignSurface;
        }

        /// <summary>
        /// Drags the control automatic workflow designer.
        /// </summary>
        /// <param name="serverName">Name of the server.</param>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="folderName">Name of the folder.</param>
        /// <param name="projectName">Name of the project.</param>
        /// <param name="p">The application.</param>
        /// <param name="overrideDblClickBehavior">if set to <c>true</c> [override double click behavior].</param>
        public void DragControlToWorkflowDesigner(string serverName, string serviceType, string folderName, string projectName, Point p)
        {
            UITestControl theControl = null;
            if(folderName != "Unassigned" && folderName != string.Empty)
            {
                theControl = VisualTreeWalker.GetChildByAutomationIDPath(_explorerTree, serverName, folderName, projectName);
            }
            else
            {
                theControl = VisualTreeWalker.GetChildByAutomationIDPath(_explorerTree, serverName, projectName);
            }
            Mouse.StartDragging(theControl);
            Playback.Wait(20);
            Mouse.StopDragging(p);
            Playback.Wait(100);
        }

        /// <summary>
        /// Rights the click rename project.
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="categoryName"></param>
        /// <param name="newName"></param>
        /// <param name="serverName"></param>
        public void RightClickRenameResource(string resourceName, string categoryName, string newName, string serverName = "localhost")
        {
            EnterExplorerSearchText(resourceName);
            UITestControl theControl = null;
            if(categoryName != "Unassigned" && categoryName != string.Empty)
            {
                theControl = VisualTreeWalker.GetChildByAutomationIDPath(_explorerTree, serverName, categoryName, resourceName);
            }
            else
            {
                theControl = VisualTreeWalker.GetChildByAutomationIDPath(_explorerTree, serverName, resourceName);
            }
            Point p = new Point(theControl.BoundingRectangle.X + 100, theControl.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(500);
            SendKeys.SendWait("{DOWN}");
            Playback.Wait(50);
            SendKeys.SendWait("{DOWN}");
            Playback.Wait(50);
            SendKeys.SendWait("{DOWN}");
            Playback.Wait(50);
            SendKeys.SendWait("{ENTER}");
            Playback.Wait(50);
            SendKeys.SendWait(newName);
            Playback.Wait(50);
            SendKeys.SendWait("{TAB}");
            Playback.Wait(150);
            SendKeys.SendWait("{TAB}");
        }

        public void ClickNewServerButton(int waitAmt = 200)
        {
            var firstConnectControlButton = GetConnectControl("Button");
            var nextConnectControlButtonPosition = new Point(firstConnectControlButton.Left + 35, firstConnectControlButton.Top + 10);
            Mouse.Click(nextConnectControlButtonPosition);
            Playback.Wait(waitAmt);
        }

        private bool ValidateResourceExists(string resourceName, string folderName, string serverName = "localhost")
        {
            try
            {
                EnterExplorerSearchText(resourceName);
                Playback.Wait(1000);
                UITestControl theControl = null;
                if(folderName != "Unassigned" && folderName != string.Empty)
                {
                    theControl = VisualTreeWalker.GetChildByAutomationIDPath(_explorerTree, serverName, folderName, resourceName);
                }
                else
                {
                    theControl = VisualTreeWalker.GetChildByAutomationIDPath(_explorerTree, serverName, resourceName);
                }
                if(theControl == null)
                {
                    return false;
                }
                return true;
            }
            catch(Exception)
            {

                return false;
            }
        }

        private bool ValidateResourceExists(string resourceName, string serverName = "localhost")
        {
            try
            {
                EnterExplorerSearchText(resourceName);
                Playback.Wait(1000);
                return GetServiceItem(serverName, resourceName) != null;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public void DoubleClickSource(string sourceName, string categoryName, string serverName = "localhost")
        {
            DoubleClickResource(sourceName, categoryName, serverName);
        }

        public void DoubleClickService(string serviceName, string categoryName, string serverName = "localhost")
        {
            DoubleClickResource(serviceName, categoryName, serverName);
        }

        private void DoubleClickResource(string resourceName, string categoryName, string serverName)
        {
            EnterExplorerSearchText(resourceName);
            UITestControl theControl = null;
            //Ensure explorer tree
            GetExplorerTree();
            if(categoryName != "Unassigned" && categoryName != string.Empty)
            {
                theControl = VisualTreeWalker.GetChildByAutomationIDPath(_explorerTree, serverName, categoryName, resourceName);
            }
            else
            {
                theControl = VisualTreeWalker.GetChildByAutomationIDPath(_explorerTree, serverName, resourceName);
            }
            Playback.Wait(100);
            Mouse.DoubleClick(theControl, new Point(theControl.BoundingRectangle.X, theControl.BoundingRectangle.Y + 200));
            Playback.Wait(5000);
            if(serverName != "localhost")
            {
                Playback.Wait(5000);
            }
        }

        public void SelectServer(string serverName)
        {
            var serverList = GetConnectControl("ComboBox") as WpfComboBox;
            if(serverList != null)
            {
                var serverItem = serverList.Items.ToList().FirstOrDefault(c => c.FriendlyName == serverName);
                if(serverItem != null)
                {
                    serverList.SelectedIndex = serverList.Items.IndexOf(serverItem);
                }
            }
        }

        public bool IsServerIsConnected(string serverName)
        {
            var serverList = GetConnectControl("ComboBox") as WpfComboBox;
            if(serverList != null)
            {
                var serverItem = serverList.Items.ToList().FirstOrDefault(c => c.FriendlyName == serverName);
                if(serverItem != null)
                {
                    serverList.SelectedIndex = serverList.Items.IndexOf(serverItem);
                }
            }

            return false;
        }

        public void ChooseServerWithKeyboard(UITestControl uITestControl, string connection)
        {
            var serverList = GetConnectControl("ComboBox") as WpfComboBox;
            Mouse.Click(serverList);
            Keyboard.SendKeys("{UP}{UP}{UP}{UP}{UP}{UP}{UP}{ENTER}");
            Playback.Wait(2000);
        }

        public void ChooseSourceServer(UITestControl uITestControl, string connection)
        {
            var serverList = GetConnectControl("ComboBox") as WpfComboBox;
            if(serverList != null)
            {
                var serverItem = serverList.Items.ToList().FirstOrDefault(c => c.FriendlyName == connection);
                if(serverItem != null)
                {
                    serverList.SelectedIndex = serverList.Items.IndexOf(serverItem);
                }
            }
        }
    }
}
