using System.Collections.Generic;

namespace Dev2.Runtime.Interfaces
{
    public interface IResourceSyncProvider
    {
        void SyncTo(string sourceWorkspacePath, string targetWorkspacePath);
        void SyncTo(string sourceWorkspacePath, string targetWorkspacePath, bool overwrite);
        void SyncTo(string sourceWorkspacePath, string targetWorkspacePath, bool overwrite, bool delete);
        void SyncTo(string sourceWorkspacePath, string targetWorkspacePath, bool overwrite, bool delete, IList<string> filesToIgnore);
    }
}