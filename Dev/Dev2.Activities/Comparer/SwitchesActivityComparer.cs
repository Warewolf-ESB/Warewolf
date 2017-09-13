using System;
using System.Collections.Generic;

namespace Dev2.Comparer
{
    internal class SwitchesActivityComparer : IEqualityComparer<Dictionary<string, IDev2Activity>>
    {
        public bool Equals(Dictionary<string, IDev2Activity> x, Dictionary<string, IDev2Activity> y)
        {
            if (x == null && y == null) return true;
            if ((x == null && y != null) || (x != null && y == null)) return false;
            var activitiesAreEqual = Common.CommonEqualityOps.CollectionEquals(x.Values, y.Values, new Dev2ActivityComparer());
            var keysAreEqual = Common.CommonEqualityOps.CollectionEquals(x.Keys, y.Keys, StringComparer.Ordinal);
            return x != null
                && y != null
                && x.Count == y.Count
                && keysAreEqual
                && activitiesAreEqual;
        }
        
        public int GetHashCode(Dictionary<string, IDev2Activity> obj)
        {
            return 1;
        }
    }
}
