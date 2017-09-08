using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.TO;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.CollectionComparer
{
    public class ActivityDtoObjectComparer: IEqualityComparer<AssignObjectDTO>
    {
        public bool Equals(AssignObjectDTO x, AssignObjectDTO y)
        {
            return x != null && y != null && string.Equals(x.FieldName, y.FieldName)
                   && string.Equals(x.FieldValue, y.FieldValue)
                   && Equals(x.IndexNumber, y.IndexNumber)
                   && Equals(x.Inserted, y.Inserted)
                   && Equals(x.IsFieldNameFocused, y.IsFieldNameFocused)
                   && Equals(x.IsFieldValueFocused, y.IsFieldValueFocused)
                   && Equals(x.WatermarkTextValue, y.WatermarkTextValue)
                   && Equals(x.WatermarkTextVariable, y.WatermarkTextVariable)
                   && Equals(x.WatermarkTextVariable, y.WatermarkTextVariable)
                   && x.OutList.SequenceEqual(y.OutList, StringComparer.Ordinal);
        }

        public int GetHashCode(AssignObjectDTO obj)
        {
            return 1;
        }
    }
}