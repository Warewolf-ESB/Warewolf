using System.Collections.Generic;
using Dev2.Common.Interfaces.Core.Graph;

namespace Dev2.Comparers
{
    public class DataSourceShapeComparer : IEqualityComparer<IDataSourceShape>
    {
        public bool Equals(IDataSourceShape x, IDataSourceShape y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return x.Equals(y);
        }

        public int GetHashCode(IDataSourceShape obj)
        {
            return obj.GetHashCode();
        }
    }
}
