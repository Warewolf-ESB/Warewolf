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
            UIMap.Drag_Toolbox_MultiAssign_Onto_DesignSurface();
            UIMap.Open_Assign_Tool_Large_View();
            UIMap.Enter_Recordset_values();
            Mouse.Move(UIMap.MainStudioWindow.DockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.InputCheckbox, new Point(10, 10));
            Mouse.Click();
            UIMap.Press_F5_To_Debug();
            UIMap.Enter_Text_Into_Debug_Input_Row2_Value_Textbox("Bob");
            UIMap.Click_Cancel_DebugInput_Window();
        }

        [TestMethod]
        public void VariableList_DeleteAColumnOffARecorset_DeleteAllButtonIsEnbaled_UITest()
        {
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneRight.Variables.DatalistView.RemoveUnused.Enabled);
            UIMap.Enter_Vaiablelist_Items();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneRight.Variables.DatalistView.RemoveUnused.Enabled);
            UIMap.Click_Remove_Unused_Variables();
            Point newPoint;
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem2.TryGetClickablePoint(out newPoint));
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
#if !DEBUG
            UIMap.CloseHangingDialogs();
#endif
            UIMap.Click_New_Workflow_Ribbon_Button();
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
