using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class TestFrameworkMockingTests
    {
        private const string HelloWorld = "Hello World";
        private const string RandomWorkFlow = "RandomTool WorkFlow";

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void StepsWithoutOutputsShouldBeMarkedInvalid()
        {
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_MultiAssign_Onto_DesignSurface();
            UIMap.Save_With_Ribbon_Button_And_Dialog("AssignWorkflow");
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.AssignToNameTreeItem.Exists);
            UIMap.Click_SaveDialog_Save_Button();
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void CreateTestFromDebugUsingUnsvaceWorkflow()
        {
            UIMap.Click_Create_Test_From_Debug();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.AssignToNameTreeItem.Exists);
            UIMap.Click_SaveDialog_Save_Button();
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
            UIMap.Filter_Explorer(HelloWorld);
            UIMap.Drag_Dice_Onto_Dice_On_The_DesignSurface();
            UIMap.Press_F6();
            UIMap.Save_With_Ribbon_Button_And_Dialog("NesetedWF");
            UIMap.Click_Create_Test_From_Debug();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DiceRollTreeItem.Exists);
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void CreateNewTestThenCreateTestFromDebugOutput()
        {
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_Random_Onto_DesignSurface();
            UIMap.Enter_Dice_Roll_Values();
            UIMap.Save_With_Ribbon_Button_And_Dialog(RandomWorkFlow);
            UIMap.Filter_Explorer(RandomWorkFlow);
            UIMap.Open_Explorer_First_Item_Tests_With_Context_Menu();
            UIMap.Click_Create_New_Tests(true);
            UIMap.Click_New_Workflow_Tab();
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
            Assert.IsTrue(UIMap.MessageBoxWindow.Exists);
            Assert.IsTrue(UIMap.MessageBoxWindow.SaveBeforeAddingTest.Exists);
            UIMap.Click_MessageBox_OK();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.Exists);
        }

        [TestMethod]
        [TestCategory("Workflow Testing")]
        public void CreateTestFromDebugOutputDeleteTestButDontCloseTestTabGoBackAndCreateTestAgain()
        {
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_Random_Onto_DesignSurface();
            UIMap.Enter_Dice_Roll_Values();
            UIMap.Save_With_Ribbon_Button_And_Dialog(RandomWorkFlow);
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
            UIMap.Click_EnableDisable_This_Test_CheckBox(true);
            UIMap.Click_Delete_Test_Button();
            UIMap.Click_New_Workflow_Tab();
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.RandomTreeItem.Exists);
        }


        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
#if !DEBUG
            UIMap.CloseHangingDialogs();
#endif            
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
