using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.Tools.ToolsUIMapClasses;

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
            UIMap.Select_NewWcfSource_From_ExplorerContextMenu();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WCFServiceSourceTab.Exists, "WCF Source Tab does now exist");
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

        ToolsUIMap ToolsUIMap
        {
            get
            {
                if (_ToolsUIMap == null)
                {
                    _ToolsUIMap = new ToolsUIMap();
                }

                return _ToolsUIMap;
            }
        }

        private ToolsUIMap _ToolsUIMap;

        #endregion
    }
}