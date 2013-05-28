using System;

namespace Unlimited.Applications.BusinessDesignStudio.Activities.Utilities
{
    [AttributeUsage(AttributeTargets.Property)]
    public class Inputs : Attribute, IActivityPropertyAttribute
    {
        public Inputs(string userVisibleName)
        {
            UserVisibleName = userVisibleName;
        }

        public string UserVisibleName { get; set; }

        public string Value { get; set; }
    }
}
