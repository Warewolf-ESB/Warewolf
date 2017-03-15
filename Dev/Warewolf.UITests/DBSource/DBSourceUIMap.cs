using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using System.Windows.Input;
using MouseButtons = System.Windows.Forms.MouseButtons;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using System.Drawing;
using System.IO;
using TechTalk.SpecFlow;
using Warewolf.UITests.Common;
using Warewolf.UITests.ExplorerUIMapClasses;
using Warewolf.UITests.WorkflowTesting.WorkflowServiceTestingUIMapClasses;
using Warewolf.UITests.DialogsUIMapClasses;
using Warewolf.UITests.Deploy.DeployUIMapClasses;
using Warewolf.UITests.Settings.SettingsUIMapClasses;
using Warewolf.UITests.ServerSource.ServerSourceUIMapClasses;

namespace Warewolf.UITests.DBSource.DBSourceUIMapClasses
{
    [Binding]
    public partial class DBSourceUIMap
    {
        [Given(@"I Click Close DB Source Wizard Tab Button")]
        [When(@"I Click Close DB Source Wizard Tab Button")]
        [Then(@"I Click Close DB Source Wizard Tab Button")]
        public void Click_Close_DB_Source_Wizard_Tab_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.CloseButton, new Point(13, 4));
        }

        [Given(@"I Click UserButton On Database Source")]
        [When(@"I Click UserButton On Database Source")]
        [Then(@"I Click UserButton On Database Source")]
        public void Click_UserButton_On_DatabaseSource()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.UserRadioButton);
        }

        [Given(@"I Click WindowsButton On Database Source")]
        [When(@"I Click WindowsButton On Database Source")]
        [Then(@"I Click WindowsButton On Database Source")]
        public void Click_WindowsButton_On_DatabaseSource()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.WindowsRadioButton);
        }

        [Given(@"I Enter TestUser Username And Password on Database source")]
        [When(@"I Enter TestUser Username And Password on Database source")]
        [Then(@"I Enter TestUser Username And Password on Database source")]
        public void IEnterRunAsUserTestUserOnDatabaseSource()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.UserNameTextBox.Text = "testuser";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.PasswordTextBox.Text = "test123";
        }

        [Given(@"I Click DB Source Wizard Test Connection Button")]
        [When(@"I Click DB Source Wizard Test Connection Button")]
        [Then(@"I Click DB Source Wizard Test Connection Button")]
        public void Click_DB_Source_Wizard_Test_Connection_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.TestConnectionButton, new Point(21, 16));
            UIMap.WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.Spinner);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.Exists, "Database Combobox is not visible.");
        }


        [Given(@"The DB Source Wizard Test Succeeded Image Is Visible")]
        [When(@"The DB Source Wizard Test Succeeded Image Is Visible")]
        [Then(@"The DB Source Wizard Test Succeeded Image Is Visible")]
        public void Assert_The_DB_Source_Wizard_Test_Succeeded_Image_Is_Visible()
        {
            var point = new Point();
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ConnectionPassedImage.TryGetClickablePoint(out point), "New DB source wizard test succeeded image is not visible after testing with RSAKLFSVRGENDEV and waiting for the spinner.");
        }

        [When(@"I Select RSAKLFSVRGENDEV From Server Source Wizard Dropdownlist")]
        public void Select_RSAKLFSVRGENDEV_From_Server_Source_Wizard_Dropdownlist()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.RSAKLFSVRGENDEV, new Point(97, 17));
            Assert.AreEqual("RSAKLFSVRGENDEV", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.Textbox.Text, "RSAKLFSVRGENDEV is not selected as the server in the DB source wizard.");
        }

        [Then(@"I Enter Text Into Database Server Tab")]
        [Given(@"I Enter Text Into Database Server Tab")]
        [Then(@"I Enter Text Into Database Server Tab")]
        public void Enter_Text_Into_DatabaseServer_Tab()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.Textbox.Text = "RSAKLFSVRGENDEV";
        }

        [When(@"I Enter RunAsUser(Root) Username And Password on Database source")]
        [Given(@"I Enter RunAsUser(Root) Username And Password on Database source")]
        [Then(@"I Enter RunAsUser(Root) Username And Password on Database source")]
        public void IEnterRunAsUserRootOnDatabaseSource()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.UserNameTextBox.Text = "root";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.PasswordTextBox.Text = "admin";
        }

        [When(@"I Enter RunAsUser(PostGres) Username And Password on Database source")]
        [Given(@"I Enter RunAsUser(PostGres) Username And Password on Database source")]
        [Then(@"I Enter RunAsUser(PostGres) Username And Password on Database source")]
        public void IEnterRunAsUserPostGresOnDatabaseSource()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.UserNameTextBox.Text = "postgres";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.PasswordTextBox.Text = "test123";
        }

        [When(@"I Select mysql From DB Source Wizard Database Combobox")]
        [Given(@"I Select mysql From DB Source Wizard Database Combobox")]
        [Then(@"I Select mysql From DB Source Wizard Database Combobox")]
        public void Select_mysql_From_DB_Source_Wizard_Database_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.ToggleButton);
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsmysqlDB);
            Assert.AreEqual("mysql", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.UIMysqlText.DisplayText);
        }

        [When(@"I Select postgres From DB Source Wizard Database Combobox")]
        [Given(@"I Select postgres From DB Source Wizard Database Combobox")]
        [Then(@"I Select postgres From DB Source Wizard Database Combobox")]
        public void Select_postgres_From_DB_Source_Wizard_Database_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.ToggleButton);
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAspostgresDB);
            Assert.AreEqual("postgres", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.UIPostgresText.DisplayText);
        }

        [When(@"I Select HR From DB Source Wizard Database Combobox")]
        [Given(@"I Select HR From DB Source Wizard Database Combobox")]
        [Then(@"I Select HR From DB Source Wizard Database Combobox")]
        public void Select_HR_From_DB_Source_Wizard_Database_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.ToggleButton);
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsHRDB);
            Assert.AreEqual("HR", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.UIHRText.DisplayText);
        }

        [When(@"I Select ExcelFiles From DB Source Wizard Database Combobox")]
        [Given(@"I Select ExcelFiles From DB Source Wizard Database Combobox")]
        [Then(@"I Select ExcelFiles From DB Source Wizard Database Combobox")]
        public void Select_ExcelFiles_From_DB_Source_Wizard_Database_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.ToggleButton);
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsExcelFilesDB);
            Assert.AreEqual("Excel Files", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.UIExcelFilesText.DisplayText);
        }

        [When(@"I Select MSAccess From DB Source Wizard Database Combobox")]
        [Given(@"I Select MSAccess From DB Source Wizard Database Combobox")]
        [Then(@"I Select MSAccess From DB Source Wizard Database Combobox")]
        public void Select_MSAccess_From_DB_Source_Wizard_Database_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.ToggleButton);
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsMSAccessDB);
            Assert.AreEqual("MS Access Database", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.MSAccessDatabaseText.DisplayText);
        }

        [When(@"I Select TestDB From DB Source Wizard Database Combobox")]
        [Given(@"I Select TestDB From DB Source Wizard Database Combobox")]
        [Then(@"I Select TestDB From DB Source Wizard Database Combobox")]
        public void Select_TestDB_From_DB_Source_Wizard_Database_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.ToggleButton);
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsTestDB);
            Assert.AreEqual("TestDB", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.TestDBText.DisplayText);
        }

        [When(@"I Select test From DB Source Wizard Database Combobox")]
        [Given(@"I Select test From DB Source Wizard Database Combobox")]
        [Then(@"I Select test From DB Source Wizard Database Combobox")]
        public void Select_test_From_DB_Source_Wizard_Database_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.ToggleButton);
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAstest);
            Assert.AreEqual("test", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.testText.DisplayText);
        }

        [When(@"I Select master From DB Source Wizard Database Combobox")]
        [Given(@"I Select master From DB Source Wizard Database Combobox")]
        [Then(@"I Select master From DB Source Wizard Database Combobox")]
        public void Select_master_From_DB_Source_Wizard_Database_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.ToggleButton);
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsmaster);
            Assert.AreEqual("master", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.masterText.DisplayText);
        }

        [When(@"I Select Dev2TestingDB From DB Source Wizard Database Combobox")]
        public void Select_Dev2TestingDB_From_DB_Source_Wizard_Database_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.ToggleButton);
            Mouse.Click(UIMap.MainStudioWindow.Dev2TestingDBCustom);
            Assert.AreEqual("Dev2TestingDB", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.UIDev2TestingDBText.DisplayText);
        }

        [Given(@"I Type rsaklfsvrgen into DB Source Wizard Server Textbox")]
        [When(@"I Type rsaklfsvrgen into DB Source Wizard Server Textbox")]
        [Then(@"I Type rsaklfsvrgen into DB Source Wizard Server Textbox")]
        public void Type_rsaklfsvrgen_into_DB_Source_Wizard_Server_Textbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.Textbox.Text = "rsaklfsvrgen";
        }

        [Given(@"RSAKLFSVRGENDEV appears as an option in the DB source wizard server combobox")]
        [Then(@"RSAKLFSVRGENDEV appears as an option in the DB source wizard server combobox")]
        public void Assert_RSAKLFSVRGENDEV_appears_as_an_option_in_the_DB_source_wizard_server_combobox()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.RSAKLFSVRGENDEV.Exists, "RSAKLFSVRGENDEV does not exist as an option in DB source wizard server combobox.");
        }

        [When(@"I Type RSAKLFSVRGENDEV into DB Source Wizard Server Textbox")]
        public void Type_RSAKLFSVRGENDEV_into_DB_Source_Wizard_Server_Textbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.Textbox.Text = "RSAKLFSVRGENDEV";
        }

        public UIMap UIMap
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

        WorkflowTabUIMap WorkflowTabUIMap
        {
            get
            {
                if (_WorkflowTabUIMap == null)
                {
                    _WorkflowTabUIMap = new WorkflowTabUIMap();
                }

                return _WorkflowTabUIMap;
            }
        }

        private WorkflowTabUIMap _WorkflowTabUIMap;

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

        DeployUIMap DeployUIMap
        {
            get
            {
                if (_DeployUIMap == null)
                {
                    _DeployUIMap = new DeployUIMap();
                }

                return _DeployUIMap;
            }
        }

        private DeployUIMap _DeployUIMap;

        SettingsUIMap SettingsUIMap
        {
            get
            {
                if (_SettingsUIMap == null)
                {
                    _SettingsUIMap = new SettingsUIMap();
                }

                return _SettingsUIMap;
            }
        }

        private SettingsUIMap _SettingsUIMap;

        ServerSourceUIMap ServerSourceUIMap
        {
            get
            {
                if (_ServerSourceUIMap == null)
                {
                    _ServerSourceUIMap = new ServerSourceUIMap();
                }

                return _ServerSourceUIMap;
            }
        }

        private ServerSourceUIMap _ServerSourceUIMap;
    }
}
