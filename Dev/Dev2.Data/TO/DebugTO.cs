using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Data.TO
{
// ReSharper disable InconsistentNaming
    public class DebugTO
    {
        #region Ctor

        public DebugTO()
        {

        }

        public DebugTO(IBinaryDataListEntry targetEntry, IBinaryDataListEntry leftEntry, string expression)
        {
            TargetEntry = targetEntry;
            LeftEntry = leftEntry;
            Expression = expression;
        }

        #endregion

        #region Properties

        public IBinaryDataListEntry TargetEntry { get; set; }
        public IBinaryDataListEntry LeftEntry { get; set; }
        public IBinaryDataListEntry RightEntry { get; set; }
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
