﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by coded UI test builder.
//      Version: 11.0.0.0
//
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------

using System.Linq;
using Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses;

namespace Dev2.CodedUI.Tests.UIMaps.ExplorerUIMapClasses
{
    using Microsoft.VisualStudio.TestTools.UITest.Extension;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    using System.CodeDom.Compiler;
    using System.Drawing;
    using System.Threading;
    using System.Windows.Forms;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;


    [GeneratedCode("Coded UITest Builder", "11.0.50727.1")]
    public partial class ExplorerUIMap
    {
        /// <summary>
        /// Clicks the Refresh button on the Explorer Tab, and waits for it to complete. You will probably need to use DocManagerUIMap.ClickOpenTabPage("Explorer"); before doing this.
        /// </summary>
        public void DoRefresh()
        {
            // Find the explorer main window
            UITestControl anItem = this.UIBusinessDesignStudioWindow.UIExplorerCustom;
            anItem.Find();
            UITestControlCollection subItems = anItem.GetChildren();

            // Find the explorer sub window
            UITestControl explorerMenu = new UITestControl(anItem);
            explorerMenu.SearchProperties["AutomationId"] = "Explorer";
            explorerMenu.Find();

            // Find the refresh button
            UITestControl refreshButton = new UITestControl(explorerMenu);
            UITestControlCollection explorerChildren = explorerMenu.GetChildren();
            var connectUserControl = explorerChildren.First(c => c.FriendlyName == "TheNavigationView");
            var connectUserControlChilren = connectUserControl.GetChildren();
            refreshButton = connectUserControlChilren.First(c => c.FriendlyName == "UI_SourceServerRefreshbtn_AutoID");
            //refreshButton.SearchProperties["AutomationId"] = "UI_SourceServerbtnRefresh_AutoID";
            //refreshButton.Find();

            // And click it
            Mouse.Click(refreshButton, new Point(5, 5));
            Thread.Sleep(3000);
        }

        public void PinPane()
        {
            // Find the explorer main window
            UITestControl anItem = this.UIBusinessDesignStudioWindow;
            anItem.Find();
            anItem.DrawHighlight();

            // Find the explorer sub window
            UITestControl DocManager = new UITestControl(anItem);
            DocManager.SearchProperties["AutomationId"] = "UI_DocManager_AutoID";
            DocManager.Find();
            DocManager.DrawHighlight();

            // Find the left pane window
            UITestControl DockLeft = new UITestControl(DocManager);
            DockLeft.SearchProperties["AutomationId"] = "DockLeft";
            DockLeft.Find();
            DockLeft.DrawHighlight();

            // Find the tab page window
            UITestControlCollection dockLeftChildren = DockLeft.GetChildren()[0].GetChildren();
            //var TabPage = dockLeftChildren.FirstOrDefault(c => c.FriendlyName == "Explorer");
            var TabPage = dockLeftChildren[0];
            TabPage.DrawHighlight();

            // Find the explorer sub window
            UITestControl ExplorerPane = new UITestControl(TabPage);
            ExplorerPane.SearchProperties["AutomationId"] = "UI_ExplorerPane_AutoID";
            ExplorerPane.Find();
            ExplorerPane.DrawHighlight();

            // Find the pin
            UITestControlCollection explorerChildren = ExplorerPane.GetChildren();
            var unpinControl = explorerChildren.First(c => c.FriendlyName == "unpinBtn");
            Mouse.Click(unpinControl);
        }

        public void OpenDebugOutput()
        {
            UITestControl anItem = this.UIBusinessDesignStudioWindow.UIExplorerCustom;
            anItem.Find();
        }

        public UITestControl GetServerDDL()
        {
            // Find the explorer main window
            UITestControl anItem = this.UIBusinessDesignStudioWindow.UIExplorerCustom;
            anItem.Find();
            UITestControlCollection subItems = anItem.GetChildren();

            // Find the explorer sub window
            UITestControl explorerMenu = new UITestControl(anItem);
            explorerMenu.SearchProperties["AutomationId"] = "Explorer";
            explorerMenu.Find();

            UITestControl serverDDL = new UITestControl(explorerMenu);
            serverDDL.SearchProperties["AutomationId"] = "UI_ExplorerServerCbx_AutoID";
            serverDDL.Find();
            return serverDDL;
        }

        public UITestControl GetExplorerEditBtn()
        {
            // Find the explorer main window
            UITestControl anItem = this.UIBusinessDesignStudioWindow.UIExplorerCustom;
            anItem.Find();
            UITestControlCollection subItems = anItem.GetChildren();

            // Find the explorer sub window
            UITestControl explorerMenu = new UITestControl(anItem);
            explorerMenu.SearchProperties["AutomationId"] = "Explorer";
            explorerMenu.Find();

            UITestControl serverDDL = new UITestControl(explorerMenu);
            serverDDL.SearchProperties["AutomationId"] = "UI_ExplorerEditBtn_AutoID";
            serverDDL.Find();
            return serverDDL;
        }

        public UITestControl GetExplorerNewBtn()
        {
            // Find the explorer main window
            UITestControl anItem = this.UIBusinessDesignStudioWindow.UIExplorerCustom;
            anItem.Find();
            UITestControlCollection subItems = anItem.GetChildren();

            // Find the explorer sub window
            UITestControl explorerMenu = new UITestControl(anItem);
            explorerMenu.SearchProperties["AutomationId"] = "Explorer";
            explorerMenu.Find();

            UITestControl serverDDL = new UITestControl(explorerMenu);
            serverDDL.SearchProperties["AutomationId"] = "UI_ExplorerNewBtn_AutoID";
            serverDDL.Find();
            return serverDDL;
        }

        public void GetMenuItem(string menuItemName)
        {
            UITestControl theStudio = this.UIBusinessDesignStudioWindow;
            UITestControl theTabMenu = new UITestControl(theStudio);
            //theTabMenu.SearchProperties["AutomationID"] = "UI_ExplorerContextMenu_AutoID";

            foreach (UITestControl theControl in theStudio.GetChildren())
            {
                string theType = theControl.ClassName;
                if (theType == "Uia.Popup")
                {
                    UITestControl contextMenu = theControl.GetChildren()[0];
                    foreach (UITestControl subControl in contextMenu.GetChildren())
                    {
                        try
                        {
                            string friendlyName = subControl.FriendlyName;
                            int j = 10;
                        }
                        catch
                        {
                            // Do Nothing - Invalid control
                        }
                    }
                }
            }

        }

        public WpfTree GetExplorerTree()
        {
            return this.UIBusinessDesignStudioWindow.UIExplorerCustom.UINavigationViewUserCoCustom.UITvExplorerTree;
        }
        /// <summary>
        /// ClickExplorer
        /// </summary>
        /// <param name="serverName">Name of the server.</param>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="folderName">Name of the folder.</param>
        /// <param name="projectName">Name of the project.</param>
        /// <param name="overrideDblClickBehavior">if set to <c>true</c> [override double click behavior].</param>
        /// <returns></returns>
        private UITestControl GetServiceItem(string serverName, string serviceType, string folderName, string projectName,bool overrideDblClickBehavior = false)
        {
            Point p;

            Thread.Sleep(100);
            SendKeys.SendWait("{HOME}");
            Thread.Sleep(100);

            WpfTree uITvExplorerTree = this.UIBusinessDesignStudioWindow.UIExplorerCustom.UINavigationViewUserCoCustom.UITvExplorerTree;
            // Uncomment these 3 lines if things start going slowly again (They help to locate the problem)

            //UITestControl theStudioWindow = this.UIBusinessDesignStudioWindow.UIExplorerCustom.UINavigationViewUserCoCustom;
            //theStudioWindow.Find();
            //uITvExplorerTree.Find();

            //UITestControl serverListItem = new UITestControl(uITvExplorerTree);
            uITvExplorerTree.SearchProperties.Add("AutomationId", serverName, PropertyExpressionOperator.Contains);
            uITvExplorerTree.SearchProperties.Add("ControlType", "TreeItem");

            uITvExplorerTree.Find();

            //// Can we see the type list? (AKA: Is the server list maximized?)
            UITestControl serviceTypeListItem = new UITestControl(uITvExplorerTree);       
            serviceTypeListItem.SearchProperties.Add("AutomationId", "UI_" + serviceType + "_AutoID");
            serviceTypeListItem.SearchConfigurations.Add(SearchConfiguration.ExpandWhileSearching);

            serviceTypeListItem.Find();


            if (!serviceTypeListItem.TryGetClickablePoint(out p) && !overrideDblClickBehavior)
            {                
                // This is causing the window to shrink
                Mouse.DoubleClick(new Point(serviceTypeListItem.BoundingRectangle.X + 50, serviceTypeListItem.BoundingRectangle.Y + 5));
            }
            else
            {
                Mouse.Click(new Point(serviceTypeListItem.BoundingRectangle.X + 50, serviceTypeListItem.BoundingRectangle.Y + 5));
            }

            Thread.Sleep(300);

            // Can we see the folder? (AKA: Is the type list maximised?)
            UITestControl folderNameListItem = new UITestControl(serviceTypeListItem);
            folderNameListItem.SearchProperties.Add("AutomationId", "UI_" + ((folderName != "Unassigned") ? folderName.ToUpper() : folderName) + "_AutoID");
            folderNameListItem.Find();
            if (!folderNameListItem.TryGetClickablePoint(out p) && !overrideDblClickBehavior)
            {
                Mouse.DoubleClick(new Point(folderNameListItem.BoundingRectangle.X + 50, folderNameListItem.BoundingRectangle.Y + 5));
            }
            else
            {
                Mouse.Click(new Point(folderNameListItem.BoundingRectangle.X + 50, folderNameListItem.BoundingRectangle.Y + 5));
            }

            Thread.Sleep(300);

            // Can we see the file? (AKA: Is the folder maximised?)
            UITestControl projectNameListItem = new UITestControl(folderNameListItem);
            projectNameListItem.SearchProperties.Add("AutomationId", "UI_" + projectName + "_AutoID");
            projectNameListItem.Find();
            if (!projectNameListItem.TryGetClickablePoint(out p) && !overrideDblClickBehavior)
            {
                Mouse.DoubleClick(new Point(projectNameListItem.BoundingRectangle.X + 50, projectNameListItem.BoundingRectangle.Y + 5));
            }
            else
            {
                Mouse.Click(new Point(projectNameListItem.BoundingRectangle.X + 50, projectNameListItem.BoundingRectangle.Y + 5));
            }

           return projectNameListItem;                       

        }

        private UITestControl GetCategory(string serverName, string serviceType, string categoryName)
        {
            Point p;
            UITestControl returnControl = null;

            Thread.Sleep(500);
            SendKeys.SendWait("{HOME}");
            SendKeys.SendWait("^{LEFT}");
            SendKeys.SendWait("^{LEFT}");
            SendKeys.SendWait("^{LEFT}");
            SendKeys.SendWait("^{LEFT}");
            SendKeys.SendWait("^{LEFT}");
            Thread.Sleep(500);

            WpfTree uITvExplorerTree = this.UIBusinessDesignStudioWindow.UIExplorerCustom.UINavigationViewUserCoCustom.UITvExplorerTree;
            // Uncomment these 3 lines if things start going slowly again (They help to locate the problem)

            //UITestControl theStudioWindow = this.UIBusinessDesignStudioWindow.UIExplorerCustom.UINavigationViewUserCoCustom;
            //theStudioWindow.Find();
            //uITvExplorerTree.Find();

            UITestControl serverListItem = new UITestControl(uITvExplorerTree);
            serverListItem.SearchProperties.Add("AutomationId", serverName, PropertyExpressionOperator.Contains);
            serverListItem.SearchProperties.Add("ControlType", "TreeItem");

            serverListItem.Find();

            Point clickablePoint = new Point();
            if (!serverListItem.TryGetClickablePoint(out clickablePoint))
            {
                // Click in it, and fix the alignment
                Mouse.Click(serverListItem, new Point(100, 100));

                // Re-allign the Explorer
                SendKeys.SendWait("{PAGEUP}");
                SendKeys.SendWait("{PAGEUP}");
                SendKeys.SendWait("{PAGEUP}");

                SendKeys.SendWait("^{LEFT}");
                SendKeys.SendWait("^{LEFT}");
                SendKeys.SendWait("^{LEFT}");
                SendKeys.SendWait("^{LEFT}");
                SendKeys.SendWait("^{LEFT}");
            }

            Thread.Sleep(500);
            SendKeys.SendWait("{HOME}");
            Thread.Sleep(500);

            // Can we see the type list? (AKA: Is the server list maximized?)
            UITestControl serviceTypeListItem = new UITestControl(serverListItem);
            serviceTypeListItem.SearchProperties.Add("AutomationId", "UI_" + serviceType + "_AutoID");
            serviceTypeListItem.Find();

            if (!serviceTypeListItem.TryGetClickablePoint(out p))
            {
                Mouse.DoubleClick(new Point(serverListItem.BoundingRectangle.X + 50, serverListItem.BoundingRectangle.Y + 5));
            }
            else
            {
                Mouse.Click(new Point(serverListItem.BoundingRectangle.X + 50, serverListItem.BoundingRectangle.Y + 5));
            }

            // Can we see the folder? (AKA: Is the type list maximised?)
            UITestControl folderNameListItem = new UITestControl(serviceTypeListItem);
            folderNameListItem.SearchProperties.Add("AutomationId", "UI_" + categoryName + "_AutoID");
            folderNameListItem.Find();
            if (!folderNameListItem.TryGetClickablePoint(out p))
            {
                Mouse.DoubleClick(new Point(serviceTypeListItem.BoundingRectangle.X + 50, serviceTypeListItem.BoundingRectangle.Y + 5));
            }
            else
            {
                Mouse.Click(new Point(serviceTypeListItem.BoundingRectangle.X + 50, serviceTypeListItem.BoundingRectangle.Y + 5));
            }
            return folderNameListItem;
        }


        public void EnterExplorerSearchText(string textToSearchWith)
        {
            #region Variable Declarations
            WpfEdit uIUI_DataListSearchtxtEdit = new UIWarewolfWindow().UITheNavigationViewCustom.UIFilterTextBoxEdit.UIUI_DataListSearchtxtEdit;
            #endregion

            // Click 'UI_DataListSearchtxt_AutoID' text box
            Mouse.Click(uIUI_DataListSearchtxtEdit, new Point(12, 8));

            SendKeys.SendWait(textToSearchWith);
        }

        #region filter box mappings

        [GeneratedCode("Coded UITest Builder", "11.0.60315.1")]
        public class UIFilterTextBoxEdit : WpfEdit
        {

            public UIFilterTextBoxEdit(UITestControl searchLimitContainer) :
                base(searchLimitContainer)
            {
                #region Search Criteria
                this.SearchProperties[WpfEdit.PropertyNames.AutomationId] = "FilterTextBox";
                this.WindowTitles.Add("Warewolf");
                #endregion
            }

            #region Properties
            public WpfEdit UIUI_DataListSearchtxtEdit
            {
                get
                {
                    if ((this.mUIUI_DataListSearchtxtEdit == null))
                    {
                        this.mUIUI_DataListSearchtxtEdit = new WpfEdit(this);
                        #region Search Criteria
                        this.mUIUI_DataListSearchtxtEdit.SearchProperties[WpfEdit.PropertyNames.AutomationId] = "UI_DataListSearchtxt_AutoID";
                        this.mUIUI_DataListSearchtxtEdit.WindowTitles.Add("Warewolf");
                        #endregion
                    }
                    return this.mUIUI_DataListSearchtxtEdit;
                }
            }
            #endregion

            #region Fields
            private WpfEdit mUIUI_DataListSearchtxtEdit;
            #endregion
        }

        [GeneratedCode("Coded UITest Builder", "11.0.60315.1")]
        public class UITheNavigationViewCustom2 : WpfCustom
        {

            public UITheNavigationViewCustom2(UITestControl searchLimitContainer) :
                base(searchLimitContainer)
            {
                #region Search Criteria
                this.SearchProperties[UITestControl.PropertyNames.ClassName] = "Uia.NavigationView";
                this.SearchProperties["AutomationId"] = "TheNavigationView";
                this.WindowTitles.Add("Warewolf");
                #endregion
            }

            #region Properties
            public UIFilterTextBoxEdit UIFilterTextBoxEdit
            {
                get
                {
                    if ((this.mUIFilterTextBoxEdit == null))
                    {
                        this.mUIFilterTextBoxEdit = new UIFilterTextBoxEdit(this);
                    }
                    return this.mUIFilterTextBoxEdit;
                }
            }
            #endregion

            #region Fields
            private UIFilterTextBoxEdit mUIFilterTextBoxEdit;
            #endregion
        }

        [GeneratedCode("Coded UITest Builder", "11.0.60315.1")]
        public class UIWarewolfWindow : WpfWindow
        {

            public UIWarewolfWindow()
            {
                #region Search Criteria
                this.SearchProperties[WpfWindow.PropertyNames.Name] = "Warewolf";
                this.SearchProperties.Add(new PropertyExpression(WpfWindow.PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains));
                this.WindowTitles.Add("Warewolf");
                #endregion
            }

            #region Properties
            public UITheNavigationViewCustom2 UITheNavigationViewCustom
            {
                get
                {
                    if ((this.mUITheNavigationViewCustom == null))
                    {
                        this.mUITheNavigationViewCustom = new UITheNavigationViewCustom2(this);
                    }
                    return this.mUITheNavigationViewCustom;
                }
            }
            #endregion

            #region Fields
            private UITheNavigationViewCustom2 mUITheNavigationViewCustom;
            #endregion
        }

        #endregion

        public void ClearExplorerSearchText()
        {
            #region Variable Declarations
            WpfEdit uIUI_DataListSearchtxtEdit = new UIWarewolfWindow().UITheNavigationViewCustom.UIFilterTextBoxEdit.UIUI_DataListSearchtxtEdit;
            #endregion

            // Click 'UI_DataListSearchtxt_AutoID' text box
            Mouse.Click(uIUI_DataListSearchtxtEdit, new Point(12, 8));
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            Mouse.Click(uIUI_DataListSearchtxtEdit, new Point(5, 5));
            SendKeys.SendWait("{HOME}");
            SendKeys.SendWait("+{END}");
            SendKeys.SendWait("{DELETE}");
        }

        public void ConnectToAllServers(WpfComboBox comboBox)
        {
            for(int itemIdx = 1; itemIdx <= comboBox.Items.Count; itemIdx++)
            {
                Microsoft.VisualStudio.TestTools.UITesting.Mouse.Click(comboBox);
                for(int findItm = 0; findItm < itemIdx; findItm++)
                {
                    Keyboard.SendKeys("{DOWN}");
                }
                Keyboard.SendKeys("{ENTER}");
            }
        }

        public UITestControlCollection GetCategoryItems()
        {
            UITestControlCollection workflows = GetNavigationItemCategories();


            return workflows;
        }

        public UITestControlCollection GetNavigationItemCategories()
        {
            WpfTree uITvExplorerTree = this.UIBusinessDesignStudioWindow.UIExplorerCustom.UINavigationViewUserCoCustom.UITvExplorerTree;


            uITvExplorerTree.SearchProperties.Add("AutomationId", "localhost", PropertyExpressionOperator.Contains);
            uITvExplorerTree.SearchProperties.Add("ControlType", "TreeItem");

            uITvExplorerTree.Find();

            //// Can we see the type list? (AKA: Is the server list maximized?)
            UITestControl serviceTypeListItem = new UITestControl(uITvExplorerTree);
            serviceTypeListItem.SearchProperties.Add("AutomationId", "UI_WORKFLOWS_AutoID");
            serviceTypeListItem.SearchConfigurations.Add(SearchConfiguration.ExpandWhileSearching);

            serviceTypeListItem.Find();

            //WpfTree tree = GetExplorerTree();

            //UITestControl serverNode = tree.GetChildren()[0];

            UITestControlCollection categories = serviceTypeListItem.GetChildren();

            UITestControlCollection categoryCollection = new UITestControlCollection();

            foreach (UITestControl category in categories)
            {
                if (category.ControlType.ToString() == "TreeItem")
                {
                    categoryCollection.Add(category);
                }
            }

            return categoryCollection;
        }

        private UITestControl GetConnectedServer(string serverName)
        {
            UITestControl returnControl = new UITestControl();
            WpfTree uITvExplorerTree = this.UIBusinessDesignStudioWindow.UINavigationViewUserCoCustom.UITvExplorerTree;
            UITestControl server = null;
            foreach (UITestControl serverListItem in uITvExplorerTree.GetChildren()) // 0 for the first server in the list
            {
                if (serverListItem.GetProperty(WpfTree.PropertyNames.AutomationId).ToString().Contains(serverName))
                {
                    server = serverListItem;
                    break;
                }
                else
                {
                    server = null;
                }
            }
            return server;
        }

        private UITestControl GetServiceTypeControl(string serverName, string serviceType)
        {
            Point p;
            UITestControl server = GetConnectedServer(serverName);
            UITestControl serviceTypeReturn = null;
            foreach (UITestControl serviceTypeListItem in server.GetChildren())
            {
                if (serviceTypeListItem.FriendlyName.Contains(serviceType))
                {
                    // If the service type is not visible, expand the server list
                    if (!serviceTypeListItem.TryGetClickablePoint(out p))
                    {
                        Mouse.DoubleClick(new Point(server.BoundingRectangle.X + 50, server.BoundingRectangle.Y + 5));
                    }
                    else
                    {
                        Mouse.Click(new Point(server.BoundingRectangle.X + 50, server.BoundingRectangle.Y + 5));
                    }
                    serviceTypeReturn = serviceTypeListItem;
                    break;
                }
                else
                {
                    serviceTypeReturn = null;
                }
            }
            return serviceTypeReturn;
        }

        #region Properties
        public UIBusinessDesignStudioWindow UIBusinessDesignStudioWindow
        {
            get
            {
                if ((this.mUIBusinessDesignStudioWindow == null))
                {
                    this.mUIBusinessDesignStudioWindow = new UIBusinessDesignStudioWindow();
                }
                return this.mUIBusinessDesignStudioWindow;
            }
        }
        #endregion


        #region Fields
        private UIBusinessDesignStudioWindow mUIBusinessDesignStudioWindow;

        #endregion
    }

    [GeneratedCode("Coded UITest Builder", "11.0.50727.1")]
    public class UIBusinessDesignStudioWindow : WpfWindow
    {

        public UIBusinessDesignStudioWindow()
        {
            #region Search Criteria
            this.SearchProperties.Add(new PropertyExpression(WpfWindow.PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains));
            this.SearchProperties.Add(new PropertyExpression(WpfWindow.PropertyNames.Name, "Warewolf", PropertyExpressionOperator.Contains));
            #endregion
        }

        #region Navigation Item

        public UINavigationViewUserCoCustom UINavigationViewUserCoCustom
        {
            get
            {
                if ((this.mUINavigationViewUserCoCustom == null))
                {
                    this.mUINavigationViewUserCoCustom = new UINavigationViewUserCoCustom(this);
                }
                return this.mUINavigationViewUserCoCustom;
            }
        }

        private UINavigationViewUserCoCustom mUINavigationViewUserCoCustom;

        #endregion Navigation Item

        #region Explorer Item

        public UIExplorerCustom UIExplorerCustom
        {
            get
            {
                if ((this.mUIExplorerCustom == null))
                {
                    this.mUIExplorerCustom = new UIExplorerCustom(this);
                }
                return this.mUIExplorerCustom;
            }
        }

        private UIExplorerCustom mUIExplorerCustom;

        #endregion Explorer Item

    }

    [GeneratedCode("Coded UITest Builder", "11.0.50727.1")]
    public class UIExplorerCustom : WpfCustom
    {

        public UIExplorerCustom(UITestControl searchLimitContainer) :
            base(searchLimitContainer)
        {
           
            #region Search Criteria
            this.SearchProperties[UITestControl.PropertyNames.ClassName] = "Uia.NavigationView";
            this.SearchProperties["AutomationId"] = "TheNavigationView";
            this.WindowTitles.Add("Warewolf");
            #endregion
        }

        #region Properties


        public WpfEdit UIUI_txtSearch_AutoIDEdit
        {
            get
            {
                //if ((this.mUIUI_txtSearch_AutoIDEdit == null))
                //{
                //    this.mUIUI_txtSearch_AutoIDEdit = new WpfEdit(this);
                //    #region Search Criteria
                //    this.mUIUI_txtSearch_AutoIDEdit.SearchProperties[WpfEdit.PropertyNames.AutomationId] = "UI_DataListSearchtxt_AutoID";
                //    this.mUIUI_txtSearch_AutoIDEdit.SearchProperties.Add(new PropertyExpression(WpfWindow.PropertyNames.Name, "Warewolf", PropertyExpressionOperator.Contains));
                //    #endregion

                //    #region Search Criteria
                //    this.SearchProperties[WpfEdit.PropertyNames.AutomationId] = "FilterTextBox";
                //    this.WindowTitles.Add("Warewolf");
                //    #endregion
                //}
                //return this.mUIUI_txtSearch_AutoIDEdit;

                if ((this.mUIUI_txtSearch_AutoIDEdit == null))
                {
                    this.mUIUI_txtSearch_AutoIDEdit = new WpfEdit(this);
                    #region Search Criteria
                    this.mUIUI_txtSearch_AutoIDEdit.SearchProperties[WpfEdit.PropertyNames.AutomationId] = "UI_DataListSearchtxt_AutoID";
                    this.mUIUI_txtSearch_AutoIDEdit.WindowTitles.Add("Warewolf");
                    #endregion
                }
                return this.mUIUI_txtSearch_AutoIDEdit;
            }
        }


        public UINavigationViewUserCoCustom UINavigationViewUserCoCustom
        {
            get
            {
                if ((this.mUINavigationViewUserCoCustom == null))
                {
                    this.mUINavigationViewUserCoCustom = new UINavigationViewUserCoCustom(this);
                }
                return this.mUINavigationViewUserCoCustom;
            }
        }


        #endregion

        #region Fields
        private UINavigationViewUserCoCustom mUINavigationViewUserCoCustom;
        private WpfEdit mUIUI_txtSearch_AutoIDEdit;
        #endregion
    }

    [GeneratedCode("Coded UITest Builder", "11.0.50727.1")]
    public class OtherServer : WpfTreeItem
    {

        public OtherServer(UITestControl searchLimitContainer, string serverAutomationId) :
            base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[WpfTreeItem.PropertyNames.AutomationId] = /*"UI_Sashens Server (http://rsaklfsashennai:77/dsf)_AutoID"*/ serverAutomationId;
            this.WindowTitles.Add(TestBase.GetStudioWindowName());
            #endregion
        }

        #region Properties
        public WpfTreeItem UIUI_WORKFLOWSERVICES_TreeItem
        {
            get
            {
                if ((this.mUIUI_WORKFLOWSERVICES_TreeItem == null))
                {
                    this.mUIUI_WORKFLOWSERVICES_TreeItem = new WpfTreeItem(this);
                    #region Search Criteria
                    this.mUIUI_WORKFLOWSERVICES_TreeItem.SearchProperties[WpfTreeItem.PropertyNames.AutomationId] = "UI_WORKFLOW SERVICES_AutoID";
                    this.mUIUI_WORKFLOWSERVICES_TreeItem.SearchConfigurations.Add(SearchConfiguration.ExpandWhileSearching);
                    this.mUIUI_WORKFLOWSERVICES_TreeItem.WindowTitles.Add(TestBase.GetStudioWindowName());
                    #endregion
                }
                return this.mUIUI_WORKFLOWSERVICES_TreeItem;
            }
        }
        #endregion

        #region Fields
        private WpfTreeItem mUIUI_WORKFLOWSERVICES_TreeItem;
        #endregion
    }


    [GeneratedCode("Coded UITest Builder", "11.0.50727.1")]
    public class UINavigationViewUserCoCustom : WpfCustom
    {

        public UINavigationViewUserCoCustom(UITestControl searchLimitContainer) :
            base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[UITestControl.PropertyNames.ClassName] = "Uia.NavigationView";
            this.SearchProperties["AutomationId"] = "TheNavigationView";
            this.WindowTitles.Add(TestBase.GetStudioWindowName());
            #endregion
        }

        #region Properties
        public UITvExplorerTree UITvExplorerTree
        {
            get
            {
                if ((this.mUITvExplorerTree == null))
                {
                    this.mUITvExplorerTree = new UITvExplorerTree(this);
                }
                return this.mUITvExplorerTree;
            }
        }
        #endregion

        #region Fields
        private UITvExplorerTree mUITvExplorerTree;
        #endregion
    }

    [GeneratedCode("Coded UITest Builder", "11.0.50727.1")]
    public class UITvExplorerTree : WpfTree
    {

        public UITvExplorerTree(UITestControl searchLimitContainer) :
            base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[WpfTree.PropertyNames.AutomationId] = "tvExplorer";
            this.WindowTitles.Add(TestBase.GetStudioWindowName());
            #endregion
        }

        #region Properties
        public UIUI_localhosthttp1270TreeItem UIUI_localhosthttp1270TreeItem
        {
            get
            {
                if ((this.mUIUI_localhosthttp1270TreeItem == null))
                {
                    this.mUIUI_localhosthttp1270TreeItem = new UIUI_localhosthttp1270TreeItem(this);
                }
                return this.mUIUI_localhosthttp1270TreeItem;
            }
        }
        #endregion

        #region Fields
        private UIUI_localhosthttp1270TreeItem mUIUI_localhosthttp1270TreeItem;
        #endregion
    }

    [GeneratedCode("Coded UITest Builder", "11.0.50727.1")]
    public class UIUI_localhosthttp1270TreeItem : WpfTreeItem
    {

        public UIUI_localhosthttp1270TreeItem(UITestControl searchLimitContainer) :
            base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[WpfTreeItem.PropertyNames.AutomationId] = "UI_localhost (http://127.0.0.1:77/dsf)_AutoID";
            this.WindowTitles.Add(TestBase.GetStudioWindowName());
            #endregion
        }

        #region Properties
        public WpfTreeItem UIUI_WORKFLOWSERVICES_TreeItem
        {
            get
            {
                if ((this.mUIUI_WORKFLOWSERVICES_TreeItem == null))
                {
                    this.mUIUI_WORKFLOWSERVICES_TreeItem = new WpfTreeItem(this);
                    #region Search Criteria
                    this.mUIUI_WORKFLOWSERVICES_TreeItem.SearchProperties[WpfTreeItem.PropertyNames.AutomationId] = "UI_WORKFLOW SERVICES_AutoID";
                    this.mUIUI_WORKFLOWSERVICES_TreeItem.SearchConfigurations.Add(SearchConfiguration.ExpandWhileSearching);
                    this.mUIUI_WORKFLOWSERVICES_TreeItem.WindowTitles.Add(TestBase.GetStudioWindowName());
                    #endregion
                }
                return this.mUIUI_WORKFLOWSERVICES_TreeItem;
            }
        }
        #endregion

        #region Fields
        private WpfTreeItem mUIUI_WORKFLOWSERVICES_TreeItem;
        #endregion
    }
}
