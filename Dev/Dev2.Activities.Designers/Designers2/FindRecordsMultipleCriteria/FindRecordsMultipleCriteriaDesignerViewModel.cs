using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers2.FindRecordsMultipleCriteria
{
    public class FindRecordsMultipleCriteriaDesignerViewModel : ActivityCollectionDesignerViewModel<FindRecordsTO>
    {
        public FindRecordsMultipleCriteriaDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarHelpToggle();
        }

        public string FieldsToSearch { get { return GetProperty<string>(); } set { SetProperty(value); } }
        public string Result { get { return GetProperty<string>(); } set { SetProperty(value); } }

        protected override string CollectionName { get { return "ResultsCollection"; } }
    }
}