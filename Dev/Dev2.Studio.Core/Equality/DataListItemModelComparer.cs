using System.Collections.Generic;
using Dev2.Studio.Interfaces.DataList;

namespace Dev2.Studio.Core.Equality
{
    public class DataListItemModelComparer : IEqualityComparer<IDataListItemModel>
    {
        public bool Equals(IDataListItemModel x, IDataListItemModel y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return x.Equals(y);
        }

        public int GetHashCode(IDataListItemModel obj)
        {
            return 1;
        }
    }
}
