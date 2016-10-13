using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class HttpWebGet
    {
        const string WebSourceName = "UITestingWebSource";

        [TestMethod]
		[TestCategory("Tools")]
        public void HttpWebGetToolOpenAndCloseLargeViewWithDoubleClickUITest()
        {
            UIMap.Open_GET_Web_Connector_Tool_Large_View();
            UIMap.Click_GET_Web_Large_View_Done_Button();
        }

        [TestMethod]
        [TestCategory("Tools")]
        public void HttpWebGetToolClickAddNewSourceButtonOpensNewSourceWizardTab()
        {
            UIMap.Open_GET_Web_Connector_Tool_Large_View();
            UIMap.Click_AddNew_Web_Source_From_tool();
        }

        [TestMethod]
        [TestCategory("Tools")]
        public void HttpWebGetToolUITest()
        {
            UIMap.Open_GET_Web_Connector_Tool_Large_View();
            UIMap.Select_Second_to_Last_Source_From_GET_Web_Large_View_Source_Combobox();
            UIMap.Click_GET_Web_Large_View_Generate_Outputs();
            UIMap.Click_GET_Web_Large_View_Test_Inputs_Button();
            UIMap.Click_GET_Web_Large_View_Test_Inputs_Done_Button();
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
            UIMap.Drag_GET_Web_Connector_Onto_DesignSurface();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            UIMap.Click_Close_Workflow_Tab_Button();
            UIMap.Click_MessageBox_No();
            UIMap.TryRemoveFromExplorer(WebSourceName);
        }

        UIMap UIMap
        {
            get
            {
                if (_uiMap == null)
                {
                    _uiMap = new UIMap();
                }

                return _uiMap;
            }
        }

        private UIMap _uiMap;

        #endregion
    }
}
