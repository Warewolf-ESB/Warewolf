#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2;
using Dev2.Common;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Deploy;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class DeployStatsViewerViewModel : BindableBase, IDeployStatsViewerViewModel
    {
        readonly IDeployDestinationExplorerViewModel _destination;
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
        ICollection<IExplorerItemViewModel> _destinationItems;

        public DeployStatsViewerViewModel(IDeployDestinationExplorerViewModel destination)
        {
            VerifyArgument.IsNotNull(@"destination", destination);
            _destination = destination;
            if (_destination.ConnectControlViewModel != null)
            {
                _destination.ConnectControlViewModel.SelectedEnvironmentChanged += ConnectControlViewModelOnSelectedEnvironmentChanged;
            }
            Status = @"";
        }

        async void ConnectControlViewModelOnSelectedEnvironmentChanged(object sender, Guid environmentId)
        {
            if (_destination?.SelectedEnvironment != null && _destination.SelectedEnvironment.AsList().Count <= 0)
            {
                await _destination.SelectedEnvironment.LoadAsync(true, true).ConfigureAwait(true);
                CheckDestinationPermissions();
            }
        }

        public DeployStatsViewerViewModel(IList<IExplorerTreeItem> items, IDeployDestinationExplorerViewModel destination)
        {
            _items = items;
            _destination = destination;
        }

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

        public string Status
        {
            get => _status;
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
                TryCalculate(_items);
            }
        }

        public void CheckDestinationPermissions()
        {
            _destinationItems = _destination.SelectedEnvironment?.AsList();
            if (_destinationItems == null || _destinationItems.Count == 0 || _destination.SelectedEnvironment == null || !_destination.SelectedEnvironment.IsConnected)
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
                        CheckDestinationPermissions(currentItem);
                    }
                }
            }
        }

        void CheckDestinationPermissions(IExplorerTreeItem currentItem)
        {
            var explorerItemViewModel = _destinationItems.FirstOrDefault(p => p.ResourceId == currentItem.ResourceId);
            {
                if (explorerItemViewModel != null)
                {
                    if (currentItem.Server.CanDeployFrom && explorerItemViewModel.Server.CanDeployTo)
                    {
                        currentItem.CanDeploy = !IsSourceAndDestinationSameServer(currentItem, explorerItemViewModel) ? explorerItemViewModel.CanContribute : true;
                    }
                }
                else
                {
                    currentItem.CanDeploy = true;
                }
            }
        }

        static bool IsSourceAndDestinationSameServer(IExplorerTreeItem currentItem, IExplorerItemViewModel explorerItemViewModel) => Equals(currentItem.Server, explorerItemViewModel.Server);

        public void TryCalculate(IList<IExplorerTreeItem> items)
        {
            _items = items;
            if (items != null)
            {
                Calculate(items);
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
            CheckDestinationPermissions();
        }

        void Calculate(IList<IExplorerTreeItem> items)
        {
            Connectors = items.Count(a => !string.IsNullOrEmpty(a.ResourceType)
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

            if (_destination.SelectedEnvironment != null && _destination.SelectedEnvironment.UnfilteredChildren != null)
            {
                var explorerItemViewModels = _destination.SelectedEnvironment.UnfilteredChildren.Flatten(model => model.UnfilteredChildren ?? new ObservableCollection<IExplorerItemViewModel>());
                var explorerTreeItems = explorerItemViewModels as IExplorerItemViewModel[] ?? explorerItemViewModels.ToArray();
                var idConflicts = from b in explorerTreeItems
                                  join explorerTreeItem in items on b.ResourceId equals explorerTreeItem.ResourceId
                                  where b.ResourceType != @"Folder" && explorerTreeItem.ResourceType != @"Folder" && explorerTreeItem.IsResourceChecked.HasValue && explorerTreeItem.IsResourceChecked.Value
                                  select new Conflict { SourceName = explorerTreeItem.ResourcePath, DestinationName = b.ResourcePath, DestinationId = b.ResourceId, SourceId = explorerTreeItem.ResourceId };

                var pathConflicts = from b in explorerTreeItems
                                    join explorerTreeItem in items on b.ResourcePath equals explorerTreeItem.ResourcePath
                                    where b.ResourceType != @"Folder" && explorerTreeItem.ResourceType != @"Folder" && explorerTreeItem.IsResourceChecked.HasValue && explorerTreeItem.IsResourceChecked.Value
                                    select new Conflict { SourceName = explorerTreeItem.ResourcePath, DestinationName = b.ResourcePath, DestinationId = b.ResourceId, SourceId = explorerTreeItem.ResourceId };
                var allConflicts = new List<Conflict>();
                allConflicts.AddRange(idConflicts);
                allConflicts.AddRange(pathConflicts);
                _conflicts = allConflicts.Distinct(new ConflictEqualityComparer()).ToList();
                _new = items.Where(p => p.IsResourceChecked == true && Conflicts.All(c => p.ResourceId != c.SourceId)).Except(explorerTreeItems);
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

        bool IsSource(string res) => res.Contains(@"Source") || res.Contains(@"Server");
    }

    public class ConflictEqualityComparer : IEqualityComparer<Conflict>
    {
        public bool Equals(Conflict x, Conflict y)
        {
            if(x?.SourceName==y?.SourceName && x?.DestinationName == y?.DestinationName)
            {
                return true;
            }
            return false;
        }

        public int GetHashCode(Conflict obj) => 0;
    }
}