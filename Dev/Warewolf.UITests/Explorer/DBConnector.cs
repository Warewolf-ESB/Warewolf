using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;


namespace Warewolf.UITests
{
    [CodedUITest]
    public class DBConnector
    {
        const string DBSourceName = "UITestingDBSource";

        [TestMethod]
        public void BigDBConnectorUITest()
        {
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Click_NewDatabaseSource_Ribbon_Button();
            Uimap.Select_MSSQLSERVER_From_DB_Source_Wizard_Address_Protocol_Dropdown();
            Uimap.Type_rsaklfsvrgen_into_DB_Source_Wizard_Server_Textbox();
            Uimap.Select_RSAKLFSVRGENDEV_From_Server_Source_Wizard_Dropdownlist();
            Uimap.Click_DB_Source_Wizard_Test_Connection_Button();
            Uimap.WaitForSpinner(Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBConnectorWizardTab.WorkSurfaceContext.ErrorText.Spinner);
            Uimap.Select_Dev2TestingDB_From_DB_Source_Wizard_Database_Combobox();
            Uimap.Save_With_Ribbon_Button_And_Dialog(DBSourceName);
            Uimap.Click_Close_DB_Source_Wizard_Tab_Button();
            Uimap.Drag_Toolbox_SQL_Server_Tool_Onto_DesignSurface();
            Uimap.TryClearToolboxFilter();
            Uimap.Open_Sql_Server_Tool_Large_View();
            Uimap.Select_UITestingDBSource_From_SQL_Server_Large_View_Source_Combobox();
            Uimap.Select_GetCountries_From_SQL_Server_Large_View_Action_Combobox();
            Uimap.Type_0_Into_SQL_Server_Large_View_Inputs_Row1_Data_Textbox();
            Uimap.Click_SQL_Server_Large_View_Generate_Outputs();
            Uimap.Type_0_Into_SQL_Server_Large_View_Test_Inputs_Row1_Test_Data_Textbox();
            Uimap.Click_SQL_Server_Large_View_Test_Inputs_Button();
            Uimap.Click_SQL_Server_Large_View_Test_Inputs_Done_Button();
            Uimap.Click_SQL_Server_Large_View_Done_Button();
            Uimap.Click_Debug_Ribbon_Button();
            Uimap.Click_DebugInput_Debug_Button();
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
            Uimap.TryCloseHangingSaveDialog();
            Uimap.TryRemoveFromExplorer(DBSourceName);
            Uimap.TryClearToolboxFilter();
            Uimap.TryCloseWorkflowTabs();
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
