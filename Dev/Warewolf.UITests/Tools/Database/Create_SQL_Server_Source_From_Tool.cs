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
            UIMap.Drag_Toolbox_SQL_Server_Tool_Onto_DesignSurface();
            UIMap.Open_SQL_Large_View_FromContextMenu();
            UIMap.Select_NewDatabaseSource_FromSqlServerTool();
            UIMap.Change_Selected_Database_ToMySql_DataBase();
            UIMap.Change_Selected_Database_ToPostgreSql_DataBase();
            UIMap.Change_Selected_Database_ToOracle_DataBase();
            UIMap.Change_Selected_Database_ToODBC_DataBase();
            UIMap.Click_DB_Source_Wizard_Test_Connection_Button();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
#if !DEBUG
            UIMap.CloseHangingDialogs();
#endif
            UIMap.InitializeABlankWorkflow();
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
