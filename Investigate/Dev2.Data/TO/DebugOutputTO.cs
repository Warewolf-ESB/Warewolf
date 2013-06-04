using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Data.TO
{
    public class DebugOutputTO
    {
        #region Ctor

        public DebugOutputTO()
        {

        }

        public DebugOutputTO(IBinaryDataListEntry targetEntry,IBinaryDataListEntry fromEntry)
        {
            TargetEntry = targetEntry;
            FromEntry = fromEntry;                            
        }

        #endregion

        #region Properties

        public IBinaryDataListEntry TargetEntry { get; set; }
        public IBinaryDataListEntry FromEntry { get; set; }


        #endregion

    }
}
