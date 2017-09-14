using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Comparer
{
    internal class DataMergeDTOComparer : IEqualityComparer<DataMergeDTO>
    {
        public bool Equals(DataMergeDTO x, DataMergeDTO y)
        {
            if (x == null && y == null) return true;
            if ((x == null && y != null) || (x != null && y == null)) return false;
            return string.Equals(x.Alignment, y.Alignment)
                && string.Equals(x.At, y.At)
                && string.Equals(x.Error, y.Error)
                && string.Equals(x.InputVariable, y.InputVariable)
                && string.Equals(x.MergeType, y.MergeType)
                && string.Equals(x.Padding, y.Padding)
                && string.Equals(x.WatermarkTextVariable, y.WatermarkTextVariable)
                && x.EnableAt.Equals(y.MergeType)
                && x.Inserted.Equals(y.Inserted)
                && x.IsFieldNameFocused.Equals(y.IsFieldNameFocused)
                && x.IsPaddingFocused.Equals(y.IsPaddingFocused)
                && x.IndexNumber.Equals(y.IndexNumber)
                && x.Path.Equals(y.Path)
                && x.Errors.Equals(y.Errors)
            ;
        }

        public int GetHashCode(DataMergeDTO obj)
        {
            return 1;
        }
    }
}
