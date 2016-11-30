using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class Folders
    {
        [TestMethod]
        [TestCategory("Explorer")]
        public void MergeFoldersUITest()
        {
            UIMap.Filter_Explorer("DragAndDropMergeFolder");
            UIMap.Drag_Explorer_First_Sub_Item_Onto_Second_Sub_Item();
            UIMap.Filter_Explorer("Workflow");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.FirstItem.ThirdSubItem.Exists, "Resource did not merge into folder after drag and drop in the explorer UI.");
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void MergeFolders_InUnfileredExplorer_UITest()
        {
            UIMap.Filter_Explorer("DragAndDropMergeFolder");
            UIMap.Filter_Explorer(string.Empty);
            UIMap.Click_Explorer_Localhost_First_Item_Expander();
            UIMap.Drag_Explorer_Second_Sub_Item_Onto_Third_Sub_Item();
            UIMap.Filter_Explorer("Workflow");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.FirstItem.ThirdSubItem.Exists, "Resource did not merge into folder after drag and drop in an unfiltered explorer UI.");
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void CreateResourceInFolderUITest()
        {
            UIMap.Filter_Explorer("Acceptance Tests");
            UIMap.Create_New_Workflow_In_Explorer_First_Item_With_Context_Menu();
            UIMap.Make_Workflow_Savable();
            UIMap.Save_With_Ribbon_Button_And_Dialog("Hello World");
        }

        #region Additional test attributes

        [TestInitialize()]
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
