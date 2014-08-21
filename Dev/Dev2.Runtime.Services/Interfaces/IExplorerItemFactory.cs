using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using System;

namespace Dev2.Runtime.Interfaces
{
    public interface IExplorerItemFactory
    {
        IExplorerItem CreateRootExplorerItem(string workSpacePath, Guid workSpaceId);
        IExplorerItem CreateRootExplorerItem(ResourceType type, string workSpacePath, Guid workSpaceId);
    }
}
