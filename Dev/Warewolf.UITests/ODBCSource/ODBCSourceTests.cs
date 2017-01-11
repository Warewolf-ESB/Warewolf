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
            //Assert.IsFalse(UIMap.ControlExistsNow(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserRadioButton), "User authentification radio button is visible.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.TestConnectionButton.Enabled, "Test Connection Button is enabled.");
            UIMap.Click_DB_Source_Wizard_Test_Connection_Button();
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