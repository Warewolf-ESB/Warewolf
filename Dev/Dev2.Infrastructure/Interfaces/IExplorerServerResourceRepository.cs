using Dev2.Data.ServiceModel;

namespace Dev2.Interfaces
{
    public interface IExplorerServerResourceRepository : IExplorerResourceRepository
    {
        IExplorerItem Load(ResourceType type, string filter);
        IExplorerItem Load(string filter);
        void MessageSubscription(IExplorerRepositorySync sync);
    }
}