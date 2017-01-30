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
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.RunAllButton.Exists, "Run All Button does not exist on service test tab after openning it by clicking the button in Hello World debug output.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.UrlText.Exists, "Test Url does not exist on service test tab after openning it by clicking the button in Hello World debug output.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Exists, "Test 1 does not exist on service test tab after openning it by clicking the button in Hello World debug output.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test2.Exists, "Test 2 does not exist on service test tab after openning it by clicking the button in Hello World debug output.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test3.Exists, "Test 3 does not exist on service test tab after openning it by clicking the button in Hello World debug output.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.Exists, "Test 4 does not exist on service test tab after openning it by clicking the button in Hello World debug output.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.CreateTest.Exists, "Create New Test Button does not exist on service test tab after openning it by clicking the button in Hello World debug output.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DecisionTreeItem.Exists, "Decision test step does not exist on service test tab after openning it by clicking the button in Hello World debug output.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.AssignToNameTreeItem.Exists, "Assign To Name Test Step does not exist on service test tab after openning it by clicking the button in Hello World debug output.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.OutputMessageStep.Exists, "Set The Output Variable Test Step does not exist on service test tab after openning it by clicking the button in Hello World debug output.");  
        }
        
        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void ClickNewTestWithUnsavedExistingTest()
        {
            UIMap.Try_Click_Create_New_Tests();
            Assert.IsTrue(UIMap.MessageBoxWindow.Exists, "Messagebox warning about unsaved tests does not exist after clicking create new test.");
            UIMap.Click_Save_Before_Continuing_MessageBox_OK();
            UIMap.Click_Close_Tests_Tab();
        }
        
        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void ClickRunTestStepAfterCreatingTestHasAllTestsPassing()
        {
            UIMap.Click_Run_Test_Button(TestResultEnum.Pass, 4);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.Passing.Exists);
            UIMap.Click_EnableDisable_This_Test_CheckBox(true, 4);
            UIMap.Click_Delete_Test_Button(4);
            UIMap.Click_MessageBox_Yes();
        }
        
        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void ClickDeleteTestStepRemovesTestStepFromTest()
        {
            UIMap.Click_Delete_On_AssignValue_TestStep();
            Point point;
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.AssignToNameTreeItem.AssignAssert.TryGetClickablePoint(out point), "Test step still visible after clicking the delete button on that test step.");
            UIMap.Click_EnableDisable_This_Test_CheckBox(true, 4);
            UIMap.Click_Delete_Test_Button(4);
            UIMap.Click_MessageBox_Yes();
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void SelectMockForTestStepAssignNameHidesTheTestStatusIcon()
        {
            UIMap.Click_MockRadioButton_On_AssignValue_TestStep(); ;
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DecisionTreeItem.DecisionAssert.SmallDataGridTable.Row1.Exists, "Pending status icon is still visible on assign test step after checking the mock radio button.");
        }
        
        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void ClickAssignNameToolOnDesignSurfaceAddsTestSteps()
        {
            UIMap.Click_Delete_On_AssignValue_TestStep();
            UIMap.Click_Output_Step();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.OutputMessageStep.Exists);
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void ChangingTheOutputMessageShouldFailTestSteps()
        {
            UIMap.Click_Run_Test_Button(TestResultEnum.Fail, 4);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.Failing.Exists, "Failed status icon does not exist after running a text with the wrong output message.");
            UIMap.Click_EnableDisable_This_Test_CheckBox(true, 4);
            UIMap.Click_Delete_Test_Button(4);
            UIMap.Click_MessageBox_Yes();
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
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
