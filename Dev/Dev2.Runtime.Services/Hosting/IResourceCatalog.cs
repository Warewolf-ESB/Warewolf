

using System;

namespace Dev2.Runtime.Hosting
{
    public interface IResourceCatalog
    {
        void Load(Guid workspaceID);
    }
}