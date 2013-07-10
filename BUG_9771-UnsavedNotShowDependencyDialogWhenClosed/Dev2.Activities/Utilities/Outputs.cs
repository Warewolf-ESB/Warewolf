using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unlimited.Applications.BusinessDesignStudio.Activities.Utilities
{
    [AttributeUsage(AttributeTargets.Property)]
    public class Outputs : Attribute, IActivityPropertyAttribute
    {        
        public Outputs(string userVisibleName)
        {
            UserVisibleName = userVisibleName;
        }

        public string UserVisibleName { get; set; }
    }
}
