using System.Collections.Generic;
using Dev2.Common.Interfaces.Studio.ViewModels;

namespace Warewolf.Studio.ViewModels.DummyModels
{
    public class DummyExplorerViewModel:ExplorerViewModel
    {
        public DummyExplorerViewModel()
        {
            ExplorerItems = new List<IExplorerItemViewModel>();
            ExplorerItems.Add(new ExplorerItemViewModel{ResourceName = "Test1"});
            ExplorerItems.Add(new ExplorerItemViewModel{ResourceName = "Test2"});
            ExplorerItems.Add(new ExplorerItemViewModel{ResourceName = "Test3"});
            ExplorerItems.Add(new ExplorerItemViewModel{ResourceName = "Test4"});
        }
    }
}
