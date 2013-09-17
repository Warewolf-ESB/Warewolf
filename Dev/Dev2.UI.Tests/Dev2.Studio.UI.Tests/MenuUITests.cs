using System.Windows.Forms;
using Dev2.CodedUI.Tests;
using Dev2.CodedUI.Tests.TabManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ExplorerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.RibbonUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Studio.UI.Tests
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    public class MenuUITests
    {
        ExplorerUIMap _explorerUIMap;

        ExplorerUIMap ExplorerUIMap
        {
            get
            {
                if(_explorerUIMap == null)
                {
                    _explorerUIMap = new ExplorerUIMap();
                }
                return _explorerUIMap;
            }
        }

        public RibbonUIMap RibbonUIMap
        {
            get
            {
                if(_ribbonUIMap == null)
                {
                    _ribbonUIMap = new RibbonUIMap();
                }

                return _ribbonUIMap;
            }
        }

        RibbonUIMap _ribbonUIMap;

        TabManagerUIMap _tabManagerUIMap;
        public TabManagerUIMap TabManagerUIMap
        {
            get
            {

                if(_tabManagerUIMap == null)
                {
                    _tabManagerUIMap = new TabManagerUIMap();
                }

                return _tabManagerUIMap;
            }
        }

        [TestMethod]
        public void DebugAWorkFlow_EnsureSaveIsEnabledAfterCompletion()
        {
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("ServiceExecutionTest");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "BUGS", "ServiceExecutionTest");
            
            Playback.Wait(3000);
            SendKeys.SendWait("{F5}");
            Playback.Wait(1000);
            SendKeys.SendWait("{F5}");
            Playback.Wait(1000);

            var uiControl = RibbonUIMap.GetControlByName("Save");

            Assert.IsTrue(uiControl.Enabled);

            // All good - Cleanup time!
            new TestBase().DoCleanup(TabManagerUIMap.GetActiveTabName(), true);
        }
    }
}
