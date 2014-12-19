using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Studio.ViewModels
{
    public interface IEnvironmentViewModel
    {
        IServer Server { get; set; }
        ICollection<IExplorerItemViewModel> ExplorerItemViewModels { get; set; }
        string DisplayName { get; set; }
        bool IsConnected { get; }
        bool IsLoaded { get; }
        void Connect();
        void Load();
        void Filter(string filter);
    }
}