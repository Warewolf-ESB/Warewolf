using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class WcfSourceTests
    {
        const string SourceName = "CodedUITestWcfSource";

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void WcfSource_CreateSourceUITests()
        {
            UIMap.Select_NewWcfSource_FromExplorerContextMenu();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WCFServiceSourceTab.WorkSurfaceContext.WCFEndpointURLEdit.Enabled, "WCF Endpoint URL Textbox is not enabled");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WCFServiceSourceTab.WorkSurfaceContext.TestConnectionButton.Enabled, "Test Connection button is enabled");
            UIMap.Enter_TextIntoAddress_On_WCFServiceTab();
            UIMap.Click_WCFServiceSource_TestConnectionButton();
            UIMap.Click_Close_WCFServiceSource_TabButton();
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

        #endregion
    }
}