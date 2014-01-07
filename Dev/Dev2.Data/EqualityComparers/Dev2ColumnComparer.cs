using System.Collections.Generic;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.DataList.Contract.EqualityComparers
{
    public class Dev2ColumnComparer : IEqualityComparer<Dev2Column>
    {
        private static Dev2ColumnComparer _instance = new Dev2ColumnComparer();

        public static Dev2ColumnComparer Instance
        {
            get
            {
                return _instance;
            }
        }

        public bool Equals(Dev2Column x, Dev2Column y)
        {           
            return x != null && y != null && x.ColumnName == y.ColumnName && x.ColumnDescription == y.ColumnDescription;            
        }

        public int GetHashCode(Dev2Column obj)
        {
            return obj.GetHashCode();
        }
    }
}
