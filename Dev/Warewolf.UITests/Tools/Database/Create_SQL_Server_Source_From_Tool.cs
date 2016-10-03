using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class Create_SQL_Server_Source_From_Tool
    {
        [TestMethod]
        [TestCategory("Tools")]
        public void SQLServerSourceFromTool()
        {
            Uimap.Drag_Toolbox_SQL_Server_Tool_Onto_DesignSurface();
            Uimap.Open_SQL_Large_View_FromContextMenu();
            Uimap.Select_NewDatabaseSource_FromSqlServerTool();
            Uimap.Change_Selected_Database_ToMySql_DataBase();
            Uimap.Change_Selected_Database_ToPostgreSql_DataBase();
            Uimap.Change_Selected_Database_ToOracle_DataBase();
            Uimap.Change_Selected_Database_ToODBC_DataBase();
            Uimap.Click_DB_Source_Wizard_Test_Connection_Button();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
#if !DEBUG
            Uimap.CloseHangingDialogs();
#endif
            Uimap.InitializeABlankWorkflow();
        }

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
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
                if ((_uiMap == null))
                {
                    _uiMap = new UIMap();
                }

                return _uiMap;
            }
        }

        private UIMap _uiMap;

        #endregion

        public UIMap UIMap
        {
            get
            {
                if ((this.map == null))
                {
                    this.map = new UIMap();
                }

                return this.map;
            }
        }

        private UIMap map;
    }
}
