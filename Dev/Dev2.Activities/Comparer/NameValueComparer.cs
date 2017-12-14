using Dev2.Common.Interfaces;
using System.Collections.Generic;

namespace Dev2.Comparer
{
    public class NameValueComparer : IEqualityComparer<INameValue>
    {
        public bool Equals(INameValue x, INameValue y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return x.Equals(y);
        }
        
        public int GetHashCode(INameValue obj)
        {
            return obj.GetHashCode();
        }
    }
}