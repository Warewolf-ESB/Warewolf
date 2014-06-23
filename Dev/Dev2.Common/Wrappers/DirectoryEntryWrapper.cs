using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using Dev2.Common.Wrappers.Interfaces;

namespace Dev2.Common.Wrappers
{
     
    public class DirectoryEntryWrapper :IDirectoryEntry
    {
        private readonly DirectoryEntry _wrapped;
        private readonly IDirectoryEntryFactory _nativeFactory;

        internal DirectoryEntryWrapper(DirectoryEntry wrapped,IDirectoryEntryFactory directoryEntryFactory)
        {
            _wrapped = wrapped;
            _nativeFactory = directoryEntryFactory;
        }
        public IDirectoryEntries Children
        {
            get { return  _nativeFactory.Create(_wrapped.Children);}
        }

        public string SchemaClassName {
            get { return _wrapped.SchemaClassName; }
        }
        public string Name {
            get { return _wrapped.SchemaClassName; }
        }

        public DirectoryEntry Instance
        {
            get { return _wrapped; }
        }
    }
}
