using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.DialogsUIMapClasses;
using Warewolf.UITests.ExplorerUIMapClasses;

// ReSharper disable InconsistentNaming

namespace Warewolf.UITests
{
    [CodedUITest]
    public class DeleteExplorerResource
    {
        const string flowSwitch = "DeleteExplorerResourceTestFile";
        const string flowSequence = "DeleteResourceRemovalTestFile";
        const string uiTestDependencyOne = "UITestDependencyOne";
        const string DeleteAnywayResourceFolder = "DeleteAnyway";
        const string DeleteRemoteServer = "RemoteServerToDelete";

        [TestMethod]
        [TestCategory("Explorer")]
        public void Delete_ExplorerResource()
        {
            ExplorerUIMap.Filter_Explorer(flowSwitch);
            ExplorerUIMap.Delete_FirstResource_From_ExplorerContextMenu();
            DialogsUIMap.Click_MessageBox_Yes();
            UIMap.WaitForSpinner(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
            ExplorerUIMap.Click_Explorer_Refresh_Button();
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void DeletedResourceIsRemovedFromResources()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources";
            Assert.IsTrue(Directory.Exists(resourcesFolder), "Resource Folder does not exist");
            ExplorerUIMap.Filter_Explorer(flowSequence);
            ExplorerUIMap.Delete_FirstResource_From_ExplorerContextMenu();
            DialogsUIMap.Click_MessageBox_Yes();
            UIMap.WaitForSpinner(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
            var allFiles = Directory.GetFiles(resourcesFolder, "*.xml", SearchOption.AllDirectories);
            var firstOrDefault = allFiles.FirstOrDefault(s => s.StartsWith(flowSequence));
            Assert.IsNull(firstOrDefault);
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void DeletedResourceShowDependencies()
        {
            ExplorerUIMap.Filter_Explorer(uiTestDependencyOne);
            ExplorerUIMap.Delete_FirstResource_From_ExplorerContextMenu();
            DialogsUIMap.Click_MessageBox_Yes();
            Assert.IsTrue(DialogsUIMap.MessageBoxWindow.Applytoall.Exists, "Apply To All button does not exist.");
            Assert.IsTrue(DialogsUIMap.MessageBoxWindow.DeleteAnyway.Exists, "Delete Anyway button does not exist.");
            Assert.IsTrue(DialogsUIMap.MessageBoxWindow.ShowDependencies.Exists, "Show Dependencies button does not exist.");
            Assert.IsTrue(DialogsUIMap.MessageBoxWindow.OKButton.Exists, "OK button does not exist.");
            Assert.IsTrue(DialogsUIMap.MessageBoxWindow.DeleteAnywayText.Exists, "Error Deleting Confirmation MessageBox does not exist");
            DialogsUIMap.Click_DeleteAnyway_MessageBox_OK();
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void DeletedFolderShowDependencies()
        {
            ExplorerUIMap.Filter_Explorer(DeleteAnywayResourceFolder);
            ExplorerUIMap.Delete_FirstResource_From_ExplorerContextMenu();
            DialogsUIMap.Click_MessageBox_Yes();
            Assert.IsTrue(DialogsUIMap.MessageBoxWindow.Applytoall.Exists, "Apply To All button does not exist.");
            Assert.IsTrue(DialogsUIMap.MessageBoxWindow.DeleteAnyway.Exists, "Delete Anyway button does not exist.");
            Assert.IsTrue(DialogsUIMap.MessageBoxWindow.ShowDependencies.Exists, "Show Dependencies button does not exist.");
            Assert.IsTrue(DialogsUIMap.MessageBoxWindow.OKButton.Exists, "OK button does not exist.");
            DialogsUIMap.Click_MessageBox_DeleteAnyway();
            Assert.IsFalse(UIMap.ControlExistsNow(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem), "Item did not delete");
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void DeletedRemoteServer_RemoveItemFromTree()
        {
            ExplorerUIMap.Filter_Explorer(DeleteRemoteServer);
            ExplorerUIMap.Delete_FirstResource_From_ExplorerContextMenu();
            Assert.IsTrue(DialogsUIMap.MessageBoxWindow.Exists);
            DialogsUIMap.Click_MessageBox_Yes();
            Assert.IsFalse(UIMap.ControlExistsNow(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem), "Remote server delete was not successful.");
        }

        #region Additional test attributes

        [TestInitialize]
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

        DialogsUIMap DialogsUIMap
        {
            get
            {
                if (_DialogsUIMap == null)
                {
                    _DialogsUIMap = new DialogsUIMap();
                }

                return _DialogsUIMap;
            }
        }

        private DialogsUIMap _DialogsUIMap;

        #endregion
    }
}