
namespace Dev2.PathOperations {
    
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide a TO for Put operations
    /// </summary>
    public class Dev2PutOperationTO : IFileWrite, IPathOverwrite {

        internal Dev2PutOperationTO(bool append, string contents, bool overwrite) {
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
