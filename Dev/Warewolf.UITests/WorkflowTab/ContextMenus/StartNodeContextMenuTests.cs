using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.DialogsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;
using Warewolf.Web.UI.Tests.ScreenRecording;

namespace Warewolf.UITests.ContextMenu
{
    [CodedUITest]
    public class StartNodeContextMenuTests
    {
        public TestContext TestContext { get; set; }
        private FfMpegVideoRecorder screenRecorder = new FfMpegVideoRecorder();

        [TestMethod]
        [DeploymentItem(@"avformat-57.dll")]
        [DeploymentItem(@"avutil-55.dll")]
        [DeploymentItem(@"swresample-2.dll")]
        [DeploymentItem(@"swscale-4.dll")]
        [DeploymentItem(@"avcodec-57.dll")]
        public void CodedUIShowStartNodeContextMenuItems()
        {
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.DisplayStartNodeContextMenu();
            Assert.IsFalse(DialogsUIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.TestEditorMenuItem.Enabled, "Test Editor must be disabled on a new workflow");
            Assert.IsTrue(DialogsUIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.DebugInputsMenuItem.Enabled, "Debug Inputs must be enabled on a new workflow");
            Assert.IsTrue(DialogsUIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.DebugStudioMenuItem.Enabled, "Debug Studio must be enabled on a new workflow");
            Assert.IsTrue(DialogsUIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.DebugBrowserMenuItem.Enabled, "Debug Browser must be enabled on a new workflow");
            Assert.IsFalse(DialogsUIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.ScheduleMenuItem.Enabled, "Schedule must be disabled on a new workflow");
            Assert.IsFalse(DialogsUIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.RunAllTestsMenuItem.Enabled, "Run All Tests must be disabled on a new workflow");
            Assert.IsFalse(DialogsUIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.DuplicateMenuItem.Enabled, "Duplicate must be disabled on a new workflow");
            Assert.IsFalse(DialogsUIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.DeployMenuItem.Enabled, "Deploy must be disabled on a new workflow");
            Assert.IsFalse(DialogsUIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.ShowDependenciesMenuItem.Enabled, "Show Dependencies must be disabled on a new workflow");
            Assert.IsFalse(DialogsUIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.ViewSwaggerMenuItem.Enabled, "View Swagger must be disabled on a new workflow");
            Assert.IsFalse(DialogsUIMap.StartNodePopupWindow.CustomWindow.StartNodeItemMenu.CopyURLtoClipboardMenuItem.Enabled, "Copy Url to Clipboard must be disabled on a new workflow");
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            screenRecorder.StartRecording(TestContext);
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
        }

        [TestCleanup]
        public void StopScreenRecording()
        {
            screenRecorder.StopRecording(TestContext);
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

        WorkflowTabUIMap WorkflowTabUIMap
        {
            get
            {
                if (_WorkflowTabUIMap == null)
                {
                    _WorkflowTabUIMap = new WorkflowTabUIMap();
                }

                return _WorkflowTabUIMap;
            }
        }

        private WorkflowTabUIMap _WorkflowTabUIMap;

        DialogsUIMap DialogsUIMap
        {
            get
            {
                if (_DialogsUIMap == null)
                {
                    _DialogsUIMap = new DialogsUIMap();
                }

                return _DialogsUIMap;
            }
        }

        private DialogsUIMap _DialogsUIMap;

        #endregion
    }
}
