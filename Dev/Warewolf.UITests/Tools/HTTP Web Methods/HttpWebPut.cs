using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class HttpWebPut
    {
        [TestMethod]
        [TestCategory("Tools")]
        public void HttpWebPutToolClickLargeViewDoneButton()
        {
            UIMap.Open_PutWeb_Tool_large_view();
        }

        [TestMethod]
        [TestCategory("Tools")]
        public void HttpWebPutToolClickAddNewSourceButtonOpensNewSourceWizardTab()
        {
            UIMap.Open_PutWeb_Tool_large_view();
            UIMap.Click_AddNew_Web_Source_From_PutWebtool();
        }

        [TestMethod]
        [TestCategory("Tools")]
        public void HttpWebPutToolClickTestInputsDoneButton()
        {
            UIMap.Open_PutWeb_Tool_large_view();
            //Uimap.Select_WebPut_Source();
            //Uimap.Click_PutWeb_GenerateOutputs_Button();
            //Uimap.Click_PutWeb_Paste_Response_Button();
            //Uimap.Click_PutWeb_Cancel_Button();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
#if !DEBUG
            Uimap.CloseHangingDialogs();
#endif
            UIMap.InitializeABlankWorkflow();
            UIMap.Drag_PutWeb_Tool_Onto_DesignSurface();
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
