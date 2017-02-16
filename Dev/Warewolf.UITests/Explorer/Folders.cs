
using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.Tools.ToolsUIMapClasses;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class Folders
    {
        const string ResourceCreatedInFolder = "Resource Created In Folder";

        [TestMethod]
        [TestCategory("Explorer")]
        public void Create_Resource_InFolderUITest()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";
            UIMap.Filter_Explorer("Acceptance Tests");
            UIMap.Create_NewWorkflow_Of_ExplorerFirstItem_With_ExplorerContextMenu();
            ToolsUIMap.Make_Workflow_Savable();
            UIMap.Save_With_Ribbon_Button_And_Dialog(ResourceCreatedInFolder);
            var allFiles = Directory.GetFiles(resourcesFolder, "*.xml", SearchOption.AllDirectories);
            var firstOrDefault = allFiles.FirstOrDefault(s => s.Contains("Resource Created In Folder.xml"));
            if (firstOrDefault != null)
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
        public void MergeFolders_InFileredExplorer_UITest()
        {
            UIMap.Filter_Explorer("DragAndDropMergeFolder");
            UIMap.Drag_Explorer_First_Item_Onto_Second_Item();
            UIMap.Filter_Explorer("DragAndDropMergeResource1");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.Exists, "Resource did not merge into folder after drag and drop in the explorer UI.");
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void Right_Click_On_The_FolderCount_ContextMenu_UITest()
        {
            ToolsUIMap.Right_Click_On_The_Folder_Count();
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

        ToolsUIMap ToolsUIMap
        {
            get
            {
                if (_ToolsUIMap == null)
                {
                    _ToolsUIMap = new ToolsUIMap();
                }

                return _ToolsUIMap;
            }
        }

        private ToolsUIMap _ToolsUIMap;

        #endregion
    }
}