using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DataList.Contract{
    [Flags]
    public enum DataListMergeFrequency {
        Never = 0,
        OnBookmark = 1,
        OnResumption = 2,
        OnCompletion = 4,
        Always = 8,
    }
}
