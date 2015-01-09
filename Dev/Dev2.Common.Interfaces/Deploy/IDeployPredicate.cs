using System.Collections.Generic;
using Dev2.Common.Interfaces.Studio.ViewModels;

namespace Dev2.Studio.TO
{
    public interface IDeployPredicate
    {
        bool Predicate(IExplorerItemViewModel resource, ICollection<IExplorerItemViewModel> selectedSourceResources,
            ICollection<IExplorerItemViewModel> destinationResources);
        string Name{get;}
    }
}