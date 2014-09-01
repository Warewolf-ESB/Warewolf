using System.Collections;
using System.DirectoryServices;

namespace Dev2.Common.Interfaces.Wrappers
{
    public interface IDirectoryEntry : IWrappedObject<DirectoryEntry>
    {
        IDirectoryEntries Children { get; }
        string SchemaClassName { get;  }
        string Name { get;  }
    }

    public interface IDirectoryEntries : IEnumerable, IWrappedObject<DirectoryEntries>  
    {

    }
}
