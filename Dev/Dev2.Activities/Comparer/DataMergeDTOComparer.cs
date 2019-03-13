using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Comparer
{
    internal class DataMergeDtoComparer : IEqualityComparer<DataMergeDTO>
    {
#pragma warning disable S1541 // Methods and properties should not be too complex
        public bool Equals(DataMergeDTO x, DataMergeDTO y)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return string.Equals(x.Alignment, y.Alignment)
                && string.Equals(x.At, y.At)
                && string.Equals(x.Error, y.Error)
                && string.Equals(x.InputVariable, y.InputVariable)
                && string.Equals(x.MergeType, y.MergeType)
                && string.Equals(x.Padding, y.Padding)
                && Equals(x.EnableAt,y.EnableAt)
                && Equals(x.Inserted,y.Inserted)
                && Equals(x.IsFieldNameFocused,y.IsFieldNameFocused)
                && Equals(x.IsPaddingFocused,y.IsPaddingFocused)
                && Equals(x.IndexNumber,y.IndexNumber)
                && Equals(x.Path,y.Path);
        }

        public int GetHashCode(DataMergeDTO obj) => 1;
    }
}
