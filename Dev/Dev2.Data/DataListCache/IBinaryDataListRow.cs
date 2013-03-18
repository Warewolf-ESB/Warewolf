namespace Dev2.Data.Binary_Objects
{
    /// <summary>
    /// The new storage format
    /// </summary>
    public interface IBinaryDataListRow
    {

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </value>
        bool IsEmpty { get; }

        /// <summary>
        /// Determines whether a column is a defered read operation
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <returns>
        ///   <c>true</c> if [is deferred read] [the specified idx]; otherwise, <c>false</c>.
        /// </returns>
        bool IsDeferredRead(int idx);

        /// <summary>
        /// Fetches the value for a column
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <returns></returns>
        string FetchValue(int idx);

        /// <summary>
        /// Updates the value for a column
        /// </summary>
        /// <param name="val">The val.</param>
        /// <param name="idx">The idx.</param>
        void UpdateValue(string val, int idx);

        /// <summary>
        /// Fetches the deferred location value for a column
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <returns></returns>
        string FetchDeferredLocation(int idx);
    }
}
