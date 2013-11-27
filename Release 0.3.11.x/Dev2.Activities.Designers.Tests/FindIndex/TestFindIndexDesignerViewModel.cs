using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.FindIndex;

namespace Dev2.Activities.Designers.Tests.FindIndex
{
    public class TestFindIndexDesignerViewModel : FindIndexDesignerViewModel
    {
        public TestFindIndexDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
        }

        public string Index { set { SetProperty(value); } get { return GetProperty<string>(); } }
        public string Direction { set { SetProperty(value); } get { return GetProperty<string>(); } }
    }
}