using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Comparer
{
    public class ActivityDtoComparer : IEqualityComparer<ActivityDTO>
    {
        public bool Equals(ActivityDTO x, ActivityDTO y)
        {
            return x != null && y != null && x.Equals(y);
        }

        public int GetHashCode(ActivityDTO obj)
        {
            return 1;
        }
    }
}
