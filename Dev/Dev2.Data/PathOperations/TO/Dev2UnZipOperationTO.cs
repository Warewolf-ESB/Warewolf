using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.PathOperations {
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide the UnZip operation its args
    /// </summary>
    public class Dev2UnZipOperationTO : IUnZip {

        public Dev2UnZipOperationTO(string passwd, bool overwrite) {
            ArchivePassword = passwd;
            Overwrite = overwrite;
        }

        public string ArchivePassword {
            get;
            set;
        }

        public bool Overwrite
        {
            get;
            set;
        }
    }
}
