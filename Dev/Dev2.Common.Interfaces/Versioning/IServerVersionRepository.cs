using System;
using Dev2.Common.Interfaces.Data;


namespace Dev2.Common.Interfaces.Versioning
{
    public interface IServerVersionRepository : IVersionRepository
    {
        void StoreVersion(IResource resource, string userName, string reason,Guid workSpaceId);
    }
}