using Dev2.Common.Interfaces.DB;
using System.Collections.Generic;

namespace Dev2.Comparer
{
    public class ServiceInputComparer : IEqualityComparer<IServiceInput>
    {
        public bool Equals(IServiceInput x, IServiceInput y)
        {
            if (x == null && y == null) return true;
            return x != null && y != null && x.Equals(y);
        }
        
        public int GetHashCode(IServiceInput obj)
        {
            return obj.GetHashCode();
        }
    }
}