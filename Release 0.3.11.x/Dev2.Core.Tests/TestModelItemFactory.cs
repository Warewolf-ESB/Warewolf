using System.Activities.Presentation;
using System.Activities.Presentation.Model;

namespace Dev2.Tests
{
    public static class TestModelItemFactory
    {
        public static ModelItem CreateModelItem(object objectToMakeModelItem)
        {
            return TestModelItemUtil.CreateModelItem(objectToMakeModelItem);
        }
    }
}
