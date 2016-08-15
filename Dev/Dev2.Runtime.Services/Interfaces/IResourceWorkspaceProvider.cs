using System;
using System.Collections.Generic;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Runtime.Interfaces
{
    public interface IResourceWorkspaceProvider
    {
        void LoadWorkspace(Guid workspaceID);
        IList<IResource> LoadWorkspaceViaBuilder(string workspacePath, params string[] folders);

        IList<DuplicateResource> GetDuplicateResources();
    }
}