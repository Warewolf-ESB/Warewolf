
namespace Dev2.PathOperations {
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide properties related to File move and copy operations
    /// </summary>
    public interface IPathOverwrite {

        /// <summary>
        /// should the operation overwrite if exist
        /// </summary>
        bool Overwrite { get; set; }
    }
}
