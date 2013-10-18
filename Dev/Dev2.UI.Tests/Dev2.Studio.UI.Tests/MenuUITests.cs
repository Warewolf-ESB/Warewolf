using System.Windows.Forms;
using Dev2.Studio.UI.Tests.UIMaps;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Studio.UI.Tests
{
    [CodedUITest]
    public class MenuUITests : UIMapBase
    {
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

            TabManagerUIMap.CloseAllTabs();
        }
    }
}
