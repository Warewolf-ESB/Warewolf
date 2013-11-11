using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Dev2.Activities.Designers2.Core;
using Dev2.DataList;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.ViewModels.Base;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers2.FindRecordsMultipleCriteria
{
    public class FindRecordsMultipleCriteriaDesignerViewModel : ActivityCollectionDesignerViewModel<FindRecordsTO>
    {
        readonly IList<string> _requiresSearchCriteria = new List<string> { "Not Contains", "Contains", "Equal", "Not Equal", "Ends With", "Starts With", "Regex", ">", "<", "<=", ">=" };

        public FindRecordsMultipleCriteriaDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarHelpToggle();

            WhereOptions = new ObservableCollection<string>(FindRecsetOptions.FindAll().Select(c => c.HandlesType()).OrderBy(c => c));
            SearchTypeUpdatedCommand = new RelayCommand(OnSearchTypeChanged, o => true);

            dynamic mi = ModelItem;
            InitializeItems(mi.ResultsCollection);
        }

        public override string CollectionName { get { return "ResultsCollection"; } }

        public ICommand SearchTypeUpdatedCommand { get; private set; }

        public ObservableCollection<string> WhereOptions { get; private set; }

        void OnSearchTypeChanged(object indexObj)
        {
            var index = (int)indexObj;

            if(index < 0 || index >= ItemCount)
            {
                return;
            }

            var mi = ModelItemCollection[index];

            var searchType = mi.GetProperty("SearchType") as string;
            var requiresCriteria = _requiresSearchCriteria.Contains(searchType);
            mi.SetProperty("IsSearchCriteriaEnabled", requiresCriteria);
            if(!requiresCriteria)
            {
                mi.SetProperty("SearchCriteria", string.Empty);
            }
        }
    }
}