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
            Uimap.TryCloseSettingsTab();
            Uimap.Click_ConfigureSetting_From_Menu();
            Uimap.Select_LoggingTab();
            Uimap.Click_Server_Log_File_Button();
            Uimap.Click_Studio_Log_File();
            Uimap.Click_Close_Settings_Tab_Button();
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
            Uimap.WaitForStudioStart();
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
