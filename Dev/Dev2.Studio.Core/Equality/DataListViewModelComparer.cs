using System.Collections.Generic;
using Dev2.Studio.Interfaces.DataList;

namespace Dev2.Studio.Core.Equality
{
    public class DataListViewModelComparer : IEqualityComparer<IDataListViewModel>
    {
        public bool Equals(IDataListViewModel x, IDataListViewModel y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return x.Equals(y);
        }

        public int GetHashCode(IDataListViewModel obj)
        {
            return 1;
        }
    }
}
