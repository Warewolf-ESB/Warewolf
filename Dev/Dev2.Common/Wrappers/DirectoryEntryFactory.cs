using Dev2.Common.Interfaces.Wrappers;
using System;
using System.DirectoryServices;

namespace Dev2.Common.Wrappers
{
    public class DirectoryEntryFactory : IDirectoryEntryFactory, IDisposable
    {
        protected IDirectoryEntry _directoryEntry;
        protected IDirectoryEntries _directoryEnties;
        public IDirectoryEntry Create(string path)
        {
            return _directoryEntry = new Dev2DirectoryEntry(path);
        }
        public IDirectoryEntry Instance => _directoryEntry;
        public void Dispose()
        {
            Instance.Dispose();
        }

        public IDirectoryEntry Create<T>(T member)
        {
            return new Dev2DirectoryEntry(new DirectoryEntry(member));
        }
    }
}
