using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class VariablesTests
    {
        [TestMethod]
        public void AssignValue_Filter_AndDeleteVariable()
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
            Uimap.Click_Assign_Tool_Remove_Variable_From_Tool();
            Uimap.Assign_Value_To_Variable();
            Uimap.Click_Assign_Tool_ExpandAll();
            Uimap.Click_Workflow_CollapseAll();
            Uimap.Click_Assign_Tool_url();
        }

        [TestMethod]
        public void Recordsets_Usage_in_Debug_Input()
        {
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Drag_Toolbox_MultiAssign_Onto_DesignSurface();
            Uimap.Open_Assign_Tool_Large_View();
            Uimap.Enter_Recordset_values();
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
