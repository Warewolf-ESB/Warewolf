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
using Dev2.CodedUI.Tests.UIMaps.ExplorerUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses;


namespace Dev2.Studio.UI.Tests
{
    /// <summary>
    /// Summary description for ExplorerUITests
    /// </summary>
    [CodedUITest]
    public class ExplorerUITests
    {
        public ExplorerUITests()
        {
        }

        [TestMethod]
        public void SearchAndRefresh_AttemptToSearch_ExpectedSearchFilteredByAllItems()
        {
            // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
            // For more information on generated code, see http://go.microsoft.com/fwlink/?LinkId=179463
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            
            // Refresh before we count :p
            ExplorerUIMap.DoRefresh();

            // Now count
            int allResources = ExplorerUIMap.GetCategoryItems().Count;
            ExplorerUIMap.EnterExplorerSearchText("Integration");
            ExplorerUIMap.DoRefresh();
            ExplorerUIMap.ClearExplorerSearchText();
            int allResourcesAfterSearch = ExplorerUIMap.GetCategoryItems().Count;
            Assert.AreEqual(allResources, allResourcesAfterSearch);
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


        private DocManagerUIMap DockManagerUIMap
        {
            get
            {
                if (_docManagerUIMap == null)
                {
                    _docManagerUIMap = new DocManagerUIMap();
                }

                return _docManagerUIMap;
            }
        }

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
    }
}
