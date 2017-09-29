using System.Collections.Generic;
using Dev2.Studio.Interfaces.DataList;

namespace Dev2.Studio.Core
{
    public class ComplexObjectItemModelComparer : IEqualityComparer<IComplexObjectItemModel>
    {
        public bool Equals(IComplexObjectItemModel x, IComplexObjectItemModel y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return x.Equals(y);
        }

        public int GetHashCode(IComplexObjectItemModel obj)
        {
            return 1;
        }
    }
}