using System.Collections.Generic;

namespace Dev2
{
    public class Dev2ActivityComparer : IEqualityComparer<IDev2Activity>
    {
        public bool Equals(IDev2Activity x, IDev2Activity y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            var @equals = x.Equals(y);
            return @equals;
        }

        public int GetHashCode(IDev2Activity obj)
        {
            return 1;
        }
    }
}
