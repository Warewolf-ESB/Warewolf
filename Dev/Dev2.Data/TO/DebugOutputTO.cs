using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Data.TO
{
    public class DebugOutputTO
    {
        #region Ctor

        public DebugOutputTO()
        {

        }

        public DebugOutputTO(IBinaryDataListEntry targetEntry,IBinaryDataListEntry fromEntry, string expression)
        {
            TargetEntry = targetEntry;
            FromEntry = fromEntry;
            Expression = expression;
        }

        #endregion

        #region Properties

        public IBinaryDataListEntry TargetEntry { get; set; }
        public IBinaryDataListEntry FromEntry { get; set; }
        public string Expression { get; set; }
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
