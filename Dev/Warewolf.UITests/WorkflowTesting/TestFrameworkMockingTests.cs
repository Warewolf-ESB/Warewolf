using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.Common;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class TestFrameworkMockingTests
    {
        private const string HelloWorld = "Hello World";

        [TestMethod]
        public void ClickGenerateTestFromDebugCreatesTestSteps()
        {
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
        }

        [TestMethod]
        public void ClickRunTestStepAfterCreatingTestHasAllTestsPassing()
        {
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
            UIMap.Click_Run_Test_Button(TestResultEnum.Pass,4);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.Passing.Exists);
        }

        [TestMethod]
        public void SettingTestStepToMockDoesNotAffectTestOutput()
        {
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
            UIMap.Click_MockRadioButton_On_Decision_TestStep();
            Point point;
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DecisionTreeItem.DecisionAssert.AssertHeader.Pending.TryGetClickablePoint(out point));
            UIMap.Click_Run_Test_Button(TestResultEnum.Pass,4);
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.Passing.TryGetClickablePoint(out point));
        }

        [TestMethod]
        public void ClickDeleteTestStepRemovesTestStepFromTest()
        {
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
            UIMap.Click_Delete_On_AssignValue_TestStep();
        }

        [TestMethod]
        public void SelectMockForTestStepAssignName()
        {
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
            UIMap.Click_MockRadioButton_On_AssignValue_TestStep();
        }

        [TestMethod]
        public void ClickAssignNameToolOnDesignSurfaceAddsTestSteps()
        {
            UIMap.Press_F6();
            UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.CreateTestFromDebugButton.DrawHighlight();
            UIMap.Click_Create_Test_From_Debug();
            UIMap.Click_Delete_On_AssignValue_TestStep();
            UIMap.Click_AssigName_From_DesignSurface();
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
#if !DEBUG
            UIMap.CloseHangingDialogs();
#endif            
            UIMap.Filter_Explorer(HelloWorld);
            UIMap.Open_Explorer_First_Item_With_Context_Menu();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            UIMap.Click_Close_Workflow_Tab_Button();
            UIMap.Click_Close_Tests_Tab();
            UIMap.Click_MessageBox_No();
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
