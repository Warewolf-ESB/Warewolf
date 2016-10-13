using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class HttpWebDelete
    {
        const string WebSourceName = "UITestingWebSource";
        const string WebDeleteName = "UITestingWebDeleteSource";

        [TestMethod]
        [TestCategory("Tools")]
        public void HttpWebDeleteToolClickLargeViewDoneButton()
        {
            UIMap.Open_DeleteWeb_Tool_Large_View();
        }

        [TestMethod]
        [TestCategory("Tools")]
        public void HttpWebDeleteToolClickAddNewSourceButtonOpensNewSourceWizardTab()
        {
            UIMap.Open_DeleteWeb_Tool_Large_View();
            UIMap.Click_AddNew_Web_Source_From_tool();
        }

        [TestMethod]
        [TestCategory("Tools")]
        public void HttpWebDeleteToolClickTestInputsDoneButton()
        {
            UIMap.Open_DeleteWeb_Tool_Large_View();
            UIMap.Select_UITestingSource_From_Web_Server_Large_View_Source_Combobox();
            UIMap.Click_DeleteWeb_Generate_Outputs();
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
            UIMap.Drag_DeleteWeb_Toolbox_Onto_Workflow_Surface();
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
