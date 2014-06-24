using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using Dev2.Common.Wrappers.Interfaces;

namespace Dev2.Common.Wrappers
{
     [ExcludeFromCodeCoverage]
    public class DirectoryEntryFactory : IDirectoryEntryFactory
    {
        public IDirectoryEntry Create(string path)
        {
            return new DirectoryEntryWrapper(new DirectoryEntry(  path),this);
        }

        public IDirectoryEntries Create(DirectoryEntries directoryEntries)
        {
            return new DirectoryEntriesWrapper(directoryEntries);
        }
    }
}
