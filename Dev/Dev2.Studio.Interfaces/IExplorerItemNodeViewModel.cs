using System.Collections.Generic;

namespace Dev2.Studio.Interfaces
{
    public interface IExplorerItemNodeViewModel : IExplorerItemViewModel
    {
        IExplorerItemNodeViewModel Self { get; set; }
        int Weight { get; set; }
        ICollection<IExplorerItemNodeViewModel> AsNodeList();
        bool IsMainNode { get; set; }
        bool IsNotMainNode { get; set; }
    }
}
