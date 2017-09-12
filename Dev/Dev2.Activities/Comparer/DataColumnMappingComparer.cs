using System.Collections.Generic;
using Dev2.TO;

namespace Dev2.Comparer
{
    internal class DataColumnMappingComparer : IEqualityComparer<DataColumnMapping>
    {
        public bool Equals(DataColumnMapping x, DataColumnMapping y)
        {
            if (x == null && y == null) return true;
            return x != null && y != null && x.Equals(y);
        }

        public int GetHashCode(DataColumnMapping obj)
        {
            return obj.GetHashCode();
        }
    }
}