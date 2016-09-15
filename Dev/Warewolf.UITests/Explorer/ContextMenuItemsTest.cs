using System;
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
            Uimap.RightClick_Dice();
            Uimap.Click_ViewSwagger_From_ExplorerContextMenu();
            Uimap.RightClick_Dice();
            Uimap.Select_Show_Version_History();
            Uimap.RightClick_Dice();
            Uimap.Select_Rename_FromExplorerContextMenu();
            Uimap.Rename_LocalWorkflow_To_SecodWorkFlow();
            Uimap.RightClick_Dice();
            Uimap.Click_Duplicate_From_ExplorerContextMenu();
            Uimap.Enter_Duplicate_workflow_name();
            Uimap.Click_UpdateDuplicateRelationships();
            Uimap.Click_Duplicate_From_Duplicate_Dialog();
            Uimap.RightClick_Dice();
            Uimap.Select_Show_Dependencies_In_Explorer_Context_Menu(DoubleDice);
            Uimap.Click_Close_Dependecy_Tab();
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            Uimap.SetGlobalPlaybackSettings();
            Uimap.WaitForStudioStart();
            Console.WriteLine("Test \"" + TestContext.TestName + "\" starting on " + System.Environment.MachineName);
        }
        
        [TestCleanup()]
        public void MyTestCleanup()
        {
            Playback.PlaybackError -= Uimap.OnError;
            Uimap.TryCloseAllTabs();
            Uimap.TryRemoveFromExplorer(Dice);
            Uimap.TryRemoveFromExplorer(DoubleDice);
            Uimap.TryRemoveFromExplorer(DuplicatedWorkFlow);
        }

        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        private TestContext testContextInstance;

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
