using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Interfaces
{
    public interface ICaseConverter
    {
        IBinaryDataListItem TryConvert(string conversionType, IBinaryDataListItem item);
    }
}
