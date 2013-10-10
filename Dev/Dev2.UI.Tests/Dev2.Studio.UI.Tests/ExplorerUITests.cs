using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        [TestMethod][Ignore]//Ashley: WORKING OK - Bring back in when all the tests are OK like this one
        public void SearchAndRefresh_AttemptToSearch_ExpectedSearchFilteredByAllItems()
        {
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            // Now count
            int allResources = ExplorerUIMap.GetCategoryItems().Count;
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("Integration");
            Playback.Wait(2000);
            int allResourcesAfterSearch = ExplorerUIMap.GetCategoryItems().Count;
            ExplorerUIMap.ClearExplorerSearchText();            
            Assert.IsTrue(allResources>allResourcesAfterSearch);
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
