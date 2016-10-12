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
        public void HttpWebGetToolUITest()
        {
            Uimap.Drag_GET_Web_Connector_Onto_DesignSurface();
            Uimap.Open_GET_Web_Connector_Tool_Large_View();
            Uimap.Click_AddNew_Web_Source_From_tool();
            Uimap.Type_TestSite_into_Web_Source_Wizard_Address_Textbox();
            Uimap.Save_With_Ribbon_Button_And_Dialog(WebSourceName);
            Uimap.Click_Close_Web_Source_Wizard_Tab_Button();

            Uimap.Open_GET_Web_Connector_Tool_Large_View();
            Uimap.Select_Last_Source_From_GET_Web_Large_View_Source_Combobox();
            Uimap.Click_GET_Web_Large_View_Generate_Outputs();
            Uimap.Click_GET_Web_Large_View_Test_Inputs_Button();
            Uimap.Click_GET_Web_Large_View_Test_Inputs_Done_Button();
            Uimap.Click_GET_Web_Large_View_Done_Button();
            Uimap.Click_Debug_Ribbon_Button();
            Uimap.Click_DebugInput_Debug_Button();
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

        [TestCleanup]
        public void MyTestCleanup()
        {
            Uimap.Click_Close_Workflow_Tab_Button();
            Uimap.Click_MessageBox_No();
            Uimap.TryRemoveFromExplorer(WebSourceName);
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
