using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class Xpath
    {
        [TestMethod]
		[TestCategory("Tools")]
        public void XpathTool_OpenLargeViewUITest()
        {
            UIMap.Open_Xpath_Tool_Large_View();
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void XpathTool_OpenQVIUITest()
        {                                
            UIMap.Open_Xpath_Tool_Qvi_Large_View();
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void ToolDesigners_XpathlargeView_TabbingToDone_FocusIsSetToDone()
        {
            UIMap.Open_Xpath_Tool_Large_View();
            UIMap.Click_EndThisWF_On_XPath_LargeView();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.XPath.DoneButton.HasFocus);
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
#if !DEBUG
            UIMap.CloseHangingDialogs();
#endif
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_XPath_Onto_DesignSurface();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            //UIMap.Click_Close_Workflow_Tab_Button();
            //UIMap.Click_MessageBox_No();
        }
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
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
