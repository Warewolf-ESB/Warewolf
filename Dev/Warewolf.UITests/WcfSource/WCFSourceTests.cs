using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.Explorer.ExplorerUIMapClasses;
using Warewolf.UITests.WcfSource.WcfSourceUIMapClasses;
using Warewolf.Web.UI.Tests.ScreenRecording;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class WcfSourceTests
    {
        public TestContext TestContext { get; set; }
        private FfMpegVideoRecorder screenRecorder = new FfMpegVideoRecorder();

        [TestMethod]
        [DeploymentItem(@"avformat-57.dll")]
        [DeploymentItem(@"avutil-55.dll")]
        [DeploymentItem(@"swresample-2.dll")]
        [DeploymentItem(@"swscale-4.dll")]
        [DeploymentItem(@"avcodec-57.dll")]
        [TestCategory("Source Wizards")]
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