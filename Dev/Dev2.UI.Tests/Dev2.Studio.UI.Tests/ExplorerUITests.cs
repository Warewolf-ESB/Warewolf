using System.Windows.Forms;
using Dev2.CodedUI.Tests.TabManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.RibbonUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps;
using Dev2.Studio.UI.Tests.UIMaps.DebugUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.SaveDialogUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.CodedUI.Tests.UIMaps.ExplorerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses;

namespace Dev2.Studio.UI.Tests
{
    [CodedUITest, System.Runtime.InteropServices.GuidAttribute("DAA88B10-98C4-488E-ACB2-1256C95CE8F0")]
    public class ExplorerUITests
    {
        #region Cleanup

        [TestCleanup]
        public void TestCleanup()
        {
            var window = new UIBusinessDesignStudioWindow();
            //close any open wizards
            var tryFindDialog = window.GetChildren()[0];
            if(tryFindDialog.GetType() == typeof(WpfWindow))
            {
                Mouse.Click(tryFindDialog);
                SendKeys.SendWait("{ESCAPE}");
                Assert.Fail("Resource changed dialog hanging after test, might not have rendered properly");
            }
            //close any open tabs
            TabManagerUIMap.CloseAllTabs();
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
        }

        #endregion

        [TestMethod]
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

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("RenameResource_WithDashes")]
        public void RenameResource_WithDashes_ResourceRenamed()
        {
            TabManagerUIMap.CloseAllTabs();
            const string newTestResourceWithDashes = "New-Test-Resource-With-Dashes";
            const string oldResourceName = "OldResourceName";
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(newTestResourceWithDashes);
            if(ExplorerUIMap.ServiceExists("Localhost", "WORKFLOWS", "Unassigned", newTestResourceWithDashes))
            {
                ExplorerUIMap.RightClickDeleteProject("Localhost", "WORKFLOWS", "Unassigned", newTestResourceWithDashes);
            }
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(oldResourceName);
            if(ExplorerUIMap.ServiceExists("Localhost", "WORKFLOWS", "Unassigned", oldResourceName))
            {
                ExplorerUIMap.RightClickDeleteProject("Localhost", "WORKFLOWS", "Unassigned", oldResourceName);
            }
            RibbonUIMap.CreateNewWorkflow();
            SendKeys.SendWait("^s");
            if(WizardsUIMap.WaitForWizard(5000))
            {
                SaveDialogUIMap.ClickAndTypeInNameTextbox(oldResourceName);
                //wait for save tab switch
                Playback.Wait(2000);
            }
            else
            {
                Assert.Fail("Save wizard did not display in the given time period");
            }
            TabManagerUIMap.CloseAllTabs();
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(oldResourceName);
            ExplorerUIMap.RightClickRenameProject("Localhost", "WORKFLOWS", "Unassigned", oldResourceName);
            SendKeys.SendWait("New-Test-Resource-With-Dashes{ENTER}");
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DoRefresh();
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(newTestResourceWithDashes);
            ExplorerUIMap.DoubleClickOpenProject("Localhost", "WORKFLOWS", "Unassigned", newTestResourceWithDashes);
            SendKeys.SendWait("^s");

            RibbonUIMap.ClickRibbonMenuItem("Debug");
            if(DebugUIMap.WaitForDebugWindow(5000))
            {
                SendKeys.SendWait("{F5}");
                Playback.Wait(1000);
            }
            TabManagerUIMap.CloseAllTabs();
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

        #region Context Init

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
        
        #endregion

        #region UI Maps

        #region Explorer UI Map

        private ExplorerUIMap ExplorerUIMap
        {
            get
            {
                if(_explorerUIMap == null)
                {
                    _explorerUIMap = new ExplorerUIMap();
                }
                return _explorerUIMap;
            }
        }

        private ExplorerUIMap _explorerUIMap;

        #endregion

        #region Generic UI Map

        public UIMap UIMap
        {
            get
            {
                if((this.map == null))
                {
                    this.map = new UIMap();
                }

                return this.map;
            }
        }

        private UIMap map;

        #endregion

        #region Ribbon UI Map

        public RibbonUIMap RibbonUIMap
        {
            get
            {
                if(_ribbonMap == null)
                {
                    _ribbonMap = new RibbonUIMap();
                }

                return _ribbonMap;
            }
        }

        private RibbonUIMap _ribbonMap;

        #endregion

        #region TabManager UI Map

        public TabManagerUIMap TabManagerUIMap
        {
            get
            {
                if(_tabManagerUIMap == null)
                {
                    _tabManagerUIMap = new TabManagerUIMap();
                }

                return _tabManagerUIMap;
            }
        }

        private TabManagerUIMap _tabManagerUIMap;

        #endregion TabManager UI Map

        #region Save Dialog UI Map

        public SaveDialogUIMap SaveDialogUIMap
        {
            get
            {
                if((_saveDialogUIMap == null))
                {
                    _saveDialogUIMap = new SaveDialogUIMap();
                }

                return _saveDialogUIMap;
            }
        }

        private SaveDialogUIMap _saveDialogUIMap;

        #endregion
        
        #endregion
    }
}
