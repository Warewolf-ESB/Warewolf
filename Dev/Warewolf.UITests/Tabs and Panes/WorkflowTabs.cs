using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Workflow
{
    [CodedUITest]
    public class WorkflowTabs
    {
        [TestMethod]
        [TestCategory("Tabs and Panes")]
        public void Workflow_Name_Counter()
        {
            UIMap.Create_New_Workflow_Using_Shortcut();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.Exists, "Workflow exists after creating it with shortcut");
        }

        [TestMethod]
        [TestCategory("Tabs and Panes")]
        public void Unsaved_Workflow_Name_Asterisk()
        {
            UIMap.Create_New_Workflow_Using_Shortcut();
            UIMap.Make_Workflow_Savable_By_Dragging_Start();
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
