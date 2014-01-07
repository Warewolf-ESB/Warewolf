using Dev2.DataList.Contract;

namespace Dev2.Data.Compilers
{
    /// <summary>
    /// Used to process recordset data for upsert ;)
    /// </summary>
    internal class TransientRecordsetProcessGroup
    {

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string TargetValue { get; private set; }

        /// <summary>
        /// Gets the target value.
        /// </summary>
        /// <value>
        /// The target value.
        /// </value>
        public enRecordsetIndexType IdxType { get; private set; }

        public bool IsTargetRecordSet { get; private set; }


        internal TransientRecordsetProcessGroup(string targetValue, enRecordsetIndexType typeOf, bool isRS)
        {
            TargetValue = targetValue;
            IdxType = typeOf;
            IsTargetRecordSet = isRS;
        }
    }
}
