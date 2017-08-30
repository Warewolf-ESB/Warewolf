using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.WebSource.WebSourceUIMapClasses;

namespace Warewolf.UI.Tests.WebSource
{
    [CodedUITest]
    public class WebSourceTests
    {
        const string SourceName = "CodedUITestWebServiceSource";

        [TestMethod]
        [TestCategory("Web Sources")]
        // ReSharper disable once InconsistentNaming
        public void Create_Save_And_Edit_WebServiceSource_From_ExplorerContextMenu_UITests()
        {
            //Create Source
            ExplorerUIMap.Click_NewWebSource_From_ExplorerContextMenu();
            Assert.IsTrue(WebSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.AddressTextbox.Enabled, "Web server address textbox not enabled.");
            Assert.IsFalse(WebSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.TestConnectionButton.Enabled, "Test Connection button is enabled");
            Assert.IsTrue(WebSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.AnonymousRadioButton.Enabled, "Anonymous Radio button is not enabled");
            Assert.IsTrue(WebSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.UserRadioButton.Enabled, "User Radio button is not enabled");
            Assert.IsTrue(WebSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.DefaultQueryTextBox.Enabled, "Default Query Textbox is not enabled");
            WebSourceUIMap.Click_UserButton_On_WebServiceSourceTab();
            Assert.IsTrue(WebSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.UserNameTextBox.Enabled, "Username Textbox not enabled");
            Assert.IsTrue(WebSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.PasswordTextBox.Enabled, "Password Textbox not enabled");
            WebSourceUIMap.Enter_TextIntoAddress_On_WebServiceSourceTab("http://RSAKLFSVRTFSBLD:9810");
            WebSourceUIMap.Enter_RunAsUser_On_WebServiceSourceTab();
            WebSourceUIMap.Enter_DefaultQuery_On_WebServiceSourceTab("");
            Assert.IsTrue(WebSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.TestConnectionButton.Enabled, "Test Connection button not enabled");
            WebSourceUIMap.Click_NewWebSource_TestConnectionButton();
            //Save Source
            UIMap.MainStudioWindow.SideMenuBar.SaveButton.WaitForControlEnabled(60000);
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save Ribbon Button is not enabled after clicking new web source test button and waiting one minute (60000ms).");
            UIMap.Save_With_Ribbon_Button_And_Dialog(SourceName);
            ExplorerUIMap.Filter_Explorer(SourceName);
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists, "Source did not save in the explorer UI.");
            //Edit Source
            WebSourceUIMap.Click_Close_Web_Source_Wizard_Tab_Button();
            ExplorerUIMap.Select_Source_From_ExplorerContextMenu(SourceName);
            Assert.IsTrue(WebSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.AddressTextbox.Enabled, "Web server address textbox not enabled.");
            WebSourceUIMap.Click_AnonymousButton_On_WebServiceSourceTab();
            UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            WebSourceUIMap.Click_Close_Web_Source_Wizard_Tab_Button();
            ExplorerUIMap.Select_Source_From_ExplorerContextMenu(SourceName);
            Assert.IsTrue(WebSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.AnonymousRadioButton.Selected);
        }

        /// <summary>
        /// If this test is failing, check first to see if the Link is still working.
        /// </summary>
        [TestMethod]
        [TestCategory("Web Sources")]
        // ReSharper disable once InconsistentNaming        
        public void Test_WebServiceSource_DefaulQuery_UITests()
        {
            //Create Source
            ExplorerUIMap.Click_NewWebSource_From_ExplorerContextMenu();
            WebSourceUIMap.Enter_TextIntoAddress_On_WebServiceSourceTab("https://data.gov.in");
            WebSourceUIMap.Enter_DefaultQuery_On_WebServiceSourceTab("/api/datastore/resource.json?resource_id=0a076478-3fd3-4e2c-b2d2-581876f56d77&api-key=fd6eaccb00617ecf8d225d4573f8f7be");
            WebSourceUIMap.Click_NewWebSource_TestConnectionButton();
            //Save Source
            UIMap.MainStudioWindow.SideMenuBar.SaveButton.WaitForControlEnabled(60000);
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save Ribbon Button is not enabled after clicking new web source test button and waiting one minute (60000ms).");
            Assert.IsTrue(WebSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.DefaultQueryTextBox.TestPassedImage.Exists, "Expected Test to Pass, but got different results after clicking test button.");
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
        }
        
        public UIMap UIMap
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

        ExplorerUIMap ExplorerUIMap
        {
            get
            {
                if (_ExplorerUIMap == null)
                {
                    _ExplorerUIMap = new ExplorerUIMap();
                }

                return _ExplorerUIMap;
            }
        }

        private ExplorerUIMap _ExplorerUIMap;

        WebSourceUIMap WebSourceUIMap
        {
            get
            {
                if (_WebSourceUIMap == null)
                {
                    _WebSourceUIMap = new WebSourceUIMap();
                }

                return _WebSourceUIMap;
            }
        }

        private WebSourceUIMap _WebSourceUIMap;

        #endregion
    }
}
