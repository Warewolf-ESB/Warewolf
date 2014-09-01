using System;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Hosting;

namespace Dev2.Common.Interfaces.Infrastructure
{
    public interface IExplorerResourceRepository
    {
        IExplorerItem Load(Guid workSpaceId);
        IExplorerItem Load(ResourceType type, Guid workSpaceId);
        IExplorerRepositoryResult RenameItem(IExplorerItem itemToRename, string newName, Guid workSpaceId);
        IExplorerRepositoryResult RenameFolder(string path, string newName, Guid workSpaceId);
        IExplorerRepositoryResult DeleteItem(IExplorerItem itemToRename, Guid workSpaceId);
        IExplorerRepositoryResult AddItem(IExplorerItem itemToRename, Guid workSpaceId);
    }

    public interface IExplorerRepositoryResult
    {
        ExecStatus Status { get; }

        string Message { get; }


    }
}
