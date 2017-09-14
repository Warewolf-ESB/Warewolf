using System.Collections.Generic;
using Dev2.TO;
using System.Activities;

namespace Dev2.Comparer
{
    internal class ActivityComparer : IEqualityComparer<Activity>
    {
        public bool Equals(Activity x, Activity y)
        {
            if (x == null && y == null) return true;
            if ((x != null && y == null) || (x == null && y != null)) return false;

            return string.Equals(x.DisplayName, y.DisplayName)
                && string.Equals(x.Id, y.Id);
        }

        public int GetHashCode(Activity obj)
        {
            return 1;
        }
    }
}