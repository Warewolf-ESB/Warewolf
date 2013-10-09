using System;
using System.Drawing;
using Dev2.CodedUI.Tests;
using Dev2.Studio.UI.Tests.UIMaps.ServerWizardClasses;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using Dev2.CodedUI.Tests.UIMaps.ExplorerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses;


namespace Dev2.Studio.UI.Tests
{
    /// <summary>
    /// Summary description for ExplorerUITests
    /// </summary>
    [CodedUITest, System.Runtime.InteropServices.GuidAttribute("DAA88B10-98C4-488E-ACB2-1256C95CE8F0")]
    public class ExplorerUITests
    {
        public ExplorerUITests()
        {
        }

        [TestMethod]
        public void SearchAndRefresh_AttemptToSearch_ExpectedSearchFilteredByAllItems()
        {
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            // Now count
            int allResources = ExplorerUIMap.GetCategoryItems().Count;
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("Integration");
            int allResourcesAfterSearch = ExplorerUIMap.GetCategoryItems().Count;
            ExplorerUIMap.ClearExplorerSearchText();            
            Assert.IsTrue(allResources>allResourcesAfterSearch);
        }

        #region Deprecated Test
        //2013.03.11: Ashley Lewis - Bug 9124
        [TestMethod]
        public void TryConnectWhereBusyConnectingExpectedWizardCanCreateAndDeleteBoth()
        {
            //Initialize
            var connectionWizard = new ServerWizard();
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            var expected = ExplorerUIMap.CountServers() + 2;
            var firstNewServer = Guid.NewGuid().ToString().Substring(0, 5);
            var secondNewServer = Guid.NewGuid().ToString().Substring(0, 5);          

            //Create first server.
            var explorerPane = ExplorerUIMap.ClickConnectControl("Button");
            Mouse.Click(explorerPane, new Point(5, 5));

            System.Threading.Thread.Sleep(100);
            connectionWizard.ClickNewServerAddress();
            Keyboard.SendKeys(@"http://RSAKLFSVRTFSBLD:77/dsf{TAB}{TAB}{ENTER}");
            System.Threading.Thread.Sleep(100);
            Keyboard.SendKeys("{TAB}{ENTER}");
            System.Threading.Thread.Sleep(100);
            Keyboard.SendKeys("{ENTER}CODEDUITEST" + firstNewServer + "{ENTER}");
            System.Threading.Thread.Sleep(100);
            Keyboard.SendKeys("{TAB}{TAB}{TAB}" + firstNewServer + "{ENTER}");

            //Try create second server
            explorerPane = ExplorerUIMap.ClickConnectControl("Button");
            Mouse.Click(explorerPane, new Point(5, 5));

            System.Threading.Thread.Sleep(100);
            connectionWizard.ClickNewServerAddress();
            Keyboard.SendKeys(@"http://RSAKLFSVRWRWBLD:77/dsf{TAB}{TAB}{ENTER}");
            System.Threading.Thread.Sleep(100);
            Keyboard.SendKeys("{TAB}{ENTER}");
            System.Threading.Thread.Sleep(100);
            Keyboard.SendKeys("{ENTER}CODEDUITEST" + secondNewServer + "{ENTER}");
            System.Threading.Thread.Sleep(100);
            Keyboard.SendKeys("{TAB}{TAB}{TAB}" + secondNewServer + "{ENTER}");

            //Assert
            System.Threading.Thread.Sleep(5000);//Servers need time to initialize before they can be counted
            Assert.AreEqual(expected, ExplorerUIMap.CountServers());

            //Clean up
            ExplorerUIMap.Server_RightClick_Delete(firstNewServer);
            ExplorerUIMap.ConnectedServer_RightClick_Delete(secondNewServer);
            ExplorerUIMap.DoRefresh();
            new TestBase().DoCleanup(firstNewServer);
            new TestBase().DoCleanup(secondNewServer);
        }

        #endregion

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

        private DocManagerUIMap _docManagerUIMap;


        private ExplorerUIMap ExplorerUIMap
        {
            get
            {
                if (_explorerUIMap == null)
                {
                    _explorerUIMap = new ExplorerUIMap();
                }
                return _explorerUIMap;
            }
        }

        private ExplorerUIMap _explorerUIMap;

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
