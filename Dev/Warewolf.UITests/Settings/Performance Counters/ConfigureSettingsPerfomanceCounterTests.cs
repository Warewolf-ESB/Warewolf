using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class ConfigureSettingsperfomanceCounter
    {
        [TestMethod]
        [TestCategory("Settings")]
        public void ResettingPerfomanceCounter()
        {
            UIMap.Click_Reset_Perfomance_Counter();
        }

        [TestMethod]
        [TestCategory("Settings")]
        public void ConfigureSettingPerfomanceCounter()
        {
            UIMap.Click_Select_Resource_Button();
            var serviceName = "Hello World";
            UIMap.Select_Service_From_Service_Picker(serviceName);
            Assert.AreEqual(serviceName, UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.PerfomanceCounterTab.PerfmonViewContent.ResourceTable.Row1.ResourceCell.ResourceTextBox.DisplayText, "Resource Name is not set to Dice after selecting Dice from Service picker");
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
            UIMap.Click_ConfigureSetting_From_Menu();
            UIMap.Select_PerfomanceCounterTab();
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
