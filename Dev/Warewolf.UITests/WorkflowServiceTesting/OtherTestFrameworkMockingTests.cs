using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;
using Warewolf.UITests.Common;
using Warewolf.UITests.ExplorerUIMapClasses;
using Warewolf.UITests.WorkflowTesting.WorkflowServiceTestingUIMapClasses;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class OtherTestFrameworkMockingTests
    {
        private const string Resource = "Resource For MockRadioButton";
        
        [TestMethod]
        [TestCategory("Workflow Mocking Tests")]
        public void SettingTestStepToMockDoesNotAffectTestOutput()
        {
            ExplorerUIMap.Filter_Explorer(Resource);
            ExplorerUIMap.DoubleClick_Explorer_Localhost_First_Item();
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.StartNode.Exists);
            UIMap.Press_F6();            
            UIMap.Click_Create_Test_From_Debug();
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.Exists, "Test tab does not exist after clicking Create Test from debug button");
            WorkflowServiceTestingUIMap.Click_MockRadioButton_On_TestStep();
            WorkflowServiceTestingUIMap.Click_Run_Test_Button(TestResultEnum.Pass);
            Point point;
            Assert.IsFalse(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Passing.TryGetClickablePoint(out point), "Passing status icon is still visible on test after running test with mocking enabled.");
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

        WorkflowTabUIMap WorkflowTabUIMap
        {
            get
            {
                if (_WorkflowTabUIMap == null)
                {
                    _WorkflowTabUIMap = new WorkflowTabUIMap();
                }

                return _WorkflowTabUIMap;
            }
        }

        private WorkflowTabUIMap _WorkflowTabUIMap;

        WorkflowServiceTestingUIMap WorkflowServiceTestingUIMap
        {
            get
            {
                if (_WorkflowServiceTestingUIMap == null)
                {
                    _WorkflowServiceTestingUIMap = new WorkflowServiceTestingUIMap();
                }

                return _WorkflowServiceTestingUIMap;
            }
        }

        private WorkflowServiceTestingUIMap _WorkflowServiceTestingUIMap;

        ExplorerUIMap ExplorerUIMap
        {
            get
            {
                if (_ExplorerUIMap == null)
                {
                    _ExplorerUIMap = new ExplorerUIMap();
                }

                return _ExplorerUIMap;
            }
        }

        private ExplorerUIMap _ExplorerUIMap;

        #endregion
    }
}
