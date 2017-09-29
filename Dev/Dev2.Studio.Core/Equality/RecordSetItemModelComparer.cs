using System.Collections.Generic;
using Dev2.Studio.Interfaces.DataList;

namespace Dev2.Studio.Core
{
    public class RecordSetItemModelComparer : IEqualityComparer<IRecordSetItemModel>
    {
        public bool Equals(IRecordSetItemModel x, IRecordSetItemModel y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return x.Equals(y);
        }

        public int GetHashCode(IRecordSetItemModel obj)
        {
            return 1;
        }
    }
}