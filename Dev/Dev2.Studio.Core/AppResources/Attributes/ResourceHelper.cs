using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Attributes
{
    //From http://stackoverflow.com/questions/1150874/c-sharp-attribute-text-from-resource-file
    public class ResourceHelper
    {
        public static T GetResourceLookup<T>(Type resourceType, string resourceName)
        {
            if((resourceType != null) && (resourceName != null))
            {
                PropertyInfo property = resourceType.GetProperty(resourceName,
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
                if(property == null)
                {
                    return default(T);
                }

                return (T)property.GetValue(null, null);
            }
            return default(T);
        }
    }
}
