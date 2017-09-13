using System.Collections.Generic;
using Dev2.Common.Interfaces.Core.Convertors.Case;

namespace Dev2.Comparer
{
    internal class CaseConvertToComparer:IEqualityComparer<ICaseConvertTO>
    {
        public bool Equals(ICaseConvertTO x, ICaseConvertTO y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return x.Equals(y);
        }

        public int GetHashCode(ICaseConvertTO obj)
        {
            return obj.GetHashCode();
        }
    }
}