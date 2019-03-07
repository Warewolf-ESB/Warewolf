using System;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Comparer
{
    internal class FindRecordsTOComparer : IEqualityComparer<FindRecordsTO>
    {
#pragma warning disable S1541 // Methods and properties should not be too complex
        public bool Equals(FindRecordsTO x, FindRecordsTO y)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            if (x == null && y == null)
            {
                return true;
            }

            if ((x == null && y != null) || (x != null && y == null))
            {
                return false;
            }

            var optionListsAreEqual = Common.CommonEqualityOps.CollectionEquals(x.WhereOptionList, y.WhereOptionList, StringComparer.Ordinal);
            return string.Equals(x.From, y.From)
                && string.Equals(x.To, y.To)
                && string.Equals(x.SearchCriteria, y.SearchCriteria)
                && string.Equals(x.SearchType, y.SearchType)
                && x.IsFromFocused.Equals(y.IsFromFocused)
                && x.IsToFocused.Equals(y.IsToFocused)
                && x.IsSearchCriteriaEnabled.Equals(y.IsSearchCriteriaEnabled)
                && x.IsSearchCriteriaFocused.Equals(y.IsSearchCriteriaFocused)
                && x.IsSearchCriteriaVisible.Equals(y.IsSearchCriteriaVisible)
                && x.IsSearchTypeFocused.Equals(y.IsSearchTypeFocused)
                && x.Inserted.Equals(y.Inserted)
                && x.IndexNumber.Equals(y.IndexNumber)
                && optionListsAreEqual
            ;
        }

        public int GetHashCode(FindRecordsTO obj) => 1;
    }
}
