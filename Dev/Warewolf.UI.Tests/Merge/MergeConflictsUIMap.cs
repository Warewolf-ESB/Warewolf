using Microsoft.VisualStudio.TestTools.UITesting;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;

namespace Warewolf.UI.Tests.Merge.MergeConflictsUIMapClasses
{
    public partial class MergeConflictsUIMap
    {
        public void OpenMerge_For_MergWfWithVersion(string workflow)
        {
            RightClick_On_MergeWfWithVersion(workflow);
            ExplorerUIMap.Click_Merge_From_Context_Menu();
            MergeDialogViewWindow.MergeResourceVersionList.WarewolfStudioViewMoListItem.ItemRadioButton.Selected = true;
            Mouse.Click(MergeDialogViewWindow.MergeButton);
        }

        public void RightClick_On_MergeWfWithVersion(string workflow)
        {
            ExplorerUIMap.Filter_Explorer(workflow);
            ExplorerUIMap.RightClick_Explorer_Localhost_FirstItem();
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

    }
}
