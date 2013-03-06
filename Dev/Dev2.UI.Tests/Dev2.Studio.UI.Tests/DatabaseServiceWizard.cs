using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Forms;
using System.Drawing;
using Dev2.CodedUI.Tests;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;


namespace Dev2.Studio.UI.Tests.UIMaps
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    public class DatabaseServiceWizard : UIMapBase
    {
        TestBase myTestBase = new TestBase();
        // These run at the start of every test to make sure everything is sane
        [TestInitialize]
        public void CheckStartIsValid()
        {
            // Use the base class for validity checks - Easier to control :D
            myTestBase.CheckStartIsValid();
        }

        #region Additional test attributes

        // You can use the following additional attributes as you write your tests:

        ////Use TestInitialize to run code before running each test 
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        //    // For more information on generated code, see http://go.microsoft.com/fwlink/?LinkId=179463
        //}

        ////Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        //    // For more information on generated code, see http://go.microsoft.com/fwlink/?LinkId=179463
        //}

        #endregion

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

        [TestMethod]
        public void DatabaseServiceWizardCreateNewSourceExpectedSourceCreated()
        {
            RibbonUIMap.ClickRibbonMenuItem("Home", "Database Service");
            System.Threading.Thread.Sleep(5000);
            Keyboard.SendKeys("{TAB}{TAB}{ENTER}RSAKLFSVRGENDEV{TAB}{RIGHT}{TAB}testuser{TAB}test123{TAB}{ENTER}{TAB}{DOWN}{TAB}{ENTER}{ENTER}CODEDUITESTS{ENTER}{TAB}{TAB}{TAB}codeduitest123{ENTER}{TAB}{ENTER}");
            var WorkspaceDirectory = myTestBase.ServerExeLocation.Substring(0, myTestBase.ServerExeLocation.LastIndexOf('\\')) + "\\Workspaces";

            var success = false;
            foreach (var workspace in Directory.GetDirectories(WorkspaceDirectory).ToList())
            {
                foreach (var resource in Directory.GetFiles(workspace + "\\Sources").ToList())
                {
                    if (resource.Contains("codeduitest123"))
                    {
                        success = true;
                        File.Delete(resource);
                    }
                }
            }
            Assert.IsTrue(success);
        }
    }
}
