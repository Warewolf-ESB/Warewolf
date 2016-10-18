using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class Update_File
    {
        [TestMethod]
        [TestCategory("Tools")]
        public void Sharepoint_Update_Tool_Large_View()
        {
            UIMap.Open_Sharepoint_Update_Tool_Large_View();
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
#if !DEBUG
            UIMap.CloseHangingDialogs();
#endif
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_Sharepoint_Update_Onto_DesignSurface();
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
