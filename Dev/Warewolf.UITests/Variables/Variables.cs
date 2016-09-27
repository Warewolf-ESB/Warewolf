using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class VariablesTests
    {
        [TestMethod]
        public void Recordsets_Usage_in_Debug_Input()
        {
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Drag_Toolbox_MultiAssign_Onto_DesignSurface();
            Uimap.Open_Assign_Tool_Large_View();
            Uimap.Enter_Recordset_values();
            Uimap.MainStudioWindow.DockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.InputCheckbox.Checked = true;
            Uimap.Press_F5_To_Debug();
            Uimap.Enter_Text_Into_Debug_Input_Row1_Value_Textbox_With_Special_Test_For_Textbox_Height("Bob");
            Uimap.Enter_Text_Into_Debug_Input_Row2_Value_Textbox("Bob");
            Uimap.Click_Cancel_DebugInput_Window();
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
#if !DEBUG
            Uimap.CloseHangingDialogs();
#endif
            Console.WriteLine("Test \"" + TestContext.TestName + "\" starting on " + System.Environment.MachineName);
        }
        
        [TestCleanup()]
        public void MyTestCleanup()
        {
            Playback.PlaybackError -= Uimap.OnError;
            //Uimap.TryCloseHangingSaveDialog();
            //Uimap.TryClearToolboxFilter();
            //Uimap.TryCloseWorkflowTabs();
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
