using System.Collections.Generic;
using Dev2.Studio.Core.Interfaces;
using Dev2.Workspaces;

namespace Dev2.Studio.Core.Workspaces
{
    public interface IWorkspaceItemRepository
    {
        IList<IWorkspaceItem> WorkspaceItems { get; }
        void Write();
        void AddWorkspaceItem(IContextualResourceModel model);
        string UpdateWorkspaceItem(IContextualResourceModel resource);
        void Remove(IContextualResourceModel resourceModel);
    }
}