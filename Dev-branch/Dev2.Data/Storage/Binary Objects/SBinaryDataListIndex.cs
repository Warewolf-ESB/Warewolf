namespace Dev2.Data.Storage
{
    /// <summary>
    /// Used to store index data for the datalist ;)
    /// </summary>
    internal struct SBinaryDataListIndex
    {
        public long position;
        public int length;
        public char[] uniqueKey; // 36 chars for a GUID ;)
       
    }
}
