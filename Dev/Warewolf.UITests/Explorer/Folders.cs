using System;
using System.IO;
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
            UIMap.Filter_Explorer("DragAndDropMergeFolder", true);
            UIMap.Drag_Explorer_First_Sub_Item_Onto_Second_Sub_Item();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.FirstItem.SecondSubItem.Exists, "Resource did not merge into folder after drag and drop in the explorer UI.");
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void CreateResourceInFolderUITest()
        {
            UIMap.Filter_Explorer("Acceptance Tests");
            UIMap.Create_New_Workflow_In_Explorer_First_Item_With_Context_Menu();
            UIMap.Make_Workflow_Savable();
            UIMap.Save_With_Ribbon_Button_And_Dialog("Hello World", true);
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
