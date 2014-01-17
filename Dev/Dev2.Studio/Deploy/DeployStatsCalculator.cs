using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.TO;
using Dev2.Studio.ViewModels.Navigation;
using Dev2.ViewModels.Deploy;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Deploy
{
    public class DeployStatsCalculator : IDeployStatsCalculator
    {
        public ObservableCollection<DeployDialogTO> ConflictingResources { get; set; }

        public DeployStatsCalculator()
        {
            ConflictingResources = new ObservableCollection<DeployDialogTO>();
        }

        /// <summary>
        ///     Calculates the stastics from navigation item view models
        /// </summary>
        public void CalculateStats(IEnumerable<ITreeNode> items,
                                   Dictionary<string, Func<ITreeNode, bool>> predicates,
                                   ObservableCollection<DeployStatsTO> stats, out int deployItemCount)
        {
            deployItemCount = 0;
            var predicateCounts = new Dictionary<string, int>();

            foreach(var predicate in predicates)
            {
                var deployStatsTO = stats.FirstOrDefault(s => s.Name == predicate.Key);

                if(deployStatsTO == null)
                {
                    deployStatsTO = new DeployStatsTO(predicate.Key, "");
                    stats.Add(deployStatsTO);
                }

                predicateCounts.Add(predicate.Key, 0);
            }

            foreach(var treeNode in items)
            {
                var item = (AbstractTreeViewModel)treeNode;
                foreach(var predicate in predicates)
                {
                    if(!predicate.Value(item)) continue;

                    predicateCounts[predicate.Key]++;
                    break;
                }
            }

            foreach(var predicateCount in predicateCounts)
            {
                var deployStatsTO = stats.FirstOrDefault(s => s.Name == predicateCount.Key);
                if(deployStatsTO != null)
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
            if(vm == null
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
            if(vm == null
                || !vm.IsChecked.HasValue
                || !vm.IsChecked.Value) return false;

            return vm.DataContext != null;
        }

        /// <summary>
        ///     The predicate used to detemine which resources are going to be overridden
        /// </summary>
        public bool DeploySummaryPredicateExisting(ITreeNode node,
                                                   NavigationViewModel targetNavViewModel)
        {
            var vm = node as ResourceTreeViewModel;
            if(vm == null
                || !vm.IsChecked.HasValue
                || !vm.IsChecked.Value) return false;

            if(targetNavViewModel != null && targetNavViewModel.Environments.Any())
            {
                IEnvironmentModel targetEnvironment = targetNavViewModel.Environments[0];
                if(vm.DataContext == null
                    || targetEnvironment == null
                    || targetEnvironment.ResourceRepository == null) return false;


                var conflictingItems = targetEnvironment.ResourceRepository.All()
                                        .Where(r => ResourceModelEqualityComparer
                                        .Current.Equals(r, vm.DataContext))
                                        .Select(r => new Tuple<IResourceModel, IContextualResourceModel, ITreeNode, NavigationViewModel>(r, vm.DataContext, node, targetNavViewModel))
                                        .ToList();

                conflictingItems.ForEach(AddConflictingResources);

                return targetEnvironment.ResourceRepository.All()
                                        .Any(r =>
                                             ResourceModelEqualityComparer
                                                 .Current.Equals(r, vm.DataContext));
            }
            return false;
        }

        /// <summary>
        ///     Add items that are found to be in conflicts
        /// </summary>
        /// <param name="resourceInConflict"></param>
        void AddConflictingResources(Tuple<IResourceModel, IContextualResourceModel, ITreeNode, NavigationViewModel> resourceInConflict)
        {
            if(ConflictingResources.Any(c => c.SourceName == resourceInConflict.Item1.ResourceName)) return;
            ConflictingResources.Add(new DeployDialogTO(ConflictingResources.Count + 1, resourceInConflict.Item2.ResourceName, resourceInConflict.Item1.ResourceName, resourceInConflict.Item1));
            resourceInConflict.Item3.IsOverwrite = true;
            resourceInConflict.Item4.SetNodeOverwrite(resourceInConflict.Item1 as IContextualResourceModel, true);
        }

        /// <summary>
        ///     The predicate used to detemine which resources are going to be overridden
        /// </summary>
        public bool DeploySummaryPredicateNew(ITreeNode node, IEnvironmentModel targetEnvironment)
        {
            var vm = node as ResourceTreeViewModel;
            if(vm == null
                || !vm.IsChecked.HasValue
                || !vm.IsChecked.Value) return false;

            return vm.DataContext != null &&
                   targetEnvironment != null && targetEnvironment.ResourceRepository != null &&
                   !targetEnvironment.ResourceRepository.All()
                                     .Any(r =>
                                          ResourceModelEqualityComparer
                                              .Current.Equals(r, vm.DataContext));
        }
    }
}