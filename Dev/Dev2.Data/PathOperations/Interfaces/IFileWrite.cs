
namespace Dev2.PathOperations {
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide properties related to File write operations
    /// </summary>
    public interface IFileWrite {

        /// <summary>
        /// Is the operation in append mode
        /// </summary>
        bool Append { get; set; }

        /// <summary>
        /// The payload to write
        /// </summary>
        string FileContents { get; set; }
    }
}
