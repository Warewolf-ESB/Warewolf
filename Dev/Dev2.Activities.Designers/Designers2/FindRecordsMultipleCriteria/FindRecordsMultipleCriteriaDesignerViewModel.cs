using System.Activities.Presentation.Model;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Activities.Designers2.Core;
using Dev2.DataList;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers2.FindRecordsMultipleCriteria
{
    public class FindRecordsMultipleCriteriaDesignerViewModel : ActivityCollectionDesignerViewModel<FindRecordsTO>
    {
        public FindRecordsMultipleCriteriaDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarHelpToggle();

            WhereOptions = new ObservableCollection<string>(FindRecsetOptions.FindAll().Select(c => c.HandlesType()).OrderBy(c => c));

            dynamic mi = ModelItem;
            InitializeItems(mi.ResultsCollection);
        }

        public override string CollectionName { get { return "ResultsCollection"; } }

        public ObservableCollection<string> WhereOptions { get; private set; }
    }
}