using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class ContextMenuItemsTest
    {
        const string DoubleDice = "Local_DoubleDice";
        const string DuplicatedWorkFlow = "DuplicatedWorkFlow";
        const string SecondWorkflowName = "SecondWorkflow";

        [TestMethod]
        public void ContextMenuItemsUITest()
        {
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Drag_Dice_Roll_Example_Onto_DesignSurface();
            Uimap.Click_Subworkflow_Done_Button();
            Uimap.Drag_Dice_Onto_Dice_On_The_DesignSurface();
            Uimap.Click_Workflow_CollapseAll();
            Uimap.Save_With_Ribbon_Button_And_Dialog(DoubleDice);
            Uimap.Click_Close_Workflow_Tab_Button();
            Uimap.Filter_Explorer(DoubleDice);
            Uimap.Open_Explorer_First_Item_Dependancies_With_Context_Menu();
            Uimap.Click_ViewSwagger_From_ExplorerContextMenu();
            Uimap.Open_Explorer_First_Item_Dependancies_With_Context_Menu();
            Uimap.Open_Explorer_First_Item_Version_History_With_Context_Menu();
            Uimap.Rename_LocalWorkflow_To_SecodWorkFlowParams.ItemEditText = SecondWorkflowName;
            Uimap.Rename_LocalWorkflow_To_SecodWorkFlow();
            Uimap.Open_Explorer_First_Item_Dependancies_With_Context_Menu();
            Uimap.Click_Duplicate_From_ExplorerContextMenu();
            Uimap.Enter_Duplicate_workflow_nameParams.ServiceNameTextBoxText = DuplicatedWorkFlow;
            Uimap.Enter_Duplicate_workflow_name();
            Uimap.Click_UpdateDuplicateRelationships();
            Uimap.Click_Duplicate_From_Duplicate_Dialog();
            Uimap.Filter_Explorer(SecondWorkflowName);
            Uimap.Open_Explorer_First_Item_Dependancies_With_Context_Menu();
            Uimap.Select_Show_Dependencies_In_Explorer_Context_Menu(SecondWorkflowName);
            Uimap.Click_Close_Dependecy_Tab();
            Uimap.Click_View_Api_From_Context_Menu();
        }


        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
#if !DEBUG
            Uimap.CloseHangingDialogs();
#endif
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
