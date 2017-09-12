using System.Collections.Generic;
using Dev2.TO;

namespace Dev2.Comparer
{
    public class SharepointSearchToComparer : IEqualityComparer<SharepointSearchTo>
    {
        public bool Equals(SharepointSearchTo x, SharepointSearchTo y)
        {
            if (x == null && y == null) return true;
            return x != null && y != null && x.Equals(y);
        }

        public int GetHashCode(SharepointSearchTo obj)
        {
            return obj.GetHashCode();
        }
    }
}