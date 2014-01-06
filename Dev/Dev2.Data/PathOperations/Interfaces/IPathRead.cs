namespace Dev2.PathOperations {
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide properties related to file/folder read operations
    /// </summary>
    public interface IPathRead {

        /// <summary>
        /// The location in the datalist to place the tmp of a read operation
        /// </summary>
        string ResultExpression { get; set; }
    }
}
