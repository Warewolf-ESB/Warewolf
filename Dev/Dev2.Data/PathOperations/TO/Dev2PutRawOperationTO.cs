namespace Dev2.PathOperations
{
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide a TO for RawPut operations
    /// </summary>
    public class Dev2PutRawOperationTO
    {

        public Dev2PutRawOperationTO(WriteType writeType, string contents)
        {
            WriteType = writeType;
            FileContents = contents;
        }

        public WriteType WriteType { get; set; }

        public string FileContents
        {
            get;
            set;
        }


    }
}
