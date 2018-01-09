﻿using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.Common;
using Warewolf.UI.Tests.DialogsUIMapClasses;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.WorkflowServiceTesting.WorkflowServiceTestingUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UI.Tests
{
    [CodedUITest]
    public class HelloWorldTestFrameworkMockingTests
    {
        private const string HelloWorld = "Hello World";
        private const string Message = "Hello There World";

        [TestMethod]
        [TestCategory("Hello World Mocking Tests")]
        public void ClickGenerateTestFromDebugCreatesTestSteps()
        {   
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.RunAllButton.Exists, "Run All Button does not exist on service test tab after openning it by clicking the button in Hello World debug output.");
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.UrlText.Exists, "Test Url does not exist on service test tab after openning it by clicking the button in Hello World debug output.");
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Exists, "Test 1 does not exist on service test tab after openning it by clicking the button in Hello World debug output.");
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test2.Exists, "Test 2 does not exist on service test tab after openning it by clicking the button in Hello World debug output.");
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test3.Exists, "Test 3 does not exist on service test tab after openning it by clicking the button in Hello World debug output.");
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.Exists, "Test 4 does not exist on service test tab after openning it by clicking the button in Hello World debug output.");
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.CreateTest.Exists, "Create New Test Button does not exist on service test tab after openning it by clicking the button in Hello World debug output.");
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DecisionTreeItem.Exists, "Decision test step does not exist on service test tab after openning it by clicking the button in Hello World debug output.");
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.AsssignNameTreeItem.Exists, "Assign To Name Test Step does not exist on service test tab after openning it by clicking the button in Hello World debug output.");
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.SetOutputTreeItem.Exists, "Set The Output Variable Test Step does not exist on service test tab after openning it by clicking the button in Hello World debug output.");  
        }
        
        [TestMethod]
        [TestCategory("Hello World Mocking Tests")]
        public void ClickNewTestWithUnsavedExistingTest()
        {
            WorkflowServiceTestingUIMap.Click_Workflow_Testing_Tab_Create_New_Test_Button();
            Assert.IsTrue(DialogsUIMap.MessageBoxWindow.Exists, "Messagebox warning about unsaved tests does not exist after clicking create new test.");
            DialogsUIMap.Click_Save_Before_Continuing_MessageBox_OK();
            WorkflowServiceTestingUIMap.Click_Close_Tests_Tab();
        }
        
        [TestMethod]
        [TestCategory("Hello World Mocking Tests")]
        public void ClickRunTestStepAfterCreatingTestHasAllTestsPassing()
        {
            WorkflowServiceTestingUIMap.Click_Run_Test_Button(TestResultEnum.Pass, 4);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.Passing.Exists);
            WorkflowServiceTestingUIMap.Click_EnableDisable_This_Test_CheckBox(true, 4);
            WorkflowServiceTestingUIMap.Click_Delete_Test_Button(4);
            DialogsUIMap.Click_MessageBox_Yes();
        }
        
        [TestMethod]
        [TestCategory("Hello World Mocking Tests")]
        public void ClickDeleteTestStepRemovesTestStepFromTest()
        {
            WorkflowServiceTestingUIMap.Click_Delete_On_AssignValue_TestStep();
            Point point;
            Assert.IsFalse(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.SetOutputTreeItem.OutputMessageAssert.TryGetClickablePoint(out point), "Test step still visible after clicking the delete button on that test step.");
            WorkflowServiceTestingUIMap.Click_EnableDisable_This_Test_CheckBox(true, 4);
            WorkflowServiceTestingUIMap.Click_Delete_Test_Button(4);
            DialogsUIMap.Click_MessageBox_Yes();
        }

        [TestMethod]
        [TestCategory("Hello World Mocking Tests")]
        public void SelectMockForTestStepAssignNameHidesTheTestStatusIcon()
        {
            WorkflowServiceTestingUIMap.Click_MockRadioButton_On_AssignValue_TestStep();
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.AsssignNameTreeItem.AssignAssert.SmallDataGridTable.Row1.Exists, "Row1 is not visible after selecting Mock.");
        }
        
        [TestMethod]
        [TestCategory("Hello World Mocking Tests")]
        public void ClickAssignNameToolOnDesignSurfaceAddsTestSteps()
        {
            WorkflowServiceTestingUIMap.Click_Delete_On_AssignValue_TestStep();
            WorkflowServiceTestingUIMap.PinUnpinOutPutButton();
            WorkflowServiceTestingUIMap.Click_Output_Step();
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.SetOutputTreeItem.OutputMessageAssert.Exists, "Test assert/mock step not added after clicking activity on design surface.");
        }

        [TestMethod]
        [TestCategory("Hello World Mocking Tests")]
        public void ChangingTheOutputMessageShouldFailTestSteps()
        {
            WorkflowServiceTestingUIMap.Click_Run_Test_Button(TestResultEnum.Fail, 4);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.Failing.Exists, "Failed status icon does not exist after running a text with the wrong output message.");
            WorkflowServiceTestingUIMap.Click_EnableDisable_This_Test_CheckBox(true, 4);
            WorkflowServiceTestingUIMap.Click_Delete_Test_Button(4);
            DialogsUIMap.Click_MessageBox_Yes();
        }

        [TestMethod]
        [TestCategory("Hello World Mocking Tests")]
        public void DuplicatedWorkflowShouldUpdateTestResult()
        {
            ExplorerUIMap.Filter_Explorer("Hello World");
            ExplorerUIMap.Duplicate_FirstResource_From_ExplorerContextMenu();
            WorkflowTabUIMap.Enter_Duplicate_workflow_name("Duplicated_HelloWorld_Testing");
            DialogsUIMap.Click_Duplicate_From_Duplicate_Dialog();
            ExplorerUIMap.Filter_Explorer("Duplicated_HelloWorld_Testing");
            ExplorerUIMap.DoubleClick_Explorer_Localhost_First_Item();
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();

            WorkflowServiceTestingUIMap.Click_Run_Test_Button(TestResultEnum.Pass, 1);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Passing.Exists, "Failed status icon does not exist after running a test with the new duplicated workflow.");
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            ExplorerUIMap.Filter_Explorer(HelloWorld);
            ExplorerUIMap.DoubleClick_Explorer_Localhost_First_Item();
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

        #endregion
    }
}
