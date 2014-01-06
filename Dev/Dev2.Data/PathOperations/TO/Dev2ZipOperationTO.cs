
namespace Dev2.PathOperations {
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide the Zip methods its arguments
    /// </summary>
    public class Dev2ZipOperationTO : IZip {

        public Dev2ZipOperationTO(string ratio, string passwd, string name,bool overwrite) {
            CompressionRatio = ratio;
            ArchivePassword = passwd;
            ArchiveName = name;
            Overwrite = overwrite;
        }

        public string CompressionRatio {
            get;
            set;
        }

        public string ArchivePassword {
            get;
            set;
        }

        public string ArchiveName {
            get;
            set;
        }

        public bool Overwrite { get; set; }
    }
}
