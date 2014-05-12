using System.DirectoryServices;
using Dev2.Common.Wrappers.Interfaces;

namespace Dev2.Common.Wrappers
{
    public class DirectoryEntriesWrapper : IDirectoryEntries
    {
        private readonly DirectoryEntries _directoryEntries;

        internal DirectoryEntriesWrapper(DirectoryEntries directory)
        {
            _directoryEntries = directory;
        }
        public System.Collections.IEnumerator GetEnumerator()
        {
            return _directoryEntries.GetEnumerator();
        }

        public DirectoryEntries Instance {
            get { return _directoryEntries; }
        }
    }
}