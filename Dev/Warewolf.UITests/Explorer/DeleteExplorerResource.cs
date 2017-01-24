using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming

namespace Warewolf.UITests
{
    [CodedUITest]
    public class DeleteExplorerResource
    {
        const string flowSwitch = "DeleteExplorerResourceTestFile";
        const string flowSequence = "DeleteResourceRemovalTestFile";
        const string uiTestDependencyOne = "UITestDependencyOne";
        const string uiTestDependencyFolder = "UITestDependency";

        [TestMethod]
        [TestCategory("Explorer")]
        public void Delete_ExplorerResource()
        {
            UIMap.Filter_Explorer(flowSwitch);
            UIMap.Delete_FirstResource_From_ExplorerContextMenu();
            UIMap.Click_MessageBox_Yes();
            UIMap.WaitForSpinner(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
            UIMap.Click_Explorer_Refresh_Button();
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void DeletedResourceIsRemovedFromResources()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources";
            Assert.IsTrue(Directory.Exists(resourcesFolder), "Resource Folder does not exist");
            UIMap.Filter_Explorer(flowSequence);
            UIMap.Delete_FirstResource_From_ExplorerContextMenu();
            UIMap.Click_MessageBox_Yes();
            UIMap.WaitForSpinner(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
            var allFiles = Directory.GetFiles(resourcesFolder, "*.xml", SearchOption.AllDirectories);
            var firstOrDefault = allFiles.FirstOrDefault(s => s.StartsWith(flowSequence));
            Assert.IsNull(firstOrDefault);
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void DeletedResourceShowDependencies()
        {
            UIMap.Filter_Explorer(uiTestDependencyOne);
            UIMap.Delete_FirstResource_From_ExplorerContextMenu();
            UIMap.Click_MessageBox_Yes();
            Assert.IsTrue(UIMap.MessageBoxWindow.Applytoall.Exists, "Apply To All button does not exist.");
            Assert.IsTrue(UIMap.MessageBoxWindow.DeleteAnyway.Exists, "Delete Anyway button does not exist.");
            Assert.IsTrue(UIMap.MessageBoxWindow.ShowDependencies.Exists, "Show Dependencies button does not exist.");
            Assert.IsTrue(UIMap.MessageBoxWindow.OKButton.Exists, "OK button does not exist.");
            Assert.IsTrue(UIMap.MessageBoxWindow.DeleteAnywayText.Exists, "Error Deleting Confirmation MessageBox does not exist");
            UIMap.Click_DeleteAnyway_MessageBox_OK();
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void DeletedFolderShowDependencies()
        {
            UIMap.Filter_Explorer(uiTestDependencyFolder);
            UIMap.Delete_FirstResource_From_ExplorerContextMenu();
            UIMap.Click_MessageBox_Yes();
            Assert.IsTrue(UIMap.MessageBoxWindow.Applytoall.Exists, "Apply To All button does not exist.");
            Assert.IsTrue(UIMap.MessageBoxWindow.DeleteAnyway.Exists, "Delete Anyway button does not exist.");
            Assert.IsTrue(UIMap.MessageBoxWindow.ShowDependencies.Exists, "Show Dependencies button does not exist.");
            Assert.IsTrue(UIMap.MessageBoxWindow.OKButton.Exists, "OK button does not exist.");
            UIMap.Click_MessageBox_DeleteAnyway();
            Assert.IsFalse(UIMap.ControlExistsNow(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem), "Item did not delete");
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

        #endregion
    }
}