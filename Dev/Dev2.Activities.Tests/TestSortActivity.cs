using Dev2.Diagnostics;
using System.Activities;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities
{
    public class TestSortActivity : DsfSortRecordsActivity
    {
        public int GetDataListItemsCallCount;
        public bool? GetDataListItemsParamIncludeIndexInRecordSetName;

        public void Reset()
        {
            GetDataListItemsCallCount = 0;
            GetDataListItemsParamIncludeIndexInRecordSetName = null;
        }

        protected override IList<IDebugItem> GetDataListItems(NativeActivityContext context, bool includeIndexInRecordSetName = false)
        {
            GetDataListItemsCallCount++;
            GetDataListItemsParamIncludeIndexInRecordSetName = includeIndexInRecordSetName;
            return new List<IDebugItem>();
        }

    }
}
