using System;
using System.Collections.Generic;

namespace Dev2.Data
{
    public class ScalarNameComparer : IEqualityComparer<IScalar>
    {
        #region Implementation of IEqualityComparer<in IScalar>
        
        public bool Equals(IScalar x, IScalar y)
        {
            if (x == null)
            {
                return false;
            }

            if (y == null)
            {
                return false;
            }

            if (x.Name == null && y.Name == null)
            {
                return true;
            }

            if (x.Name != null)
            {
                return x.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
        
        public int GetHashCode(IScalar obj)
        {
            return obj.Name.GetHashCode();
        }

        #endregion
    }
}