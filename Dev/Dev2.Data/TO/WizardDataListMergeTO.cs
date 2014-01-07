using System.Collections.Generic;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.DataList.Contract.TO {
    public class WizardDataListMergeTO {

        public IList<IBinaryDataListEntry> AddedRegions { get; private set; }

        public IList<IBinaryDataListEntry> RemovedRegions { get; private set; }

        public string IntersectedDataList { get; private set; }

        public WizardDataListMergeTO() {
            AddedRegions = new List<IBinaryDataListEntry>();
            RemovedRegions = new List<IBinaryDataListEntry>();
        }

        public void AddNewRegion(IBinaryDataListEntry region) {
            AddedRegions.Add(region);
        }

        public void AddRemovedRegion(IBinaryDataListEntry region) {
            RemovedRegions.Add(region);
        }

        public void SetIntersectedDataList(string bdl) {
            IntersectedDataList = bdl;
        }
    }
}
