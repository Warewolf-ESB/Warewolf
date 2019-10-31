using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Input;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UI.Tests.DebugInputWindow
{
    [CodedUITest]
    public class DebugInputWindowTests
    {
        [TestMethod]
        [TestCategory("Debug Input")]
        public void DebugInputWindow_Validation_UITest()
        {
            UIMap.Click_NewWorkflow_RibbonButton();
            UIMap.Click_Debug_RibbonButton();
            Assert.IsTrue(UIMap.MainStudioWindow.DebugInputDialog.Exists, "Debug Input window does not exist after clicking debug ribbon button.");
            Assert.IsTrue(UIMap.MainStudioWindow.DebugInputDialog.DebugF6Button.Exists, "Debug button in Debug Input window does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DebugInputDialog.CancelButton.Exists, "Cancel Debug Input Window button does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DebugInputDialog.RememberDebugInputCheckBox.Exists, "Remember Checkbox does not exist in the Debug Input window.");
            Assert.IsTrue(UIMap.MainStudioWindow.DebugInputDialog.ViewInBrowserF7Button.Enabled, "View in Browser button does not exist in Debug Input window.");
            Assert.IsTrue(UIMap.MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Exists, "Input Data Window does not exist in Debug Input window.");
            Assert.IsTrue(UIMap.MainStudioWindow.DebugInputDialog.TabItemsTabList.XMLTab.Exists, "Xml tab does not Exist in the Debug Input window.");
            Assert.IsTrue(UIMap.MainStudioWindow.DebugInputDialog.TabItemsTabList.JSONTab.Exists, "Assert Json tab does not exist in the debug input window.");
            UIMap.Click_DebugInput_ViewInBrowser_Button();
        }

        [TestMethod]
        [TestCategory("Debug Input")]
        public void DebugInputWindow_TabSelectionChanged_UITest()
        {
            ExplorerUIMap.Open_Item_With_Double_Click("DebugInputRecordSet");
            UIMap.Press_F5_To_Debug();
            Mouse.Click(UIMap.MainStudioWindow.DebugInputDialog.TabItemsTabList.XMLTab);
            Assert.IsTrue(UIMap.MainStudioWindow.DebugInputDialog.TabItemsTabList.XMLTab.Exists);
            Mouse.Click(UIMap.MainStudioWindow.DebugInputDialog.TabItemsTabList.JSONTab);
            Assert.IsTrue(UIMap.MainStudioWindow.DebugInputDialog.TabItemsTabList.JSONTab.JSONWindow.Exists);
        }

        [TestMethod]
        [TestCategory("Debug Input")]
        public void DebugInputWindow_AddAndRemoveRows_UITest()
        {
            ExplorerUIMap.Open_Item_With_Double_Click("DebugInputRecordSet");
            UIMap.Press_F5_To_Debug();
            Assert.IsTrue(UIMap.MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.Exists);
            Assert.IsFalse(UIMap.ControlExistsNow(UIMap.MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row2), "Row 2 exists on startup when previously did not.");
            Keyboard.SendKeys(UIMap.MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.InputValueCell.InputValueComboboxl.InputValueText, "{Insert}", ModifierKeys.Shift);
            Assert.IsTrue(UIMap.ControlExistsNow(UIMap.MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row2));
            Keyboard.SendKeys(UIMap.MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.InputValueCell.InputValueComboboxl.InputValueText, "{Delete}", ModifierKeys.Shift);
            Assert.IsFalse(UIMap.ControlExistsNow(UIMap.MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row2), "Row 2 exists on after delete.");
        }

        [TestMethod]
        [TestCategory("Debug Input")]
        public void DebugInputWindow_Move_UITest()
        {
            ExplorerUIMap.Open_Item_With_Double_Click("DebugInputRecordSet");
            UIMap.Press_F5_To_Debug();
            Mouse.StartDragging(UIMap.MainStudioWindow.DebugInputDialog);
            Mouse.StopDragging(100, 100);
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
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

        ExplorerUIMap ExplorerUIMap
        {
            get
            {
                if (_ExplorerUIMap == null)
                {
                    _ExplorerUIMap = new ExplorerUIMap();
                }

                return _ExplorerUIMap;
            }
        }

        private ExplorerUIMap _ExplorerUIMap;

        #endregion
    }
}
