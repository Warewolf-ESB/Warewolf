using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Recordset
{
    [CodedUITest]
    public class Length
    {
        [TestMethod]
		[TestCategory("Recordset Tools")]
        public void LengthToolUITest()
        {
            UIMap.Open_Length_Tool_Large_View();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
            UIMap.InitializeABlankWorkflow();
            UIMap.Drag_Toolbox_Length_Onto_DesignSurface();
        }

        UIMap UIMap
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
