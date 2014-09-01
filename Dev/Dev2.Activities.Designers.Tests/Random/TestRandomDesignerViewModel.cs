using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Random;
using Dev2.Common.Interfaces.Enums;

namespace Dev2.Activities.Designers.Tests.Random
{
    public class TestRandomDesignerViewModel : RandomDesignerViewModel
    {
        public TestRandomDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
        }
        public enRandomType RandomType { set { SetProperty(value); } get { return GetProperty<enRandomType>(); } }
    }
}