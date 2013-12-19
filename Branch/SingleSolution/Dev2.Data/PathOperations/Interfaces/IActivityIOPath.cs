using Dev2.Data.PathOperations.Interfaces;

namespace Dev2.PathOperations
{

    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide a generic path abstraction for IO operations
    /// </summary>
    public interface IActivityIOPath : IPathAuth, IPathCertVerify
    {

        /// <summary>
        /// The path type FTP, FTPS, FileSystem
        /// </summary>
        enActivityIOPathType PathType { get; }

        /// <summary>
        /// The string version of the path
        /// </summary>
        string Path { get; set; }

        /// <summary>
        /// Convert this object into XML
        /// </summary>
        /// <returns></returns>
        string ToXML();
    }
}
