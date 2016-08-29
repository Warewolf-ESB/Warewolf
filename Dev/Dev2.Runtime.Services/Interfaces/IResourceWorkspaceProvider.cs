using System;
using System.Collections.Generic;
using Dev2.Common;

namespace Dev2.Runtime.Interfaces
{
    public interface IResourceWorkspaceProvider
    {
        void LoadWorkspace(Guid workspaceID);
        IList<DuplicateResource> GetDuplicateResources();
    }
}