using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.ContextMenu
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    public class DesignSurfaceContextMenuTests
    {
        [TestMethod]
        public void CodedUIShowStartNode()
        {
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.DisplayStartNodeContextMenu();
            Assert.AreEqual(UIMap.DisplayStartNodeContextMenuExpectedValues.DebugInputsMenuItemExists, UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.DebugInputsMenuItem.Exists, "Debug Inputs does not exist on the Start Node context menu");
            Assert.AreEqual(UIMap.DisplayStartNodeContextMenuExpectedValues.DebugStudioMenuItemExists, UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.DebugStudioMenuItem.Exists, "Debug Studio does not exist on the Start Node context menu");
            Assert.AreEqual(UIMap.DisplayStartNodeContextMenuExpectedValues.DebugBrowserMenuItemExists, UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.DebugBrowserMenuItem.Exists, "Debug Browser does not exist on the Start Node context menu");
            Assert.AreEqual(UIMap.DisplayStartNodeContextMenuExpectedValues.ScheduleMenuItemExists, UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.ScheduleMenuItem.Exists, "Schedule does not exist on the Start Node context menu");
            Assert.AreEqual(UIMap.DisplayStartNodeContextMenuExpectedValues.TestEditorMenuItemExists, UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.TestEditorMenuItem.Exists, "Test Editor does not exist on the Start Node context menu");
            Assert.AreEqual(UIMap.DisplayStartNodeContextMenuExpectedValues.TestEditorMenuItemEnabled, UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.TestEditorMenuItem.Enabled, "Test Editor must be disabled on a new workflow");
            Assert.AreEqual(UIMap.DisplayStartNodeContextMenuExpectedValues.DebugInputsMenuItemEnabled, UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.DebugInputsMenuItem.Enabled, "Debug Inputs must be enabled on a new workflow");
            Assert.AreEqual(UIMap.DisplayStartNodeContextMenuExpectedValues.DebugStudioMenuItemEnabled, UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.DebugStudioMenuItem.Enabled, "Debug Studio must be enabled on a new workflow");
            Assert.AreEqual(UIMap.DisplayStartNodeContextMenuExpectedValues.DebugBrowserMenuItemEnabled, UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.DebugBrowserMenuItem.Enabled, "Debug Browser must be enabled on a new workflow");
            Assert.AreEqual(UIMap.DisplayStartNodeContextMenuExpectedValues.ScheduleMenuItemEnabled, UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.ScheduleMenuItem.Enabled, "Schedule must be disabled on a new workflow");
            Assert.AreEqual(UIMap.DisplayStartNodeContextMenuExpectedValues.RunAllTestsMenuItemExists, UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.RunAllTestsMenuItem.Exists, "Run All Tests does not exist on the Start Node context menu");
            Assert.AreEqual(UIMap.DisplayStartNodeContextMenuExpectedValues.RunAllTestsMenuItemEnabled, UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.RunAllTestsMenuItem.Enabled, "Run All Tests must be disabled on a new workflow");
            Assert.AreEqual(UIMap.DisplayStartNodeContextMenuExpectedValues.DuplicateMenuItemEnabled, UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.DuplicateMenuItem.Enabled, "Duplicate must be disabled on a new workflow");
            Assert.AreEqual(UIMap.DisplayStartNodeContextMenuExpectedValues.DuplicateMenuItemExists, UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.DuplicateMenuItem.Exists, "Duplicate does not exist on the Start Node context menu");
            Assert.AreEqual(UIMap.DisplayStartNodeContextMenuExpectedValues.DeployMenuItemExists, UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.DeployMenuItem.Exists, "Deploy does not exist on the Start Node context menu");
            Assert.AreEqual(UIMap.DisplayStartNodeContextMenuExpectedValues.DeployMenuItemEnabled, UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.DeployMenuItem.Enabled, "Deploy must be disabled on a new workflow");
            Assert.AreEqual(UIMap.DisplayStartNodeContextMenuExpectedValues.ShowDependenciesMenuItemEnabled, UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.ShowDependenciesMenuItem.Enabled, "Show Dependencies must be disabled on a new workflow");
            Assert.AreEqual(UIMap.DisplayStartNodeContextMenuExpectedValues.ShowDependenciesMenuItemExists, UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.ShowDependenciesMenuItem.Exists, "Show Dependencies does not exist on the Start Node context menu");
            Assert.AreEqual(UIMap.DisplayStartNodeContextMenuExpectedValues.ViewSwaggerMenuItemExists, UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.ViewSwaggerMenuItem.Exists, "View Swagger does not exist on the Start Node context menu");
            Assert.AreEqual(UIMap.DisplayStartNodeContextMenuExpectedValues.ViewSwaggerMenuItemEnabled, UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.ViewSwaggerMenuItem.Enabled, "View Swagger must be disabled on a new workflow");
            Assert.AreEqual(UIMap.DisplayStartNodeContextMenuExpectedValues.CopyURLtoClipboardMenuItemEnabled, UIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.CopyURLtoClipboardMenuItem.Enabled, "Copy Url to Clipboard must be disabled on a new workflow");
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
#if !DEBUG
            UIMap.CloseHangingDialogs();
#endif
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
