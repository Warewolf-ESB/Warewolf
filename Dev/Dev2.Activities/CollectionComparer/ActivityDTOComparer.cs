using System;
using System.Collections.Generic;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.CollectionComparer
{
    public class ActivityDtoComparer: IEqualityComparer<ActivityDTO>
    {
        public bool Equals(ActivityDTO x, ActivityDTO y)
        {
            return x != null && y != null && string.Equals(x.FieldName, y.FieldName)
                   && string.Equals(x.FieldValue, y.FieldValue)
                   && string.Equals(x.ErrorMessage, y.ErrorMessage)
                   && Equals(x.HasError, y.HasError)
                   && Equals(x.IndexNumber, y.IndexNumber)
                   && Equals(x.Inserted, y.Inserted)
                   && Equals(x.IsFieldNameFocused, y.IsFieldNameFocused)
                   && Equals(x.IsFieldValueFocused, y.IsFieldValueFocused)
                   && Equals(x.WatermarkTextValue, y.WatermarkTextValue)
                   && Equals(x.WatermarkTextVariable, y.WatermarkTextVariable)
                   && Equals(x.WatermarkTextVariable, y.WatermarkTextVariable)
                   && x.OutList.SequenceEqual(y.OutList, StringComparer.Ordinal);
        }

        public int GetHashCode(ActivityDTO obj)
        {
            return 1;
        }
    }
}
