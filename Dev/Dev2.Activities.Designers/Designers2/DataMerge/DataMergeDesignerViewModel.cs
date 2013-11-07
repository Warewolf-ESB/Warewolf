using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Activities.Designers2.Core;
using Dev2.Studio.Core.Activities.Utils;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers2.DataMerge
{
    public class DataMergeDesignerViewModel : ActivityCollectionDesignerViewModel<DataMergeDTO>
    {
        public IList<string> ItemsList { get; private set; }

        public DataMergeDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarQuickVariableInputToggle();
            AddTitleBarHelpToggle();
            dynamic mi = ModelItem;
          
            if (mi.MergeCollection == null || mi.MergeCollection.Count <= 0)
            {
                mi.MergeCollection.Add(new DataMergeDTO("", "None", "", 1, "", "Left"));
                mi.MergeCollection.Add(new DataMergeDTO("", "None", "", 2, "", "Left"));
            }

            InitializeItems(mi.MergeCollection);
            ItemsList = new List<string> { "None", "Index", "Chars", "New Line", "Tab" };
        }

        public override string CollectionName { get { return "MergeCollection"; } }

        public void OnMergeTypeChanged(int index)
        {
            if (index < 0 || index >= ItemCount)
            {
                return;
            }

            var mi = ModelItemCollection[index];
            var mergeType = mi.GetProperty("MergeType") as string;

            if (mergeType == "Index" || mergeType == "Chars")
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