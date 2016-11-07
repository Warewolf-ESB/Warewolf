using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class HttpWebGet
    {
        [TestMethod]
		[TestCategory("HTTP Tools")]
        public void HttpWebGetToolClickLargeViewDoneButton()
        {
            UIMap.Open_GET_Web_Connector_Tool_Large_View();
            UIMap.Click_GET_Web_Large_View_Done_Button_With_Invalid_Large_View();
        }

        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void HttpWebGetToolClickAddNewSourceButtonOpensNewSourceWizardTab()
        {
            UIMap.Open_GET_Web_Connector_Tool_Large_View();
            UIMap.Click_AddNew_Web_Source_From_tool();
        }

        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void HttpWebGetToolClickTestInputsDoneButton()
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
