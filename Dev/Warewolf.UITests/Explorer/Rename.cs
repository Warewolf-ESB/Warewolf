﻿using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming

namespace Warewolf.UITests
{
    [CodedUITest]
    public class RenameExplorerResource
    {
        private const string Folder = "Acceptance Tests";
        [TestMethod]
        [TestCategory("Explorer")]
        public void RenameFolder_ThenFolderItem()
        {
            UIMap.Filter_Explorer("Control Flow - Decision");
            UIMap.Rename_LocalFolder_To_SecondFolder();
            UIMap.Rename_FolderItem_ToNewFolderItem();
            UIMap.WaitForSpinner(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
            UIMap.Click_Explorer_Refresh_Button();
            var itemEdit = UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.ResourceNameTextBlock;
            Assert.AreEqual("Control Flow - Decision2", itemEdit.DisplayText);
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void RenameFolder_UsingF2_Shortcut()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";
            var renamedFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests_Renamed";
            UIMap.Filter_Explorer(Folder);
            UIMap.Rename_Folder_Using_Shortcut(Folder + "_Renamed");
            Assert.IsTrue(Directory.Exists(renamedFolder));
            //Put back the Original Name
            Directory.Move(renamedFolder, resourcesFolder);
            Assert.IsTrue(Directory.Exists(resourcesFolder));
            UIMap.Click_Explorer_Refresh_Button();
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void RenameFolder_Updates_The_Workflow_Url()
        {
            const string ExistingFloder = "Acceptance Tests";
            const string AcceptanceTestsRenamed = "Acceptance Tests_Renamed";
            const string WorkflowName = "LoopTest";
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";
            var renamedFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests_Renamed";

            UIMap.Filter_Explorer(WorkflowName);
            UIMap.Open_Explorer_FirstSubItem_Item_With_Context_Menu();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.TopScrollViewerPane.HttpLocalHostText.NewWorkflowHyperLink.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.TopScrollViewerPane.HttpLocalHostText.NewWorkflowHyperLink.Alt.Contains(ExistingFloder));
            UIMap.Rename_Folder_Using_Shortcut(AcceptanceTestsRenamed);
            UIMap.WaitForSpinner(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
            UIMap.Click_Close_Workflow_Tab_Button();
            UIMap.Open_Explorer_FirstSubItem_Item_With_Context_Menu();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.TopScrollViewerPane.UrlWithRenamedFolder.UrlWithRenamedFolderHyperlink.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.TopScrollViewerPane.UrlWithRenamedFolder.UrlWithRenamedFolderHyperlink.Alt.Contains(AcceptanceTestsRenamed));
            Directory.Move(renamedFolder, resourcesFolder);
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
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
