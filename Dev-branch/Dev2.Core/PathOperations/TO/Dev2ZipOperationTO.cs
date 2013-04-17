using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.PathOperations {
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide the Zip methods its arguments
    /// </summary>
    public class Dev2ZipOperationTO : IZip {

        public Dev2ZipOperationTO(string ratio, string passwd, string name) {
            CompressionRatio = ratio;
            ArchivePassword = passwd;
            ArchiveName = name;
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
    }
}
