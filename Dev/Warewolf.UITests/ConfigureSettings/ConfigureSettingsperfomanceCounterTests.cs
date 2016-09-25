using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class ConfigureSettingsperfomanceCounter
    {
        private string _serviceName = "Dice Roll";

        [TestMethod]
        public void ConfigureSettingPerfomanceCounter()
        {
            Uimap.TryCloseSettingsTab();
            Uimap.Click_ConfigureSetting_From_Menu();
            Uimap.Select_PerfomanceCounterTab();
            Uimap.Click_Reset_Perfomance_Counter();
            Uimap.Click_Select_Resource_Button();
            Uimap.Select_Service_From_Service_Picker(_serviceName, true);
            Assert.AreEqual("My Category\\" + _serviceName, Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.PerfomanceCounterTab.PerfmonViewContent.ResourceTable.Row1.ResourceCell.ResourceTextBox.DisplayText, "Resource Name is not set to Dice after selecting Dice from Service picker");
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Click_Close_Settings_Tab_Button();
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
#if RELEASE
            Uimap.WaitForStudioStart();
#endif
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
