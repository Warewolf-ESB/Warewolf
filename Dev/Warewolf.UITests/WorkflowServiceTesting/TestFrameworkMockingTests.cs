using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.DialogsUIMapClasses;
using Warewolf.UITests.Explorer.ExplorerUIMapClasses;
using Warewolf.UITests.WorkflowServiceTesting.WorkflowServiceTestingUIMapClasses;
using Warewolf.UITests.Common;
using Warewolf.UITests.WorkflowTab.Tools.Data.DataToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.Utility.UtilityToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UITests.WorkflowServiceTesting
{
    [CodedUITest]
    public class TestFrameworkMockingTests
    {

        [TestMethod]
        [TestCategory("Workflow Mocking Tests")]
        public void StepsWithoutOutputsShouldBeMarkedInvalid()
        {
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.Drag_Toolbox_MultiAssign_Onto_DesignSurface();
            UIMap.Save_With_Ribbon_Button_And_Dialog("AssignWorkflow");
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.Exists, "Test tab does not exist after clicking Create Test from debug button");
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.UIWarewolfStudioViewMoTreeItem.AssignAssert.Exists);
            UIMap.Click_Save_Ribbon_Button_Without_Expecting_A_Dialog();
        }

        [TestMethod]
        [TestCategory("Workflow Mocking Tests")]
        public void WorkflowWithSequenceToolLoadsAllContainedTools()
        {
            ExplorerUIMap.Filter_Explorer("Control Flow - Sequence");
            ExplorerUIMap.DoubleClick_Explorer_Localhost_First_Item();
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.UIUI_VariableTreeView_Tree.SequenceItem.FindUniqueNamesItem.FindOnlyUniqueNamesExpander.Exists);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.UIUI_VariableTreeView_Tree.SequenceItem.SortNamesItem.SortNamesExpandar.Exists);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.UIUI_VariableTreeView_Tree.SequenceItem.SplitNamesItem.SplitNamesExpandar.Exists);
        }

        [TestMethod]
        [TestCategory("Workflow Mocking Tests")]
        public void CreateTestFromDebugButtonDisabledForUnsavedWorkflows()
        {
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.Make_Workflow_Savable_By_Dragging_Start();
            Assert.IsFalse(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.CreateTestFromDebugButton.Enabled);
        }

        [TestMethod]
        [TestCategory("Workflow Mocking Tests")]
        public void NestedWorkflowCreatsATestStepAfterClickingCreateTestFromDebugButton()
        {
            UIMap.Click_NewWorkflow_RibbonButton();
            ExplorerUIMap.Filter_Explorer(WorkflowServiceTestingTests.DiceRoll);
            WorkflowTabUIMap.Drag_Explorer_Localhost_First_Items_First_Sub_Item_Onto_Workflow_Design_Surface();
            WorkflowTabUIMap.Drag_Dice_Onto_Dice_On_The_DesignSurface();
            UIMap.Press_F6();
            UIMap.Save_With_Ribbon_Button_And_Dialog(WorkflowServiceTestingTests.Nestedwf);
            UIMap.Click_Create_Test_From_Debug();
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.Exists, "Test tab does not exist after clicking Create Test from debug button");
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.UIWarewolfStudioViewMoTreeItem.DiceRollExpander.Exists);
        }

        [TestMethod]
        [TestCategory("Workflow Mocking Tests")]
        public void WorkflowTesting_AddDecisionStep_WhenStepClickedAfterRun_ShouldAddCorrectStep()
        {
            //------------Setup for test--------------------------
            ExplorerUIMap.Filter_Explorer("DecisionWF");
            ExplorerUIMap.Open_ExplorerFirstItemTests_With_ExplorerContextMenu();
            WorkflowServiceTestingUIMap.Click_Create_New_Tests(true);
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------                        
            WorkflowServiceTestingUIMap.PinUnpinOutPutButton();
            WorkflowServiceTestingUIMap.Click_DecisionOn_Service_TestView();
            //------------Assert Results-------------------------
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.UIWarewolfStudioViewMoTreeItem.DecisionAssert.Exists);
        }

        [TestMethod]
        [TestCategory("Workflow Mocking Tests")]
        public void CreateNewTestThenCreateTestFromDebugOutput()
        {
            ExplorerUIMap.Filter_Explorer(WorkflowServiceTestingTests.RandomWorkFlow);
            ExplorerUIMap.Open_ExplorerFirstItemTests_With_ExplorerContextMenu();
            WorkflowServiceTestingUIMap.Click_Create_New_Tests(true);
            ExplorerUIMap.Open_Explorer_First_Item_With_Double_Click();
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
            Assert.IsTrue(DialogsUIMap.MessageBoxWindow.Exists);
            Assert.IsTrue(DialogsUIMap.MessageBoxWindow.SaveBeforeAddingTest.Exists);
            DialogsUIMap.Click_Save_Before_Continuing_MessageBox_OK();
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.Exists);
        }

        [TestMethod]
        [TestCategory("Workflow Mocking Tests")]
        public void CreateTestFromDebugOutputDeleteTestButDontCloseTestTabGoBackAndCreateTestAgain()
        {
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.Drag_Toolbox_Random_Onto_DesignSurface();
            UtilityToolsUIMap.Enter_Dice_Roll_Values();
            UIMap.Save_With_Ribbon_Button_And_Dialog(WorkflowServiceTestingTests.RandomNewWorkFlow);
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.Exists, "Test tab does not exist after clicking Create Test from debug button");
            WorkflowServiceTestingUIMap.Click_EnableDisable_This_Test_CheckBox(true);
            WorkflowServiceTestingUIMap.Click_Delete_Test_Button();
            DialogsUIMap.Click_MessageBox_Yes();
            UIMap.Click_New_Workflow_Tab();
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.Exists, "Test tab does not exist after clicking Create Test from debug button");
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.UIWarewolfStudioViewMoTreeItem.RandomTreeItem.Exists);
        }

        [TestMethod]
        [TestCategory("Workflow Mocking Tests")]
        public void CreateTestFromDebugOutputDontSaveCreateAnotherTestFromDebugOutput()
        {
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.Drag_Toolbox_Random_Onto_DesignSurface();
            UtilityToolsUIMap.Enter_Dice_Roll_Values();
            UIMap.Save_With_Ribbon_Button_And_Dialog("RandomWFForSaveButtonState");
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
            UIMap.Click_New_Workflow_Tab();
            UIMap.Click_Create_Test_From_Debug();
            DialogsUIMap.Click_MessageBox_OK();
            UIMap.Save_Button_IsEnabled();
        }

        [TestMethod]
        [TestCategory("Workflow Mocking Tests")]
        public void SettingTestStepToMockDoesNotAffectTestOutput()
        {
            ExplorerUIMap.Filter_Explorer(WorkflowServiceTestingTests.Resource);
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
            ExplorerUIMap.Filter_Explorer(WorkflowServiceTestingTests.DotnetWfWithObjOutput);
            ExplorerUIMap.DoubleClick_Explorer_Localhost_First_Item();
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
            WorkflowServiceTestingUIMap.Click_Run_Test_Button(TestResultEnum.Pass);
            WorkflowServiceTestingUIMap.Click_EnableDisable_This_Test_CheckBox(true);
            WorkflowServiceTestingUIMap.Click_Delete_Test_Button();
            DialogsUIMap.Click_MessageBox_Yes();
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void Cant_Delete_Off_Design_Surface_When_Editting_Tests()
        {
            ExplorerUIMap.Filter_Explorer(WorkflowServiceTestingTests.HelloWorld);
            ExplorerUIMap.DoubleClick_Explorer_Localhost_First_Item();
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.UserControl_1Custom.ScrollViewerPane.ActivityBuilderCustom.WorkflowItemPresenteCustom.FlowchartCustom.MultiAssign2.Exists, "This test expects hello world to have two assign tools on it's deesign surface when editting test mocks.");
            WorkflowServiceTestingUIMap.Click_Output_Step();
            Keyboard.SendKeys("{DEL}");
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.UserControl_1Custom.ScrollViewerPane.ActivityBuilderCustom.WorkflowItemPresenteCustom.FlowchartCustom.MultiAssign2.Exists, "Tool does not exist on the design surface after it was selected with the mouse and delete was pressed.");
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

        UtilityToolsUIMap UtilityToolsUIMap
        {
            get
            {
                if (_UtilityToolsUIMap == null)
                {
                    _UtilityToolsUIMap = new UtilityToolsUIMap();
                }

                return _UtilityToolsUIMap;
            }
        }

        private UtilityToolsUIMap _UtilityToolsUIMap;

        DataToolsUIMap DataToolsUIMap
        {
            get
            {
                if (_DataToolsUIMap == null)
                {
                    _DataToolsUIMap = new DataToolsUIMap();
                }

                return _DataToolsUIMap;
            }
        }

        private DataToolsUIMap _DataToolsUIMap;

        #endregion
    }
}
