using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Versioning;

namespace Dev2.Common.Interfaces
{
    public interface IVersionStrategy
    {
        IVersionInfo GetNextVersion(IResource newResource, IResource oldresource, string userName , string reason );
        IVersionInfo GetCurrentVersion(IResource newResource, IResource oldresource, string userName, string reason);
        IVersionInfo GetCurrentVersion(IResource newResource, IVersionInfo oldresource, string userName, string reason);
    }
}