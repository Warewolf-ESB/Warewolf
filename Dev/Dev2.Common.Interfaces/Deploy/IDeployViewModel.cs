using System.Collections.Generic;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Studio.TO;

namespace Dev2.Common.Interfaces.Deploy
{

    /// <summary>
    /// Wpf Bindings and actions for deploy
    /// </summary>
    public interface IDeployViewModel
    {
        /// <summary>
        /// Source Server tree.
        /// </summary>
        IExplorerViewModel Source { get; }
        /// <summary>
        /// Destination explorere tree
        /// </summary>
        IExplorerViewModel Destination { get;}

        /// <summary>
        /// Provides stats like conflicts, new, types etc
        /// </summary>
        IDeployStatsProvider StatsProvider { get; }
        /// <summary>
        ///source server selected in explorer tree 
        /// </summary>
        IDeployModel SelectedSourceModel { get; set; }

        /// <summary>
        /// destination server  selected in explorer tree
        /// </summary>
        IDeployModel SelectedDestinationModel { get; set; }
        /// <summary>
        /// Stats for the purpose of binding
        /// </summary>
        ICollection<IDeployStatsTO> Stats { get; set; }
        /// <summary>
        /// Predicates to apply
        /// </summary>
        ICollection<IDeployPredicate> Predicates { get; }

        IConflictHandlerViewModel ConflictHandlerViewModel { get; }
        /// <summary>
        /// Calculate stats
        /// </summary>
        void CalculateStats();
        /// <summary>
        /// deploy selected items
        /// </summary>
        void Deploy();

        /// <summary>
        /// delgates showing dependancies to another class
        /// </summary>
        /// <param name="item"></param>
        void SelectDependencies(IExplorerItemViewModel item);

        /// <summary>
        /// select an item the explorer for deploy. This will clear other selected items and aslo set the connected source server to the selected item server
        /// </summary>
        /// <param name="item"></param>
        void SelectSourceItem(IExplorerItemViewModel item);

    }
}