using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Data
{
    [CodedUITest]
    public class Data_Split
    {
        [TestMethod]
		[TestCategory("Tools")]
        public void DataSplitTool_OpenLargeViewUITest()
        {
            Uimap.Open_Data_Split_Large_View();
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void DataSplitTool_OpenQVIUITest()
        {
            Uimap.Open_Data_Split_Tool_Qvi_Large_View();
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
            Uimap.Drag_Toolbox_Data_Split_Onto_DesignSurface();
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
