using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.SortRecords;

namespace Dev2.Activities.Designers.Tests.SortRecords
{
    public class TestSortRecordsDesignerViewModel : SortRecordsDesignerViewModel
    {
        public TestSortRecordsDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
        }

        public string SelectedSort { get { return GetProperty<string>(); } set { SetProperty(value); } }
        public string SortField { get { return GetProperty<string>(); } set { SetProperty(value); } }
    }
}