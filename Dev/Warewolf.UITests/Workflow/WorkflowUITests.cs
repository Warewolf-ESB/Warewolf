using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Workflow
{
    [CodedUITest]
    public class WorkflowUITests
    {
        [TestMethod]
        public void Shortcut_Control_W_Opens_NewWorkflow()
        {
            UIMap.Click_LocalHost_Once();
            UIMap.Create_New_Workflow_Using_Shortcut();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.Exists);
        }

        [TestMethod]
        public void Shortcut_Control_W_Opens_NewWorkflow_In_The_Selected_Folder()
        {
            UIMap.Filter_Explorer("Acceptance Tests");
            UIMap.Click_Explorer_Localhost_First_Item();
            UIMap.Create_New_Workflow_Using_Shortcut();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.Exists);
        }

        [TestMethod]
        public void Shortcut_Control_D_Opens_DeployTab()
        {
            UIMap.Click_LocalHost_Once();
            UIMap.Open_Deploy_Using_Shortcut();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.Exists);
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
