using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Data
{
    [CodedUITest]
    public class Case_Convert
    {
        [TestMethod]
		[TestCategory("Tools")]
        public void CaseConvertTool_OpenLargeViewUITest()
        {
            Uimap.Open_Case_Conversion_Tool_Large_View();
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void CaseConvertTool_OpenQVIUITest()
        {
            Uimap.Open_Case_Conversion_Tool_Qvi_Large_View();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
#if !DEBUG
            Uimap.CloseHangingDialogs();
#endif
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Drag_Toolbox_Case_Conversion_Onto_DesignSurface();
        }
        
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
