using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Studio.UI.Tests
{
    [CodedUITest]
    public class MenuUITests : UIMapBase
    {

        #region Cleanup


        [ClassInitialize]
        public static void ClassInit(TestContext tctx)
        {
            Playback.Initialize();
            Playback.PlaybackSettings.ContinueOnError = true;
            Playback.PlaybackSettings.ShouldSearchFailFast = true;
            Playback.PlaybackSettings.SmartMatchOptions = SmartMatchOptions.None;
            Playback.PlaybackSettings.MatchExactHierarchy = true;
            Playback.PlaybackSettings.DelayBetweenActions = 1;

            // make the mouse quick ;)
            Mouse.MouseMoveSpeed = 10000;
            Mouse.MouseDragSpeed = 10000;
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            TabManagerUIMap.CloseAllTabs();
        }

        #endregion

        [TestMethod]
        public void DebugAWorkFlow_EnsureSaveIsEnabledAfterCompletion()
        {

            ExplorerUIMap.EnterExplorerSearchText("ServiceExecutionTest");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "BUGS", "ServiceExecutionTest");

            PopupDialogUIMap.WaitForDialog();
            KeyboardCommands.SendKey("{F5}");
            PopupDialogUIMap.WaitForDialog();
            KeyboardCommands.SendKey("{F6}");
            PopupDialogUIMap.WaitForDialog();

            var uiControl = RibbonUIMap.GetControlByName("Save");

            uiControl.WaitForControlReady();

            Assert.IsTrue(uiControl.Enabled);
        }
    }
}
