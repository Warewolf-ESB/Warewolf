using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.MultiAssign;

namespace Dev2.Core.Tests.Activities
{
    class TestDsfMultiAssignActivityViewModel : MultiAssignDesignerViewModel
    {
        public TestDsfMultiAssignActivityViewModel(ModelItem modelItem)
            : base(modelItem)
        {
        }

        public string GetCollectionName()
        {
            return CollectionName;
        }
    }
}