using System.Collections.Generic;

namespace Dev2.Diagnostics
{
    public class DebugItemResultEqualityComparer : IEqualityComparer<IDebugItemResult>
    {
        public bool Equals(IDebugItemResult x, IDebugItemResult y)
        {
            if(x != null && y != null)
            {
                return x.Type == y.Type
                       && x.Label == y.Label
                       && x.Variable == y.Variable
                       && x.Value == y.Value
                       && x.GroupName == y.GroupName
                       && x.GroupIndex == y.GroupIndex
                       && x.MoreLink == y.MoreLink;
            }
            return false;
        }

        public int GetHashCode(IDebugItemResult obj)
        {
            var hCode = obj.Value.Length ^ obj.GroupName.Length ^ obj.GroupIndex ^ obj.MoreLink.Length;
            return hCode.GetHashCode();
        }


    }
}