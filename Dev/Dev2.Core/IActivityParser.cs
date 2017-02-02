using System.Activities;

namespace Dev2
{
    public interface IActivityParser
    {
        IDev2Activity Parse(DynamicActivity dynamicActivity);
    }
}