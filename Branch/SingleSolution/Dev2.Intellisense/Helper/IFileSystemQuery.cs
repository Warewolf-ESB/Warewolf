using System.Collections.Generic;

namespace Dev2.Intellisense.Helper
{
    public interface IFileSystemQuery
    {
        List<string> QueryCollection { get; }
        void QueryList(string searchPath);
    }
}