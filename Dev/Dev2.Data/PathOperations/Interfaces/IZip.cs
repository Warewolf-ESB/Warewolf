
namespace Dev2.PathOperations {
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide properties related to Zip operations as per DotNetZip : http://dotnetzip.codeplex.com/
    /// </summary>
    public interface IZip {

        /// <summary>
        /// The compression ratio for ziping
        /// </summary>
        string CompressionRatio { get; set; }

        /// <summary>
        /// The password for the created archive
        /// </summary>
        string ArchivePassword { get; set; }

        /// <summary>
        /// The name of the archive
        /// </summary>
        string ArchiveName { get; set; }

    }
}
