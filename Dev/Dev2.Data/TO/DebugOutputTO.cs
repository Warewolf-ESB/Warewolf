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
        /// <summary>
        /// Gets or sets the index of the used recordset.
        /// </summary>
        /// <value>
        /// The index of the used recordset.
        /// </value>
        public int UsedRecordsetIndex { get; set; }

        #endregion

    }
}
