using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.FormatNumber;

namespace Dev2.Activities.Designers.Tests.FormatNumber
{
    public class TestFormatNumberDesignerViewModel : FormatNumberDesignerViewModel
    {
        public TestFormatNumberDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
        }

        public string RoundingType { get { return GetProperty<string>(); } set { SetProperty(value); } }
        public string RoundingDecimalPlaces { get { return GetProperty<string>(); } set { SetProperty(value); } }
    }
}