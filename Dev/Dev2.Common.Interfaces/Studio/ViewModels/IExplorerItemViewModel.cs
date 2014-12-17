using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Studio.ViewModels
{
    public interface IExplorerItemViewModel
    {
        string ResourceName { get; set; }
        ICollection<IExplorerItemViewModel> Children { get; set; } 
    }
}