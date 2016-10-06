using System;
using System.Drawing;
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
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_MultiAssign_Onto_DesignSurface();
            UIMap.Open_Assign_Tool_Large_View();
            UIMap.Enter_Recordset_values();
            Mouse.Move(UIMap.MainStudioWindow.DockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.InputCheckbox, new Point(10, 10));
            Mouse.Click();
            UIMap.Press_F5_To_Debug();
            UIMap.Enter_Text_Into_Debug_Input_Row1_Value_Textbox_With_Special_Test_For_Textbox_Height("Bob");
            UIMap.Enter_Text_Into_Debug_Input_Row2_Value_Textbox("Bob");
            UIMap.Click_Cancel_DebugInput_Window();
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
#if !DEBUG
            UIMap.CloseHangingDialogs();
#endif
            Console.WriteLine("Test \"" + TestContext.TestName + "\" starting on " + System.Environment.MachineName);
        }
        
        [TestCleanup()]
        public void MyTestCleanup()
        {
            Playback.PlaybackError -= UIMap.OnError;
            //UIMap.TryCloseHangingSaveDialog();
            //UIMap.TryClearToolboxFilter();
            //UIMap.TryCloseWorkflowTabs();
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
