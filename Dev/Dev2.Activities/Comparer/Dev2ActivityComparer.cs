using System.Collections.Generic;

namespace Dev2.Comparer
{
    internal class Dev2ActivityComparer:IEqualityComparer<IDev2Activity>
    {
        public bool Equals(IDev2Activity x, IDev2Activity y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return x.Equals(y);
        }

        public int GetHashCode(IDev2Activity obj)
        {
            return obj.GetHashCode();
        }
    }
}
