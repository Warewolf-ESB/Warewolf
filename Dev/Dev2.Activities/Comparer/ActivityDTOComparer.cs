using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Comparer
{
    internal class ActivityDtoComparer : IEqualityComparer<ActivityDTO>
    {
        public bool Equals(ActivityDTO x, ActivityDTO y)
        {
            if (x == null && y == null) return true;
            return x != null && y != null && x.Equals(y);
        }

        public int GetHashCode(ActivityDTO obj)
        {
            return 1;
        }
    }
}
