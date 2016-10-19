using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Data
{
    [CodedUITest]
    public class AssignToolInUnpinnedWorkflowTab
    {
        [TestMethod]
        public void AssignToolInUnpinnedWorkflowTabOpenAndCloseLargeViewWithDoubleClickUITest()
        {
            UIMap.Open_Assign_Tool_On_Unpinned_Tab_Large_View();
            UIMap.Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeInvalidVariableName_On_Unpinned_Tab();
            UIMap.Click_Assign_Tool_Large_View_Done_Button_With_Row1_Variable_Textbox_As_SomeInvalidVariableName_On_Unpinned_Tab();
            UIMap.Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariable_On_Unpinned_Tab();
            UIMap.Click_Assign_Tool_Large_View_Done_Button_On_Unpinned_Tab();
        }

        [TestMethod]
        public void AssignToolInUnpinnedWorkflowTabOpenAndCloseLargeViewWithExpandAllToggleUITest()
        {
            UIMap.Click_Unpinned_Workflow_ExpandAll();
            Assert.IsTrue(UIMap.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.Exists, "Multiassign large view does not exist after openning it with the expand all button on unpinned tab.");
            UIMap.Click_Unpinned_Workflow_CollapseAll();
            Assert.IsTrue(UIMap.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.Exists, "Multiassign small view does not exist after collapsing it with the collapse all button on unpinned tab.");
        }

        [TestMethod]
        public void AssignToolInUnpinnedWorkflowTabUrlUITest()
        {
            UIMap.Click_Assign_Tool_url_On_Unpinned_Tab();
        }

        [TestMethod]
        public void AssignToolInUnpinnedWorkflowTabQviUITest()
        {
            UIMap.Open_Assign_Tool_Qvi_Large_View_On_Unpinned_Tab();
        }

        [TestMethod]
        public void AssignToolInUnpinnedWorkflowTabDebugOutputUITest()
        {
            UIMap.Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_1_On_Unpinned_tab();
            UIMap.Debug_Workflow_With_Ribbon_Button();
            UIMap.Click_Debug_Output_Assign_Cell_For_Unpinned_Workflow_Tab();
        }

        [TestMethod]
        public void AssignToolInUnpinnedWorkflowTabAddRemoveVariablesUITest()
        {
            const string Variable1Name = "SomeVariable";
            const string Variable1Value = "50";
            UIMap.Enter_Variable_And_Value_Into_Assign_On_Unpinned_Tab("[[" + Variable1Name + "]]", Variable1Value, 1);
            Assert.AreEqual(Variable1Name, UIMap.MainStudioWindow.DockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.Text, "Scalar variable not found in variable list after adding to assign tool row 1.");
            const string Variable2Name = "SomeOtherVariable";
            const string Variable2Value = "100";
            UIMap.Enter_Variable_And_Value_Into_Assign_On_Unpinned_Tab("[[" + Variable2Name + "]]", Variable2Value, 2);
            Assert.AreEqual(Variable2Name, UIMap.MainStudioWindow.DockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem2.ScrollViewerPane.NameTextbox.Text, "Scalar variable not found in variable list after adding to assign tool row 2.");
            UIMap.Remove_Assign_Row_1_With_Context_Menu_On_Unpinned_Tab();
        }

        [TestMethod]
        public void AssignToolInUnpinnedWorkflowTabDeleteToolUITest()
        {
            UIMap.Delete_Assign_With_Context_Menu_On_Unpinned_Tab();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
#if !DEBUG
            UIMap.CloseHangingDialogs();
#endif
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Unpin_Tab_With_Drag(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab);
            UIMap.Drag_Toolbox_MultiAssign_Onto_Unpinned_DesignSurface();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            UIMap.Restore_Unpinned_Tab_Using_Context_Menu();
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
