using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.PostgreSQLSource
{
    [CodedUITest]
    public class PostgreSQLSourceTests
    {
        const string SourceName = "CodedUITestMyPostgreSQLSource";
        private const string editSourceName = "PostgreSQLDatabaseSourceToEdit";

        [TestMethod]
        [TestCategory("Database Sources")]
        // ReSharper disable once InconsistentNaming
        public void Create_PostgreSQLSource_From_ExplorerContextMenu_UITests()
        {
            UIMap.Select_NewPostgreSQLSource_From_ExplorerContextMenu();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.Exists, "PostgreSQL Source Tab does not exist");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.Enabled, "PostgreSQL Server Address combobox is disabled new PostgreSQL Source wizard tab");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.UserRadioButton.Enabled, "User authentification rabio button is not enabled.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.TestConnectionButton.Enabled, "Test Connection Button is enabled.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.UserNameTextBox.Enabled, "Username textbox is not enabled.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.PasswordTextBox.Enabled, "Password textbos is not enabled.");
            UIMap.Enter_Text_Into_DatabaseServer_Tab();
            UIMap.IEnterRunAsUserPostGresOnDatabaseSource();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.TestConnectionButton.Enabled, "Test Connection Button is not enabled.");
            UIMap.Click_DB_Source_Wizard_Test_Connection_Button();
            UIMap.Select_postgres_From_DB_Source_Wizard_Database_Combobox();
            UIMap.Save_With_Ribbon_Button_And_Dialog(SourceName);
            UIMap.Filter_Explorer(SourceName);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists, "Source did not save in the explorer UI.");
        }

        [TestMethod]
        [TestCategory("Database Sources")]
        // ReSharper disable once InconsistentNaming
        public void Edit_PostgreSQLSource_From_ExplorerContextMenu_UITests()
        {
            UIMap.Select_Source_From_ExplorerContextMenu(editSourceName);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.Exists, "PostgreSQL Source Tab does not exist");
            UIMap.Select_TestDB_From_DB_Source_Wizard_Database_Combobox();
            UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            UIMap.Click_Close_DB_Source_Wizard_Tab_Button();
            UIMap.Select_Source_From_ExplorerContextMenu(editSourceName);
            Assert.AreEqual("TestDB", UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.TestDBText.DisplayText);
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
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

        #endregion
    }
}
