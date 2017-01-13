﻿using Microsoft.VisualStudio.TestTools.UITesting;
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
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.AssignToNameTreeItem.Exists);
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
            UIMap.Save_With_Ribbon_Button_And_Dialog(RandomNewWorkFlow);
            UIMap.Press_F6();
            UIMap.Click_Create_Test_From_Debug();
            UIMap.Click_EnableDisable_This_Test_CheckBox(true);
            UIMap.Click_Delete_Test_Button();
            UIMap.Click_MessageBox_Yes();
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

        #endregion
    }
}
