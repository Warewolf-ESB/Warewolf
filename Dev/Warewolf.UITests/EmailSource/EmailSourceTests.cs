using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class EmailSourceTests
    {
        const string SourceName = "CodedUITestEmailSource";

        [TestMethod]
        public void EmailSource_CreateSourceUITests()
        {
            UIMap.Select_NewEmailSource_FromExplorerContextMenu();
            UIMap.Enter_Text_Into_EmailSource_Tab();
            UIMap.Click_EmailSource_TestConnection_Button();
            UIMap.Save_With_Ribbon_Button_And_Dialog(SourceName);
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
        }
        
        public UIMap UIMap
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
