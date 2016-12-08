using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.ContextMenu
{
    [CodedUITest]
    public class StartNodeContextMenuTests
    {
        [TestMethod]
        public void CodedUIShowStartNodeContextMenuItems()
        {
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.DisplayStartNodeContextMenu();
            Assert.IsFalse(UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.TestEditorMenuItem.Enabled, "Test Editor must be disabled on a new workflow");
            Assert.IsTrue(UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.DebugInputsMenuItem.Enabled, "Debug Inputs must be enabled on a new workflow");
            Assert.IsTrue(UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.DebugStudioMenuItem.Enabled, "Debug Studio must be enabled on a new workflow");
            Assert.IsTrue(UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.DebugBrowserMenuItem.Enabled, "Debug Browser must be enabled on a new workflow");
            Assert.IsFalse(UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.ScheduleMenuItem.Enabled, "Schedule must be disabled on a new workflow");
            Assert.IsFalse(UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.RunAllTestsMenuItem.Enabled, "Run All Tests must be disabled on a new workflow");
            Assert.IsFalse(UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.DuplicateMenuItem.Enabled, "Duplicate must be disabled on a new workflow");
            Assert.IsFalse(UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.DeployMenuItem.Enabled, "Deploy must be disabled on a new workflow");
            Assert.IsFalse(UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.ShowDependenciesMenuItem.Enabled, "Show Dependencies must be disabled on a new workflow");
            Assert.IsFalse(UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.ViewSwaggerMenuItem.Enabled, "View Swagger must be disabled on a new workflow");
            Assert.IsFalse(UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.CopyURLtoClipboardMenuItem.Enabled, "Copy Url to Clipboard must be disabled on a new workflow");
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
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
