using System.Collections.Generic;
using Dev2.Common;
using Dev2.TO;

namespace Dev2.Comparer
{
    internal class DataColumnMappingComparer : IEqualityComparer<DataColumnMapping>
    {
        public bool Equals(DataColumnMapping x, DataColumnMapping y)
        {
            return CommonEqualityOps.AreObjectsEqual(x, y);
        }

        public int GetHashCode(DataColumnMapping obj)
        {
            return obj.GetHashCode();
        }
    }
}