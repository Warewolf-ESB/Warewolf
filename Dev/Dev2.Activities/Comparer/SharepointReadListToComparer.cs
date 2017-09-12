using System.Collections.Generic;
using Dev2.Common.Interfaces;

namespace Dev2.Comparer
{
    internal class SharepointReadListToComparer : IEqualityComparer<ISharepointReadListTo>
    {
        public bool Equals(ISharepointReadListTo x, ISharepointReadListTo y)
        {
            if (x == null && y == null) return true;
            return x != null && y != null && x.Equals(y);
        }

        public int GetHashCode(ISharepointReadListTo obj)
        {
            return obj.GetHashCode();
        }
    }
}
