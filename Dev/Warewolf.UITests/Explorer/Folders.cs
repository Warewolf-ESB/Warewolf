
using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class Folders
    {
        const string HelloWorld = "Hello World";
        [TestMethod]
        [TestCategory("Explorer")]
        public void MergeFoldersUITest()
        {
            UIMap.Filter_Explorer("DragAndDropMergeFolder");
            UIMap.Drag_Explorer_First_Sub_Item_Onto_Second_Sub_Item();
            UIMap.Filter_Explorer("Workflow");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.FirstItem.Exists, "Resource did not merge into folder after drag and drop in the explorer UI.");
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void MergeFolders_InUnfileredExplorer_UITest()
        {
            UIMap.TryClearExplorerFilter();
            UIMap.Expand_Explorer_Localhost_First_Item_With_Double_Click();
            UIMap.Drag_Explorer_Second_Sub_Item_Onto_Third_Sub_Item();
            UIMap.Filter_Explorer("Workflow");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.FirstItem.ThirdSubItem.Exists, "Resource did not merge into folder after drag and drop in an unfiltered explorer UI.");
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void CreateResourceInFolderUITest()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";
            UIMap.Filter_Explorer("Acceptance Tests");
            UIMap.Create_New_Workflow_In_Explorer_First_Item_With_Context_Menu();
            UIMap.Make_Workflow_Savable();
            UIMap.Save_With_Ribbon_Button_And_Dialog(HelloWorld);
            var allFiles = Directory.GetFiles(resourcesFolder, "*.xml", SearchOption.AllDirectories);
            var firstOrDefault = allFiles.FirstOrDefault(s => s.Contains("Hello World.xml"));
            if(firstOrDefault != null)
                File.Delete(firstOrDefault);
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void CreateNewFolderInLocalHostUsingShortcutKeysUITest()
        {
            UIMap.Click_LocalHost_Once();
            UIMap.Create_New_Folder_Using_Shortcut();
            UIMap.Filter_Explorer("New Folder");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists);
        }
        
        [TestMethod]
        [TestCategory("Explorer")]
        public void Right_Click_On_The_FolderCount_ContextMenu_UITest()
        {
            UIMap.Right_Click_On_The_Folder_Count();
            Assert.IsFalse(UIMap.ControlExistsNow(UIMap.ErrorWindow), "The studio throws an error when you right click on the folder count part of the explorer.");
        }
        
        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
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