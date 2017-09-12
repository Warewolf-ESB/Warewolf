using System.Collections.Generic;
using Dev2.TO;

namespace Dev2.Comparer
{
    internal class Dev2ActivityComparer : IEqualityComparer<IDev2Activity>
    {
        public bool Equals(IDev2Activity x, IDev2Activity y)
        {
            return x != null && y != null && x.Equals(y);
        }

        public int GetHashCode(IDev2Activity obj)
        {
            return 1;
        }
    }
}