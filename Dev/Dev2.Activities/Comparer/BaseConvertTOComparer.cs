﻿using System.Collections.Generic;

namespace Dev2.Comparer
{
    internal class BaseConvertToComparer:IEqualityComparer<BaseConvertTO>
    {
        public bool Equals(BaseConvertTO x, BaseConvertTO y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return x.Equals(y);
        }

        public int GetHashCode(BaseConvertTO obj)
        {
            return obj.GetHashCode();
        }
    }
}
