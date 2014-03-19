using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.RecordsLength;

namespace Dev2.Activities.Designers.Tests.RecordsLength
{
    public class TestRecordsLengthDesignerViewModel : RecordsLengthDesignerViewModel
    {
        public TestRecordsLengthDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
        }

        public TestRecordsLengthDesignerViewModel(ModelItem modelItem, string recordSetName)
            : base(modelItem)
        {
            RecordsetName = recordSetName;
        }

        public string CountNumber { get { return GetProperty<string>(); } set { SetProperty(value); } }
        public string RecordsetName { get { return GetProperty<string>(); } set { SetProperty(value); } }
    }
}