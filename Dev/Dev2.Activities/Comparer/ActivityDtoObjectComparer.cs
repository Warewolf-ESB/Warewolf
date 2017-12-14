using System.Collections.Generic;
using Dev2.TO;

namespace Dev2.Comparer
{
    internal class ActivityDtoObjectComparer : IEqualityComparer<AssignObjectDTO>
    {
        public bool Equals(AssignObjectDTO x, AssignObjectDTO y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return x.Equals(y);
        }

        public int GetHashCode(AssignObjectDTO obj)
        {
            return 1;
        }
    }
}