using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;

namespace Dev2.Interfaces
{
    public interface IExplorerServerResourceRepository : IExplorerResourceRepository
    {
        IExplorerItem Load(ResourceType type, string filter);
        IExplorerItem Load(string filter);
        void MessageSubscription(IExplorerRepositorySync sync);
    }
}