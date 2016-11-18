using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.DebugInputWindow
{
    [CodedUITest]
    public class DebugInputWindowTests
    {
        [TestMethod]
        [TestCategory("Debug Input")]
        public void DebugInputWindow_InputDataTab()
        {
            Assert.IsTrue(UIMap.MainStudioWindow.DebugInputDialog.DebugF6Button.Exists, "Debug button in Debug Input window does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DebugInputDialog.CancelButton.Exists, "Cancel Debug Input Window button does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DebugInputDialog.RememberDebugInputCheckBox.Exists, "Remember Checkbox does not exist in the Debug Input window.");
            Assert.IsTrue(UIMap.MainStudioWindow.DebugInputDialog.ViewInBrowserF7Button.Enabled, "View in Browser button does not exist in Debug Input window.");
            Assert.IsTrue(UIMap.MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Exists, "Input Data Window does not exist in Debug Input window.");
            Assert.IsTrue(UIMap.MainStudioWindow.DebugInputDialog.TabItemsTabList.XMLTab.Exists, "Xml tab does not Exist in the Debug Input window.");
            Assert.IsTrue(UIMap.MainStudioWindow.DebugInputDialog.TabItemsTabList.JSONTab.Exists, "Assert Json tab does not exist in the debug input window.");
            UIMap.Click_Cancel_DebugInput_Window();
            UIMap.Click_Close_Workflow_Tab_Button();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Click_Debug_Ribbon_Button();
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
