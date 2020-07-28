#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Dev2;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Deploy;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Triggers;

namespace Warewolf.Studio.ViewModels
{
    public class DeployStatsViewerViewModel : BindableBase, IDeployStatsViewerViewModel
    {
        readonly IDeployDestinationExplorerViewModel _destination;
        int _connectors;
        int _services;
        int _sources;
        int _tests;
        int _triggers;
        int _unknown;
        int _newResources;
        int _newTests;
        int _newTriggers;
        int _overrides;
        int _overridesTests;
        int _overridesTriggers;
        string _status;
        public string RenameErrors { get; private set; }
        List<Conflict> _conflicts;
        List<Conflict> _testsConflicts;
        List<Conflict> _triggersConflicts;
        IEnumerable<IExplorerTreeItem> _new;
        IEnumerable<IExplorerTreeItem> _testsNew;
        IEnumerable<IExplorerTreeItem> _triggersNew;
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
            get => _connectors;
            set
            {
                _connectors = value;
                OnPropertyChanged(() => Connectors);
            }
        }

        public int Services
        {
            get => _services;
            set
            {
                _services = value;
                OnPropertyChanged(() => Services);
            }
        }

        public int Sources
        {
            get => _sources;
            set
            {
                _sources = value;
                OnPropertyChanged(() => Sources);
            }
        }

        public int Tests
        {
            get => _tests;
            set
            {
                _tests = value;
                OnPropertyChanged(() => Tests);
            }
        }

        public int Triggers
        {
            get => _triggers;
            set
            {
                _triggers = value;
                OnPropertyChanged(() => Triggers);
            }
        }

        public int Unknown
        {
            get => _unknown;
            set
            {
                _unknown = value;
                OnPropertyChanged(() => Unknown);
            }
        }

        public int NewResources
        {
            get => _newResources;
            set
            {
                _newResources = value;
                OnPropertyChanged(() => NewResources);
            }
        }

        public int NewTests
        {
            get => _newTests;
            set
            {
                _newTests = value;
                OnPropertyChanged(() => NewTests);
            }
        }

        public int NewTriggers
        {
            get => _newTriggers;
            set
            {
                _newTriggers = value;
                OnPropertyChanged(() => NewTriggers);
            }
        }

        public int Overrides
        {
            get => _overrides;
            set
            {
                _overrides = value;
                OnPropertyChanged(() => Overrides);
            }
        }
        public int OverridesTests
        {
            get => _overridesTests;
            set
            {
                _overridesTests = value;
                OnPropertyChanged(() => OverridesTests);
            }
        }

        public int OverridesTriggers
        {
            get => _overridesTriggers;
            set
            {
                _overridesTriggers = value;
                OnPropertyChanged(() => OverridesTriggers);
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
                        currentItem.CanDeploy = IsSourceAndDestinationSameServer(currentItem, explorerItemViewModel) || explorerItemViewModel.CanContribute;
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
                Tests = 0;
                Triggers = 0;
                Unknown = 0;
                _conflicts = new List<Conflict>();
                _testsConflicts = new List<Conflict>();
                _triggersConflicts = new List<Conflict>();

                _new = new List<IExplorerTreeItem>();
                _testsNew = new List<IExplorerTreeItem>();
                _triggersNew = new List<IExplorerTreeItem>();
            }

            UpdateStatsArea();
        }
        public void UpdateStatsArea()
        {
            OnPropertyChanged(() => Conflicts);
            OnPropertyChanged(() => New);
            CalculateAction?.Invoke();
            CheckDestinationPermissions();
        }
        public void UpdateTestsStatsArea()
        {
            OnPropertyChanged(() => TestsConflicts);
            OnPropertyChanged(() => NewTests);
            CalculateNewTests();
            CalculateAction?.Invoke();
        }
        public void UpdateTriggersStatsArea()
        {
            OnPropertyChanged(() => TriggersConflicts);
            OnPropertyChanged(() => NewTriggers);
            CalculateNewTriggers();
            CalculateAction?.Invoke();
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

            CalculateTestsCount();
            CalculateTriggersCount();

            Unknown = items.Count(a => a.ResourceType == @"Unknown" || string.IsNullOrEmpty(a.ResourceType));

            CalculateNewItems(items);

            Overrides = Conflicts.Count;
            NewResources = New.Count;
        }

        public void CalculateTestsCount()
        {
            Tests = _destination.DeployTests ? _items.Where(IsServiceResource).Sum(CountResourceTests) : 0;
        }

        public void CalculateTriggersCount()
        {
            Triggers = _destination.DeployTriggers ? _items.Where(IsServiceResource).Sum(CountResourceTriggerQueues) : 0;
        }

        private static bool IsServiceResource(IExplorerTreeItem item)
        {
            return !string.IsNullOrEmpty(item.ResourceType)
                   && item.IsResourceChecked is true
                   && item.ResourceType == @"WorkflowService";
        }

        private static int CountResourceTests(IExplorerTreeItem item)
        {
            return LoadResourceTestsForDeploy(item)?.Count ?? 0;
        }

        private static int CountResourceTriggerQueues(IExplorerTreeItem item)
        {
            return LoadResourceTriggersForDeploy(item)?.Count ?? 0;
        }

        private void CalculateNewItems(IList<IExplorerTreeItem> items)
        {
            if (_destination.SelectedEnvironment?.UnfilteredChildren != null)
            {
                var explorerTreeItems = SetAllConflictsAndGetTreeItems(items);

                _new = items.Where(p => p.IsResourceChecked == true && Conflicts.All(c => p.ResourceId != c.SourceId)).Except(explorerTreeItems);

                CalculateRenameErrors(items, explorerTreeItems);
            }
            else
            {
                _conflicts = new List<Conflict>();
                _new = new List<IExplorerTreeItem>();
            }
        }

        private IExplorerItemViewModel[] SetAllConflictsAndGetTreeItems(IList<IExplorerTreeItem> items)
        {
            var explorerItemViewModels = _destination.SelectedEnvironment.UnfilteredChildren.Flatten(model => model.UnfilteredChildren ?? new ObservableCollection<IExplorerItemViewModel>());
            var explorerTreeItems = explorerItemViewModels as IExplorerItemViewModel[] ?? explorerItemViewModels.ToArray();
            var idConflicts = from b in explorerTreeItems
                              join explorerTreeItem in items on b.ResourceId equals explorerTreeItem.ResourceId
                              where IsValidConflicts(b, explorerTreeItem)
                              select CreateConflict(explorerTreeItem, b);

            var pathConflicts = from b in explorerTreeItems
                                join explorerTreeItem in items on b.ResourcePath equals explorerTreeItem.ResourcePath
                                where IsValidConflicts(b, explorerTreeItem)
                                select CreateConflict(explorerTreeItem, b);

            var allConflicts = new List<Conflict>();
            allConflicts.AddRange(idConflicts);
            allConflicts.AddRange(pathConflicts);
            _conflicts = allConflicts.Distinct(new ConflictEqualityComparer()).ToList();
            return explorerTreeItems;
        }

        private void CalculateNewTests()
        {
            if (_destination.SelectedEnvironment?.UnfilteredChildren != null && _destination.DeployTests)
            {
                var tests = SetAllTestsConflictsAndGetTreeItems();
                _testsNew = _items.Where(p => p.IsResourceChecked == true
                                              && TestsConflicts.All(c => p.ResourceId != c.SourceId)).Except(tests);
            }
            else
            {
                _testsConflicts = new List<Conflict>();
                _testsNew = new List<IExplorerTreeItem>();
            }
            OverridesTests = TestsConflicts.Count;
            NewTests = TestsNew.Count;
        }

        private IEnumerable<IExplorerItemViewModel> SetAllTestsConflictsAndGetTreeItems()
        {
            var sourceTests = new List<IServiceTestModelTO>();

            var services = _items.Where(IsServiceResource).ToList();
            foreach (var testsForDeploy in services.Select(LoadResourceTestsForDeploy).Where(testsForDeploy => testsForDeploy != null))
            {
                sourceTests.AddRange(testsForDeploy);
            }

            var explorerItemViewModels = _destination.SelectedEnvironment.UnfilteredChildren.Flatten(model => model.UnfilteredChildren ?? new ObservableCollection<IExplorerItemViewModel>());
            var explorerTreeItems = explorerItemViewModels as IExplorerItemViewModel[] ?? explorerItemViewModels.ToArray();

            var treeItems = services.Select(item => explorerTreeItems.First(o => o.ResourceId == item.ResourceId)).Where(treeItem => treeItem != null).Cast<IExplorerTreeItem>().ToList();

            var destinationTests = new List<IServiceTestModelTO>();

            foreach (var testsForDeploy in treeItems.Select(LoadResourceTestsForDeploy).Where(testsForDeploy => testsForDeploy != null))
            {
                destinationTests.AddRange(testsForDeploy);
            }

            var allConflicts = (from source in sourceTests
            let destination = destinationTests.First(o => o.ResourceId == source.ResourceId)
            where destination != null
            select new Conflict
            {
                SourceName = source.TestName, DestinationName = destination.TestName, DestinationId = destination.ResourceId, SourceId = source.ResourceId,
            }).ToList();

            _testsConflicts = allConflicts.Distinct(new ConflictEqualityComparer()).ToList();
            return explorerTreeItems;
        }

        private static List<IServiceTestModelTO> LoadResourceTestsForDeploy(IExplorerTreeItem explorerTreeItem)
        {
            return explorerTreeItem.Server?.ResourceRepository?.LoadResourceTestsForDeploy(explorerTreeItem.ResourceId);
        }

        private void CalculateNewTriggers()
        {
            if (_destination.SelectedEnvironment?.UnfilteredChildren != null && _destination.DeployTriggers)
            {
                var triggers = SetAllTriggersConflictsAndGetTreeItems();
                _triggersNew = _items.Where(p => p.IsResourceChecked == true
                                              && TriggersConflicts.All(c => p.ResourceId != c.SourceId)).Except(triggers);
            }
            else
            {
                _triggersConflicts = new List<Conflict>();
                _triggersNew = new List<IExplorerTreeItem>();
            }
            OverridesTriggers = TriggersConflicts.Count;
            NewTriggers = TriggersNew.Count;
        }

        private IEnumerable<IExplorerItemViewModel> SetAllTriggersConflictsAndGetTreeItems()
        {
            var sourceTriggers = new List<ITriggerQueue>();

            var services = _items.Where(IsServiceResource).ToList();
            foreach (var triggersForDeploy in services.Select(LoadResourceTriggersForDeploy).Where(triggersForDeploy => triggersForDeploy != null))
            {
                sourceTriggers.AddRange(triggersForDeploy);
            }

            var explorerItemViewModels = _destination.SelectedEnvironment.UnfilteredChildren.Flatten(model => model.UnfilteredChildren ?? new ObservableCollection<IExplorerItemViewModel>());
            var explorerTreeItems = explorerItemViewModels as IExplorerItemViewModel[] ?? explorerItemViewModels.ToArray();

            var treeItems = services.Select(item => explorerTreeItems.First(o => o.ResourceId == item.ResourceId)).Where(treeItem => treeItem != null).Cast<IExplorerTreeItem>().ToList();

            var destinationTriggers = new List<ITriggerQueue>();

            foreach (var triggersForDeploy in treeItems.Select(LoadResourceTriggersForDeploy).Where(triggersForDeploy => triggersForDeploy != null))
            {
                destinationTriggers.AddRange(triggersForDeploy);
            }

            var allConflicts = (from source in sourceTriggers
                let destination = destinationTriggers.First(o => o.ResourceId == source.ResourceId)
                where destination != null
                select new Conflict
                {
                    SourceName = source.QueueName, DestinationName = destination.QueueName, DestinationId = destination.TriggerId, SourceId = source.TriggerId,
                }).ToList();

            _triggersConflicts = allConflicts.Distinct(new ConflictEqualityComparer()).ToList();
            return explorerTreeItems;
        }

        private static List<ITriggerQueue> LoadResourceTriggersForDeploy(IExplorerTreeItem explorerTreeItem)
        {
            return explorerTreeItem.Server?.ResourceRepository?.LoadResourceTriggersForDeploy(explorerTreeItem.ResourceId);
        }

        private static bool IsValidConflicts(IExplorerTreeItem b, IExplorerTreeItem treeItem)
        {
            return b.ResourceType != @"Folder" && treeItem.ResourceType != @"Folder" && treeItem.IsResourceChecked.HasValue && treeItem.IsResourceChecked.Value;
        }
        private static Conflict CreateConflict(IExplorerTreeItem explorerTreeItem, IExplorerTreeItem b)
        {
            return new Conflict { SourceName = explorerTreeItem.ResourcePath, DestinationName = b.ResourcePath, DestinationId = b.ResourceId, SourceId = explorerTreeItem.ResourceId };
        }

        private void CalculateRenameErrors(IList<IExplorerTreeItem> treeItems, IExplorerItemViewModel[] viewModels)
        {
            var ren = from viewModel in viewModels
                      join item in treeItems on new { viewModel.ResourcePath } equals new { item.ResourcePath }
                      where viewModel.ResourceType != @"Folder" && item.ResourceType != @"Folder" && item.IsResourceChecked.HasValue && item.IsResourceChecked.Value
                      select new { SourceName = item.ResourcePath, DestinationName = viewModel.ResourcePath, SourceId = item.ResourceId, DestinationId = viewModel.ResourceId };
            var errors = ren.Where(ax => ax.SourceId != ax.DestinationId).ToArray();
            var sb = new StringBuilder();
            if (errors.Any())
            {
                RenameErrors = Resources.Languages.Core.DeployResourcesSamePathAndName;
                foreach (var error in errors)
                {
                    RenameErrors = sb.Append($"\n{error.SourceName}-->{error.DestinationName}").ToString();
                }
                RenameErrors = sb.Append(Environment.NewLine + Resources.Languages.Core.DeployRenameBeforeContinue).ToString();
            }
            else
            {
                RenameErrors = @"";
            }
        }

        public IList<Conflict> Conflicts => _conflicts.ToList();
        public IList<Conflict> TestsConflicts => _testsConflicts.ToList();
        public IList<Conflict> TriggersConflicts => _triggersConflicts.ToList();

        public IList<IExplorerTreeItem> New
        {
            get
            {
                var explorerTreeItems = _new.Where(a => a.ResourceType != @"Folder").ToList();
                return explorerTreeItems;
            }
        }
        public IList<IExplorerTreeItem> TestsNew
        {
            get
            {
                var explorerTreeItems = _testsNew.Where(a => a.ResourceType != @"Folder").ToList();
                return explorerTreeItems;
            }
        }
        public IList<IExplorerTreeItem> TriggersNew
        {
            get
            {
                var explorerTreeItems = _triggersNew.Where(a => a.ResourceType != @"Folder").ToList();
                return explorerTreeItems;
            }
        }
        public Action CalculateAction { get; set; }

        static bool IsSource(string res) => res.Contains(@"Source") || res.Contains(@"Server");
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