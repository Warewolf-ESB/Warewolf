using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class Web_Request
    {
        [TestMethod]
		[TestCategory("Tools")]
        public void WebRequestToolUITest()
        {
            UIMap.Drag_Toolbox_Web_Request_Onto_DesignSurface();
            UIMap.Open_WebRequest_LargeView();
            UIMap.Enter_Text_Into_Web_Request_Url();
            UIMap.Enter_Result_Variable_Into_Web_Request();
            UIMap.Click_WebRequest_Tool_Large_View_Done_Button();
            UIMap.Press_F6();
            UIMap.WaitForSpinner(UIMap.MainStudioWindow.DockManager.SplitPaneRight.DebugOutput.StatusBar.Spinner);
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
#if !DEBUG
            UIMap.CloseHangingDialogs();
#endif
            UIMap.InitializeABlankWorkflow();
        }
        [TestCleanup]
        public void MyTestCleanup()
        {
            UIMap.Click_Close_Workflow_Tab_Button();
            UIMap.Click_MessageBox_No();
        }
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        private TestContext testContextInstance;

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
