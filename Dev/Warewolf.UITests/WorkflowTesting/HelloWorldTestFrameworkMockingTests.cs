using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.Common;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class HelloWorldTestFrameworkMockingTests
    {
        private const string HelloWorld = "Hello World";
        private const string Message = "Hello There World";

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void ClickGenerateTestFromDebugCreatesTestSteps()
        {
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
        }
        
        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void ClickNewTestWithUnsavedExistingTest()
        {            
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
            UIMap.Try_Click_Create_New_Tests();
            Assert.IsTrue(UIMap.MessageBoxWindow.Exists);
            Assert.IsTrue(UIMap.MessageBoxWindow.SaveBeforeAddingTest.Exists);
            UIMap.Click_MessageBox_OK();
        }
        
        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void ClickRunTestStepAfterCreatingTestHasAllTestsPassing()
        {
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
            UIMap.Click_Run_Test_Button(TestResultEnum.Pass,4);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.Passing.Exists);
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
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
        [TestCategory("Workflow Testing")]
        public void ClickDeleteTestStepRemovesTestStepFromTest()
        {
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
            UIMap.Click_Delete_On_AssignValue_TestStep();
            Point point;
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.AssignToNameTreeItem.AssignAssert.TryGetClickablePoint(out point));
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void SelectMockForTestStepAssignNameHidesTheTestStatusIcon()
        {
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
            UIMap.Click_MockRadioButton_On_AssignValue_TestStep();
            Point point;
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.AssignToNameTreeItem.AssignAssert.AssertHeader.Pending.TryGetClickablePoint(out point));
        }
        
        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void ClickAssignNameToolOnDesignSurfaceAddsTestSteps()
        {
            UIMap.Press_F6();
            UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.CreateTestFromDebugButton.DrawHighlight();
            UIMap.Click_Create_Test_From_Debug();
            UIMap.Click_Delete_On_AssignValue_TestStep();
            UIMap.Click_AssigName_From_DesignSurface();
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void ChangingTheOutputMessageShouldFailTestSteps()
        {
            UIMap.Press_F6();
            UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.CreateTestFromDebugButton.DrawHighlight();
            UIMap.Click_Create_Test_From_Debug();
            UIMap.EnterOutMessageValue_On_OutputMessage_TestStep(Message);
            UIMap.Click_Run_Test_Button(TestResultEnum.Pass, 4);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.SetOutputTreeItem.OutputMessageAssert.AssertHeader.Failed.Exists);
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
