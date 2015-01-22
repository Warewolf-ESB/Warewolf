using Dev2.Common.Interfaces.Studio.ViewModels;

namespace Dev2.Common.Interfaces.Explorer
{
    public interface IExplorerRepository
    {
        bool Rename(IExplorerItemViewModel vm, string newName);

        bool Move(IExplorerItemViewModel explorerItemViewModel, IExplorerItemViewModel destination);

        bool Delete(IExplorerItemViewModel explorerItemViewModel);
    }
}
