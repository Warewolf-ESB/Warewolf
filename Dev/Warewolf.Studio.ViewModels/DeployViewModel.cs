using System.Collections.Generic;
using System.Linq;
using Dev2;
using Dev2.Common.Interfaces.Deploy;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Studio.TO;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class DeployViewModel : BindableBase,IDeployViewModel
    {
        // ReSharper disable TooManyDependencies
        public DeployViewModel(IExplorerViewModel source, IExplorerViewModel destination, 
            // ReSharper restore TooManyDependencies
            IDeployStatsProvider statsProvider, ICollection<IDeployPredicate> predicates, 
            IConflictHandlerViewModel conflictHandlerViewModel,
            IDeployModelFactory deployModelFactory)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "source", source }, { "destination", destination }, { "statsProvider", statsProvider }, { "predicates", predicates }, { "conflictHandlerViewModel", conflictHandlerViewModel },{"deployModelFactory",deployModelFactory} });
            ConflictHandlerViewModel = conflictHandlerViewModel;
            DeployModelFactory = deployModelFactory;
            Predicates = predicates;
            StatsProvider = statsProvider;
            Destination = destination;
            Source = source;


        }



        #region Implementation of IDeployViewModel

        /// <summary>
        /// Source Server tree.
        /// </summary>
        public IExplorerViewModel Source { get; private set; }
        /// <summary>
        /// Destination explorere tree
        /// </summary>
        public IExplorerViewModel Destination { get; private set; }
        /// <summary>
        /// Provides stats like conflicts, new, types etc
        /// </summary>
        public IDeployStatsProvider StatsProvider { get; private set; }
        /// <summary>
        ///source server selected in explorer tree 
        /// </summary>
        public IDeployModel SelectedSourceModel { get; set; }
        /// <summary>
        /// destination server  selected in explorer tree
        /// </summary>
        public IDeployModel SelectedDestinationModel { get; set; }
        /// <summary>
        /// Stats for the purpose of binding
        /// </summary>
        public ICollection<IDeployStatsTO> Stats { get; set; }
        /// <summary>
        /// Predicates to apply
        /// </summary>
        public ICollection<IDeployPredicate> Predicates { get; private set; }
        /// <summary>
        /// Conflict handler popup
        /// </summary>
        public IConflictHandlerViewModel ConflictHandlerViewModel { get; private set; }
        public IDeployModelFactory DeployModelFactory { get; private set; }

        public void CalculateStats()
        {
           Stats = StatsProvider.CalculateStats(Source.SelectedEnvironment.ExplorerItemViewModels,Destination.SelectedEnvironment.ExplorerItemViewModels,Predicates);
        }

        public void Deploy()
        {
            SelectedDestinationModel = DeployModelFactory.Create(Destination.SelectedServer);
            SelectedSourceModel = DeployModelFactory.Create(Source.SelectedServer);
            var itemsTodeploy = Source.FindItems(a => a.Checked && (a.Children == null || a.Children.Count == 0));
            if (HandleConflicts())
                foreach (var explorerItemModel in itemsTodeploy)
                {
                    var item = Source.SelectedServer.Load().FirstOrDefault(a => a.ResourceID == explorerItemModel.ResourceId);
                    if (SelectedDestinationModel.CanDeploy(item))
                        SelectedDestinationModel.Deploy(item);
                }
        }

        /// <summary>
        /// delegates showing dependancies to another class
        /// </summary>
        /// <param name="item"></param>
        public void SelectDependencies(IExplorerItemViewModel item)
        {
            var dependencies = SelectedSourceModel.GetDependancies(item.ResourceId);
            foreach(var dependency in dependencies)
            {
                Destination.SelectedEnvironment.SetItemCheckedState(dependency.ResourceID,true);
            }
           //todo: when refactoring the dependency tree 
        }

        /// <summary>
        /// select an item the explorer for deploy. This will clear other selected items and aslo set the connected source server to the selected item server
        /// </summary>
        /// <param name="item"></param>
        public void SelectSourceItem(IExplorerItemViewModel item)
        {
        }

        bool HandleConflicts()
        {
            return ConflictHandlerViewModel.HandleConflicts(Source.SelectedEnvironment,Destination.SelectedEnvironment);
        }


        #endregion
    }
}
