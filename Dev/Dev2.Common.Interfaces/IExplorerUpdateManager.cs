using System;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Infrastructure;

namespace Dev2.Common.Interfaces
{
    public interface IExplorerUpdateManager
    {
        void AddFolder(string path, string name, Guid id);
        void DeleteFolder(string path);
        void DeleteResource(Guid id);
        void Rename(Guid id, string newName);
        void RenameFolder(string path, string newName);
        Task<IExplorerRepositoryResult> MoveItem(Guid sourceId, string destinationPath,string itemPath);

    }
}