using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Common.Interfaces.Studio.ViewModels
{
    public interface IExplorerItemViewModel
    {
        string ResourceName { get; set; }
        ICollection<IExplorerItemViewModel> Children { get; set; }
        bool Checked { get; set; }
        Guid ResourceId { get; set; }
        ResourceType ResourceType { get; set; }
    }
}