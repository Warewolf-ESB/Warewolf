using Dev2.Data.Storage.ProtocolBuffers;

namespace Dev2.Data.Storage
{
    public struct IndexBasedBinaryDataListRow
    {
        public BinaryDataListRow Row { get; set; }
        public int Index { get; set; }
    }
}