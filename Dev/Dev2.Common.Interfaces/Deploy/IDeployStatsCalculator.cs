using System.Collections.Generic;
using Dev2.Common.Interfaces.Studio.ViewModels;

namespace Dev2.Studio.TO
{
    public interface IDeployStatsProvider
    {
        IList<IDeployStatsTO> CalculateStats(ICollection<IExplorerItemViewModel> selectedSourceResources,
            ICollection<IExplorerItemViewModel> destinationResources,
            ICollection<IDeployPredicate> predicates);
    }
}   