using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Data
{
    /// <summary>
    /// Summary description for WorkflowDesignSurface
    /// </summary>
    [CodedUITest]
    public class Assign
    {
        const string WorkflowName = "SomeWorkflow";

        [TestMethod]
		[TestCategory("Tools")]
        public void AssignToolUITest()
        {
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Drag_Toolbox_MultiAssign_Onto_DesignSurface();
            Uimap.Open_Assign_Tool_Large_View();
            Uimap.Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeInvalidVariableName();
            Uimap.Click_Assign_Tool_Large_View_Done_Button_With_Row1_Variable_Textbox_As_SomeInvalidVariableName();
            Uimap.Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariable();
            Uimap.Click_Assign_Tool_Large_View_Done_Button();
            Uimap.Enter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_UsingIntellisense();
            Uimap.Enter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_Using_Click_Intellisense_Suggestion();
            Uimap.Click_Assign_Tool_ExpandAll();
            Uimap.Assign_Value_To_Variable_With_Assign_Tool_large_View_Row_1();
            Uimap.Click_Workflow_CollapseAll();
            Uimap.Click_Assign_Tool_url();
            Uimap.Open_Assign_Tool_Qvi_Large_View();
            Uimap.Save_With_Ribbon_Button_And_Dialog(WorkflowName);
            Uimap.Click_Debug_Ribbon_Button();
            Uimap.Click_DebugInput_Debug_Button();
            Uimap.WaitForControlNotVisible(Uimap.MainStudioWindow.DockManager.SplitPaneRight.DebugOutput.StatusBar.Spinner);
            Uimap.Click_Debug_Output_Assign_Cell();
            
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void DeleteAssignToolFromContextMenuUITest()
        {
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Drag_Toolbox_MultiAssign_Onto_DesignSurface();
            Uimap.Open_Assign_Tool_Large_View();
            Uimap.Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeInvalidVariableName();
            Uimap.Click_Assign_Tool_Large_View_Done_Button_With_Row1_Variable_Textbox_As_SomeInvalidVariableName();
            Uimap.Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariable();
            Uimap.Click_Assign_Tool_Large_View_Done_Button();
            Uimap.DeleteAssign_FromContextMenu();            
        }

        #region Additional test attributes
        
        [TestInitialize]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
            Uimap.WaitForStudioStart();
        }

        UIMap Uimap
        {
            get
            {
                if (_uiMap == null)
                {
                    _uiMap = new UIMap();
                }

                return _uiMap;
            }
        }

        private UIMap _uiMap;

        #endregion
    }
}
