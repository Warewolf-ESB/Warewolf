using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class DragAndDropService
    {
        const string DeleteResourcesWithSameName = "DeleteResourcesWithSameNameResource";
        [TestMethod]
        [TestCategory("Explorer")]
        public void DragAndDropServiceFromExplorerUITest()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";
            UIMap.Move_AcceptanceTestd_To_AcceptanceTestingResopurces();
            UIMap.WaitForControlNotVisible(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
            Assert.IsTrue(Directory.Exists(resourcesFolder));
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void MergeFoldersUITest()
        {
            var acceptanceResources = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Testing Resources";
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests\Acceptance Testing Resources";
            Assert.IsTrue(Directory.Exists(acceptanceResources));
            Assert.IsFalse(Directory.Exists(resourcesFolder));
            UIMap.Filter_Explorer("Acceptance");
            UIMap.Create_SubFolder_In_Folder1();
            Assert.IsTrue(Directory.Exists(resourcesFolder));
            UIMap.WaitForSpinner(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
            UIMap.Move_AcceptanceTestd_To_AcceptanceTestingResopurces();
            Assert.IsTrue(Directory.Exists(resourcesFolder));
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void DebugUsingPlayIconRemoteServerUITest()
        {
            UIMap.Filter_Explorer("Hello World");
            Mouse.Hover(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem);
            Mouse.Hover(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ExecuteIcon);
            UIMap.Debug_Using_Play_Icon();
            UIMap.Click_DebugInput_Debug_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.Exists, "Debug Output does not exist after clicking Debug button from Debug Dialog");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.SettingsButton.Exists, "Output SettingsButton does not exist after clicking Debug button from Debug Dialog");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.SearchTextBox.Exists, "Output SearchTextBox does not exist after clicking Debug button from Debug Dialog");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Exists, "DebugOutputTree does not exist after clicking Debug button from Debug Dialog");
            UIMap.Click_Close_Workflow_Tab_Button();
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void DeleteResourcesWithSameNameInDifferentLocationsUITest()
        {
            UIMap.Filter_Explorer("Acceptance Tests");
            UIMap.Create_Resource_In_Folder1();
            UIMap.Save_With_Ribbon_Button_And_Dialog("Hello World");
            UIMap.WaitForControlNotVisible(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
            UIMap.Click_Close_Workflow_Tab_Button();
            UIMap.Delete_Nested_Hello_World();
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources";
            Assert.IsTrue(File.Exists(resourcesFolder + @"\" + "Hello World" + ".xml"));
            UIMap.Click_Explorer_Filter_Clear_Button();
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void DisconnectedRemoteServerUITest()
        {
            UIMap.Select_RemoteConnectionIntegration_From_Explorer();
            UIMap.Click_Explorer_RemoteServer_Connect_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.Exists);
            UIMap.WaitForControlNotVisible(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
            UIMap.Click_Explorer_RemoteServer_Edit_Button();
            UIMap.Click_Server_Source_Wizard_Test_Connection_Button();
            UIMap.WaitForControlNotVisible(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab.WorkSurfaceContext.NewServerSourceWizard.Spinner);
            UIMap.Click_Close_Server_Source_Wizard_Tab_Button();
            UIMap.Click_MessageBox_No();
            UIMap.WaitForControlNotVisible(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab.WorkSurfaceContext.NewServerSourceWizard.Spinner);
            UIMap.Select_localhost_From_Explorer_Remote_Server_Dropdown_List();
        }
    
        [TestMethod]
        [TestCategory("Explorer")]
        public void ShowDependenciesUITest()
        {            
            UIMap.Select_Show_Dependencies_In_Explorer_Context_Menu("Hello World");
            UIMap.Click_Close_Dependecy_Tab();
            UIMap.Select_Show_Dependencies_In_Explorer_Context_Menu("SharepointPlugin");
            UIMap.Click_Close_Dependecy_Tab();
            UIMap.Select_Show_Dependencies_In_Explorer_Context_Menu("MySQLDATA");
            UIMap.Click_Close_Dependecy_Tab();
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
#if !DEBUG
            UIMap.CloseHangingDialogs();
#endif
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
