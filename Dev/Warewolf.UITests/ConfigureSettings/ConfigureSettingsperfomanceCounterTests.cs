using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class ConfigureSettingsperfomanceCounter
    {
        [TestMethod]
        [Ignore]
        public void ConfigureSettingPerfomanceCounter()
        {
            Uimap.Click_ConfigureSetting_From_Menu();
            Uimap.Select_PerfomanceCounterTab();
            Uimap.Click_Reset_Perfomance_Counter();
            Uimap.Click_Select_Resource_Button();
            Uimap.Select_Dice_From_Service_Picker("PerfomanceCounterTab");

        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
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
