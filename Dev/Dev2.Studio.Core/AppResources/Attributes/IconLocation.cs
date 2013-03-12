using System;

namespace Dev2.Studio.Core.AppResources.Attributes
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public sealed class IconLocation : Attribute
    {
        public IconLocation(string value)
        {
            Value = value;
        }

        public IconLocation(string value, Type resourceType)
        {
            Value = ResourceHelper.GetResourceLookup<string>(resourceType, value);
        }

        public string Value { get; private set; }
    }
}
