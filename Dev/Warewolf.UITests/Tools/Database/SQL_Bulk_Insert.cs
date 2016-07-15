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


namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class SQL_Bulk_Insert
    {
        [TestMethod]
        public void SQLBulkInsertToolUITest()
        {
            //Scenario: Drag toolbox SQL_Bulk_Insert onto a new workflow
            Uimap.Drag_Toolbox_SQL_Bulk_Insert_Onto_DesignSurface();
            Uimap.Assert_Sql_Bulk_insert_Exists_OnDesignSurface();

            //@NeedsSQL_Bulk_InsertToolSmallViewOnTheDesignSurface
            //Scenario: Double Clicking SQL_Bulk_Insert Tool Small View on the Design Surface Opens Large View
            Uimap.Open_SQL_Bulk_Insert_Tool_Large_View();
            Uimap.Assert_SQL_Bulk_Insert_Large_View_Exists_OnDesignSurface();

            //@NeedsSQLBulkInsertLargeViewOnTheDesignSurface
            //Scenario: Click SQL Bulk Insert Tool QVI Button Opens Qvi
            Uimap.Open_SQL_Bulk_Insert_Tool_Qvi_Large_View();
            Uimap.Assert_Sql_Bulk_insert_Qvi_Exists_OnDesignSurface();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            Uimap.SetGlobalPlaybackSettings();
            Uimap.WaitIfStudioDoesNotExist();
            Console.WriteLine("Test \"" + TestContext.TestName + "\" starting on " + System.Environment.MachineName);
            Uimap.InitializeABlankWorkflow();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            Uimap.CleanupWorkflow();
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
    }
}
