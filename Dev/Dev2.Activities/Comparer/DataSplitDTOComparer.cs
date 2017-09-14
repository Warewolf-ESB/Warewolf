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
            if ((x == null && y != null) || (x != null && y == null)) return false;
            var oulistsAreEqual = Common.CommonEqualityOps.CollectionEquals(x.OutList, y.OutList, StringComparer.Ordinal);
            return string.Equals(x.At, y.At)
                && x.EnableAt.Equals(y.EnableAt)
                && string.Equals(x.Error, y.Error)
                && string.Equals(x.EscapeChar, y.EscapeChar)
                && string.Equals(x.OutputVariable, y.OutputVariable)
                && string.Equals(x.SplitType, y.SplitType)
                && oulistsAreEqual
                && x.Include.Equals(y.Include)
                && string.Equals(x.WatermarkTextVariable, y.WatermarkTextVariable)
                && x.Inserted.Equals(y.Inserted)
                && x.IsEscapeCharEnabled.Equals(y.IsEscapeCharEnabled)
                && x.IsEscapeCharFocused.Equals(y.IsEscapeCharFocused)
                && x.IndexNumber.Equals(y.IndexNumber)
                && x.Errors.Equals(y.Errors)
            ;
        }

        public int GetHashCode(DataSplitDTO obj)
        {
            return 1;
        }
    }
}
