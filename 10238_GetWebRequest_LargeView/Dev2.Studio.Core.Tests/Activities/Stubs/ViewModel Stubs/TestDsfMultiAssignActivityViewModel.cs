using System.Activities.Presentation.Model;
using Dev2.Activities.Designers.DsfMultiAssign;

namespace Dev2.Core.Tests.Activities
{
    class TestDsfMultiAssignActivityViewModel : DsfMultiAssignActivityViewModel
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