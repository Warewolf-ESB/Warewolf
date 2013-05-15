namespace Dev2.Data.Storge
{
    /// <summary>
    /// Core storage interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBinaryDataListStorage<T>
    {

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </value>
        bool IsEmpty { get; }


        /// <summary>
        /// Fetches the value for a column
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <returns></returns>
        T FetchValue(int idx);

        /// <summary>
        /// Updates the value for a column
        /// </summary>
        /// <param name="val">The val.</param>
        /// <param name="idx">The idx.</param>
        void UpdateValue(T val, int idx);

    }
}
