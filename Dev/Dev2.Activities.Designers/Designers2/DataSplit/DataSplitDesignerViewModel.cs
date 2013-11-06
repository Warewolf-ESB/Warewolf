using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Activities.Designers2.Core;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers2.DataSplit
{
    public class DataSplitDesignerViewModel : ActivityCollectionDesignerViewModel<DataSplitDTO>
    {
        public IList<string> ItemsList { get; private set; }

        public DataSplitDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarHelpToggle();
            dynamic mi = ModelItem;
            InitializeItems(mi.ResultsCollection);

            if(mi.ResultsCollection == null || mi.ResultsCollection.Count <= 0)
            {
                var test = mi.ResultsCollection;
                mi.ResultsCollection.Add(new DataSplitDTO("", "Index", "", 1));
                mi.ResultsCollection.Add(new DataSplitDTO("", "Index", "", 2));
            }

            ItemsList = new List<string> { "Index", "Chars", "New Line", "Space", "Tab", "End" };
        }
        public override string CollectionName { get { return "ResultsCollection"; } }
    }
}