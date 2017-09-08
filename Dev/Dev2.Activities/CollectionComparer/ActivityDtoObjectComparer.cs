using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.TO;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.CollectionComparer
{
    public class ActivityDtoObjectComparer : IEqualityComparer<AssignObjectDTO>
    {
        public bool Equals(AssignObjectDTO x, AssignObjectDTO y)
        {
            return x != null && y != null && x.Equals(y);
        }

        public int GetHashCode(AssignObjectDTO obj)
        {
            return 1;
        }
    }
}