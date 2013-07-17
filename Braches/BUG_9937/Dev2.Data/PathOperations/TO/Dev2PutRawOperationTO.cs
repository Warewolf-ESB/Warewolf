namespace Dev2.PathOperations {
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide a TO for RawPut operations
    /// </summary>
    public class Dev2PutRawOperationTO : IFileWrite, IPathOverwrite{

        public Dev2PutRawOperationTO(bool append, string contents, bool overwrite) {
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
