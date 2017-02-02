using System.Collections.Generic;

namespace Dev2.Runtime.Interfaces
{
    public interface IResourceSyncProvider
    {
        void SyncTo(string sourceWorkspacePath, string targetWorkspacePath, bool overwrite = true, bool delete = true, IList<string> filesToIgnore = null);
    }
}