using Dev2.Common.Interfaces.Wrappers;
using System.DirectoryServices;

namespace Dev2.Common.Wrappers
{
    public class DirectoryEntryFactory : IDirectoryEntryFactory
    {
        public IDirectoryEntry Create(string path)
        {
            return new Dev2DirectoryEntry(path);
        }

        public IDirectoryEntry Create<T>(T member)
        {
            return new Dev2DirectoryEntry(new DirectoryEntry(member));
        }
    }
}
