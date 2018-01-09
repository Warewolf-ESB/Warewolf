using System.Collections.Generic;
using Dev2.Data.SystemTemplates.Models;

namespace Dev2.Data
{
    internal class Dev2DecisionComparer : IEqualityComparer<Dev2Decision>
    {
        public bool Equals(Dev2Decision x, Dev2Decision y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return x.Equals(y);
        }

        public int GetHashCode(Dev2Decision obj)
        {
            return obj.GetHashCode();
        }
    }
}