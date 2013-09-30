namespace Dev2.Data.Storage.Binary_Objects
{
    /// <summary>
    /// Store the key alias on the mmf stack ;)
    /// Each key is 64 bytes long
    /// PBI : 10440
    /// </summary>
    public struct SBinaryKeyAlias
    {
        public byte[] guid; // 32 spaces for guid
        public byte[] colName; // 32 spaces for column name
    }
}
