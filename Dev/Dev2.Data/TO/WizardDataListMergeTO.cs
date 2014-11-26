
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
