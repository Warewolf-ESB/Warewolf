using System.Collections.Generic;

namespace Dev2.Studio.InterfaceImplementors
{
    public interface IFileSystemQuery
    {
        List<string> QueryCollection { get; }
        void QueryList(string searchPath);
    }
}