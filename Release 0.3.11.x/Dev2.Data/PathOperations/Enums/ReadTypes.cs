using System.ComponentModel;

namespace Dev2.PathOperations
{
    public enum ReadTypes
    {
        [Description("Files")]
        Files,
        [Description("Folders")]
        Folders,
        [Description("Files & Folders")]
        FilesAndFolders
    }
}
