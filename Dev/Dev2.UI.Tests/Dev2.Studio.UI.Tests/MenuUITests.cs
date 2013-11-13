using System.Windows.Forms;
using Dev2.CodedUI.Tests.TabManagerUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Studio.UI.Tests
{
    [CodedUITest]
    public class MenuUITests : UIMapBase
    {

        #region Cleanup

        private static TabManagerUIMap _tabManager = new TabManagerUIMap();

        [ClassInitialize]
        public static void ClassInit(TestContext tctx)
        {
            Playback.Initialize();
            Playback.PlaybackSettings.ContinueOnError = true;
            Playback.PlaybackSettings.ShouldSearchFailFast = true;
            Playback.PlaybackSettings.SmartMatchOptions = SmartMatchOptions.None;
            Playback.PlaybackSettings.MatchExactHierarchy = true;

            // make the mouse quick ;)
            Mouse.MouseMoveSpeed = 10000;
            Mouse.MouseDragSpeed = 10000;
        }

        //[ClassCleanup]
        //public static void MyTestCleanup()
        //{
        //    _tabManager.CloseAllTabs();
        //}

        #endregion

        [TestMethod]
        public void DebugAWorkFlow_EnsureSaveIsEnabledAfterCompletion()
        {
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("ServiceExecutionTest");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "BUGS", "ServiceExecutionTest");
            
            Playback.Wait(3000);
            SendKeys.SendWait("{F5}");
            PopupDialogUIMap.WaitForDialog();
            SendKeys.SendWait("{F5}");
            Playback.Wait(1000);

            var uiControl = RibbonUIMap.GetControlByName("Save");

            var count = 0;
            while(count < 10 && !uiControl.Enabled)
            {
                count++;
            }

            Assert.IsTrue(uiControl.Enabled);
        }
    }
}
