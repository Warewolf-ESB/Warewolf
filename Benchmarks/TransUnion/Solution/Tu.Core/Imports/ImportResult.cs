
namespace Tu.Imports
{
    public class ImportResult
    {
        public int SentCount { get; set; }
        public int ReturnedCount { get; set; }
        public int ImportedCount { get; set; }
        public int DifferenceCount { get { return ReturnedCount - ImportedCount; } }
    }
}