using System;
using Dev2.Data.ServiceModel;
using Dev2.Interfaces;

namespace Dev2.Runtime.Interfaces
{
    public interface IExplorerItemFactory
    {
        IExplorerItem CreateRootExplorerItem(string workSpacePath, Guid workSpaceId);
        IExplorerItem CreateRootExplorerItem(ResourceType type, string workSpacePath, Guid workSpaceId);
    }
}
