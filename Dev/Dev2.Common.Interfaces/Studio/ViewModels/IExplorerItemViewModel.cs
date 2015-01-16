using System;
using System.Collections.Generic;
using System.Windows.Input;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Common.Interfaces.Studio.ViewModels
{
    public interface IExplorerItemViewModel
    {
        string ResourceName { get; set; }
        ICollection<IExplorerItemViewModel> Children { get; set; }
<<<<<<< HEAD
        bool Checked { get; set; }
        Guid ResourceId { get; set; }
        ResourceType ResourceType { get; set; }
        ICommand OpenCommand { get; set; }
=======
        bool IsVisible { get; set; }
>>>>>>> 31df4453e6dab038a2c50d93aa17db590e988de6
    }
}