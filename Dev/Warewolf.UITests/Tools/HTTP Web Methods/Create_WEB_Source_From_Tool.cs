using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class Create_WEB_Source_From_Tool
    {
        const string WebSourceName = "UITestingWebSourceFromTool";
        [TestMethod]
        [TestCategory("Tools")]
        public void WebSourceFromTool()
        {
            UIMap.Drag_GET_Web_Connector_Onto_DesignSurface();
            UIMap.Open_GET_Web_Connector_Tool_Large_View();
            UIMap.Click_AddNew_Web_Source_From_tool();
            UIMap.Type_TestSite_into_Web_Source_Wizard_Address_Textbox();
            UIMap.Save_With_Ribbon_Button_And_Dialog(WebSourceName);
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

        UIMap UIMap
        {
            get
            {
                if ((_UIMap == null))
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
