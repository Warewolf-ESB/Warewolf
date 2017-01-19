using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Workflow
{
    [CodedUITest]
    public class Shortcut_Keys
    {
        private const string Folder = "Acceptance Tests";
        private const string HelloWorld = "Hello World";
        
        [TestMethod]
        [TestCategory("Shortcut Keys")]
        public void Shortcut_Control_W_Opens_NewWorkflow()
        {
            UIMap.Click_LocalHost_Once();
            UIMap.Create_New_Workflow_Using_Shortcut();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.Exists);
        }

        [TestMethod]
        [TestCategory("Shortcut Keys")]
        public void Shortcut_Control_S_Opens_SaveWorkflow_Dialog_For_New_Workflows()
        {
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_MultiAssign_Onto_DesignSurface();
            UIMap.Save_Workflow_Using_Shortcut();
            Assert.IsTrue(UIMap.SaveDialogWindow.Exists);
            UIMap.Click_SaveDialog_CancelButton();
        }

        [TestMethod]
        [TestCategory("Shortcut Keys")]
        public void Shortcut_Control_S_Saves_Dirty_Workflows()
        {
            UIMap.Filter_Explorer("Hello World");
            UIMap.Open_ExplorerFirstItem_From_ExplorerContextMenu();
            UIMap.Move_Assign_Message_Tool_On_The_Design_Surface();
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled);
            UIMap.Save_Workflow_Using_Shortcut();
            Assert.IsFalse(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled);
        }

        [TestMethod]
        [TestCategory("Shortcut Keys")]
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

        [TestMethod]
        [TestCategory("Shortcut Keys")]
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
        [TestCategory("Shortcut Keys")]
        public void Shortcut_Control_D_Opens_DeployWizardTab()
        {
            UIMap.Click_LocalHost_Once();
            UIMap.Open_Deploy_Using_Shortcut();
            UIMap.WaitForControlVisible(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.Exists);
            UIMap.Click_Close_Deploy_Tab_Button();
        }


        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
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
