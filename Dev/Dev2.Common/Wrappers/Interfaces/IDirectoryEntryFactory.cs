using System.DirectoryServices;

namespace Dev2.Common.Wrappers.Interfaces
{
    public interface IDirectoryEntryFactory
    {
        IDirectoryEntry Create(string path);
        IDirectoryEntries Create(DirectoryEntries directoryEntries);
    }
}
