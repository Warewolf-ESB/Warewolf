using System;
using System.Collections.Generic;
using System.Linq;
using Dev2;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Studio.TO;

namespace Warewolf.Studio.Models.Deploy
{
    public class DeployStatsProvider : IDeployStatsProvider
    {
        #region Implementation of IDeployStatsProvider

        public IList<IDeployStatsTO> CalculateStats(IList<IExplorerItemViewModel> selectedSourceResources, IList<IExplorerItemViewModel> destinationResources, IList<IDeployPredicate> predicates)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "selectedSourceResources", selectedSourceResources }, { "destinationResources", destinationResources }, { "predicates", predicates } });
            if (predicates.Count == 0)
                return new List<IDeployStatsTO>();
            return predicates.Select(a => CreateDeployStatsTO(selectedSourceResources, destinationResources, a)).ToList();

        }
        public IList<IExplorerItemViewModel> RetrieveMatchingSelecteItems(IList<IExplorerItemViewModel> selectedSourceResources, IList<IExplorerItemViewModel> destinationResources, IList<IDeployPredicate> predicates)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "selectedSourceResources", selectedSourceResources }, { "destinationResources", destinationResources }, { "predicates", predicates } });
            if (predicates.Count == 0)
                return new List<IExplorerItemViewModel>();
            return predicates.SelectMany(GetItemsThatMatchPredicate(selectedSourceResources, destinationResources)).ToList();

        }

        static Func<IDeployPredicate, IEnumerable<IExplorerItemViewModel>> GetItemsThatMatchPredicate(IList<IExplorerItemViewModel> selectedSourceResources, IList<IExplorerItemViewModel> destinationResources)
        {
            return a => selectedSourceResources.Where(b=>a.Predicate(b,selectedSourceResources,destinationResources));
        }

        static IDeployStatsTO CreateDeployStatsTO(ICollection<IExplorerItemViewModel> selectedSourceResources, ICollection<IExplorerItemViewModel> destinationResources, IDeployPredicate a)
        {
            return new DeployStatsTO(a.Name, selectedSourceResources.Count(b => a.Predicate(b, selectedSourceResources, destinationResources)).ToString());
        }

        #endregion

        #region Implementation of IDeployStatsProvider

        public IList<IDeployStatsTO> CalculateStats(ICollection<IExplorerItemViewModel> selectedSourceResources, ICollection<IExplorerItemViewModel> destinationResources, ICollection<IDeployPredicate> predicates)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "selectedSourceResources", selectedSourceResources }, { "destinationResources", destinationResources }, { "predicates", predicates } });
            if (predicates.Count == 0)
                return new List<IDeployStatsTO>();
            return predicates.Select(a => CreateDeployStatsTO(selectedSourceResources, destinationResources, a)).ToList();
        }

        #endregion
    }
}
