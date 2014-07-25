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
using Dev2.Studio.UI.Tests;
using Dev2.Studio.UI.Tests.Extensions;
using Dev2.Studio.UI.Tests.Utils;

namespace Dev2.CodedUI.Tests.UIMaps.ExplorerUIMapClasses
{
    using System.CodeDom.Compiler;
    using System.Drawing;
    using Microsoft.VisualStudio.TestTools.UITest.Extension;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using System.Threading;


    [GeneratedCode("Coded UITest Builder", "11.0.50727.1")]
    public partial class ExplorerUIMap : UIMapBase
    {
        /// <summary>
        /// Clicks the Refresh button on the Explorer Tab, and waits for it to complete. You will probably need to use DockManagerUIMap.ClickOpenTabPage("Explorer"); before doing this.
        /// </summary>
        public void DoRefresh()
        {
            // Click refresh
            Mouse.Click(ExplorerRefresh, new Point(5, 5));
            ExplorerRefresh.WaitForControlReady();
        }

        public void PinPane()
        {
            // Find the explorer main window
            UITestControl anItem = this.UIBusinessDesignStudioWindow;
            anItem.Find();

            // Find the explorer sub window
            UITestControl DocManager = new UITestControl(anItem);
            DocManager.SearchProperties["AutomationId"] = "UI_DocManager_AutoID";
            DocManager.Find();

            // Find the left pane window
            UITestControl DockLeft = new UITestControl(DocManager);
            DockLeft.SearchProperties["AutomationId"] = "DockLeft";
            DockLeft.Find();

            // Find the tab page window
            UITestControlCollection dockLeftChildren = DockLeft.GetChildren()[0].GetChildren();
            //var TabPage = dockLeftChildren.FirstOrDefault(c => c.FriendlyName == "Explorer");
            var TabPage = dockLeftChildren[0];

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

        public UITestControl GetServerDDL()
        {
            return VisualTreeWalker.GetChildByAutomationIDPath(_explorerNewConnectionControl, "UI_ExplorerServerCbx_AutoID");
        }

        public UITestControl GetExplorerEditBtn()
        {
            // Find the explorer main window
            UITestControl anItem = this.UIBusinessDesignStudioWindow.UIExplorerCustom;
            anItem.Find();

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

            // Find the explorer sub window
            UITestControl explorerMenu = new UITestControl(anItem);
            explorerMenu.SearchProperties["AutomationId"] = "Explorer";
            explorerMenu.Find();

            UITestControl serverDDL = new UITestControl(explorerMenu);
            serverDDL.SearchProperties["AutomationId"] = "UI_ExplorerNewBtn_AutoID";
            serverDDL.Find();
            return serverDDL;
        }

        private UITestControl GetServiceItem(string serverName, string folderName, string projectName, bool overrideDblClickBehavior = false)
        {
            Playback.Wait(200);

            var args = new string[] { serverName, folderName, projectName };

            var parent = _explorerTree;

            foreach(var arg in args)
            {
                if(parent != null)
                {
                    var kids = parent.GetChildren();

                    UITestControl canidate = null;
                    foreach(var kid in kids)
                    {
                        if(kid.Exists)
                        {
                            var id = kid.GetProperty("AutomationID").ToString();

                            if(id.ToUpper().Contains(arg.ToUpper()) ||
                                kid.FriendlyName.ToUpper().Equals(arg.ToUpper()) ||
                                kid.ControlType.Name.ToUpper().Equals(arg.ToUpper()) ||
                                kid.ClassName.ToUpper().Contains(arg.ToUpper()))
                            {
                                if(arg == projectName)
                                {
                                    if(id == "UI_" + projectName + "_AutoID")
                                    {
                                        canidate = kid;
                                    }
                                }
                                else
                                {
                                    canidate = kid;
                                }
                            }
                        }
                    }

                    if(canidate != null)
                    {
                        parent = canidate;
                    }
                }
            }

            return parent;

        }

        private UITestControl GetServiceItem(string serverName, string projectName, bool overrideDblClickBehavior = false)
        {
            Playback.Wait(200);

            var args = new string[] { serverName, projectName };

            var parent = _explorerTree;

            foreach(var arg in args)
            {
                if(parent != null)
                {
                    var kids = parent.GetChildren();

                    UITestControl canidate = null;
                    foreach(var kid in kids)
                    {
                        if(kid.Exists)
                        {
                            var id = kid.GetProperty("AutomationID").ToString();

                            if(id.ToUpper().Contains(arg.ToUpper()) ||
                                kid.FriendlyName.ToUpper().Equals(arg.ToUpper()) ||
                                kid.ControlType.Name.ToUpper().Equals(arg.ToUpper()) ||
                                kid.ClassName.ToUpper().Contains(arg.ToUpper()))
                            {
                                if(arg == projectName)
                                {
                                    if(id == "UI_" + projectName + "_AutoID")
                                    {
                                        canidate = kid;
                                    }
                                }
                                else
                                {
                                    canidate = kid;
                                }
                            }
                        }
                    }

                    if(canidate != null)
                    {
                        parent = canidate;
                    }
                }
            }

            return parent;

        }

        /// <summary>
        /// Enters the explorer search text.
        /// </summary>
        /// <param name="textToSearchWith">The text automatic search with.</param>
        /// <param name="waitAmt"></param>
        public void EnterExplorerSearchText(string textToSearchWith, int waitAmt = 0)
        {
            _explorerSearch.EnterText(textToSearchWith);
            Playback.Wait(waitAmt);
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
                    if((this.mUIUI_DataListSearchtxtEdit == null))
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
                    if((this.mUIFilterTextBoxEdit == null))
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
                    if((this.mUITheNavigationViewCustom == null))
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



        private UITestControl GetConnectedServer(string serverName)
        {
            UITestControl server = null;
            var kids = _explorerTree.GetChildren();
            foreach(UITestControl serverListItem in kids) // 0 for the first server in the list
            {
                var automationID = serverListItem.GetProperty(WpfTree.PropertyNames.AutomationId).ToString();
                if(automationID.Contains(serverName))
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
            foreach(UITestControl serviceTypeListItem in server.GetChildren())
            {
                if(serviceTypeListItem.FriendlyName.Contains(serviceType))
                {
                    // If the service type is not visible, expand the server list
                    if(!serviceTypeListItem.TryGetClickablePoint(out p))
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
                if((this.mUIBusinessDesignStudioWindow == null))
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
                if((this.mUINavigationViewUserCoCustom == null))
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
                if((this.mUIExplorerCustom == null))
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

                if((this.mUIUI_txtSearch_AutoIDEdit == null))
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
                if((this.mUINavigationViewUserCoCustom == null))
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
                if((this.mUIUI_WORKFLOWSERVICES_TreeItem == null))
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
                if((this.mUITvExplorerTree == null))
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
                if((this.mUIUI_localhosthttp1270TreeItem == null))
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
                if((this.mUIUI_WORKFLOWSERVICES_TreeItem == null))
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
