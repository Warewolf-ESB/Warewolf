using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class HttpWebPost
    {
        [TestMethod]
		[TestCategory("Tools")]
        public void HttpWebPostToolClickLargeViewDoneButton()
        {
            UIMap.Open_PostWeb_RequestTool_Large_View();
        }

        [TestMethod]
        [TestCategory("Tools")]
        public void HttpWebPostToolClickAddNewSourceButtonOpensNewSourceWizardTab()
        {
            UIMap.Open_PostWeb_RequestTool_Large_View();
            UIMap.Click_AddNew_Web_Source_From_tool();
        }

        [TestMethod]
        [TestCategory("Tools")]
        public void HttpWebPostToolClickTestInputsDoneButton()
        {
            UIMap.Open_PostWeb_RequestTool_Large_View();
            //Uimap.Select_PostWeb_Source();
            //Uimap.Click_PostWeb_GenerateOutputs_Button();
            //Uimap.Click_PostWeb_Test_Inputs_Done_Button();
        }

        [TestMethod]
        [TestCategory("Tools")]
        public void HttpWebPostToolClickPasteResponseButton()
        {
            UIMap.Open_PostWeb_RequestTool_Large_View();
            //Uimap.Select_PostWeb_Source();
            //Uimap.Click_PostWeb_GenerateOutputs_Button();
            //Uimap.Click_PostWeb_Paste_Response_Button();
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
            UIMap.Drag_PostWeb_RequestTool_Onto_DesignSurface();
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
