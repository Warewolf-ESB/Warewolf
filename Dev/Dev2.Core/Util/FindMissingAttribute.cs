using System;

namespace Dev2.Util
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FindMissingAttribute : Attribute
    {
        // ReSharper disable EmptyConstructor
        public FindMissingAttribute()
        // ReSharper restore EmptyConstructor
        {
        }
    }
}
