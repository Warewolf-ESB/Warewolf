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
            UIMap.TryCloseSettingsTab();
            UIMap.Click_ConfigureSetting_From_Menu();
            UIMap.Select_PerfomanceCounterTab();
            UIMap.Click_Reset_Perfomance_Counter();
            UIMap.Click_Select_Resource_Button();
            UIMap.Select_Service_From_Service_Picker(_serviceName, true);
            Assert.AreEqual("My Category\\" + _serviceName, UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.PerfomanceCounterTab.PerfmonViewContent.ResourceTable.Row1.ResourceCell.ResourceTextBox.DisplayText, "Resource Name is not set to Dice after selecting Dice from Service picker");
            UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
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
