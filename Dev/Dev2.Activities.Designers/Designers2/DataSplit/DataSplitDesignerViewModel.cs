using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Activities.Designers2.Core;
using Dev2.Studio.Core.Activities.Utils;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers2.DataSplit
{
    public class DataSplitDesignerViewModel : ActivityCollectionDesignerViewModel<DataSplitDTO>
    {
        public IList<string> ItemsList { get; private set; }

        public DataSplitDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarQuickVariableInputToggle();
            AddTitleBarHelpToggle();
            dynamic mi = ModelItem;
            InitializeItems(mi.ResultsCollection);

            if (mi.ResultsCollection == null || mi.ResultsCollection.Count <= 0)
            {
                mi.ResultsCollection.Add(new DataSplitDTO("", "Index", "", 1));
                mi.ResultsCollection.Add(new DataSplitDTO("", "Index", "", 2));
            }

            ItemsList = new List<string> { "Index", "Chars", "New Line", "Space", "Tab", "End" };
        }

        public override string CollectionName { get { return "ResultsCollection"; } }

        public void OnSplitTypeChanged(int index)
        {
            if(index < 0 || index >= ItemCount)
            {
                return;
            }

            var mi = ModelItemCollection[index];
            var splitType = mi.GetProperty("SplitType") as string;

            if(splitType == "Index" || splitType == "Chars")
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