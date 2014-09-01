
using Dev2.Common.Interfaces.Explorer;

namespace Dev2.Common.Interfaces.Infrastructure
{
    public interface IExplorerRepositorySync
    {
        void AddItemMessage(IExplorerItem itemToRename);
    }
}