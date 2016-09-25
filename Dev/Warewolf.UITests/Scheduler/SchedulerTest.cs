using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class SchedulerTest
    {
        [TestMethod]
        public void SchedulerUITest()
        {
            Uimap.Click_Scheduler_Ribbon_Button();
            Uimap.Click_Scheduler_Create_New_Task_Ribbon_Button();
            Uimap.Click_Scheduler_ResourcePicker();
            Uimap.Select_Service_From_Service_Picker_Dialog("Hello World");
            Uimap.Enter_LocalSchedulerAdmin_Credentials_Into_Scheduler_Tab();
            Uimap.Click_Scheduler_Disable_Task_Radio_Button();
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog(30000);
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
#if !DEBUG
            Uimap.CloseHangingDialogs();
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
