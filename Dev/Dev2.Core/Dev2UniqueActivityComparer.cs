using System.Collections.Generic;

namespace Dev2
{
    public class Dev2UniqueActivityComparer : IEqualityComparer<IDev2Activity>
    {
        public bool Equals(IDev2Activity x, IDev2Activity y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return x.UniqueID.Equals(y.UniqueID);
        }

        public int GetHashCode(IDev2Activity obj)
        {
            return obj.GetHashCode();
        }
    }
}