using Dev2.Data.Interfaces.Enums;

namespace Dev2.Data.Interfaces
{
    public interface IDev2PutRawOperationTO
    {
        WriteType WriteType { get; set; }
        string FileContents { get; set; }
        bool FileContentsAsBase64 { get; set; }
    }
}