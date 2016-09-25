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
            Uimap.Select_InputOutput_CheckBox_Recordset_values();
            Uimap.F5_Keyboard_Click();
            Uimap.Type_Value_And_Click_Enter_Keyboard_Then_Backspace();
            Uimap.Click_Cancel_DebugInput_Window();

        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
#if RELEASE
            Uimap.WaitForStudioStart();
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
