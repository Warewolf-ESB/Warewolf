using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Deploy;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class DeployStatsViewerViewModel : BindableBase, IDeployStatsViewerViewModel
    {
        readonly IDeployDestinationExplorerViewModel _destination;
        //readonly IExplorerViewModel _destination;
        int _connectors;
        int _services;
        int _sources;
        int _unknown;
        int _newResources;
        int _overrides;
        string _status;
        public string RenameErrors { get; private set; }
        List<Conflict> _conflicts;
        IEnumerable<IExplorerTreeItem> _new;
        IList<IExplorerTreeItem> _items;
        private ICollection<IExplorerItemViewModel> _destinationItems;

        public DeployStatsViewerViewModel(IDeployDestinationExplorerViewModel destination)
        {
            VerifyArgument.IsNotNull(@"destination", destination);
            _destination = destination;            
            Status = @"";
        }

        public DeployStatsViewerViewModel(IList<IExplorerTreeItem> items, IDeployDestinationExplorerViewModel destination)
        {
            _items = items;
            _destination = destination;
        }

        #region Implementation of IDeployStatsViewerViewModel

        /// <summary>
        /// Services being deployed
        /// </summary>
        public int Connectors
        {
            get
            {
                return _connectors;
            }
            set
            {
                _connectors = value;
                OnPropertyChanged(() => Connectors);
            }
        }
        /// <summary>
        /// Services Being Deployed
        /// </summary>
        public int Services
        {
            get
            {
                return _services;
            }
            set
            {
                _services = value;
                OnPropertyChanged(() => Services);
            }
        }
        /// <summary>
        /// Sources being Deployed
        /// </summary>
        public int Sources
        {
            get
            {
                return _sources;
            }
            set
            {
                _sources = value;
                OnPropertyChanged(() => Sources);
            }
        }
        /// <summary>
        /// What is unknown is unknown to me
        /// </summary>
        public int Unknown
        {
            get
            {
                return _unknown;
            }
            set
            {
                _unknown = value;
                OnPropertyChanged(() => Unknown);
            }
        }
        /// <summary>
        /// The count of new resources being deployed
        /// </summary>
        public int NewResources
        {
            get
            {
                return _newResources;
            }
            set
            {
                _newResources = value;
                OnPropertyChanged(() => NewResources);
            }
        }
        /// <summary>
        /// The count of overidded resources
        /// </summary>
        public int Overrides
        {
            get
            {
                return _overrides;
            }
            set
            {
                _overrides = value;
                OnPropertyChanged(() => Overrides);
            }
        }
        /// <summary>
        /// The status of the last deploy
        /// </summary>
        public string Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                OnPropertyChanged(() => Status);
            }
        }
        public void ReCalculate()
        {
            if (_items != null)
            {
                Calculate(_items);
            }
        }

        public void CheckDestinationPersmisions()
        {
            _destinationItems = _destination.SelectedEnvironment?.AsList();
            if (_destinationItems == null || _destinationItems.Count == 0 || _destination.SelectedEnvironment==null || !_destination.SelectedEnvironment.IsConnected)
            {
                foreach (var currentItem in _items)
                {
                    currentItem.CanDeploy = currentItem.Server.CanDeployFrom;
                }
            }
            else
            {
                if (_items?.Count > 0)
                {
                    foreach (var currentItem in _items)
                    {
                        var explorerItemViewModel =
                            _destinationItems.FirstOrDefault(p => p.ResourceId == currentItem.ResourceId);
                        {
                            if (explorerItemViewModel != null)
                            {
                                if (currentItem.Server.CanDeployFrom && explorerItemViewModel.Server.CanDeployTo)
                                {
                                    if (!IsSourceAndDestinationSameServer(currentItem, explorerItemViewModel))
                                    {
                                        currentItem.CanDeploy = explorerItemViewModel.CanContribute;
                                    }
                                    else
                                        currentItem.CanDeploy = true;
                                }
                            }
                            else
                                currentItem.CanDeploy = true;
                        }
                    }
                }
            }
        }

        private static bool IsSourceAndDestinationSameServer(IExplorerTreeItem currentItem, IExplorerItemViewModel explorerItemViewModel)
        {            
            return currentItem.Server == explorerItemViewModel.Server;
        }


        public void Calculate(IList<IExplorerTreeItem> items)
        {
            _items = items;
            if (items != null)
            {
                Connectors = items.Count(a =>
                                        !string.IsNullOrEmpty(a.ResourceType)
                                        && a.ResourceType.Contains(@"Service")
                                        && a.ResourceType != @"WorkflowService"
                                        && a.ResourceType != @"ReservedService");

                Services = items.Count(a => !string.IsNullOrEmpty(a.ResourceType)
                                        && a.ResourceType == @"WorkflowService"
                                        && a.IsResourceChecked == true);

                Sources = items.Count(a => !string.IsNullOrEmpty(a.ResourceType)
                                            && IsSource(a.ResourceType)
                                            && a.IsResourceChecked == true);

                Unknown = items.Count(a => a.ResourceType == @"Unknown" || string.IsNullOrEmpty(a.ResourceType));

                if (_destination.SelectedEnvironment != null)
                {
                    var explorerItemViewModels = _destination.SelectedEnvironment.UnfilteredChildren.Flatten(model => model.UnfilteredChildren?? new ObservableCollection<IExplorerItemViewModel>());
                    var explorerTreeItems = explorerItemViewModels as IExplorerItemViewModel[] ?? explorerItemViewModels.ToArray();
                    var idConflicts = from b in explorerTreeItems
                               join explorerTreeItem in items on b.ResourceId equals explorerTreeItem.ResourceId
                               where b.ResourceType != @"Folder" && explorerTreeItem.ResourceType != @"Folder" && explorerTreeItem.IsResourceChecked.HasValue && explorerTreeItem.IsResourceChecked.Value
                               select new Conflict { SourceName = explorerTreeItem.ResourcePath, DestinationName = b.ResourcePath, DestinationId = b.ResourceId,SourceId = explorerTreeItem.ResourceId};

                    var pathConflicts = from b in explorerTreeItems
                                      join explorerTreeItem in items on b.ResourcePath equals explorerTreeItem.ResourcePath
                                      where b.ResourceType != @"Folder" && explorerTreeItem.ResourceType != @"Folder" && explorerTreeItem.IsResourceChecked.HasValue && explorerTreeItem.IsResourceChecked.Value
                                      select new Conflict { SourceName = explorerTreeItem.ResourcePath, DestinationName = b.ResourcePath, DestinationId = b.ResourceId, SourceId = explorerTreeItem.ResourceId };
                    var allConflicts = new List<Conflict>();
                    allConflicts.AddRange(idConflicts);
                    allConflicts.AddRange(pathConflicts);
                    _conflicts = allConflicts.Distinct(new ConflictEqualityComparer()).ToList();
                    _new = items.Where(p=>p.IsResourceChecked == true && Conflicts.All(c => p.ResourceId != c.SourceId)).Except(explorerTreeItems);
                    var ren = from b in explorerTreeItems
                              join explorerTreeItem in items on new { b.ResourcePath } equals new { explorerTreeItem.ResourcePath }
                              where b.ResourceType != @"Folder" && explorerTreeItem.ResourceType != @"Folder" && explorerTreeItem.IsResourceChecked.HasValue && explorerTreeItem.IsResourceChecked.Value
                              select new { SourceName = explorerTreeItem.ResourcePath, DestinationName = b.ResourcePath, SourceId = explorerTreeItem.ResourceId, DestinationId = b.ResourceId };
                    var errors = ren.Where(ax => ax.SourceId != ax.DestinationId).ToArray();
                    if (errors.Any())
                    {
                        RenameErrors = Resources.Languages.Core.DeployResourcesSamePathAndName;
                        foreach (var error in errors)
                        {
                            RenameErrors += $"\n{error.SourceName}-->{error.DestinationName}";
                        }
                        RenameErrors += Environment.NewLine + Resources.Languages.Core.DeployRenameBeforeContinue;
                    }
                    else
                    {
                        RenameErrors = @"";
                    }
                }
                else
                {
                    _conflicts = new List<Conflict>();
                    _new = new List<IExplorerTreeItem>();
                }

                Overrides = Conflicts.Count;
                NewResources = New.Count;
            }
            else
            {
                Connectors = 0;
                Services = 0;
                Sources = 0;
                Unknown = 0;
                _conflicts = new List<Conflict>();
                _new = new List<IExplorerTreeItem>();
            }

            OnPropertyChanged(() => Conflicts);
            OnPropertyChanged(() => New);
            CalculateAction?.Invoke();
            CheckDestinationPersmisions();
        }

        public IList<Conflict> Conflicts => _conflicts.ToList();

        public IList<IExplorerTreeItem> New
        {
            get
            {
                var explorerTreeItems = _new.Where(a => a.ResourceType != @"Folder").ToList();
                return explorerTreeItems;
            }
        }
        public Action CalculateAction { get; set; }

        bool IsSource(string res)
        {
            return res.Contains(@"Source") || res.Contains(@"Server");
        }
        #endregion
    }

    public class ConflictEqualityComparer : IEqualityComparer<Conflict>
    {
        #region Implementation of IEqualityComparer<in Conflict>

        public bool Equals(Conflict x, Conflict y)
        {
            if(x?.SourceName==y?.SourceName && x?.DestinationName == y?.DestinationName)
            {
                return true;
            }
            return false;
        }

        public int GetHashCode(Conflict obj)
        {
            return 0;
        }

        #endregion
    }
}