using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class ConfigureSettingsPermissionsTests
    {
        const string Dice = "Dice1";

        [TestMethod]
        public void ConfigureSettingPermission()
        {
            UIMap.TryRemoveFromExplorer(Dice);
            UIMap.Select_NewWorkFlowService_From_ContextMenu();
            UIMap.Drag_Toolbox_Random_Onto_DesignSurface();
            UIMap.Enter_Dice_Roll_Values();
            UIMap.Save_With_Ribbon_Button_And_Dialog(Dice, true);
            UIMap.Click_Close_Workflow_Tab_Button();
            UIMap.Click_Explorer_Refresh_Button();
            UIMap.Click_ConfigureSetting_From_Menu();
            UIMap.Check_Public_Contribute();
            UIMap.Check_Public_Administrator();
            UIMap.UnCheck_Public_View();
            UIMap.Check_Public_Administrator();
            UIMap.UnCheck_Public_Administrator();
            UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            UIMap.Click_Select_Resource_Button_From_Resource_Permissions();
            UIMap.Select_Service_From_Service_Picker(Dice);
            Assert.AreEqual(Dice, UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.ResourceCell.AddResourceText.DisplayText, "Resource Name is not set to Dice after selecting Dice from Service picker");
            UIMap.Enter_Public_As_Windows_Group();
            UIMap.Check_Resource_Contribute();
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
