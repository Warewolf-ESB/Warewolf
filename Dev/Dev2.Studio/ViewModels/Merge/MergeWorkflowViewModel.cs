﻿/*
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

            Conflicts = new LinkedList<IConflict>(conflictList.Conflicts);
            var firstConflict = Conflicts.FirstOrDefault();
            SetupBindings(currentResourceModel, differenceResourceModel, firstConflict as IToolConflict);
        }

        void SetupBindings(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel, IToolConflict firstConflict)
        {
            if (CurrentConflictModel == null)
            {
                CurrentConflictModel = new ConflictModelFactory
                {
                    Model = firstConflict?.CurrentViewModel ?? new MergeToolModel
                    {
                        IsCurrent = true,
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
                        IsCurrent = false,
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

        List<IConflict> BuildConflicts(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel, (List<ConflictTreeNode> current, List<ConflictTreeNode> diff) currentChanges)
        {
            var conflicts = new List<IConflict>();
            var armConnectorConflicts = new List<IArmConnectorConflict>();

            var current = currentChanges.current.AsEnumerable();
            var diff = currentChanges.diff.AsEnumerable();
            var distinctOrderedList = current.Union(diff).DistinctBy(o => o.UniqueId).ToList();

            foreach (var distinctItem in distinctOrderedList)
            {
                var currentItem = current.FirstOrDefault(o => o.UniqueId == distinctItem.UniqueId);
                if (currentItem != null)
                {
                    ProcessCurrentItem(currentResourceModel, conflicts, armConnectorConflicts, currentItem);
                }

                var currentIndex = current.ToList().IndexOf(currentItem);
                var diffIndex = diff.ToList().IndexOf(currentItem);

                if (diffIndex != -1 && diffIndex != currentIndex && (diffIndex + 1) <= current.Count())
                {
                    var diffItm = diff.ToList()[currentIndex];
                    if (diffItm != null)
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

        void ProcessExistingDiffItem(IContextualResourceModel differenceResourceModel, List<IConflict> conflicts, List<IArmConnectorConflict> armConnectorConflicts, ConflictTreeNode treeItem, string foundId)
        {
            var id = Guid.Parse(foundId);
            var foundConflict = conflicts.Where(s => s is IToolConflict).Cast<IToolConflict>().FirstOrDefault(t => t.UniqueId.ToString() == foundId);
            foundConflict.HasConflict = true;

            var conflictTreeNode = treeItem;

            var currentFactory = new ConflictModelFactory(differenceResourceModel, conflictTreeNode, WorkflowDesignerViewModel);
            foundConflict.DiffViewModel = currentFactory.Model;
            foundConflict.DiffViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
            foundConflict.DiffViewModel.Container = foundConflict;
            foundConflict.DiffViewModel.IsCurrent = false;
            foundConflict.HasConflict = foundConflict.HasConflict || conflictTreeNode.IsInConflict;

            AddDiffArmConnectors(armConnectorConflicts, treeItem, id);
            ShowArmConnectors(conflicts, armConnectorConflicts);
        }

        void ProcessDiffItem(IContextualResourceModel differenceResourceModel, List<IConflict> conflicts, List<IArmConnectorConflict> armConnectorConflicts, ConflictTreeNode treeItem)
        {
            IToolConflict conflict = null;
            var node = treeItem;
            var foundConflict = conflicts.Where(s => s is IToolConflict).Cast<IToolConflict>().FirstOrDefault(t => t.UniqueId.ToString() == node.UniqueId);
            var id = Guid.Parse(node.UniqueId);
            if (foundConflict == null)
            {
                conflict = new ToolConflict
                {
                    UniqueId = id,
                    CurrentViewModel = EmptyConflictViewModel(id, WorkflowDesignerViewModel),
                };
                conflict.CurrentViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
                conflict.CurrentViewModel.Container = conflict;
                conflict.CurrentViewModel.IsCurrent = true;
                conflicts.Add(conflict);
            }
            else
            {
                conflict = foundConflict;
            }
            var conflictTreeNode = node;
            var currentFactory = new ConflictModelFactory(differenceResourceModel, conflictTreeNode, WorkflowDesignerViewModel);
            conflict.DiffViewModel = currentFactory.Model;
            conflict.DiffViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
            conflict.DiffViewModel.Container = conflict;
            conflict.DiffViewModel.IsCurrent = false;
            conflict.HasConflict = conflict.HasConflict || node.IsInConflict;
            AddDiffArmConnectors(armConnectorConflicts, treeItem, id);
            ShowArmConnectors(conflicts, armConnectorConflicts);
        }

        void ProcessCurrentItem(IContextualResourceModel currentResourceModel, List<IConflict> conflicts, List<IArmConnectorConflict> armConnectorConflicts, ConflictTreeNode treeItem)
        {
            var conflict = new ToolConflict();
            var modelFactory = new ConflictModelFactory(currentResourceModel, treeItem, WorkflowDesignerViewModel);
            var id = Guid.Parse(treeItem.UniqueId);
            conflict.UniqueId = id;
            conflict.DiffViewModel = EmptyConflictViewModel(id, WorkflowDesignerViewModel);
            conflict.DiffViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
            conflict.CurrentViewModel = modelFactory.Model;
            conflict.CurrentViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
            conflict.CurrentViewModel.Container = conflict;
            conflict.CurrentViewModel.IsCurrent = true;
            conflict.DiffViewModel.Container = conflict;
            conflict.DiffViewModel.IsCurrent = false;
            conflict.HasConflict = treeItem.IsInConflict;
            ShowArmConnectors(conflicts, armConnectorConflicts);
            conflicts.Add(conflict);
            AddArmConnectors(armConnectorConflicts, treeItem, id);
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
                completeConflict.CurrentViewModel.IsMergeChecked = !completeConflict.HasConflict;
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, ex.Message);
            }
        }

        void SourceOnModelToolChanged(object sender, IMergeToolModel mergeToolModel)
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
                container.IsChecked = mergeToolModel.IsMergeChecked;
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
                var model = mergeToolModel as MergeToolModel;
                model.RemovePreviousContainerActivity();

                var conflict = conflictList.GetNextConflict(mergeToolModel.Container);

                switch (conflict)
                {
                    case IToolConflict nextConflict:
                        UpdateNextToolState(nextConflict);
                        RemoveMatchingActivity(mergeToolModel, container);
                        break;
                    case IArmConnectorConflict nextArmConflict:
                        UpdateNextArmState(nextArmConflict);
                        break;
                    default:
                        break;
                }

                model.AddActivity();
            }
        }

        void RemoveMatchingActivity(IMergeToolModel mergeToolModel, IToolConflict container)
        {
            foreach (var conf in conflictList.Where(o => o is IToolConflict))
            {
                var confl = conf as IToolConflict;
                if (mergeToolModel.IsCurrent)
                {
                    container.DiffViewModel.IsMergeChecked = false;
                    WorkflowDesignerViewModel?.RemoveItem(confl.DiffViewModel);
                }
                else
                {
                    container.CurrentViewModel.IsMergeChecked = false;
                    WorkflowDesignerViewModel?.RemoveItem(confl.CurrentViewModel);
                }
            }
        }

        static void UpdateNextArmState(IArmConnectorConflict nextArmConflict)
        {
            nextArmConflict.CurrentArmConnector.IsChecked = !nextArmConflict.HasConflict;
        }

        void RemovePreviousToolArm(IConflict container)
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

        static void UpdateNextToolState(IToolConflict nextConflict)
        {
            if (!nextConflict.HasConflict || nextConflict.IsContainerTool)
            {
                ExpandPreviousItems(nextConflict);
                nextConflict.CurrentViewModel.IsMergeChecked = true;
            }
        }

        void ArmCheck(IArmConnectorConflict container, bool isChecked)
        {
            if (isChecked)
            {
                container.IsChecked = isChecked;
                var conflict = conflictList.GetNextConflict(container);
                if (conflict is IToolConflict nextConflict)
                {
                    UpdateNextToolState(nextConflict);
                }
            }
        }

        static void ShowArmConnectors(List<IConflict> conflicts, List<IArmConnectorConflict> armConnectorConflicts)
        {
            var itemsToAdd = new List<IConflict>();
            var foundConflicts = ShouldShowArmConnector(conflicts.Where(s => s is IToolConflict).Select(a => a.UniqueId.ToString()), armConnectorConflicts);
            foreach (var found in foundConflicts ?? new List<IArmConnectorConflict>())
            {
                AddToTempConflictList(conflicts, itemsToAdd, found);
            }
            conflicts.AddRange(itemsToAdd);
        }

        static IEnumerable<IArmConnectorConflict> ShouldShowArmConnector(IEnumerable<string> toolIds, List<IArmConnectorConflict> armConnectorConflicts) => armConnectorConflicts.Where(s =>
        {
            var foundCurrentDestination = FindMatchingConnector(s.CurrentArmConnector.DestinationUniqueId, toolIds);
            var foundDiffDestination = FindMatchingConnector(s.DifferentArmConnector.DestinationUniqueId, toolIds);
            var foundCurrentSource = FindMatchingConnector(s.CurrentArmConnector.SourceUniqueId, toolIds);
            var foundDiffSource = FindMatchingConnector(s.DifferentArmConnector.SourceUniqueId, toolIds);
            var hasValues = foundCurrentDestination && foundDiffDestination && foundCurrentSource && foundDiffSource;
            if (!hasValues)
            {
                if ((s.CurrentArmConnector.DestinationUniqueId == Guid.Empty.ToString() || s.CurrentArmConnector.SourceUniqueId == Guid.Empty.ToString()) && foundDiffDestination && foundDiffSource)
                {
                    return true;
                }
                if ((s.DifferentArmConnector.DestinationUniqueId == Guid.Empty.ToString() || s.DifferentArmConnector.SourceUniqueId == Guid.Empty.ToString()) && foundCurrentDestination && foundCurrentSource)
                {
                    return true;
                }
            }
            return hasValues;
        });
        static bool FindMatchingConnector(string connectorId, IEnumerable<string> toolIds) => toolIds.Contains(connectorId);

        static void AddToTempConflictList(List<IConflict> conflicts, List<IConflict> itemsToAdd, IArmConnectorConflict found)
        {
            if (found != null && conflicts.Where(t => t is IArmConnectorConflict)?.FirstOrDefault(s => s.UniqueId == found.UniqueId && ((IArmConnectorConflict)s).Key == found.Key) == null)
            {
                itemsToAdd.Add(found);
            }
        }

        void AddDiffArmConnectors(List<IArmConnectorConflict> armConnectorConflicts, ConflictTreeNode treeItem, Guid id)
        {
            var armConnectors = treeItem.Activity.ArmConnectors();
            foreach (var (Description, Key, SourceUniqueId, DestinationUniqueId) in armConnectors)
            {
                var foundConnector = armConnectorConflicts.FirstOrDefault(s => s.UniqueId == id && s.Key == Key);
                if (foundConnector != null)
                {
                    var mergeArmConnectorConflict = new MergeArmConnectorConflict(Description, SourceUniqueId, DestinationUniqueId, Key, foundConnector)
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
                    var armConnector = new ArmConnectorConflict
                    {
                        UniqueId = id,
                        Key = Key,
                        HasConflict = true
                    };
                    var mergeArmConnectorConflict = new MergeArmConnectorConflict(Description, SourceUniqueId, DestinationUniqueId, Key, armConnector)
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
            var armConnectors = treeItem.Activity.ArmConnectors();
            foreach (var (Description, Key, SourceUniqueId, DestinationUniqueId) in armConnectors)
            {
                var armConnector = new ArmConnectorConflict
                {
                    UniqueId = id,
                    Key = Key,
                    HasConflict = true
                };
                var mergeArmConnectorConflict = new MergeArmConnectorConflict(Description, SourceUniqueId, DestinationUniqueId, Key, armConnector)
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
                SourceUniqueId = uniqueId.ToString(),
                DestinationUniqueId = Guid.Empty.ToString(),
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

        bool All(Func<IConflict, bool> check)
        {
            if (check == null)
            {
                return false;
            }
            var conflictsMatch = Conflicts.All(check);
            var childrenMatch = true;
            foreach (var completeConflict in Conflicts)
            {
                if (completeConflict is IToolConflict toolConflict)
                {
                    childrenMatch &= toolConflict.All(check);
                }
                if (completeConflict is IArmConnectorConflict armConflict)
                {
                    childrenMatch &= check(armConflict);
                }
            }
            return conflictsMatch && childrenMatch;
        }

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

        static void ExpandPreviousItems(IToolConflict nextConflict)
        {
            if (nextConflict.Parent != null)
            {
                nextConflict.Parent.IsMergeExpanded = true;
                ExpandPreviousItems(nextConflict.Parent);
            }
        }

        public LinkedList<IConflict> Conflicts { get; set; }

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
                if (Conflicts.Any(a => a.HasConflict))
                {
                    canSave &= All(conflict =>
                    {
                        var isValid = false;
                        if (conflict.HasConflict)
                        {
                            isValid = conflict.IsChecked;
                        }
                        return isValid;
                    });
                }
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

    public class StateApplier
    {
        private int[] ToolsAndConnectors;

        public StateApplier(int[] ToolsAndConnectors)
        {
            this.ToolsAndConnectors = ToolsAndConnectors;
        }

        /// <summary>
        /// Set the selection of each connector option to
        /// CurrentWorkFlow's connector state
        /// </summary>
        /// <param name="WorkFlow">Which workflow to treat as current</param>
        public void SetConnectorSelectionsToCurrentState(int WorkFlow)
        {
        }




    }
}