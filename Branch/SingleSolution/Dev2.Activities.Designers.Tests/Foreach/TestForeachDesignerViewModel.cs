using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Foreach;
using Dev2.Data.Enums;

namespace Dev2.Activities.Designers.Tests.Foreach
{
    public class TestForeachDesignerViewModel : ForeachDesignerViewModel
    {
        public TestForeachDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
        }
        public enForEachType ForEachType { set { SetProperty(value); } get { return GetProperty<enForEachType>(); } }
    }
}