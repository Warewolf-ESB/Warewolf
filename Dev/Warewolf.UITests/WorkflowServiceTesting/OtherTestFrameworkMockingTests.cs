using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;
using Warewolf.UITests.Common;
using Warewolf.UITests.DialogsUIMapClasses;
using Warewolf.UITests.Explorer.ExplorerUIMapClasses;
using Warewolf.UITests.WorkflowServiceTesting.WorkflowServiceTestingUIMapClasses;

namespace Warewolf.UITests.WorkflowServiceTesting
{
    [CodedUITest]
    public class OtherTestFrameworkMockingTests
    {
        private const string Resource = "Resource For MockRadioButton";
        private const string DotnetWfWithObjOutput = "DotnetWfWithObjOutput";
        
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

        [TestMethod]
        [TestCategory("Workflow Mocking Tests")]
        public void WorkflowWithObjectoutPutTests()
        {
            ExplorerUIMap.Filter_Explorer(DotnetWfWithObjOutput);
            ExplorerUIMap.DoubleClick_Explorer_Localhost_First_Item();
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
            WorkflowServiceTestingUIMap.Click_Run_Test_Button(TestResultEnum.Pass);
            WorkflowServiceTestingUIMap.Click_EnableDisable_This_Test_CheckBox(true);
            WorkflowServiceTestingUIMap.Click_Delete_Test_Button();
            DialogsUIMap.Click_MessageBox_Yes();
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

        DialogsUIMap DialogsUIMap
        {
            get
            {
                if (_DialogsUIMap == null)
                {
                    _DialogsUIMap = new DialogsUIMap();
                }

                return _DialogsUIMap;
            }
        }

        private DialogsUIMap _DialogsUIMap;

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
