using System;
using Dev2.CodedUI.Tests;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Studio.UI.Tests.UIMaps
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    [Ignore]
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

        //2013.03.14: Ashley Lewis - Bug 9217
        [TestMethod]
        public void DatabaseServiceWizardCreateNewServiceExpectedServiceCreated()
        {
            //Initialization
            var serverSourceCategoryName = Guid.NewGuid().ToString().Substring(0, 5);
            var serverSourceName = Guid.NewGuid().ToString().Substring(0, 5);
            DatabaseServiceWizardUIMap.InitializeFullTestServiceAndSource("CODEDUITESTS" + serverSourceCategoryName, "codeduitest" + serverSourceName);

            //Assert
            Assert.IsTrue(ExplorerUIMap.ServiceExists("localhost", "SERVICES", "CODEDUITESTS" + serverSourceName, "codeduitest" + serverSourceName));
            Assert.IsTrue(ExplorerUIMap.ServiceExists("localhost", "SOURCES", "CODEDUITESTS" + serverSourceName, "codeduitest" + serverSourceName));

            //Cleanup
            ExplorerUIMap.RightClickDeleteProject("localhost", "SERVICES", "CODEDUITESTS" + serverSourceName, "codeduitest" + serverSourceName);
            ExplorerUIMap.RightClickDeleteProject("localhost", "SOURCES", "CODEDUITESTS" + serverSourceName, "codeduitest" + serverSourceName);
        }

        //2013.03.15: Ashley Lewis - Bug 9217
        [TestMethod]
        public void DatabaseServiceWizardCreateNewServiceWithTotallyBlankWorkspaceExpectedServiceCreated()
        {
            //Initialize server
           // var serverManipulations = new ServerBugTests();
           // serverManipulations.ClearServerWorkSpace(TestContext.TestRunDirectory);
            System.Threading.Thread.Sleep(2000);//Give the server time to initialize (shouldn't take long since server workspace is clear)
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DoRefresh();
            //Initialize service complete with its own new source
            var bothServerAndSourceCategoryNames = "CODEDUITESTS" + Guid.NewGuid().ToString().Substring(0, 5);
            var bothServerAndSourceNames = "codeduitest" + Guid.NewGuid().ToString().Substring(0, 5);
            DatabaseServiceWizardUIMap.InitializeFullTestServiceAndSource(bothServerAndSourceCategoryNames, bothServerAndSourceNames);

            //Assert
            Assert.IsTrue(ExplorerUIMap.ServiceExists("localhost", "SERVICES", bothServerAndSourceCategoryNames, bothServerAndSourceNames));
            Assert.IsTrue(ExplorerUIMap.ServiceExists("localhost", "SOURCES", bothServerAndSourceCategoryNames, bothServerAndSourceNames));

            //Clean up
            ExplorerUIMap.RightClickDeleteProject("localhost", "SERVICES", bothServerAndSourceCategoryNames, bothServerAndSourceNames);
            ExplorerUIMap.RightClickDeleteProject("localhost", "SOURCES", bothServerAndSourceCategoryNames, bothServerAndSourceNames);
            //serverManipulations.RefillWorkSpaceAfterClear(TestContext.TestRunDirectory);
        }
    }
}
