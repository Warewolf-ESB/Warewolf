using Microsoft.VisualStudio.TestTools.UITesting;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.MergeDialog.MergeDialogUIMapClasses;

namespace Warewolf.UI.Tests.Merge.MergeConflictsUIMapClasses
{
    public partial class MergeConflictsUIMap
    {
        public void OpenMerge_For_Workflow(string workflow)
        {
            ExplorerUIMap.Open_Context_Menu_For_Service(workflow);
            ExplorerUIMap.Click_Merge_From_Context_Menu();
            MergeDialogViewWindow.MergeResourceVersionList.WarewolfStudioViewMoListItem.ItemRadioButton.Selected = true;
            Mouse.Click(MergeDialogUIMap.MergeDialogWindow.MergeButton);
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

        public MergeDialogUIMap MergeDialogUIMap
        {
            get
            {
                if (_MergeDialogUIMap == null)
                {
                    _MergeDialogUIMap = new MergeDialogUIMap();
                }

                return _MergeDialogUIMap;
            }
        }

        private MergeDialogUIMap _MergeDialogUIMap;

    }
}
