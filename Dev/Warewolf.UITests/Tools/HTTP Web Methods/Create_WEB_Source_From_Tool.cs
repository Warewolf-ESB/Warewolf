using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class Create_WEB_Source_From_Tool
    {
        const string WebSourceName = "UITestingWebSourceFromTool";
        [TestMethod]
        [Ignore]//Failing due to "executeIcon button does not exist"
        [TestCategory("Tools")]
        public void WebSourceFromTool()
        {
            Uimap.Drag_GET_Web_Connector_Onto_DesignSurface();
            Uimap.Open_GET_Web_Connector_Tool_Large_View();
            Uimap.Click_AddNew_Web_Source_From_tool();
            Uimap.Type_TestSite_into_Web_Source_Wizard_Address_Textbox();
            Uimap.Save_With_Ribbon_Button_And_Dialog(WebSourceName);
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
            Uimap.WaitForStudioStart();
            Uimap.InitializeABlankWorkflow();
        }

        UIMap Uimap
        {
            get
            {
                if ((_uiMap == null))
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
