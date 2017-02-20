using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.ExplorerUIMapClasses;
using Warewolf.UITests.WcfSource.WcfSourceUIMapClasses;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class WcfSourceTests
    { 
        [TestMethod]
        [TestCategory("WCF Source")]
        // ReSharper disable once InconsistentNaming
        public void Create_WcfSource_From_ExplorerContextMenu_UITests()
        {
            ExplorerUIMap.Select_NewWcfSource_From_ExplorerContextMenu();
            Assert.IsTrue(WcfSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WCFServiceSourceTab.Exists, "WCF Source Tab does now exist");
            Assert.IsTrue(WcfSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WCFServiceSourceTab.WorkSurfaceContext.WCFEndpointURLEdit.Enabled, "WCF Endpoint URL Textbox is not enabled");
            Assert.IsFalse(WcfSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WCFServiceSourceTab.WorkSurfaceContext.TestConnectionButton.Enabled, "Test Connection button is enabled");
            WcfSourceUIMap.Enter_TextIntoAddress_On_WCFServiceTab();
            WcfSourceUIMap.Click_WCFServiceSource_TestConnectionButton();
            WcfSourceUIMap.Click_Close_WCFServiceSource_TabButton();
        }
        
        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
        }
        
        public UIMap UIMap
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

        WcfSourceUIMap WcfSourceUIMap
        {
            get
            {
                if (_WcfSourceUIMap == null)
                {
                    _WcfSourceUIMap = new WcfSourceUIMap();
                }

                return _WcfSourceUIMap;
            }
        }

        private WcfSourceUIMap _WcfSourceUIMap;

        #endregion
    }
}