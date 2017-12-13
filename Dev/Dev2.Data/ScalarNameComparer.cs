using System;
using System.Collections.Generic;

namespace Dev2.Data
{
    public class ScalarNameComparer : IEqualityComparer<IScalar>
    {
        public int GetHashCode(IScalar obj)
        {
            return obj.Name.GetHashCode();
        }

        public bool Equals(IScalar x, IScalar y) => throw new NotImplementedException();
    }
}