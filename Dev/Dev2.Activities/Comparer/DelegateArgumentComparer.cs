using System;
using System.Activities;
using System.Collections.Generic;

namespace Dev2.Comparer
{
    internal class DelegateArgumentComparer : IEqualityComparer<DelegateArgument>
    {
        public bool Equals(DelegateArgument x, DelegateArgument y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            var @equals = string.Equals(x.Name, y.Name);
            var typesAreEqual = x.Type == y.Type;
            var directionIsTheSame = x.Direction == y.Direction;
            
            return @equals && typesAreEqual && directionIsTheSame;
        }

        public int GetHashCode(DelegateArgument obj)
        {
            return 1;
        }
    }
}
