using System;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;

namespace Dev2.Common.Interfaces.Runtime
{
    public interface IExplorerItemFactory
    {
        IExplorerItem CreateRootExplorerItem(string workSpacePath, Guid workSpaceId);
        IExplorerItem CreateRootExplorerItem(ResourceType type, string workSpacePath, Guid workSpaceId);
    }
}
