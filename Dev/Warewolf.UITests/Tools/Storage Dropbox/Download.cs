using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Storage_Dropbox
{
    [CodedUITest]
    public class Download
    {
        [TestMethod]
        [TestCategory("Dropbox Tools")]
        public void Click_Dropbox_Download_Large_View_Done_Button_With_Invalid_Large_View()
        {
            UIMap.Open_DropboxFileOperation_Large_View();
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
            UIMap.Drag_Toolbox_Dropbox_Download_Onto_DesignSurface();
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
