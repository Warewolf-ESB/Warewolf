using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using Dev2.Studio.Core.Activities.Utils;

namespace Dev2.Core.Tests.Utils
{
    public static class TestModelItemFactory
    {
        public static ModelItem CreateModelItem(object objectToMakeModelItem)
        {
            return ModelItemUtils.CreateModelItem(objectToMakeModelItem);
        }
    }
}
