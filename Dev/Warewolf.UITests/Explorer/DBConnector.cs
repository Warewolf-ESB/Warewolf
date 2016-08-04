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
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    public class DBConnector
    {
        [TestMethod]
        public void BigDBConnectorUITest()
        {
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Select_NewDatabaseSource_FromExplorerContextMenu();
            Uimap.Select_MSSQLSERVER_From_DB_Source_Wizard_Address_Protocol_Dropdown();
            // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        }

        #region Additional test attributes

        // You can use the following additional attributes as you write your tests:
        
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
            // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
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
