using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class HttpWebPost
    {
        const string WebSourceName = "UITestingWebSourceFromTool";

        [TestMethod]
		[TestCategory("Tools")]
        public void HttpWebPostToolUITest()
        {
            Uimap.Drag_PostWeb_RequestTool_Onto_DesignSurface();
            Uimap.Open_PostWeb_RequestTool_Large_View();            
            Uimap.Click_AddNew_Web_Source_From_tool();
            Uimap.Type_TestSite_into_Web_Source_Wizard_Address_Textbox();
            Uimap.Save_With_Ribbon_Button_And_Dialog(WebSourceName);
            Uimap.Click_PostWeb_GenerateOutputs_Button();
            Uimap.Click_PostWeb_Paste_Response_Button();
            Uimap.Click_PostWeb_Cancel_Button();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
#if !DEBUG
            Uimap.CloseHangingDialogs();
#endif
            Uimap.InitializeABlankWorkflow();
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

        UIMap Uimap
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
