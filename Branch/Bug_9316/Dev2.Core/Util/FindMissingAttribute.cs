using System;

namespace Dev2.Util
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FindMissingAttribute : Attribute
    {
        public FindMissingAttribute()
        {            
        }        
    }
}
