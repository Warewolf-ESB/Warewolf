using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class ItemDragAndDropTest
    {        
        [TestMethod]
        public void ItemDragAndDropUITest()
        {            
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests\Acceptance Testing Resources";
            Assert.IsFalse(Directory.Exists(resourcesFolder));
            UIMap.Move_AcceptanceTestd_To_AcceptanceTestingResopurces();
            UIMap.WaitForControlNotVisible(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
            Assert.IsTrue(Directory.Exists(resourcesFolder));
        }

        [TestMethod]
        public void MergeFoldersUITest()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests\Acceptance Testing Resources";
            Assert.IsFalse(Directory.Exists(resourcesFolder));
            UIMap.Create_SubFolder_In_Folder1();
            Assert.IsTrue(Directory.Exists(resourcesFolder));
            UIMap.Move_AcceptanceTestd_To_AcceptanceTestingResopurces();
            Assert.IsTrue(Directory.Exists(resourcesFolder));
        }

        [TestMethod]
        public void DebugUsingPlayIconRemoteServerUITest()
        {
            UIMap.Filter_Explorer("Hello World");
            Mouse.Hover(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem);
            UIMap.Debug_Using_Play_Icon();
            UIMap.Click_DebugInput_Debug_Button();
            UIMap.Click_Close_Workflow_Tab_Button();
        }

        [TestMethod]
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
        public void DisconnectedRemoteServerUITest()
        {
            UIMap.Select_RemoteConnectionIntegration_From_Explorer();
            UIMap.Click_Explorer_RemoteServer_Connect_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.Exists);
            UIMap.WaitForControlNotVisible(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
            UIMap.Click_Explorer_RemoteServer_Edit_Button();
            UIMap.Click_Server_Source_Wizard_Test_Connection_Button();
            UIMap.WaitForControlNotVisible(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.ServerSourceWizardTab.WorkSurfaceContext.NewServerSourceWizard.Spinner);
            UIMap.Click_Close_Server_Source_Wizard_Tab_Button();
            UIMap.Click_MessageBox_No();
            UIMap.WaitForControlNotVisible(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.ServerSourceWizardTab.WorkSurfaceContext.NewServerSourceWizard.Spinner);
            UIMap.Select_localhost_From_Explorer_Remote_Server_Dropdown_List();
        }
    
        [TestMethod]
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
