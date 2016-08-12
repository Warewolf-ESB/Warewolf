using System.Collections.Generic;
using System.IO;

namespace Dev2.Common.Interfaces
{
    public interface IResourceHolder
    {
        List<FileInfo> GetFilesList();
    }
}