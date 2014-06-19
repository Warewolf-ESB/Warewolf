using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dev2.Data.ServiceModel;
using Dev2.Models;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.TO;
using Dev2.ViewModels.Deploy;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Deploy
{
    public interface IDeployStatsCalculator
    {
        /// <summary>
        /// List of resources in conflict ITEM1 : source and ITEM2 : destination
        /// </summary>
        ObservableCollection<DeployDialogTO> ConflictingResources { get; set; }

        /// <summary>
        ///     Calculates the stastics from navigation item view models
        /// </summary>
        void CalculateStats(IEnumerable<ExplorerItemModel> items,
                            Dictionary<string, Func<ExplorerItemModel, bool>> predicates,
                            ObservableCollection<DeployStatsTO> stats, out int deployItemCount);

        /// <summary>
        ///     The predicate used to detemine if an item should be deployed
        /// </summary>
        bool SelectForDeployPredicateWithTypeAndCategories(ExplorerItemModel node,
                                                           ResourceType type, List<string> inclusionCategories,
                                                           List<string> exclusionCategories);

        /// <summary>
        ///     The predicate used to detemine if an item should be deployed
        /// </summary>
        bool SelectForDeployPredicate(ExplorerItemModel node);

        /// <summary>
        ///     The predicate used to detemine which resources are going to be overridden
        /// </summary>
        bool DeploySummaryPredicateExisting(ExplorerItemModel node,
                                            DeployNavigationViewModel targetNavViewMode);

        /// <summary>
        ///     The predicate used to detemine which resources are going to be overridden
        /// </summary>
        bool DeploySummaryPredicateNew(ExplorerItemModel node, IEnvironmentModel targetEnvironment);
    }
}