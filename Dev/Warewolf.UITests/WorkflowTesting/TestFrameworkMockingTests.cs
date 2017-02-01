using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class TestFrameworkMockingTests
    {
        private const string HelloWorld = "Hello World";
        private const string RandomWorkFlow = "RandomToolWorkFlow";
        private const string RandomNewWorkFlow = "RandomToolNewWorkFlow";
        private const string DiceRoll = "Dice Roll";
        private const string Nestedwf = "NestedWF";

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void StepsWithoutOutputsShouldBeMarkedInvalid()
        {
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_MultiAssign_Onto_DesignSurface();
            UIMap.Save_With_Ribbon_Button_And_Dialog("AssignWorkflow");
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.Exists, "Test tab does not exist after clicking Create Test from debug button");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.AssignToNameTreeItem.Exists);
            UIMap.Click_Save_Ribbon_Button_Without_Expecting_A_Dialog();
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void CreateTestFromDebugUsingUnsvaceWorkflow()
        {
            UIMap.Filter_Explorer(HelloWorld);            
            UIMap.DoubleClick_Explorer_Localhost_First_Item();
            UIMap.Move_Assign_Message_Tool_On_The_Design_Surface();
            UIMap.Press_F6();
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.CreateTestFromDebugButton.Enabled);           
        }
        
        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void CreateTestFromDebugButtonDisabledForUnsavedWorkflows()
        {
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_Random_Onto_DesignSurface();
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.CreateTestFromDebugButton.Enabled);
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void NestedWorkflowCreatsATestStepAfterClickingCreateTestFromDebugButton()
        {
            UIMap.Click_New_Workflow_Ribbon_Button();            
            UIMap.Filter_Explorer(DiceRoll);
            UIMap.Drag_Explorer_Localhost_First_Items_First_Sub_Item_Onto_Workflow_Design_Surface();
            UIMap.Drag_Dice_Onto_Dice_On_The_DesignSurface();
            UIMap.Press_F6();            
            UIMap.Save_With_Ribbon_Button_And_Dialog(Nestedwf);
            UIMap.Click_Create_Test_From_Debug();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.Exists, "Test tab does not exist after clicking Create Test from debug button");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DiceRollTreeItem.Exists);           
        }

//        Re-Introduce this test when the decision can be click on the test editor design surface
//        Test was added as part of work for WOLF-2381
//        [TestMethod]
//        [Owner("Hagashen Naidu")]
//        [TestCategory("WorkflowTesting_AddTestStep")]
//        public void WorkflowTesting_AddTestStep_WhenStepClickedAfterRun_ShouldAddCorrectStep()
//        {
//            //------------Setup for test--------------------------           
//            UIMap.Filter_Explorer(HelloWorld);
//            UIMap.Open_ExplorerFirstItemTests_With_ExplorerContextMenu();
//            UIMap.Click_Create_New_Tests(true, 4);
//            UIMap.Click_Run_Test_Button(TestResultEnum.Fail, 4);
//            //------------Assert Preconditions-------------------
//            //------------Execute Test---------------------------            
//            Mouse.Click(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.UserControl_1Custom.ScrollViewerPane.ActivityBuilderCustom.WorkflowItemPresenteCustom.FlowchartCustom.DsfDecisioActiviCustom);
//            //------------Assert Results-------------------------
//            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.OutputMessageStep.Exists);
//            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DecisionTreeItem.Exists);
//        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void CreateNewTestThenCreateTestFromDebugOutput()
        {
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_Random_Onto_DesignSurface();
            UIMap.Enter_Dice_Roll_Values();
            UIMap.Save_With_Ribbon_Button_And_Dialog(RandomWorkFlow);
            UIMap.Filter_Explorer(RandomWorkFlow);
            UIMap.Open_ExplorerFirstItemTests_With_ExplorerContextMenu();
            UIMap.Click_Create_New_Tests(true);
            UIMap.Click_New_Workflow_Tab();
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
            Assert.IsTrue(UIMap.MessageBoxWindow.Exists);
            Assert.IsTrue(UIMap.MessageBoxWindow.SaveBeforeAddingTest.Exists);
            UIMap.Click_Save_Before_Continuing_MessageBox_OK();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.Exists);
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void CreateTestFromDebugOutputDeleteTestButDontCloseTestTabGoBackAndCreateTestAgain()
        {
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_Random_Onto_DesignSurface();
            UIMap.Enter_Dice_Roll_Values();
            UIMap.Save_With_Ribbon_Button_And_Dialog(RandomNewWorkFlow);
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.Exists, "Test tab does not exist after clicking Create Test from debug button");
            UIMap.Click_EnableDisable_This_Test_CheckBox(true);
            UIMap.Click_Delete_Test_Button();
            UIMap.Click_MessageBox_Yes();
            UIMap.Click_New_Workflow_Tab();
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.Exists, "Test tab does not exist after clicking Create Test from debug button");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.RandomTreeItem.Exists);
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void CreateTestFromDebugOutputDontSaveCreateAnotherTestFromDebugOutput()
        {
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_Random_Onto_DesignSurface();
            UIMap.Enter_Dice_Roll_Values();
            UIMap.Save_With_Ribbon_Button_And_Dialog("RandomWFForSaveButtonState");
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
            UIMap.Click_New_Workflow_Tab();
            UIMap.Click_Create_Test_From_Debug();
            UIMap.Click_MessageBox_OK();
            UIMap.Save_Button_IsEnabled();
        }


        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            //UIMap.AssertStudioIsRunning();
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
