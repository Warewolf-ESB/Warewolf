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
            Uimap.Move_AcceptanceTestd_To_AcceptanceTestingResopurces();
            Uimap.WaitForControlNotVisible(Uimap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
            Assert.IsTrue(Directory.Exists(resourcesFolder));
        }

        [TestMethod]
        public void MergeFoldersUITest()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests\Acceptance Testing Resources";
            Assert.IsFalse(Directory.Exists(resourcesFolder));
            Uimap.Create_SubFolder_In_Folder1();
            Assert.IsTrue(Directory.Exists(resourcesFolder));
            Uimap.Move_AcceptanceTestd_To_AcceptanceTestingResopurces();
            Assert.IsTrue(Directory.Exists(resourcesFolder));
        }

        [TestMethod]
        public void DebugUsingPlayIconRemoteServerUITest()
        {
            Uimap.Filter_Explorer("Hello World");
            Mouse.Hover(Uimap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem);
            Uimap.Debug_Using_Play_Icon();
            Uimap.Click_DebugInput_Debug_Button();
            Uimap.Click_Close_Workflow_Tab_Button();
        }

        [TestMethod]
        public void DeleteResourcesWithSameNameInDifferentLocationsUITest()
        {
            Uimap.Filter_Explorer("Acceptance Tests");
            Uimap.Create_Resource_In_Folder1();
            Uimap.Save_With_Ribbon_Button_And_Dialog("Hello World");
            Uimap.WaitForControlNotVisible(Uimap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
            Uimap.Click_Close_Workflow_Tab_Button();
            Uimap.Delete_Nested_Hello_World();
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources";
            Assert.IsTrue(File.Exists(resourcesFolder + @"\" + "Hello World" + ".xml"));
            Uimap.Click_Explorer_Filter_Clear_Button();
        }

        [TestMethod]
        public void DisconnectedRemoteServerUITest()
        {
            Uimap.Select_RemoteConnectionIntegration_From_Explorer();
            Uimap.Click_Explorer_RemoteServer_Connect_Button();
            Assert.IsTrue(Uimap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.Exists);
            Uimap.WaitForControlNotVisible(Uimap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
            Uimap.Click_Explorer_RemoteServer_Edit_Button();
            Uimap.Click_Server_Source_Wizard_Test_Connection_Button();
            Uimap.WaitForControlNotVisible(Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.ServerSourceWizardTab.WorkSurfaceContext.NewServerSourceWizard.Spinner);
            Uimap.Click_Close_Server_Source_Wizard_Tab_Button();
            Uimap.Click_MessageBox_No();
            Uimap.WaitForControlNotVisible(Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.ServerSourceWizardTab.WorkSurfaceContext.NewServerSourceWizard.Spinner);
            Uimap.Select_localhost_From_Explorer_Remote_Server_Dropdown_List();
        }
    
        [TestMethod]
        public void ShowDependenciesUITest()
        {            
            Uimap.Select_Show_Dependencies_In_Explorer_Context_Menu("Hello World");
            Uimap.Click_Close_Dependecy_Tab();
            Uimap.Select_Show_Dependencies_In_Explorer_Context_Menu("SharepointPlugin");
            Uimap.Click_Close_Dependecy_Tab();
            Uimap.Select_Show_Dependencies_In_Explorer_Context_Menu("MySQLDATA");
            Uimap.Click_Close_Dependecy_Tab();
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
#if !DEBUG
            Uimap.CloseHangingDialogs();
#endif
        }


        //[TestCleanup()]
        //public void MyTestCleanup()
        //{            
        //    var destinationDirectory = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources";
        //    var sourceDirectory = destinationDirectory + @"\Acceptance Tests\Acceptance Testing Resources";
        //    if (Directory.Exists(sourceDirectory))
        //    {
        //        try
        //        {
        //            Directory.Move(sourceDirectory, destinationDirectory);
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine(e.Message);
        //        }
        //    }
        //}
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
