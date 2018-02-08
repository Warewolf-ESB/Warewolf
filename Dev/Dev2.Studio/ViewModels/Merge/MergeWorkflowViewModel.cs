/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common;
using System.Windows;
using Dev2.Studio.Interfaces.DataList;
using Dev2.ViewModels.Merge.Utils;

namespace Dev2.ViewModels.Merge
{
    public class MergeWorkflowViewModel : BindableBase, IMergeWorkflowViewModel
    {
        readonly IServiceDifferenceParser _serviceDifferenceParser;
        string _displayName;
        bool _hasMergeStarted;
        bool _hasWorkflowNameConflict;
        bool _hasVariablesConflict;
        bool _isVariablesEnabled;
        readonly IContextualResourceModel _resourceModel;
        readonly ConflictList conflictList;

        public MergeWorkflowViewModel(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel, bool loadworkflowFromServer)
        {
            _serviceDifferenceParser = CustomContainer.Get<IServiceDifferenceParser>();
            UpdateHelpDescriptor(Warewolf.Studio.Resources.Languages.HelpText.MergeWorkflowStartupHelp);

            WorkflowDesignerViewModel = new WorkflowDesignerViewModel(currentResourceModel, false);
            WorkflowDesignerViewModel.CreateBlankWorkflow();

            _resourceModel = currentResourceModel;
            var currentChanges = _serviceDifferenceParser.GetDifferences(currentResourceModel, differenceResourceModel, loadworkflowFromServer);
            conflictList = new ConflictList
            {
                Conflicts = BuildConflicts(currentResourceModel, differenceResourceModel, currentChanges)
            };

            Conflicts = new LinkedList<IConflictRow>(conflictList.Conflicts);
            var firstConflict = Conflicts.FirstOrDefault();
            SetupBindings(currentResourceModel, differenceResourceModel, firstConflict as IToolConflict);

            var stateApplier = new ConflictListStateApplier(conflictList);
            stateApplier.SetConnectorSelectionsToCurrentState();

            var mergePreviewWorkflowStateApplier = new MergePreviewWorkflowStateApplier(conflictList);
        }

        void SetupBindings(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel, IToolConflict firstConflict)
        {
            if (CurrentConflictModel == null)
            {
                CurrentConflictModel = new ConflictModelFactory
                {
                    Model = firstConflict?.CurrentViewModel ?? new MergeToolModel
                    {
                        WorkflowDesignerViewModel = WorkflowDesignerViewModel
                    },
                    WorkflowName = currentResourceModel.ResourceName,
                    ServerName = currentResourceModel.Environment.Name
                };

                CurrentConflictModel.GetDataList(currentResourceModel);
                CurrentConflictModel.SomethingConflictModelChanged += SourceOnConflictModelChanged;
            }

            if (DifferenceConflictModel == null)
            {
                DifferenceConflictModel = new ConflictModelFactory
                {
                    Model = firstConflict?.DiffViewModel ?? new MergeToolModel
                    {
                        WorkflowDesignerViewModel = WorkflowDesignerViewModel
                    },
                    WorkflowName = differenceResourceModel.ResourceName,
                    ServerName = differenceResourceModel.Environment.Name
                };
                DifferenceConflictModel.GetDataList(differenceResourceModel);
                DifferenceConflictModel.SomethingConflictModelChanged += SourceOnConflictModelChanged;
            }

            HasMergeStarted = false;

            HasVariablesConflict = currentResourceModel.DataList != differenceResourceModel.DataList;
            HasWorkflowNameConflict = currentResourceModel.ResourceName != differenceResourceModel.ResourceName;
            IsVariablesEnabled = !HasWorkflowNameConflict && HasVariablesConflict;

            DisplayName = nameof(Merge);

            WorkflowDesignerViewModel.CanViewWorkflowLink = false;
            WorkflowDesignerViewModel.IsTestView = true;
            CurrentConflictModel.IsWorkflowNameChecked = !HasWorkflowNameConflict;
            CurrentConflictModel.IsVariablesChecked = !HasVariablesConflict;
        }

        List<IConflictRow> BuildConflicts(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel, (List<ConflictTreeNode> current, List<ConflictTreeNode> diff) currentChanges)
        {
            var conflicts = new List<IConflictRow>();
            var armConnectorConflicts = new List<IArmConnectorConflict>();

            var current = currentChanges.current;
            var diff = currentChanges.diff;
            var distinctOrderedList = current.Union(diff).DistinctBy(o => o.UniqueId).ToList();

            var first = distinctOrderedList[0];
            var isInConflict = current[0].UniqueId != diff[0].UniqueId;

            if (first.IsInConflict || isInConflict)
            {
                var startRow = CreateStartRow();
                AddStartNodeConnectors(armConnectorConflicts, current[0], diff[0]);
                conflicts.Add(startRow);
            }

            foreach (var distinctItem in distinctOrderedList)
            {
                var currentItem = current.FirstOrDefault(o => o.UniqueId == distinctItem.UniqueId);
                if (currentItem != null)
                {
                    ProcessCurrentItem(currentResourceModel, conflicts, armConnectorConflicts, currentItem);
                }

                var currentIndex = current.IndexOf(currentItem);
                var diffIndex = diff.IndexOf(currentItem);

                if (diffIndex != -1 && diffIndex != currentIndex && (diffIndex + 1) <= current.Count())
                {
                    var diffItm = diff[currentIndex];
                    if (diffItm != null && currentItem != null)
                    {
                        ProcessExistingDiffItem(differenceResourceModel, conflicts, armConnectorConflicts, diffItm, currentItem.UniqueId);
                    }
                }
                else
                {
                    var diffItem = diff.FirstOrDefault(o => o.UniqueId == distinctItem.UniqueId);
                    if (diffItem != null)
                    {
                        ProcessDiffItem(differenceResourceModel, conflicts, armConnectorConflicts, diffItem);
                    }
                }
            }
            ShowArmConnectors(conflicts, armConnectorConflicts);
            return conflicts;
        }

        void ProcessExistingDiffItem(IContextualResourceModel differenceResourceModel, List<IConflictRow> conflicts, List<IArmConnectorConflict> armConnectorConflicts, ConflictTreeNode treeItem, string foundId)
        {
            var id = Guid.Parse(foundId);
            var foundConflict = conflicts.Where(s => s is IToolConflict).Cast<IToolConflict>().FirstOrDefault(t => t.UniqueId.ToString() == foundId);
            foundConflict.HasConflict = true;

            var conflictTreeNode = treeItem;

            var currentFactory = new ConflictModelFactory(differenceResourceModel, conflictTreeNode, WorkflowDesignerViewModel);
            foundConflict.DiffViewModel = currentFactory.Model;
            foundConflict.DiffViewModel.NotifyToolModelChanged += OnToolModelChangedHandler;
            foundConflict.DiffViewModel.Container = foundConflict;
            foundConflict.HasConflict = foundConflict.HasConflict || conflictTreeNode.IsInConflict;

            AddDiffArmConnectors(armConnectorConflicts, treeItem, id);
            ShowArmConnectors(conflicts, armConnectorConflicts);
        }

        void ProcessDiffItem(IContextualResourceModel differenceResourceModel, List<IConflictRow> conflicts, List<IArmConnectorConflict> armConnectorConflicts, ConflictTreeNode treeItem)
        {
            IToolConflict conflict = null;
            var node = treeItem;
            var foundConflict = conflicts.Where(s => s is IToolConflict).Cast<IToolConflict>().FirstOrDefault(t => t.UniqueId.ToString() == node.UniqueId);
            var id = Guid.Parse(node.UniqueId);
            if (foundConflict == null)
            {
                conflict = new ToolConflictRow
                {
                    UniqueId = id,
                    CurrentViewModel = EmptyConflictViewModel(id, WorkflowDesignerViewModel),
                };
                conflict.CurrentViewModel.NotifyToolModelChanged += OnToolModelChangedHandler;
                conflict.CurrentViewModel.Container = conflict;
                conflicts.Add(conflict);
            }
            else
            {
                conflict = foundConflict;
            }
            var conflictTreeNode = node;
            var currentFactory = new ConflictModelFactory(differenceResourceModel, conflictTreeNode, WorkflowDesignerViewModel);
            conflict.DiffViewModel = currentFactory.Model;
            conflict.DiffViewModel.NotifyToolModelChanged += OnToolModelChangedHandler;
            conflict.DiffViewModel.Container = conflict;
            conflict.HasConflict = conflict.HasConflict || node.IsInConflict;
            AddDiffArmConnectors(armConnectorConflicts, treeItem, id);
            ShowArmConnectors(conflicts, armConnectorConflicts);
        }

        void ProcessCurrentItem(IContextualResourceModel currentResourceModel, List<IConflictRow> conflicts, List<IArmConnectorConflict> armConnectorConflicts, ConflictTreeNode treeItem)
        {
            var conflict = new ToolConflictRow();
            var modelFactory = new ConflictModelFactory(currentResourceModel, treeItem, WorkflowDesignerViewModel);
            var id = Guid.Parse(treeItem.UniqueId);
            conflict.UniqueId = id;
            conflict.DiffViewModel = EmptyConflictViewModel(id, WorkflowDesignerViewModel);
            conflict.DiffViewModel.NotifyToolModelChanged += OnToolModelChangedHandler;
            conflict.CurrentViewModel = modelFactory.Model;
            conflict.CurrentViewModel.NotifyToolModelChanged += OnToolModelChangedHandler;
            conflict.CurrentViewModel.Container = conflict;
            conflict.DiffViewModel.Container = conflict;
            conflict.HasConflict = treeItem.IsInConflict;
            ShowArmConnectors(conflicts, armConnectorConflicts);
            conflicts.Add(conflict);
            AddArmConnectors(armConnectorConflicts, treeItem, id);
        }

        ToolConflictRow CreateStartRow()
        {
            var conflict = new ToolConflictRow
            {
                UniqueId = Guid.Empty,
                HasConflict = false
            };

            conflict.CurrentViewModel = new MergeToolModel
            {
                MergeDescription = "Start",
                Container = conflict
            };

            conflict.DiffViewModel = new MergeToolModel
            {
                Container = conflict
            };

            return conflict;
        }

        void AddStartNodeConnectors(List<IArmConnectorConflict> armConnectorConflicts, ConflictTreeNode currentItem, ConflictTreeNode diffItem)
        {
            const string key = "Start";
            var armConnector = new ConnectorConflictRow
            {
                UniqueId = Guid.Empty,
                Key = key,
                HasConflict = true
            };
            var currentConnectorConflict = new MergeArmConnectorConflict("Start -> " + currentItem.Activity.GetDisplayName(), Guid.Empty, Guid.Parse(currentItem.UniqueId), key, armConnector)
            {
                WorkflowDesignerViewModel = WorkflowDesignerViewModel
            };
            var diffConnectorConflict = new MergeArmConnectorConflict("Start -> " + diffItem.Activity.GetDisplayName(), Guid.Empty, Guid.Parse(diffItem.UniqueId), key, armConnector)
            {
                WorkflowDesignerViewModel = WorkflowDesignerViewModel
            };
            armConnector.CurrentArmConnector = currentConnectorConflict;
            armConnector.DifferentArmConnector = diffConnectorConflict;
            armConnector.CurrentArmConnector.OnChecked += ArmCheck;
            armConnector.DifferentArmConnector.OnChecked += ArmCheck;
            armConnector.HasConflict = true;
            armConnectorConflicts.Add(armConnector);
        }

        void SourceOnConflictModelChanged(object sender, IConflictModelFactory conflictModel)
        {
            try
            {
                if (!HasMergeStarted)
                {
                    HasMergeStarted = conflictModel.IsWorkflowNameChecked || conflictModel.IsVariablesChecked;
                }
                if (!conflictModel.IsVariablesChecked)
                {
                    return;
                }

                IsVariablesEnabled = HasVariablesConflict;
                if (conflictModel.IsVariablesChecked)
                {
                    DataListViewModel = conflictModel.DataListViewModel;
                }
                var completeConflict = Conflicts.First.Value as IToolConflict;
                completeConflict.IsStartNode = true;
                if (completeConflict.IsChecked)
                {
                    return;
                }
                completeConflict.CurrentViewModel.IsChecked = !completeConflict.HasConflict;
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, ex.Message);
            }
        }

        void OnToolModelChangedHandler(IMergeToolModel mergeToolModel)
        {
            try
            {
                // init
                if (!HasMergeStarted)
                {
                    HasMergeStarted = true;
                }

                // setup
                var container = mergeToolModel.Container;

                RemovePreviousToolArm(container);
                container.IsChecked = mergeToolModel.IsChecked;
                container.IsEmptyItemSelected = mergeToolModel.ModelItem == null;

                UpdateState(container);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, ex.Message);
            }

            void UpdateState(IToolConflict container)
            {
                // update
                var conflict = conflictList.GetNextConflict(mergeToolModel.Container);

                switch (conflict)
                {
                    case IToolConflict nextConflict:
                        if (!nextConflict.HasConflict || nextConflict.IsContainerTool)
                        {
                            nextConflict.CurrentViewModel.IsChecked = true;
                        }
                        break;
                    case IArmConnectorConflict nextArmConflict:
                        nextArmConflict.CurrentArmConnector.IsChecked = !nextArmConflict.HasConflict;
                        break;
                    default:
                        break;
                }
            }
        }

        void RemovePreviousToolArm(IConflictRow container)
        {
            var updateNextConflict = conflictList.GetNextConlictToUpdate(container);
            if (updateNextConflict != null)
            {
                if (updateNextConflict is IArmConnectorConflict updateNextArmConflict)
                {
                    ResetToolArmEvents(updateNextArmConflict);
                }
                else
                {
                    RemovePreviousToolArm(updateNextConflict);
                }
            }
        }

        void ResetToolArmEvents(IArmConnectorConflict connectorConflict)
        {
            var index = conflictList.IndexOf(connectorConflict);
            if (index == -1 || index > conflictList.Count)
            {
                return;
            }

            var restOfItems = conflictList.Skip(index).ToList();
            foreach (var conf in restOfItems)
            {
                if (conf is IArmConnectorConflict tool)
                {
                    if (tool.HasConflict)
                    {
                        tool.IsChecked = false;
                    }
                    else
                    {
                        tool.CurrentArmConnector.IsChecked = true;
                    }
                }
            }
        }

        void ArmCheck(IArmConnectorConflict container, bool isChecked)
        {
            if (isChecked)
            {
                container.IsChecked = isChecked;
                var conflict = conflictList.GetNextConflict(container);
                if (conflict is IToolConflict nextConflict && (!nextConflict.HasConflict || nextConflict.IsContainerTool))
                {
                    nextConflict.CurrentViewModel.IsChecked = true;
                }
            }
        }

        static void ShowArmConnectors(List<IConflictRow> conflicts, List<IArmConnectorConflict> armConnectorConflicts)
        {
            var itemsToAdd = new List<IConflictRow>();
            var foundConflicts = ShouldShowArmConnector(conflicts.Where(s => s is IToolConflict).Select(a => a.UniqueId), armConnectorConflicts);
            foreach (var found in foundConflicts ?? new List<IArmConnectorConflict>())
            {
                AddToTempConflictList(conflicts, itemsToAdd, found);
            }
            conflicts.AddRange(itemsToAdd);
        }

        static IEnumerable<IArmConnectorConflict> ShouldShowArmConnector(IEnumerable<Guid> toolIds, List<IArmConnectorConflict> armConnectorConflicts) => armConnectorConflicts.Where(s =>
        {
            var foundCurrentDestination = FindMatchingConnector(s.CurrentArmConnector.DestinationUniqueId, toolIds);
            var foundDiffDestination = FindMatchingConnector(s.DifferentArmConnector.DestinationUniqueId, toolIds);
            var foundCurrentSource = FindMatchingConnector(s.CurrentArmConnector.SourceUniqueId, toolIds);
            var foundDiffSource = FindMatchingConnector(s.DifferentArmConnector.SourceUniqueId, toolIds);
            var hasValues = foundCurrentDestination && foundDiffDestination && foundCurrentSource && foundDiffSource;
            if (!hasValues)
            {
                if ((s.CurrentArmConnector.DestinationUniqueId == Guid.Empty || s.CurrentArmConnector.SourceUniqueId == Guid.Empty) && foundDiffDestination && foundDiffSource)
                {
                    return true;
                }
                if ((s.DifferentArmConnector.DestinationUniqueId == Guid.Empty || s.DifferentArmConnector.SourceUniqueId == Guid.Empty) && foundCurrentDestination && foundCurrentSource)
                {
                    return true;
                }
            }
            return hasValues;
        });
        static bool FindMatchingConnector(Guid connectorId, IEnumerable<Guid> toolIds) => toolIds.Contains(connectorId);

        static void AddToTempConflictList(List<IConflictRow> conflicts, List<IConflictRow> itemsToAdd, IArmConnectorConflict found)
        {
            if (found != null && conflicts.Where(t => t is IArmConnectorConflict)?.FirstOrDefault(s => s.UniqueId == found.UniqueId && ((IArmConnectorConflict)s).Key == found.Key) == null)
            {
                itemsToAdd.Add(found);
            }
        }

        void AddDiffArmConnectors(List<IArmConnectorConflict> armConnectorConflicts, ConflictTreeNode treeItem, Guid id)
        {
            foreach (var (Description, Key, SourceUniqueId, DestinationUniqueId) in treeItem.Activity.ArmConnectors())
            {
                var foundConnector = armConnectorConflicts.FirstOrDefault(s => s.UniqueId == id && s.Key == Key);
                if (foundConnector != null)
                {
                    var mergeArmConnectorConflict = new MergeArmConnectorConflict(Description, Guid.Parse(SourceUniqueId), Guid.Parse(DestinationUniqueId), Key, foundConnector)
                    {
                        WorkflowDesignerViewModel = WorkflowDesignerViewModel
                    };
                    mergeArmConnectorConflict.OnChecked += ArmCheck;
                    foundConnector.DifferentArmConnector = mergeArmConnectorConflict;
                    var hasConflict = !foundConnector.CurrentArmConnector.Equals(foundConnector.DifferentArmConnector);
                    foundConnector.HasConflict = hasConflict;
                }
                else
                {
                    var armConnector = new ConnectorConflictRow
                    {
                        UniqueId = id,
                        Key = Key,
                        HasConflict = true
                    };
                    var mergeArmConnectorConflict = new MergeArmConnectorConflict(Description, Guid.Parse(SourceUniqueId), Guid.Parse(DestinationUniqueId), Key, armConnector)
                    {
                        WorkflowDesignerViewModel = WorkflowDesignerViewModel
                    };
                    armConnector.DifferentArmConnector = mergeArmConnectorConflict;
                    armConnector.CurrentArmConnector = EmptyMergeArmConnectorConflict(id, armConnector, WorkflowDesignerViewModel);
                    armConnector.CurrentArmConnector.OnChecked += ArmCheck;
                    armConnector.DifferentArmConnector.OnChecked += ArmCheck;
                    armConnector.HasConflict = true;
                    armConnectorConflicts.Add(armConnector);
                }
            }
        }

        void AddArmConnectors(List<IArmConnectorConflict> armConnectorConflicts, ConflictTreeNode treeItem, Guid id)
        {
            foreach (var (Description, Key, SourceUniqueId, DestinationUniqueId) in treeItem.Activity.ArmConnectors())
            {
                var armConnector = new ConnectorConflictRow
                {
                    UniqueId = id,
                    Key = Key,
                    HasConflict = true
                };
                var mergeArmConnectorConflict = new MergeArmConnectorConflict(Description, Guid.Parse(SourceUniqueId), Guid.Parse(DestinationUniqueId), Key, armConnector)
                {
                    WorkflowDesignerViewModel = WorkflowDesignerViewModel
                };
                armConnector.CurrentArmConnector = mergeArmConnectorConflict;
                armConnector.DifferentArmConnector = EmptyMergeArmConnectorConflict(id, armConnector, WorkflowDesignerViewModel);
                armConnector.CurrentArmConnector.OnChecked += ArmCheck;
                armConnector.DifferentArmConnector.OnChecked += ArmCheck;
                armConnector.HasConflict = true;
                if (armConnectorConflicts.FirstOrDefault(s => s.UniqueId == id && s.Key == Key) == null)
                {
                    armConnectorConflicts.Add(armConnector);
                }
            }
        }

        static MergeArmConnectorConflict EmptyMergeArmConnectorConflict(Guid uniqueId, IArmConnectorConflict container, IWorkflowDesignerViewModel workflowDesignerViewModel)
        {
            return new MergeArmConnectorConflict(container)
            {
                SourceUniqueId = uniqueId,
                DestinationUniqueId = Guid.Empty,
                Key = container.Key,
                WorkflowDesignerViewModel = workflowDesignerViewModel
            };
        }

        static MergeToolModel EmptyConflictViewModel(Guid uniqueId, IWorkflowDesignerViewModel workflowDesignerViewModel) => new MergeToolModel
        {
            ModelItem = null,
            WorkflowDesignerViewModel = workflowDesignerViewModel,
            NodeLocation = new Point(),
            IsMergeVisible = false,
            UniqueId = uniqueId
        };

        IDataListViewModel _dataListViewModel;

        public IDataListViewModel DataListViewModel
        {
            get => _dataListViewModel;
            set
            {
                _dataListViewModel = value;
                OnPropertyChanged(nameof(DataListViewModel));
            }
        }

        public LinkedList<IConflictRow> Conflicts { get; set; }

        public IWorkflowDesignerViewModel WorkflowDesignerViewModel { get; set; }

        public IConflictModelFactory CurrentConflictModel { get; set; }
        public IConflictModelFactory DifferenceConflictModel { get; set; }

        public void Save()
        {
            try
            {
                var resourceId = _resourceModel.ID;
                if (HasWorkflowNameConflict)
                {
                    var resourceName = CurrentConflictModel.IsWorkflowNameChecked ? CurrentConflictModel.WorkflowName : DifferenceConflictModel.WorkflowName;
                    _resourceModel.Environment.ExplorerRepository.UpdateManagerProxy.Rename(resourceId, resourceName);
                }
                if (HasVariablesConflict)
                {
                    _resourceModel.DataList = CurrentConflictModel.IsVariablesChecked ? CurrentConflictModel.DataListViewModel.WriteToResourceModel() : DifferenceConflictModel.DataListViewModel.WriteToResourceModel();
                }
                _resourceModel.WorkflowXaml = WorkflowDesignerViewModel.ServiceDefinition;
                _resourceModel.Environment.ResourceRepository.SaveToServer(_resourceModel, "Merge");

                HasMergeStarted = false;

                var mainViewModel = CustomContainer.Get<IShellViewModel>();
                var environmentID = _resourceModel.Environment.EnvironmentID;
                mainViewModel?.CloseResource(resourceId, environmentID);
                mainViewModel?.CloseResourceMergeView(resourceId, _resourceModel.ServerID, environmentID);
                mainViewModel?.OpenCurrentVersion(resourceId, environmentID);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, ex.Message);
            }
            finally
            {
                SetDisplayName(IsDirty);
            }
        }

        void SetDisplayName(bool isDirty)
        {
            if (isDirty)
            {
                if (!DisplayName.EndsWith(" *", StringComparison.InvariantCultureIgnoreCase))
                {
                    DisplayName += " *";
                }
            }
            else
            {
                DisplayName = _displayName.Replace("*", "").TrimEnd(' ');
            }
        }
        public bool CanSave
        {
            get
            {
                var canSave = CurrentConflictModel.IsWorkflowNameChecked || DifferenceConflictModel.IsWorkflowNameChecked;
                canSave &= CurrentConflictModel.IsVariablesChecked || DifferenceConflictModel.IsVariablesChecked;

                return canSave;
            }
        }

        public bool IsDirty => HasMergeStarted;

        public string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                OnPropertyChanged(() => DisplayName);
            }
        }

        public bool HasMergeStarted
        {
            get => _hasMergeStarted;
            set
            {
                _hasMergeStarted = value && Conflicts.Any(a => a.HasConflict);
                if (_hasMergeStarted)
                {
                    SetDisplayName(_hasMergeStarted);
                }
                OnPropertyChanged(() => HasMergeStarted);
            }
        }

        public bool HasWorkflowNameConflict
        {
            get => _hasWorkflowNameConflict;
            set
            {
                _hasWorkflowNameConflict = value;
                OnPropertyChanged(() => HasWorkflowNameConflict);
            }
        }

        public bool HasVariablesConflict
        {
            get => _hasVariablesConflict;
            set
            {
                _hasVariablesConflict = value;
                OnPropertyChanged(() => HasVariablesConflict);
            }
        }

        public bool IsVariablesEnabled
        {
            get => _isVariablesEnabled;
            set
            {
                _isVariablesEnabled = value;
                OnPropertyChanged(() => IsVariablesEnabled);
            }
        }

        public void Dispose()
        {
        }

        public void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel?.UpdateHelpText(helpText);
        }
    }
    /// <summary>
    /// Manage the selection of items in a
    /// list of IConflictRow entries.
    /// </summary>
    public class ConflictListStateApplier
    {
        readonly ConflictList conflicts;
        // TODO: this state applier should also be used to disable Connectors that are unavailable
        public ConflictListStateApplier(ConflictList conflicts)
        {
            this.conflicts = conflicts;
        }
        public void SetConnectorSelectionsToCurrentState()
        {
            foreach (var conflict in conflicts)
            {
                if (conflict is IConflictCheckable check)
                {
                    check.IsCurrentChecked = true;
                }
            }
        }
    }
    /// <summary>
    /// Apply selection state from a list of IConflict items
    /// to the Merge Preview Workflow
    /// </summary>
    public class MergePreviewWorkflowStateApplier
    {
        public MergePreviewWorkflowStateApplier(ConflictList conflictList)
        {
            RegisterEventHandler(conflictList);
        }

        public void RegisterEventHandler(ConflictList conflictList)
        {
            foreach (var conflictRow in conflictList)
            {
                var innerConflictRow = conflictRow;

                conflictRow.Current.NotifyIsCheckedChanged += (current, isChecked) =>
                {
                    Handler(current, innerConflictRow.Different);
                };
                conflictRow.Different.NotifyIsCheckedChanged += (diff, isChecked) =>
                {
                    Handler(innerConflictRow.Current, diff);
                };
            }
        }

        private void Handler(IConflictItem currentItem, IConflictItem diffItem)
        {
            // apply tool or connector state to workflow
            // DelinkActivities, LinkActivities, AddActivity, RemoveActivity all should be here?
        }
    }
}