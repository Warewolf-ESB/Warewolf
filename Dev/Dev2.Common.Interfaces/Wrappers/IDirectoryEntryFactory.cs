using System.DirectoryServices;

namespace Dev2.Common.Interfaces.Wrappers
{
    public interface IDirectoryEntryFactory
    {
        IDirectoryEntry Create(string path);
        IDirectoryEntries Create(DirectoryEntries directoryEntries);
    }
}
