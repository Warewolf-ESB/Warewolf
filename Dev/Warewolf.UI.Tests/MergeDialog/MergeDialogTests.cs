using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.MergeDialog.MergeDialogUIMapClasses;


namespace Warewolf.UI.Tests.MergeDialog
{
    [CodedUITest]
    public class MergeDialogTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("Merge")]
        public void MergeDialog_OpenWindow_ControlStatus_ExpectedResults()
        {
            Assert.IsTrue(MergeDialogUiMap.MergeDialogWindow.LocalMergeText.Exists);
            Assert.IsTrue(MergeDialogUiMap.MergeDialogWindow.MergewithText.Exists);
            Assert.IsFalse(MergeDialogUiMap.MergeDialogWindow.ResourceToMergeText.Exists);
            Assert.IsTrue(MergeDialogUiMap.MergeDialogWindow.MergeButton.Exists);
            Assert.IsFalse(MergeDialogUiMap.MergeDialogWindow.MergeButton.Enabled);
            Assert.IsTrue(MergeDialogUiMap.MergeDialogWindow.CancelButton.Exists);
            Assert.IsTrue(MergeDialogUiMap.MergeDialogWindow.CancelButton.Enabled);

            MergeDialogUiMap.MergeDialogWindow.MergeExplorerView.SearchTextBox.Text = "Hello World";
            Mouse.Click(MergeDialogUiMap.MergeDialogWindow.MergeExplorerView.MergeExplorerTree.ExplorerTreeItem.ExplorerItemTreeItemOne);

            Assert.IsTrue(MergeDialogUiMap.MergeDialogWindow.MergeButton.Enabled);
        }

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UiMap.AssertStudioIsRunning();
            ExplorerUiMap.Filter_Explorer("Hello World");
            ExplorerUiMap.Open_ExplorerFirstItemMerge_With_ExplorerContextMenu();
        }

        private UIMap UiMap => _uiMap ?? (_uiMap = new UIMap());

        private UIMap _uiMap;

        private MergeDialogUIMap MergeDialogUiMap => _mergeDialogUiMap ?? (_mergeDialogUiMap = new MergeDialogUIMap());

        private MergeDialogUIMap _mergeDialogUiMap;

        ExplorerUIMap ExplorerUiMap => _explorerUiMap ?? (_explorerUiMap = new ExplorerUIMap());

        private ExplorerUIMap _explorerUiMap;
    }
}
