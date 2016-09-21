using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class ContextMenuItemsTest
    {
        const string Dice = "Dice";
        const string DoubleDice = "DoubleDice";
        const string DuplicatedWorkFlow = "DuplicatedWorkFlow";

        [TestMethod]
        [Ignore]
        public void ContextMenuItemsUITest()
        {            
            Uimap.CreateAndSave_Dice_Workflow();
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Click_Explorer_Refresh_Button();
            Uimap.Drag_Dice_Onto_DesignSurface();
            Uimap.Drag_Dice_Onto_Dice_On_The_DesignSurface();
            Uimap.Click_Workflow_CollapseAll();
            Uimap.Save_With_Ribbon_Button_And_Dialog(DoubleDice);
            Uimap.Click_Close_Workflow_Tab_Button();
            Uimap.Click_Explorer_Refresh_Button();
            Uimap.Open_Explorer_First_Item_Dependancies_With_Context_Menu();
            Uimap.Click_ViewSwagger_From_ExplorerContextMenu();
            Uimap.Open_Explorer_First_Item_Dependancies_With_Context_Menu();
            Uimap.Select_Show_Version_History();
            Uimap.Open_Explorer_First_Item_Dependancies_With_Context_Menu();
            Uimap.Select_Rename_FromExplorerContextMenu();
            Uimap.Rename_LocalWorkflow_To_SecodWorkFlow();
            Uimap.Open_Explorer_First_Item_Dependancies_With_Context_Menu();
            Uimap.Click_Duplicate_From_ExplorerContextMenu();
            Uimap.Enter_Duplicate_workflow_name();
            Uimap.Click_UpdateDuplicateRelationships();
            Uimap.Click_Duplicate_From_Duplicate_Dialog();
            Uimap.Open_Explorer_First_Item_Dependancies_With_Context_Menu();
            Uimap.Select_Show_Dependencies_In_Explorer_Context_Menu();
            Uimap.Click_Close_Dependecy_Tab();
            Uimap.Click_View_Api_From_Context_Menu();
        }

        #region Additional test attributes

        [TestInitialize()]
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
