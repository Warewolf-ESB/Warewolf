using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class HttpWebPost
    {
        const string WebSourceName = "UITestingWebSource";
        const string WebPostName = "UITestingWebPostSource";

        [TestMethod]
		[TestCategory("Tools")]
        public void HttpWebPostToolUITest()
        {
            Uimap.Drag_PostWeb_RequestTool_Onto_DesignSurface();
            Uimap.Open_PostWeb_RequestTool_Large_View();            
            Uimap.Click_AddNew_Web_Source_From_PostWeb_tool();
            Uimap.Type_TestSite_into_Web_Source_Wizard_Address_Textbox();
            Uimap.Save_With_Ribbon_Button_And_Dialog(WebSourceName);
            Uimap.Click_Close_Web_Source_Wizard_Tab_Button();
            Uimap.Save_With_Ribbon_Button_And_Dialog(WebPostName);
            Uimap.Click_Workflow_ExpandAll();            
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
            Uimap.TryRemoveFromExplorer(WebSourceName);
            Uimap.TryRemoveFromExplorer(WebPostName);
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
