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
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.RunAllButton.Exists, "Run All Button does not exist on service test tab after openning it by clicking the button in Hello World debug output.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.UrlText.Exists, "Test Url does not exist on service test tab after openning it by clicking the button in Hello World debug output.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Exists, "Test 1 does not exist on service test tab after openning it by clicking the button in Hello World debug output.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test2.Exists, "Test 2 does not exist on service test tab after openning it by clicking the button in Hello World debug output.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test3.Exists, "Test 3 does not exist on service test tab after openning it by clicking the button in Hello World debug output.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.Exists, "Test 4 does not exist on service test tab after openning it by clicking the button in Hello World debug output.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.CreateTest.Exists, "Create New Test Button does not exist on service test tab after openning it by clicking the button in Hello World debug output.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DecisionTreeItem.Exists, "Decision test step does not exist on service test tab after openning it by clicking the button in Hello World debug output.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.AssignToNameTreeItem.Exists, "Assign To Name Test Step does not exist on service test tab after openning it by clicking the button in Hello World debug output.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.SetOutputTreeItem.Exists, "Set The Output Variable Test Step does not exist on service test tab after openning it by clicking the button in Hello World debug output.");
        }
        
        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void ClickNewTestWithUnsavedExistingTest()
        {
            UIMap.Try_Click_Create_New_Tests();
            Assert.IsTrue(UIMap.MessageBoxWindow.Exists, "Messagebox warning about unsaved tests does not exist after clicking create new test.");
            Assert.IsTrue(UIMap.MessageBoxWindow.SaveBeforeAddingTest.Exists, "Messagebox does not warn about unsaved tests after clicking create new test.");
            UIMap.Click_MessageBox_OK();
        }
        
        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void ClickRunTestStepAfterCreatingTestHasAllTestsPassing()
        {
            UIMap.Click_Run_Test_Button(TestResultEnum.Pass, 4);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.Passing.Exists);
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void SettingTestStepToMockDoesNotAffectTestOutput()
        {
            UIMap.Click_MockRadioButton_On_Decision_TestStep();
            Point point;
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DecisionTreeItem.DecisionAssert.AssertHeader.Pending.TryGetClickablePoint(out point), "Pending status icon is still visible on decision test step after checking the mock radio button.");
            UIMap.Click_Run_Test_Button(TestResultEnum.Pass, 4);
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.Passing.TryGetClickablePoint(out point), "Passing status icon is still visible on test after running test with mocking enabled.");
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void ClickDeleteTestStepRemovesTestStepFromTest()
        {
            UIMap.Click_Delete_On_AssignValue_TestStep();
            Point point;
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.AssignToNameTreeItem.AssignAssert.TryGetClickablePoint(out point), "Test step still visible after clicking the delete button on that test step.");
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void SelectMockForTestStepAssignNameHidesTheTestStatusIcon()
        {
            UIMap.Click_MockRadioButton_On_AssignValue_TestStep();
            Point point;
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.AssignToNameTreeItem.AssignAssert.AssertHeader.Pending.TryGetClickablePoint(out point), "Pending status icon is still visible on assign test step after checking the mock radio button.");
        }
        
        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void ClickAssignNameToolOnDesignSurfaceAddsTestSteps()
        {
            UIMap.Click_Delete_On_AssignValue_TestStep();
            UIMap.Click_AssigName_From_DesignSurface();
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void ChangingTheOutputMessageShouldFailTestSteps()
        {
            UIMap.EnterOutMessageValue_On_OutputMessage_TestStep(Message);
            UIMap.Click_Run_Test_Button(TestResultEnum.Pass, 4);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.SetOutputTreeItem.OutputMessageAssert.AssertHeader.Failed.Exists, "Failed status icon does not exist after running a text with the wrong output message.");
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
            UIMap.DoubleClick_Explorer_Localhost_First_Item();
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
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
