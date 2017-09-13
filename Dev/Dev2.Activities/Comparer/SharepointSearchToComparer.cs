using System.Collections.Generic;
using Dev2.TO;

namespace Dev2.Comparer
{
    internal class SharepointSearchToComparer : IEqualityComparer<SharepointSearchTo>
    {
        public bool Equals(SharepointSearchTo x, SharepointSearchTo y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return x.Equals(y);
        }

        public int GetHashCode(SharepointSearchTo obj)
        {
            return obj.GetHashCode();
        }
    }
}