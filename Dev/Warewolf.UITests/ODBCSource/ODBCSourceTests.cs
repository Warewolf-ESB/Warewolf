using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class ODBCSourceTests
    {
        const string SourceName = "CodedUITestODBCSource";

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void ODBCSource_CreateSourceUITests()
        {
            UIMap.Select_NewODBCSource_FromExplorerContextMenu();
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.Enabled, "ODBC server combobox is enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.TestConnectionButton.Enabled, "Test Connection Button is enabled.");
            UIMap.Click_DB_Source_Wizard_Test_Connection_Button();
            UIMap.Select_ExcelFiles_From_DB_Source_Wizard_Database_Combobox();
            UIMap.Save_With_Ribbon_Button_And_Dialog("TestODBCDBSource");
            UIMap.Filter_Explorer("TestODBCDBSource");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists, "Database did not save in the explorer UI.");
            UIMap.Click_Close_DB_Source_Wizard_Tab_Button();
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