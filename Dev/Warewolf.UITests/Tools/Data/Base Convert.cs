using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Data
{
    [CodedUITest]
    public class Base_Convert
    {
        [TestMethod]
		[TestCategory("Tools")]
        public void BaseConvert_OpenLargeViewToolUITest()
        {
            UIMap.Open_Base_Conversion_Tool_Large_View();
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void BaseConvert_OpenQVI_UITest()
        {
            UIMap.Open_Base_Conversion_Tool_Qvi_Large_View();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
#if !DEBUG
            UIMap.CloseHangingDialogs();
#endif
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_Base_Conversion_Onto_DesignSurface();
        }
        [TestCleanup]
        public void MyTestCleanup()
        {
            UIMap.Click_Close_Workflow_Tab_Button();
            UIMap.Click_MessageBox_No();
        }
        UIMap UIMap
        {
            get
            {
                if (_UIMap == null)
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
