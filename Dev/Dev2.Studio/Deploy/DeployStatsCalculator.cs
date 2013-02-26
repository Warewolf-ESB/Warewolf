#region

using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.TO;
using Dev2.Studio.ViewModels.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

#endregion

namespace Dev2.Studio.Deploy
{
    public class DeployStatsCalculator
    {
        /// <summary>
        ///     Calculates the stastics from navigation item view models
        /// </summary>
        public void CalculateStats(IEnumerable<ITreeNode> items,
                                   Dictionary<string, Func<ITreeNode, bool>> predicates,
                                   ObservableCollection<DeployStatsTO> stats, out int deployItemCount)
        {
            deployItemCount = 0;
            var predicateCounts = new Dictionary<string, int>();

            foreach (var predicate in predicates)
            {
                var deployStatsTO = stats.FirstOrDefault(s => s.Name == predicate.Key);

                if (deployStatsTO == null)
                {
                    deployStatsTO = new DeployStatsTO(predicate.Key, "");
                    stats.Add(deployStatsTO);
                }

                predicateCounts.Add(predicate.Key, 0);
            }

            foreach (AbstractTreeViewModel item in items)
            {
                foreach (var predicate in predicates)
                {
                    if (!predicate.Value(item)) continue;

                    predicateCounts[predicate.Key]++;
                    break;
                }
            }

            foreach (var predicateCount in predicateCounts)
            {
                var deployStatsTO = stats.FirstOrDefault(s => s.Name == predicateCount.Key);
                if (deployStatsTO != null)
                {
                    deployStatsTO.Description = predicateCount.Value.ToString(CultureInfo.InvariantCulture);
                    deployItemCount += predicateCount.Value;
                }
            }
        }

        /// <summary>
        ///     The predicate used to detemine if an item should be deployed
        /// </summary>
        public bool SelectForDeployPredicateWithTypeAndCategories(ITreeNode node,
                                                                  ResourceType type, List<string> inclusionCategories,
                                                                  List<string> exclusionCategories)
        {
            var vm = node as ResourceTreeViewModel;
            if (vm == null
                || !vm.IsChecked.HasValue
                || !vm.IsChecked.Value) return false;

            return vm.DataContext != null &&
                   vm.DataContext.ResourceType == type &&
                   (inclusionCategories.Count == 0 ||
                    inclusionCategories.Contains(vm.DataContext.Category)) &&
                   (exclusionCategories.Count == 0 ||
                    !exclusionCategories.Contains(vm.DataContext.Category));
        }

        /// <summary>
        ///     The predicate used to detemine if an item should be deployed
        /// </summary>
        public bool SelectForDeployPredicate(ITreeNode node)
        {
            var vm = node as ResourceTreeViewModel;
            if (vm == null
                || !vm.IsChecked.HasValue
                || !vm.IsChecked.Value) return false;

            return vm.DataContext != null;
        }

        /// <summary>
        ///     The predicate used to detemine which resources are going to be overridden
        /// </summary>
        public bool DeploySummaryPredicateExisting(ITreeNode node,
                                                   IEnvironmentModel targetEnvironment)
        {
            var vm = node as ResourceTreeViewModel;
            if (vm == null
                || !vm.IsChecked.HasValue
                || !vm.IsChecked.Value) return false;

            return vm.DataContext != null &&
                   targetEnvironment != null && targetEnvironment.Resources != null &&
                   targetEnvironment.Resources.All()
                                    .Any(r =>
                                         ResourceModelEqualityComparer
                                             .Current.Equals(r, vm.DataContext));
        }

        /// <summary>
        ///     The predicate used to detemine which resources are going to be overridden
        /// </summary>
        public bool DeploySummaryPredicateNew(ITreeNode node, IEnvironmentModel targetEnvironment)
        {
            var vm = node as ResourceTreeViewModel;
            if (vm == null
                || !vm.IsChecked.HasValue
                || !vm.IsChecked.Value) return false;

            return vm.DataContext != null &&
                   targetEnvironment != null && targetEnvironment.Resources != null &&
                   !targetEnvironment.Resources.All()
                                     .Any(r =>
                                          ResourceModelEqualityComparer
                                              .Current.Equals(r, vm.DataContext));
        }
    }
}