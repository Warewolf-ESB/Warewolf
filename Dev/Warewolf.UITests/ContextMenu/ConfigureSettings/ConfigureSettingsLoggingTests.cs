using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class ConfigureSettingsLoggingTests
    {
        [TestMethod]
        public void ConfigureSettingLogging()
        {
            UIMap.TryCloseSettingsTab();
            UIMap.Click_ConfigureSetting_From_Menu();
            UIMap.Select_LoggingTab();
            UIMap.Click_Server_Log_File_Button();
            UIMap.Click_Studio_Log_File();
            UIMap.Click_Close_Settings_Tab_Button();
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
#if !DEBUG
            UIMap.CloseHangingDialogs();
#endif
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
