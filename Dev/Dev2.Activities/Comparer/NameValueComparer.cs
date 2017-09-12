using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using System.Collections.Generic;

namespace Dev2.Comparer
{
    public class NameValueComparer : IEqualityComparer<INameValue>
    {
        public bool Equals(INameValue x, INameValue y)
        {
            if (x == null && y == null) return true;
            return x != null && y != null && x.Equals(y);
        }
        
        public int GetHashCode(INameValue obj)
        {
            return obj.GetHashCode();
        }
    }
}