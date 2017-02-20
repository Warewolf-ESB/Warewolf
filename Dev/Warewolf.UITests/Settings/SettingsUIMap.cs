using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using System.Windows.Input;
using MouseButtons = System.Windows.Forms.MouseButtons;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.Tools.ToolsUIMapClasses;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using System.Drawing;
using System.IO;
using TechTalk.SpecFlow;
using Warewolf.UITests.Common;
using Warewolf.UITests.ExplorerUIMapClasses;
using Warewolf.UITests.WorkflowTesting.WorkflowServiceTestingUIMapClasses;
using Warewolf.UITests.DialogsUIMapClasses;

namespace Warewolf.UITests.Settings.SettingsUIMapClasses
{
    [Binding]
    public partial class SettingsUIMap
    {
        ToolsUIMap ToolsUIMap
        {
            get
            {
                if (_ToolsUIMap == null)
                {
                    _ToolsUIMap = new ToolsUIMap();
                }

                return _ToolsUIMap;
            }
        }

        private ToolsUIMap _ToolsUIMap;

        WorkflowServiceTestingUIMap WorkflowServiceTestingUIMap
        {
            get
            {
                if (_WorkflowServiceTestingUIMap == null)
                {
                    _WorkflowServiceTestingUIMap = new WorkflowServiceTestingUIMap();
                }

                return _WorkflowServiceTestingUIMap;
            }
        }

        private WorkflowServiceTestingUIMap _WorkflowServiceTestingUIMap;

        ExplorerUIMap ExplorerUIMap
        {
            get
            {
                if (_ExplorerUIMap == null)
                {
                    _ExplorerUIMap = new ExplorerUIMap();
                }

                return _ExplorerUIMap;
            }
        }

        private ExplorerUIMap _ExplorerUIMap;

        DialogsUIMap DialogsUIMap
        {
            get
            {
                if (_DialogsUIMap == null)
                {
                    _DialogsUIMap = new DialogsUIMap();
                }

                return _DialogsUIMap;
            }
        }

        private DialogsUIMap _DialogsUIMap;

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

        public UITestControl FindAddResourceButton(UITestControl row)
        {
            var firstOrDefaultCell = row.GetChildren().Where(child => child.ControlType == ControlType.Cell).ElementAtOrDefault(0);
            return firstOrDefaultCell?.GetChildren().FirstOrDefault(child => child.ControlType == ControlType.Button);
        }

        public WpfText FindSelectedResourceText(UITestControl row)
        {
            var firstOrDefaultCell = row.GetChildren().Where(child => child.ControlType == ControlType.Cell).ElementAtOrDefault(0);
            return firstOrDefaultCell?.GetChildren().FirstOrDefault(child => child.ControlType == ControlType.Text) as WpfText;
        }

        public UITestControl FindAddWindowsGroupButton(UITestControl row)
        {
            var firstOrDefaultCell = row.GetChildren().Where(child => child.ControlType == ControlType.Cell).ElementAtOrDefault(1);
            return firstOrDefaultCell?.GetChildren().FirstOrDefault(child => child.ControlType == ControlType.Button);
        }

        public WpfEdit FindWindowsGroupTextbox(UITestControl row)
        {
            var firstOrDefaultCell = row.GetChildren().Where(child => child.ControlType == ControlType.Cell).ElementAtOrDefault(1);
            return firstOrDefaultCell?.GetChildren().FirstOrDefault(child => child.ControlType == ControlType.Edit) as WpfEdit;
        }

        public WpfCheckBox FindViewPermissionsCheckbox(UITestControl row)
        {
            var firstOrDefaultCell = row.GetChildren().Where(child => child.ControlType == ControlType.Cell).ElementAtOrDefault(2);
            return firstOrDefaultCell?.GetChildren().FirstOrDefault(child => child.ControlType == ControlType.CheckBox) as WpfCheckBox;
        }

        public WpfCheckBox FindExecutePermissionsCheckbox(UITestControl row)
        {
            var firstOrDefaultCell = row.GetChildren().Where(child => child.ControlType == ControlType.Cell).ElementAtOrDefault(3);
            return firstOrDefaultCell?.GetChildren().FirstOrDefault(child => child.ControlType == ControlType.CheckBox) as WpfCheckBox;
        }

        public WpfCheckBox FindContributePermissionsCheckbox(UITestControl row)
        {
            var firstOrDefaultCell = row.GetChildren().Where(child => child.ControlType == ControlType.Cell).ElementAtOrDefault(4);
            return firstOrDefaultCell?.GetChildren().FirstOrDefault(child => child.ControlType == ControlType.CheckBox) as WpfCheckBox;
        }

        public UITestControl FindAddRemoveRowButton(UITestControl row)
        {
            var firstOrDefaultCell = row.GetChildren().Where(child => child.ControlType == ControlType.Cell).ElementAtOrDefault(5);
            return firstOrDefaultCell?.GetChildren().FirstOrDefault(child => child.ControlType == ControlType.Button);
        }

        [When(@"I Select SecurityTab")]
        public void Select_SecurityTab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab, new Point(102, 10));
        }

        [When(@"I Select PerfomanceCounterTab")]
        public void Select_PerfomanceCounterTab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.PerfomanceCounterTab, new Point(124, 14));
        }

        [When(@"I Select LoggingTab")]
        public void Select_LoggingTab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.LoggingTab, new Point(57, 7));
        }

        public void Click_Settings_Resource_Permissions_Row1_Add_Resource_Button()
        {
            Mouse.Click(FindAddResourceButton(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1));
            Assert.IsTrue(DialogsUIMap.ServicePickerDialog.Exists, "Service picker dialog does not exist.");
        }

        [When(@"I Click Select Resource Button From Resource Permissions")]
        public void Click_Select_Resource_Button_From_Resource_Permissions()
        {
            Mouse.Click(FindAddResourceButton(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1), new Point(13, 16));
            Assert.IsTrue(DialogsUIMap.ServicePickerDialog.Exists, "Service window does not exist after clicking SelectResource button");
        }

        [When(@"I Click Reset Perfomance Counter")]
        public void Click_Reset_Perfomance_Counter()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.PerfomanceCounterTab.PerfmonViewContent.ResetCounter.ItemHyperlink, new Point(49, 9));
            Assert.IsTrue(DialogsUIMap.MessageBoxWindow.Exists, "MessageBoxWindow did not show after clicking reset counters");
            Mouse.Click(DialogsUIMap.MessageBoxWindow.OKButton, new Point(50, 12));
        }

        [When(@"I Click Select Resource Button")]
        public void Click_Select_ResourceButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.PerfomanceCounterTab.PerfmonViewContent.ResourceTable.Row1.ResourceCell.ResourceButton, new Point(9, 8));
        }

        [Given(@"I Check Public Administrator")]
        [When(@"I Check Public Administrator")]
        [Then(@"I Check Public Administrator")]
        public void Check_Public_Administrator()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox.Checked = true;
        }

        [Given(@"I Check Public Deploy To")]
        [When(@"I Check Public Deploy To")]
        [Then(@"I Check Public Deploy To")]
        public void Check_Public_Deploy_To()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployToCell.Public_DeployToCheckBox.Checked = true;
        }

        [Given(@"I Check Public Deploy From")]
        [When(@"I Check Public Deploy From")]
        [Then(@"I Check Public Deploy From")]
        public void Check_Public_Deploy_From()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployFromCell.Public_DeployFromCheckBox.Checked = true;
        }

        [Given(@"I Check Public View")]
        [When(@"I Check Public View")]
        [Then(@"I Check Public View")]
        public void Check_Public_View()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox.Checked = true;
        }

        [Given(@"I Check Public Execute")]
        [When(@"I Check Public Execute")]
        [Then(@"I Check Public Execute")]
        public void Check_Public_Execute()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ExecuteCell.Public_ExecuteCheckBox.Checked = true;
        }

        [Given(@"I Check Public Contribute")]
        [When(@"I Check Public Contribute")]
        [Then(@"I Check Public Contribute")]
        public void Check_Public_Contribute()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ContributeCell.Public_ContributeCheckBox.Checked = true;
        }

        [Given(@"I Uncheck Public Administrator")]
        [When(@"I Uncheck Public Administrator")]
        [Then(@"I Uncheck Public Administrator")]
        public void Uncheck_Public_Administrator()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox.Checked = false;
        }

        [Given(@"I Uncheck Public Deploy To")]
        [When(@"I Uncheck Public Deploy To")]
        [Then(@"I Uncheck Public Deploy To")]
        public void Uncheck_Public_Deploy_To()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployToCell.Public_DeployToCheckBox.Checked = false;
        }

        [Given(@"I Uncheck Public Deploy From")]
        [When(@"I Uncheck Public Deploy From")]
        [Then(@"I Uncheck Public Deploy From")]
        public void Uncheck_Public_Deploy_From()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployFromCell.Public_DeployFromCheckBox.Checked = false;
        }

        [Given(@"I Uncheck Public View")]
        [When(@"I Uncheck Public View")]
        [Then(@"I Uncheck Public View")]
        public void Uncheck_Public_View()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox.Checked = false;
        }

        [Given(@"I Uncheck Public Execute")]
        [When(@"I Uncheck Public Execute")]
        [Then(@"I Uncheck Public Execute")]
        public void Uncheck_Public_Execute()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ExecuteCell.Public_ExecuteCheckBox.Checked = false;
        }

        [Given(@"I Uncheck Public Contribute")]
        [When(@"I Uncheck Public Contribute")]
        [Then(@"I Uncheck Public Contribute")]
        public void Uncheck_Public_Contribute()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ContributeCell.Public_ContributeCheckBox.Checked = false;
        }

        [When(@"I UnCheck Public Administrator")]
        public void UnCheck_Public_Administrator()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox.Checked = false;
            Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox.Checked, "Public Administrator checkbox is checked after UnChecking Administrator.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox.Checked, "Public View checkbox is unchecked after unChecking Administrator.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ExecuteCell.Public_ExecuteCheckBox.Checked, "Public Execute checkbox unchecked after unChecking Administrator.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ContributeCell.Public_ContributeCheckBox.Checked, "Public Contribute checkbox is unchecked after unChecking Administrator.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployFromCell.Public_DeployFromCheckBox.Checked, "Public DeplotFrom checkbox is unchecked after unChecking Administrator.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployToCell.Public_DeployToCheckBox.Checked, "Public DeployTo checkbox is unchecked after unChecking Administrator.");
        }

        [Given(@"I Check Resource Contribute")]
        [When(@"I Check Resource Contribute")]
        [Then(@"I Check Resource Contribute")]
        public void Check_Resource_Contribute()
        {
            FindContributePermissionsCheckbox(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Checked = true;
            Assert.IsTrue(FindContributePermissionsCheckbox(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Checked, "Resource View checkbox is NOT checked after Checking Contribute.");
            Assert.IsTrue(FindContributePermissionsCheckbox(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Checked, "Resource Execute checkbox is NOT checked after Checking Contribute.");
            Assert.IsTrue(FindAddRemoveRowButton(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Enabled, "Resource Delete button is disabled");
        }

        [Given(@"I UnCheck Public View")]
        [When(@"I UnCheck Public View")]
        [Then(@"I UnCheck Public View")]
        public void UnCheck_Public_View()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox.Checked = false;
            Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox.Checked, "Public View checkbox is checked after Checking Contribute.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ExecuteCell.Public_ExecuteCheckBox.Checked, "Public Execute checkbox is NOT checked after Checking Contribute.");
            Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ContributeCell.Public_ContributeCheckBox.Checked, "Public Contribute checkbox is checked after UnChecking Execute/View.");
            Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox.Checked, "Public Administrator checkbox is checked after UnChecking Contribute.");
        }

        [Given(@"I setup Public Permissions for ""(.*)"" for localhost")]
        public void SetupPublicPermissionsForForLocalhost(string resource)
        {
            UIMap.Click_Settings_RibbonButton();
            var deleteFirstResourceButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.RemovePermissionButton;
            if (deleteFirstResourceButton.Enabled)
            {
                var isViewChecked = FindViewPermissionsCheckbox(
                    MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext
                        .SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Checked;

                var isExecuteChecked = FindExecutePermissionsCheckbox(
                    MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext
                        .SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Checked;

                if (isViewChecked && isExecuteChecked)
                {
                    UIMap.Click_Close_Settings_Tab_Button();
                    return;
                }
            }
            Set_FirstResource_ResourcePermissions(resource, "Public", true, true);
            UIMap.Click_Close_Settings_Tab_Button();
        }

        public void Enter_GroupName_Into_Settings_Dialog_Resource_Permissions_Row1_Windows_Group_Textbox(string GroupName)
        {
            FindWindowsGroupTextbox(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Text = GroupName;
            Assert.AreEqual(FindWindowsGroupTextbox(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Text, GroupName, "Settings security tab resource permissions row 1 windows group textbox text does not equal Public.");
        }

        public void Click_Settings_Security_Tab_ResourcePermissions_Row1_Execute_Checkbox()
        {
            FindExecutePermissionsCheckbox(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Checked = true;
            Assert.IsTrue(FindExecutePermissionsCheckbox(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Checked, "Settings security tab resource permissions row 1 execute checkbox is not checked.");
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled");
        }

        public void Click_Settings_Security_Tab_Resource_Permissions_Row1_View_Checkbox()
        {
            FindViewPermissionsCheckbox(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Checked = true;
            Assert.IsTrue(FindViewPermissionsCheckbox(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Checked, "Settings resource permissions row1 view checkbox is not checked.");
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled");
        }

        public void Click_Settings_Security_Tab_Resource_Permissions_Row1_Contribute_Checkbox()
        {
            FindContributePermissionsCheckbox(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Checked = true;
            Assert.IsTrue(FindContributePermissionsCheckbox(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Checked, "Settings resource permissions row1 view checkbox is not checked.");
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled");
        }

        public void Click_Settings_Resource_Permissions_Row1_Delete_Button()
        {
            Mouse.Click(FindAddRemoveRowButton(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1));
        }

        [When(@"I Click Settings Security Resource Permissions Add Resource Button")]
        public void Click_Settings_Security_Resource_Permissions_Add_Resource_Button()
        {
            Mouse.Click(FindAddResourceButton(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1), new Point(6, 15));
        }

        [Given(@"I Set Resource Permissions For ""(.*)"" to Group ""(.*)"" and Permissions for View to ""(.*)"" and Contribute to ""(.*)"" and Execute to ""(.*)""")]
        [When(@"I Set Resource Permissions For ""(.*)"" to Group ""(.*)"" and Permissions for View to ""(.*)"" and Contribute to ""(.*)"" and Execute to ""(.*)""")]
        [Then(@"I Set Resource Permissions For ""(.*)"" to Group ""(.*)"" and Permissions for View to ""(.*)"" and Contribute to ""(.*)"" and Execute to ""(.*)""")]
        public void SetResourcePermissions(string ResourceName, string WindowsGroupName, bool setView = false, bool setExecute = false, bool setContribute = false)
        {
            UIMap.Click_Settings_RibbonButton();
            Click_Settings_Resource_Permissions_Row1_Add_Resource_Button();
            DialogsUIMap.Select_SubItem_Service_From_Service_Picker_Dialog(ResourceName);
            Enter_GroupName_Into_Settings_Dialog_Resource_Permissions_Row1_Windows_Group_Textbox(WindowsGroupName);
            if (setView)
            {
                Click_Settings_Security_Tab_Resource_Permissions_Row1_View_Checkbox();
            }
            if (setExecute)
            {
                Click_Settings_Security_Tab_ResourcePermissions_Row1_Execute_Checkbox();
            }
            if (setContribute)
            {
                Click_Settings_Security_Tab_Resource_Permissions_Row1_Contribute_Checkbox();
            }
            UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
        }

        [Given("Dice Is Selected InSettings Tab Permissions Row 1")]
        [When(@"I Assert Dice Is Selected InSettings Tab Permissions Row1")]
        [Then("Dice Is Selected InSettings Tab Permissions Row 1")]
        public void Assert_Dice_Is_Selected_InSettings_Tab_Permissions_Row_1()
        {
            Assert.AreEqual("Dice1", FindSelectedResourceText(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).DisplayText, "Resource Name is not set to Dice after selecting Dice from Service picker");
        }

        [When(@"I Enter Public As Windows Group")]
        public void Enter_Public_As_Windows_Group()
        {
            FindWindowsGroupTextbox(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Text = "Public";
        }

        public void Set_FirstResource_ResourcePermissions(string ResourceName, string WindowsGroupName, bool setView = false, bool setExecute = false, bool setContribute = false)
        {
            Click_Settings_Resource_Permissions_Row1_Add_Resource_Button();
            DialogsUIMap.Select_First_Service_From_Service_Picker_Dialog(ResourceName);
            Enter_GroupName_Into_Settings_Dialog_Resource_Permissions_Row1_Windows_Group_Textbox(WindowsGroupName);
            if (setView)
            {
                Click_Settings_Security_Tab_Resource_Permissions_Row1_View_Checkbox();
            }
            if (setExecute)
            {
                Click_Settings_Security_Tab_ResourcePermissions_Row1_Execute_Checkbox();
            }
            if (setContribute)
            {
                Click_Settings_Security_Tab_Resource_Permissions_Row1_Contribute_Checkbox();
            }
            UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
        }

        [When(@"I Click Server Log File Button")]
        public void Click_Server_Log_File_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.LoggingTab.LogSettingsViewConte.ServerLogs.ServerLogFile.ItemHyperlink, new Point(83, 6));
        }

        [When(@"I Click Studio Log File")]
        public void Click_Studio_Log_File()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.LoggingTab.LogSettingsViewConte.StudioLogs.StudioLogFile.ItemHyperlink, new Point(79, 10));
        }
    }
}
