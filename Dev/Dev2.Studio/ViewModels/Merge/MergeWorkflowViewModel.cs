/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
        readonly List<IConflict> _conflicts;

        public MergeWorkflowViewModel(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel, bool loadworkflowFromServer)
        : this(CustomContainer.Get<IServiceDifferenceParser>())
        {
            UpdateHelpDescriptor(Warewolf.Studio.Resources.Languages.HelpText.MergeWorkflowStartupHelp);

            WorkflowDesignerViewModel = new WorkflowDesignerViewModel(currentResourceModel, false);
            WorkflowDesignerViewModel.CreateBlankWorkflow();

            _resourceModel = currentResourceModel;
            _seenConflicts = new List<IConflict>();
            var currentChanges = _serviceDifferenceParser.GetDifferences(currentResourceModel, differenceResourceModel, loadworkflowFromServer);
            _conflicts = BuildConflicts(currentResourceModel, differenceResourceModel, currentChanges);
            Conflicts = new LinkedList<IConflict>(_conflicts);
            var firstConflict = Conflicts.FirstOrDefault();
            SetupBindings(currentResourceModel, differenceResourceModel, firstConflict as IToolConflict);
        }

        void SetupBindings(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel, IToolConflict firstConflict)
        {
            if (CurrentConflictModel == null)
            {
                CurrentConflictModel = new ConflictModelFactory
                {
                    Model = firstConflict?.CurrentViewModel ?? new MergeToolModel { IsMergeEnabled = false },
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
                    Model = firstConflict?.DiffViewModel ?? new MergeToolModel { IsMergeEnabled = false },
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
            CanSave = false;

            WorkflowDesignerViewModel.CanViewWorkflowLink = false;
            WorkflowDesignerViewModel.IsTestView = true;
            CurrentConflictModel.IsWorkflowNameChecked = !HasWorkflowNameConflict;
            CurrentConflictModel.IsVariablesChecked = !HasVariablesConflict;
        }

        List<IConflict> BuildConflicts(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel, (List<ConflictTreeNode> current, List<ConflictTreeNode> diff) currentChanges)
        {
            var conflicts = new List<IConflict>();

            var currentTree = currentChanges.current;
            var diffTree = currentChanges.diff;
            var armConnectorConflicts = new List<IArmConnectorConflict>();
            var currentCount = currentTree.Count;
            var diffCount = diffTree.Count;
            var maxItems = currentCount >= diffCount ? currentCount : diffCount;
            for(int i = 0; i < maxItems; i++)
            {
                if (i < currentCount)
                {
                    ProcessCurrentItem(currentResourceModel, conflicts, armConnectorConflicts, currentTree[i]);
                }
                if (i < diffCount)
                {
                    ProcessDiffItem(differenceResourceModel, conflicts, armConnectorConflicts, diffTree[i]);
                }
            }
            return conflicts;
        }

        private void ProcessDiffItem(IContextualResourceModel differenceResourceModel, List<IConflict> conflicts, List<IArmConnectorConflict> armConnectorConflicts, ConflictTreeNode treeItem)
        {
            IToolConflict conflict = null;
            var node = treeItem;
            var foundConflict = conflicts.Where(s => s is IToolConflict).Cast<IToolConflict>().FirstOrDefault(t => t.UniqueId.ToString() == node.UniqueId);
            var id = Guid.Parse(node.UniqueId);
            if (foundConflict == null)
            {
                conflict = new ToolConflict { UniqueId = id, CurrentViewModel = EmptyConflictViewModel(id) };
                conflict.CurrentViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
                conflict.CurrentViewModel.Container = conflict;
                conflicts.Add(conflict);
            }
            else
            {
                conflict = foundConflict;
            }
            var conflictTreeNode = node;
            var currentFactory = new ConflictModelFactory(differenceResourceModel, conflictTreeNode);
            conflict.DiffViewModel = currentFactory.Model;
            conflict.DiffViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
            conflict.DiffViewModel.Container = conflict;
            conflict.HasConflict = conflict.HasConflict || node.IsInConflict;
            AddDiffArmConnectors(armConnectorConflicts, treeItem, id);
            ShowArmConnectors(conflicts, armConnectorConflicts);
        }

        private void ProcessCurrentItem(IContextualResourceModel currentResourceModel, List<IConflict> conflicts, List<IArmConnectorConflict> armConnectorConflicts, ConflictTreeNode treeItem)
        {
            var conflict = new ToolConflict();
            var modelFactory = new ConflictModelFactory(currentResourceModel, treeItem);
            var id = Guid.Parse(treeItem.UniqueId);
            conflict.UniqueId = id;
            conflict.DiffViewModel = EmptyConflictViewModel(id);
            conflict.DiffViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
            conflict.CurrentViewModel = modelFactory.Model;
            conflict.CurrentViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
            conflict.CurrentViewModel.Container = conflict;
            conflict.DiffViewModel.Container = conflict;
            conflict.HasConflict = treeItem.IsInConflict;
            ShowArmConnectors(conflicts, armConnectorConflicts);
            conflicts.Add(conflict);
            AddArmConnectors(armConnectorConflicts, treeItem, id);
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

        static IEnumerable<IArmConnectorConflict> ShouldShowArmConnector(IEnumerable<string> toolIds, List<IArmConnectorConflict> armConnectorConflicts) => armConnectorConflicts.Where(s => {
            var foundCurrentDestination = FindMatchingConnector(s.CurrentArmConnector.DestinationUniqueId, toolIds);
            var foundDiffDestination = FindMatchingConnector(s.DifferentArmConnector.DestinationUniqueId, toolIds);
            var foundCurrentSource = FindMatchingConnector(s.CurrentArmConnector.SourceUniqueId, toolIds);
            var foundDiffSource = FindMatchingConnector(s.DifferentArmConnector.SourceUniqueId, toolIds);
            var hasValues = foundCurrentDestination && foundDiffDestination && foundCurrentSource && foundDiffSource;
            if (!hasValues)
            {
                if ((s.CurrentArmConnector.DestinationUniqueId == Guid.Empty.ToString() || s.CurrentArmConnector.SourceUniqueId == Guid.Empty.ToString()) && foundDiffDestination && foundDiffSource )
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
        static bool FindMatchingConnector(string connectorId, IEnumerable<string> toolIds)
        {
            var found = toolIds.Contains(connectorId);
            return found;
        }

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
            foreach (var connector in armConnectors)
            {
                var foundConnector = armConnectorConflicts.FirstOrDefault(s => s.UniqueId == id && s.Key == connector.Key);
                if (foundConnector != null)
                {
                    var mergeArmConnectorConflict = new MergeArmConnectorConflict(connector.Description, connector.SourceUniqueId, connector.DestinationUniqueId, connector.Key, foundConnector);
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
                        Key = connector.Key,
                        HasConflict = true
                    };
                    var mergeArmConnectorConflict = new MergeArmConnectorConflict(connector.Description, connector.SourceUniqueId, connector.DestinationUniqueId, connector.Key, armConnector);
                    armConnector.DifferentArmConnector = mergeArmConnectorConflict;
                    armConnector.CurrentArmConnector = EmptyMergeArmConnectorConflict(id,armConnector);
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
            foreach (var connector in armConnectors)
            {
                var armConnector = new ArmConnectorConflict
                {
                    UniqueId = id,
                    Key = connector.Key,
                    HasConflict = true
                };
                var mergeArmConnectorConflict = new MergeArmConnectorConflict(connector.Description, connector.SourceUniqueId, connector.DestinationUniqueId, connector.Key, armConnector);
                armConnector.CurrentArmConnector = mergeArmConnectorConflict;
                armConnector.DifferentArmConnector = EmptyMergeArmConnectorConflict(id, armConnector);
                armConnector.CurrentArmConnector.OnChecked += ArmCheck;
                armConnector.DifferentArmConnector.OnChecked += ArmCheck;
                armConnector.HasConflict = true;
                if (armConnectorConflicts.FirstOrDefault(s => s.UniqueId == id && s.Key == connector.Key) == null)
                {
                    armConnectorConflicts.Add(armConnector);
                }
            }
        }

        static MergeArmConnectorConflict EmptyMergeArmConnectorConflict(Guid uniqueId, IArmConnectorConflict container)
        {
            return new MergeArmConnectorConflict(container)
            {
                SourceUniqueId = uniqueId.ToString(),
                DestinationUniqueId = Guid.Empty.ToString(),
                Key = container.Key
            };
        }

        static MergeToolModel EmptyConflictViewModel(Guid uniqueId)
        {
            return new MergeToolModel
            {
                ModelItem = null,
                NodeLocation = new Point(),
                IsMergeEnabled = false,
                IsMergeVisible = false,
                UniqueId = uniqueId
            };
        }

        public MergeWorkflowViewModel(IServiceDifferenceParser serviceDifferenceParser)
        {
            _serviceDifferenceParser = serviceDifferenceParser;
        }

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

        void AddActivity(IMergeToolModel model)
        {
            var conflict = Conflicts.Where(s => s is IToolConflict).Cast<IToolConflict>().FirstOrDefault();
            if (conflict != null && conflict.UniqueId == model.UniqueId)
            {
                WorkflowDesignerViewModel.RemoveStartNodeConnection();
            }
            if (model.ModelItem != null)
            {
                WorkflowDesignerViewModel.AddItem(model);
                WorkflowDesignerViewModel.SelectedItem = model.ModelItem;
            }
        }
#pragma warning disable S1450 // Private fields only used as local variables in methods should become local variables
        bool _canSave;
#pragma warning restore S1450 // Private fields only used as local variables in methods should become local variables
        IDataListViewModel _dataListViewModel;
        readonly List<IConflict> _seenConflicts;

        void SourceOnConflictModelChanged(object sender, IConflictModelFactory args)
        {
            try
            {
                if (!HasMergeStarted)
                {
                    HasMergeStarted = args.IsWorkflowNameChecked || args.IsVariablesChecked;
                }
                if (!args.IsVariablesChecked)
                {
                    return;
                }

                IsVariablesEnabled = HasVariablesConflict;
                if (args.IsVariablesChecked)
                {
                    DataListViewModel = args.DataListViewModel;
                }
                var completeConflict = Conflicts.First.Value as IToolConflict;
                completeConflict.IsMergeExpanderEnabled = completeConflict.HasConflict;
                if (completeConflict.IsChecked)
                {
                    return;
                }
                if (completeConflict.HasConflict)
                {
                    completeConflict.DiffViewModel.IsMergeEnabled = completeConflict.HasConflict;
                    completeConflict.CurrentViewModel.IsMergeEnabled = completeConflict.HasConflict;
                }
                else
                {
                    completeConflict.CurrentViewModel.IsMergeChecked = true;
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, ex.Message);
            }
        }

        public IDataListViewModel DataListViewModel
        {
            get => _dataListViewModel;
            set
            {
                _dataListViewModel = value;
                OnPropertyChanged(nameof(DataListViewModel));
            }
        }

        void SourceOnModelToolChanged(object sender, IMergeToolModel args)
        {
            try
            {
                if (!HasMergeStarted)
                {
                    HasMergeStarted = true;
                }

                if (!args.IsMergeChecked)
                {
                    RemovePreviousTool(sender, args);
                    GetMatchingConflictParent();
                    return;
                }
                ResetToolEvents(args);
                var container = args.Container;
                RemovePreviousToolArm(container);

                container.IsMergeExpanderEnabled = container.HasConflict;
                AddActivity(args);
                var conflict = UpdateNextEnabledState(container);
                if (conflict is IToolConflict nextConflict)
                {
                    UpdateNextToolState(nextConflict);
                }
                container.IsChecked = args.IsMergeChecked;
                container.IsEmptyItemSelected = args.ModelItem == null;
                GetMatchingConflictParent();
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, ex.Message);
            }
        }

        void RemovePreviousToolArm(IConflict container)
        {
            var updateNextConflict = GetNextConlictToUpdate(container);
            if (updateNextConflict != null && updateNextConflict is IArmConnectorConflict updateNextArmConflict)
            {
                ResetToolArmEvents(updateNextArmConflict);
            }
        }

        IConflict GetNextConlictToUpdate(IConflict container)
        {
            var index = _conflicts.IndexOf(container) + 1;
            if (index < _conflicts.Count)
            {
                var nextConflict = _conflicts.ElementAt(index);
                return nextConflict;
            }
            return null;
        }

        void ResetToolArmEvents(IArmConnectorConflict args)
        {
            var index = _conflicts.IndexOf(args);
            if (index != -1 && index < _conflicts.Count)
            {
                var restOfItems = _conflicts.Skip(index).ToList();
                foreach (var conf in restOfItems)
                {
                    if (conf is IArmConnectorConflict tool && tool.HasConflict)
                    {
                        tool.CurrentArmConnector.DisableEvents();
                        tool.DifferentArmConnector.DisableEvents();
                        tool.IsChecked = false;
                        CanSave = false;
                    }
                }
            }
        }

        void RemovePreviousTool(object sender, IMergeToolModel args)
        {
            if (sender is IMergeToolModel previousToolValue)
            {
                args.Container.IsChecked = args.Container.IsChecked || previousToolValue.IsMergeChecked;
                if (previousToolValue.IsMergeChecked && !args.IsMergeChecked)
                {
                    if (args.Container.CurrentViewModel == args)
                    {
                        WorkflowDesignerViewModel.RemoveItem(args.Container.CurrentViewModel);
                    }
                    if (args.Container.DiffViewModel == args)
                    {
                        WorkflowDesignerViewModel.RemoveItem(args.Container.DiffViewModel);
                    }
                }
            }
        }

        void ResetToolEvents(IMergeToolModel args)
        {
            var index = _conflicts.IndexOf(args.Container);
            if (index != -1 && index + 1 < _conflicts.Count)
            {
                var restOfItems = _conflicts.Skip(index + 1).ToList();
                foreach (var conf in restOfItems)
                {
                    if (conf is IToolConflict tool && tool.HasConflict)
                    {
                        ResetToolConflict(tool.DiffViewModel);
                        ResetToolConflict(tool.CurrentViewModel);
                    }
                }
            }
        }

        void ResetToolConflict(IMergeToolModel mergeTool)
        {
            if (mergeTool != null)
            {
                mergeTool.DisableEvents();
                mergeTool.EnableEvents();
                _seenConflicts.Remove(mergeTool.Container);
                WorkflowDesignerViewModel.RemoveItem(mergeTool);
            }
        }

        static void UpdateNextToolState(IToolConflict nextConflict)
        {
            nextConflict.IsMergeExpanderEnabled = nextConflict.HasConflict;
            if (!nextConflict.HasConflict || nextConflict.IsContainerTool)
            {
                ExpandPreviousItems(nextConflict);
                nextConflict.CurrentViewModel.IsMergeChecked = true;
                nextConflict.CurrentViewModel.IsMergeEnabled = false;
                nextConflict.DiffViewModel.IsMergeEnabled = false;
            }
        }

        void ArmCheck(IArmConnectorConflict container,bool isChecked, string sourceUniqueId, string destionationUniqueId, string key)
        {
            if (isChecked)
            {
                WorkflowDesignerViewModel.LinkTools(sourceUniqueId, destionationUniqueId, key);

                RemovePreviousToolArm(container);
                var conflict = UpdateNextEnabledState(container);
                if (conflict is IToolConflict nextConflict)
                {
                    UpdateNextToolState(nextConflict);
                }
                if (conflict is IArmConnectorConflict nextArmConflict)
                {
                    UpdateNextArmState(nextArmConflict);
                }
                container.IsChecked = isChecked;
                GetMatchingConflictParent();
            }
        }

        static void UpdateNextArmState(IArmConnectorConflict nextArmConflict)
        {
            nextArmConflict.IsMergeExpanderEnabled = nextArmConflict.HasConflict;
            if (!nextArmConflict.HasConflict)
            {
                nextArmConflict.CurrentArmConnector.IsChecked = true;
                nextArmConflict.CurrentArmConnector.IsArmSelectionAllowed = false;
                nextArmConflict.DifferentArmConnector.IsArmSelectionAllowed = false;
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

        IConflict GetNextConflict(IConflict conflict)
        {
            var idx = _conflicts.IndexOf(conflict);
            var nextConflict = MoveNext(idx);
            while (_seenConflicts.Contains(nextConflict) && nextConflict!=null)
            {
                idx = idx + 1;
                nextConflict = MoveNext(idx);
            }
            _seenConflicts.Add(nextConflict);
            return nextConflict;
        }

        IConflict MoveNext(int idx)
        {
            var nextId = idx + 1;
            if (nextId >= _conflicts.Count)
            {
                return null;
            }
            var nextConflict = _conflicts[nextId];
            return nextConflict;
        }

        IConflict UpdateNextEnabledState(IConflict currentConflict)
        {
            if (Conflicts == null)
            {
                return null;
            }

            var conflict = GetNextConflict(currentConflict);
            if (conflict == null)
            {
                return null;
            }
            var nextConflict = conflict as IToolConflict;
            if (nextConflict != null)
            {
                nextConflict.CurrentViewModel.IsMergeEnabled = nextConflict.HasConflict;
                nextConflict.DiffViewModel.IsMergeEnabled = nextConflict.HasConflict;
            }
            else
            {
                var nextArmConflict = conflict as IArmConnectorConflict;
                if (nextArmConflict != null)
                {
                    GetMatchingConflictParent();
                }
                return nextArmConflict;
            }
            return nextConflict;
        }

        void GetMatchingConflictParent()
        {
            var items = _conflicts.Where(s => s is IArmConnectorConflict).Cast<IArmConnectorConflict>().ToList();
            var toolIds = _conflicts.Where(s => s is IToolConflict && s.IsChecked && !s.IsEmptyItemSelected).Select(a => a.UniqueId.ToString());
            foreach (var s in items) {
                var foundCurrentDestination = FindMatchingConnector(s.CurrentArmConnector.DestinationUniqueId, toolIds);
                var foundDiffDestination = FindMatchingConnector(s.DifferentArmConnector.DestinationUniqueId, toolIds);
                var foundCurrentSource = FindMatchingConnector(s.CurrentArmConnector.SourceUniqueId, toolIds);
                var foundDiffSource = FindMatchingConnector(s.DifferentArmConnector.SourceUniqueId, toolIds);

                if(foundCurrentDestination && foundCurrentSource)
                {
                    s.CurrentArmConnector.IsArmSelectionAllowed = true;
                    s.IsMergeExpanderEnabled = true;
                }
                if(foundDiffDestination && foundDiffSource)
                {
                    s.DifferentArmConnector.IsArmSelectionAllowed = true;
                    s.IsMergeExpanderEnabled = true;
                }
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
                _resourceModel.Environment.ResourceRepository.SaveToServer(_resourceModel);

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
                if (!DisplayName.EndsWith(" *",StringComparison.InvariantCultureIgnoreCase))
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
                    canSave &= All(conflict => conflict.IsChecked);
                }
                return canSave;
            }
            set
            {
                _canSave = value;
                OnPropertyChanged(() => CanSave);
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
}
