using Technical_Assesment.Value_Objects;

namespace Technical_Assesment.File_Import
{
    public interface IRecordImporter
    {
        BTree<T> ImportRecords<T>(string filePath, char token, bool skipHeader, ImportBuilder<T> voBuilder, BTree<T> seed);
    }
}
