using System;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Comparer
{
    internal class DataSplitDTOComparer : IEqualityComparer<DataSplitDTO>
    {
        public bool Equals(DataSplitDTO x, DataSplitDTO y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            var oulistsAreEqual = Common.CommonEqualityOps.CollectionEquals(x.OutList, y.OutList, StringComparer.Ordinal);
            var @equals = string.Equals(x.At, y.At)
                          && x.EnableAt.Equals(y.EnableAt)
                          && string.Equals(x.Error, y.Error)
                          && (string.Equals(x.EscapeChar, y.EscapeChar) || (string.IsNullOrEmpty(x.EscapeChar) && string.IsNullOrEmpty(y.EscapeChar)))
                          && string.Equals(x.OutputVariable, y.OutputVariable)
                          && string.Equals(x.SplitType, y.SplitType)
                          && oulistsAreEqual
                          && x.Include.Equals(y.Include)
                          && Equals(x.Inserted,y.Inserted)
                          && Equals(x.IsEscapeCharEnabled,y.IsEscapeCharEnabled)
                          && Equals(x.IsEscapeCharFocused,y.IsEscapeCharFocused)
                          && Equals(x.IndexNumber,y.IndexNumber);
            return @equals
            ;
        }

        public int GetHashCode(DataSplitDTO obj)
        {
            return 1;
        }
    }
}
