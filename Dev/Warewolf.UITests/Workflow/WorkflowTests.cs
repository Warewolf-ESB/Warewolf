using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Workflow
{
    [CodedUITest]
    public class WorkflowUITests
    {
        private const string Folder = "Acceptance Tests";
        private const string HelloWorld = "Hello World";

        [TestMethod]
        public void Unsaved_Workflow_Name_Counter()
        {
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.DrawHighlight();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.TabDescription.DisplayText.Contains("Unsaved 1"));
            UIMap.Click_Close_Workflow_Tab_Button();
            UIMap.Click_New_Workflow_Ribbon_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.UIUnsaved1Text.DisplayText.Contains("Unsaved 1"));
        }

        [TestMethod]
        public void Shortcut_Control_W_Opens_NewWorkflow()
        {
            UIMap.Click_LocalHost_Once();
            UIMap.Create_New_Workflow_Using_Shortcut();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.Exists);
        }

        [TestMethod]
        public void Shortcut_Control_S_Opens_SaveWorkflow_Dialog_For_New_Workflows()
        {
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_MultiAssign_Onto_DesignSurface();
            UIMap.Save_Workflow_Using_Shortcut();
            Assert.IsTrue(UIMap.SaveDialogWindow.Exists);
            UIMap.Click_SaveDialog_CancelButton();
            UIMap.Click_Close_Workflow_Tab_Button();
            UIMap.Click_MessageBox_No();
        }
        [TestMethod]
        public void Shortcut_Control_S_Saves_Dirty_Workflows()
        {
            UIMap.Filter_Explorer("Hello World");
            UIMap.Open_Explorer_First_Item_With_Context_Menu();
            UIMap.Move_Assign_Message_Tool_On_The_Design_Surface();
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled);
            UIMap.Save_Workflow_Using_Shortcut();
            Assert.IsFalse(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled);
        }

        [TestMethod]
        public void Shortcut_Control_W_Opens_NewWorkflow_In_The_Selected_Folder()
        {
            UIMap.Filter_Explorer(Folder);
            UIMap.Click_Explorer_Localhost_First_Item();
            UIMap.Create_New_Workflow_In_Explorer_First_Item_With_Shortcut();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.TopScrollViewerPane.HttpLocalHostText.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.TopScrollViewerPane.HttpLocalHostText.NewWorkflowHyperLink.Alt.Contains(Folder));
        }

        [TestMethod]
        public void Shortcut_Control_D_Opens_DeployTab()
        {
            UIMap.Click_LocalHost_Once();
            UIMap.Open_Deploy_Using_Shortcut();
            UIMap.WaitForControlVisible(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.Exists);
            UIMap.Click_Close_Deploy_Tab_Button();
        }

        [TestMethod]
        public void Shortcut_Control_D_Opens_DeployTabWith_Resource_Selected()
        {
            UIMap.Filter_Explorer(HelloWorld);
            UIMap.Click_Explorer_Localhost_First_Item();
            UIMap.Open_Deploy_Using_Shortcut();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.Exists);
            UIMap.Filter_Deploy_Source_Explorer(HelloWorld);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.LocalHost.Item1.CheckBox.Checked);
            UIMap.Click_Close_Deploy_Tab_Button();
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
        }

        UIMap UIMap
        {
            get
            {
                if (_UIMap == null)
                {
                    _UIMap = new UIMap();
                }

                return _UIMap;
            }
        }

        private UIMap _UIMap;

        #endregion
    }
}
