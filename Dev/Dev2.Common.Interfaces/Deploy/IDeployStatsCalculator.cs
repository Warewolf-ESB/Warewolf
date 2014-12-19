using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Studio.TO
{
    public interface IDeployStatsProvider
    {
        IList<IDeployStatsTO> CalculateStats(IList<IResource> selectedSourceResources, 
            IList<IResource> destinationResources,
            IList<IDeployPredicate> predicates);
    }
}   