using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class ConfigureSettingsPermissionsTests
    {
        [TestMethod]
        [Ignore]
        public void ConfigureSettingPermission()
        {
            Uimap.RightClick_Localhost();
            Uimap.Select_NewWorkFlowService_From_ContextMenu();
            Uimap.Drag_Toolbox_Random_Onto_DesignSurface();
            Uimap.Enter_Dice_Roll_Values();
            Uimap.Save_With_Ribbon_Button_And_Dialog("Dice");
            Uimap.Click_Close_Workflow_Tab_Button();
            Uimap.Click_Explorer_Refresh_Button();
            Uimap.Click_ConfigureSetting_From_Menu();
            Uimap.Check_Public_Contribute();
            Uimap.Check_Public_Administrator();
            Uimap.UnCheck_Public_View();
            Uimap.Check_Public_Administrator();
            Uimap.UnCheck_Public_Administrator();
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Click_Select_Resource_Button_From_Resource_Permissions();
            Uimap.Select_Dice_From_Service_Picker("SecurityTab");
            Uimap.Enter_Public_As_Windows_Group();
            Uimap.Check_Resource_Contribute();
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Click_Close_Settings_Tab_Button();
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            Uimap.SetGlobalPlaybackSettings();
            Uimap.WaitForStudioStart();
            Console.WriteLine("Test \"" + TestContext.TestName + "\" starting on " + System.Environment.MachineName);
        }
        
        [TestCleanup()]
        public void MyTestCleanup()
        {
            Playback.PlaybackError -= Uimap.OnError;
            Uimap.TryCloseAllTabs();
        }

        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        private TestContext testContextInstance;

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
