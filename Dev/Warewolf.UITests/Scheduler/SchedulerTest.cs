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
            Uimap.Click_Service_Picker_Dialog_First_Service_In_Explorer();
            Uimap.Click_Service_Picker_Dialog_OK();
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Click_Scheduler_Disable_Task_Radio_Button();
            Uimap.Click_Close_Workflow_Tab_Button();
            Uimap.Enter_User_Details_For_Scheduler();
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
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
