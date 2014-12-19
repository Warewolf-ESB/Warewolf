using System;
using System.Collections.Generic;
using System.Linq;
using Dev2;
using Dev2.Common.Interfaces.Data;
using Dev2.Studio.TO;

namespace Warewolf.Studio.Models
{
    public class DeployStatsProvider : IDeployStatsProvider
    {
        #region Implementation of IDeployStatsProvider

        public IList<IDeployStatsTO> CalculateStats(IList<IResource> selectedSourceResources, IList<IResource> destinationResources, IList<IDeployPredicate> predicates)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "selectedSourceResources", selectedSourceResources }, { "destinationResources", destinationResources }, { "predicates", predicates } });
            if (predicates.Count == 0)
                return new List<IDeployStatsTO>();
            return predicates.Select(a => CreateDeployStatsTO(selectedSourceResources, destinationResources, a)).ToList();

        }
        public IList<IResource> RetrieveMatchingSelecteItems(IList<IResource> selectedSourceResources, IList<IResource> destinationResources, IList<IDeployPredicate> predicates)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "selectedSourceResources", selectedSourceResources }, { "destinationResources", destinationResources }, { "predicates", predicates } });
            if (predicates.Count == 0)
                return new List<IResource>();
            return predicates.SelectMany(GetItemsThatMatchPredicate(selectedSourceResources, destinationResources)).ToList();

        }

        static Func<IDeployPredicate, IEnumerable<IResource>> GetItemsThatMatchPredicate(IList<IResource> selectedSourceResources, IList<IResource> destinationResources)
        {
            return a => selectedSourceResources.Where(b=>a.Predicate(b,selectedSourceResources,destinationResources));
        }

        static IDeployStatsTO CreateDeployStatsTO(IList<IResource> selectedSourceResources, IList<IResource> destinationResources, IDeployPredicate a)
        {
            return new DeployStatsTO(a.Name, selectedSourceResources.Count(b => a.Predicate(b, selectedSourceResources, destinationResources)).ToString());
        }

        #endregion
    }
}
