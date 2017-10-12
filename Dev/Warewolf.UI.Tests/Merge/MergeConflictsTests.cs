using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.Merge.MergeConflictsUIMapClasses;

namespace Warewolf.UI.Tests.Merge
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    public class MergeConflictsTests
    {
        public const string MergeWfWithVersion = "MergeWfWithVersion";

        [TestMethod]
        [TestCategory("Merge")]
        public void RightClick_On_MergeWfWithVersion_Has_Merge_Option()
        {
            RightClick_On_MergeWfWithVersion(MergeWfWithVersion);
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Merge.Exists, "Merge option does not show after Right cliking " + MergeWfWithVersion);
        }

        [TestMethod]
        [TestCategory("Merge")]
        public void Click_On_Merge_From_MergeWfWithVersion_ContextMenu_Show_Merge_PopUp()
        {
            RightClick_On_MergeWfWithVersion(MergeWfWithVersion);
            ExplorerUIMap.Click_Merge_From_Context_Menu();
            Assert.IsTrue(MergeConflictsUIMap.MergeDialogViewWindow.ServerSource.Exists);
            MergeConflictsUIMap.MergeDialogViewWindow.MergeResourceVersionList.WarewolfStudioViewMoListItem.ItemRadioButton.Selected = true;
            Assert.IsTrue(MergeConflictsUIMap.MergeDialogViewWindow.MergeButton.Enabled);

            Mouse.Click(MergeConflictsUIMap.MergeDialogViewWindow.MergeButton);
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.Exists);
        }

        public void RightClick_On_MergeWfWithVersion(string workflow)
        {
            ExplorerUIMap.Filter_Explorer(workflow);
            ExplorerUIMap.RightClick_Explorer_Localhost_FirstItem();
        }
        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
        }
        public MergeConflictsUIMap MergeConflictsUIMap
        {
            get
            {
                if (_MergeConflictsUIMap == null)
                {
                    _MergeConflictsUIMap = new MergeConflictsUIMap();
                }

                return _MergeConflictsUIMap;
            }
        }

        private MergeConflictsUIMap _MergeConflictsUIMap;

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

        public ExplorerUIMap ExplorerUIMap
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

        #endregion
    }
}
