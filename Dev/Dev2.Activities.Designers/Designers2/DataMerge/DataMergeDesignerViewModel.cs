using Dev2.Activities.Designers2.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.ViewModels.Base;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows.Input;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers2.DataMerge
{
    public class DataMergeDesignerViewModel : ActivityCollectionDesignerViewModel<DataMergeDTO>
    {
        public IList<string> ItemsList { get; private set; }
        public IList<string> AlignmentTypes { get; private set; }

        public DataMergeDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarLargeToggle();
            AddTitleBarQuickVariableInputToggle();
            AddTitleBarHelpToggle();

            ItemsList = new List<string> { "None", "Index", "Chars", "New Line", "Tab" };
            AlignmentTypes = new List<string> { "Left", "Right" };
            MergeTypeUpdatedCommand = new RelayCommand(OnMergeTypeChanged, o => true);

            dynamic mi = ModelItem;
            InitializeItems(mi.MergeCollection);

            for(var i = 0; i < mi.MergeCollection.Count; i++)
            {
                OnMergeTypeChanged(i);
            }
        }

        public override string CollectionName { get { return "MergeCollection"; } }

        public ICommand MergeTypeUpdatedCommand { get; private set; }

        void OnMergeTypeChanged(object indexObj)
        {
            var index = (int)indexObj;

            if(index < 0 || index >= ItemCount)
            {
                return;
            }

            var mi = ModelItemCollection[index];
            var mergeType = mi.GetProperty("MergeType") as string;

            if(mergeType == "Index" || mergeType == "Chars")
            {
                mi.SetProperty("EnableAt", true);
            }
            else
            {
                mi.SetProperty("At", string.Empty);
                mi.SetProperty("EnableAt", false);
            }
        }
    }
}