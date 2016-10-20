using System.Drawing;
using System.Windows.Forms;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    /// <summary>
    /// Summary description for RemoteServer
    /// </summary>
    [CodedUITest]
    public class RenameExplorerResource
    {
        [TestMethod]
        public void RenameFolder_ThenFolderItem()
        {
            UIMap.Filter_Explorer("Control Flow - Decision");
            UIMap.Rename_LocalFolder_To_SecondFolder();
            UIMap.Rename_FolderItem_ToNewFolderItem();
            var itemEdit = UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.ResourceNameTextBlock;
            Assert.AreEqual("Control Flow - Decision2", itemEdit.DisplayText);
        }

        #region Additional test attributes

        [TestInitialize]
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
