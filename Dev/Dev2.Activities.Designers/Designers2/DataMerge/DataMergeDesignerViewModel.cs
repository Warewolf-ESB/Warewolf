using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using System.Collections.ObjectModel;

namespace Dev2.Activities.Designers2.DataMerge
{
    public class DataMergeDesignerViewModel : ActivityCollectionDesignerViewModel<DataMergeDTO>
    {
        public ObservableCollection<string> ItemsList { get; private set; }

        public DataMergeDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarHelpToggle();
            dynamic mi = ModelItem;
            InitializeItems(mi.MergeCollection);

            if(mi.MergeCollection == null || mi.MergeCollection.Count <= 0)
            {
                mi.MergeCollection.Add(new DataMergeDTO("", "None", "", 1, "", "Left"));
                mi.MergeCollection.Add(new DataMergeDTO("", "None", "", 2, "", "Left"));
            }
            ItemsList = new ObservableCollection<string> { "None", "Index", "Chars", "New Line", "Tab" };
        }

        public override string CollectionName { get { return "MergeCollection"; } }
    }
}