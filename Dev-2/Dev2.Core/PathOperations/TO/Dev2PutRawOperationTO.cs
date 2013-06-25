using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.PathOperations {
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide a TO for RawPut operations
    /// </summary>
    public class Dev2PutRawOperationTO : IFileWrite, IPathOverwrite{

        internal Dev2PutRawOperationTO(bool append, string contents, bool overwrite) {
            Append = append;
            FileContents = contents;
            Overwrite = overwrite;
        }

        public bool Append {
            get;
            set;
        }

        public string FileContents {
            get;
            set;
        }

        public bool Overwrite {
            get;
            set;
        }
    }
}
