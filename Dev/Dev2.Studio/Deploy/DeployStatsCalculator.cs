
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.TO;
using Dev2.ViewModels.Deploy;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Deploy
// ReSharper restore CheckNamespace
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
        public void CalculateStats(IEnumerable<IExplorerItemModel> items,
                                   Dictionary<string, Func<IExplorerItemModel, bool>> predicates,
                                   ObservableCollection<DeployStatsTO> stats, out int deployItemCount)
        {
            deployItemCount = 0;
            var predicateCounts = new Dictionary<string, int>();

            foreach(var predicate in predicates)
            {
                var deployStatsTo = stats.FirstOrDefault(s => s.Name == predicate.Key);

                if(deployStatsTo == null)
                {
                    deployStatsTo = new DeployStatsTO(predicate.Key, "");
                    stats.Add(deployStatsTo);
                }

                predicateCounts.Add(predicate.Key, 0);
            }

            foreach(var treeNode in items)
            {
                var item = treeNode;
                foreach(var predicate in predicates)
                {
                    if(!predicate.Value(item)) continue;

                    predicateCounts[predicate.Key]++;
                    break;
                }
            }

            foreach(var predicateCount in predicateCounts)
            {
                var deployStatsTo = stats.FirstOrDefault(s => s.Name == predicateCount.Key);
                if(deployStatsTo != null)
                {
                    deployStatsTo.Description = predicateCount.Value.ToString(CultureInfo.InvariantCulture);
                    deployItemCount += predicateCount.Value;
                }
            }
        }

        /// <summary>
        ///     The predicate used to detemine if an item should be deployed
        /// </summary>
        public bool SelectForDeployPredicateWithTypeAndCategories(IExplorerItemModel node,
                                                                  ResourceType type, List<string> inclusionCategories,
                                                                  List<string> exclusionCategories)
        {
            var vm = node;
            if(vm == null || !vm.IsChecked.GetValueOrDefault(false)) return false;

            return (type.HasFlag(vm.ResourceType))
                    && (
                        inclusionCategories.Count == 0
                        || inclusionCategories.Contains(vm.ResourcePath)
                       )
                    && (
                        exclusionCategories.Count == 0
                        || !exclusionCategories.Contains(vm.ResourcePath)
                       );
        }

        /// <summary>
        ///     The predicate used to detemine if an item should be deployed
        /// </summary>
        public bool SelectForDeployPredicate(IExplorerItemModel node)
        {
            var vm = node;
            return vm != null && vm.IsChecked.GetValueOrDefault(false) && vm.ResourceType != ResourceType.Folder && vm.ResourceType != ResourceType.Server;
        }

        /// <summary>
        ///     The predicate used to detemine which resources are going to be overridden
        /// </summary>
        public bool DeploySummaryPredicateExisting(IExplorerItemModel node,
                                                   DeployNavigationViewModel targetNavViewModel)
        {
            var vm = node;
            if(vm == null || !vm.IsChecked.GetValueOrDefault(false)) return false;

            if(targetNavViewModel != null && targetNavViewModel.Environment != null)
            {
                IEnvironmentModel targetEnvironment = targetNavViewModel.Environment;
                if(targetEnvironment == null || targetEnvironment.ResourceRepository == null) return false;


                var conflictingItems = targetEnvironment.ResourceRepository.All()
                                        .Where(r => r.ID == vm.ResourceId)
                                        .Select(r => new Tuple<IResourceModel, IContextualResourceModel, IExplorerItemModel, DeployNavigationViewModel>(r, r as IContextualResourceModel, node, targetNavViewModel))
                                        .ToList();

                conflictingItems.ForEach(AddConflictingResources);

                return targetEnvironment.ResourceRepository.All()
                                        .Any(r => r.ID == vm.ResourceId);
            }
            return false;
        }

        /// <summary>
        ///     Add items that are found to be in conflicts
        /// </summary>
        /// <param name="resourceInConflict"></param>
        void AddConflictingResources(Tuple<IResourceModel, IContextualResourceModel, IExplorerItemModel, DeployNavigationViewModel> resourceInConflict)
        {
            if(ConflictingResources.Any(c => c.SourceName == resourceInConflict.Item1.ResourceName)) return;
            ConflictingResources.Add(new DeployDialogTO(ConflictingResources.Count + 1, resourceInConflict.Item2.ResourceName, resourceInConflict.Item1.ResourceName, resourceInConflict.Item1));
            resourceInConflict.Item3.IsOverwrite = true;
            resourceInConflict.Item3.Parent.IsOverwrite = true;
            resourceInConflict.Item4.SetNodeOverwrite(resourceInConflict.Item1 as IContextualResourceModel, true);
        }

        /// <summary>
        ///     The predicate used to detemine which resources are going to be overridden
        /// </summary>
        public bool DeploySummaryPredicateNew(IExplorerItemModel node, IEnvironmentModel targetEnvironment)
        {
            var vm = node;
            if(vm == null || !vm.IsChecked.GetValueOrDefault(false)) return false;
            if (node.ResourceType == ResourceType.Server || node.ResourceType == ResourceType.Folder)
                return false;
            return targetEnvironment != null && targetEnvironment.ResourceRepository != null &&
                   targetEnvironment.ResourceRepository.All().All(r => r.ID != vm.ResourceId);
        }
    }
}
