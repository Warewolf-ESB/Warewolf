using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.CountRecords;

namespace Dev2.Activities.Designers.Tests.CountRecords
{
    public class TestCountRecordsDesignerViewModel : CountRecordsDesignerViewModel
    {
        public TestCountRecordsDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
        }

        public TestCountRecordsDesignerViewModel(ModelItem modelItem, string recordSetName)
            : base(modelItem)
        {
            RecordsetName = recordSetName;
        }

        public string CountNumber { get { return GetProperty<string>(); } set { SetProperty(value); } }
        public string RecordsetName { get { return GetProperty<string>(); } set { SetProperty(value); } }
    }
}