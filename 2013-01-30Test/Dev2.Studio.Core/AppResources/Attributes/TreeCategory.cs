using System;

namespace Dev2.Studio.Core.AppResources.Attributes
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class TreeCategory  : Attribute
    {
        public TreeCategory(string value)
        {
            Value = value;
        }

        public string Value { get; private set; }
    }
}
