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
        List<IConflict> _conflicts;

        public MergeWorkflowViewModel(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel, bool loadworkflowFromServer)
        : this(CustomContainer.Get<IServiceDifferenceParser>())
        {
            WorkflowDesignerViewModel = new WorkflowDesignerViewModel(currentResourceModel, false);
            WorkflowDesignerViewModel.CreateBlankWorkflow();

            _resourceModel = currentResourceModel;

            var currentChanges = _serviceDifferenceParser.GetDifferences(currentResourceModel, differenceResourceModel, loadworkflowFromServer);
            _conflicts = BuildConflicts(currentResourceModel, differenceResourceModel, currentChanges);
            Conflicts = new LinkedList<IConflict>(_conflicts);
            _conflictEnumerator = Conflicts.GetEnumerator();
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
            ProcessCurrent(currentResourceModel, conflicts, currentTree, armConnectorConflicts);
            ProcessDiff(differenceResourceModel, conflicts, diffTree, armConnectorConflicts);
            return conflicts;
        }

        void ProcessDiff(IContextualResourceModel differenceResourceModel, List<IConflict> conflicts, List<ConflictTreeNode> diffTree, List<IArmConnectorConflict> armConnectorConflicts)
        {
            if (diffTree != null)
            {
                foreach (var treeItem in diffTree)
                {
                    IToolConflict conflict = null;
                    var node = treeItem;
                    var foundConflict = conflicts.Where(s => s is IToolConflict).Cast<IToolConflict>().FirstOrDefault(t => t.UniqueId.ToString() == node.UniqueId);
                    var id = Guid.Parse(node.UniqueId);
                    if (foundConflict == null)
                    {
                        conflict = new ToolConflict { UniqueId = id, CurrentViewModel = EmptyConflictViewModel(id) };
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
                    ShowArmConnectorsForDiff(conflicts, armConnectorConflicts);
                }
            }
        }

        void ProcessCurrent(IContextualResourceModel currentResourceModel, List<IConflict> conflicts, List<ConflictTreeNode> currentTree, List<IArmConnectorConflict> armConnectorConflicts)
        {
            if (currentTree != null)
            {
                foreach (var treeItem in currentTree)
                {
                    var conflict = new ToolConflict();
                    var modelFactory = new ConflictModelFactory(currentResourceModel, treeItem);
                    var id = Guid.Parse(treeItem.UniqueId);
                    conflict.UniqueId = id;
                    conflict.DiffViewModel = EmptyConflictViewModel(id);
                    conflict.CurrentViewModel = modelFactory.Model;
                    conflict.CurrentViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
                    conflict.CurrentViewModel.Container = conflict;
                    conflict.HasConflict = treeItem.IsInConflict;
                    conflicts.Add(conflict);
                    AddArmConnectors(armConnectorConflicts, treeItem, id);
                    ShowArmConnectorsForCurrent(conflicts, armConnectorConflicts);
                }
            }
        }

        static void ShowArmConnectorsForDiff(List<IConflict> conflicts, List<IArmConnectorConflict> armConnectorConflicts)
        {
            var itemsToAdd = new List<IConflict>();
            foreach (var addConflict in conflicts)
            {
                var found = armConnectorConflicts.FirstOrDefault(s => s.DifferentArmConnector.DestinationUniqueId == addConflict.UniqueId.ToString());
                AddToTempConflictList(conflicts, itemsToAdd, found);

            }
            conflicts.AddRange(itemsToAdd);
        }

        static void ShowArmConnectorsForCurrent(List<IConflict> conflicts, List<IArmConnectorConflict> armConnectorConflicts)
        {
            var itemsToAdd = new List<IConflict>();
            foreach (var addConflict in conflicts)
            {
                var found = armConnectorConflicts.FirstOrDefault(s => s.CurrentArmConnector.DestinationUniqueId == addConflict.UniqueId.ToString());
                AddToTempConflictList(conflicts, itemsToAdd, found);

            }
            conflicts.AddRange(itemsToAdd);
        }

        static void AddToTempConflictList(List<IConflict> conflicts, List<IConflict> itemsToAdd, IArmConnectorConflict found)
        {
            if (found != null && conflicts.Where(t => t is IArmConnectorConflict)?.FirstOrDefault(s => s.UniqueId == found.UniqueId && ((IArmConnectorConflict)s).Key == found.Key) == null)
            {
                itemsToAdd.Add(found);
            }
        }

        static void AddDiffArmConnectors(List<IArmConnectorConflict> armConnectorConflicts, ConflictTreeNode treeItem, Guid id)
        {
            var armConnectors = treeItem.Activity.ArmConnectors();
            foreach (var connector in armConnectors)
            {
                var mergeArmConnectorConflict = new MergeArmConnectorConflict(connector.Description, connector.SourceUniqueId, connector.DestinationUniqueId, connector.Key);
                var foundConnector = armConnectorConflicts.FirstOrDefault(s => s.UniqueId == id && s.Key == connector.Key);
                if (foundConnector != null)
                {
                    foundConnector.DifferentArmConnector = mergeArmConnectorConflict;
                    var hasConflict = !foundConnector.CurrentArmConnector.Equals(foundConnector.DifferentArmConnector);
                    foundConnector.HasConflict = hasConflict;
                    foundConnector.DifferentArmConnector.IsArmSelectionAllowed = hasConflict;
                    foundConnector.CurrentArmConnector.IsArmSelectionAllowed = hasConflict;
                    foundConnector.IsMergeExpanderEnabled = hasConflict;
                }
                else
                {
                    var armConnector = new ArmConnectorConflict
                    {
                        UniqueId = id,
                        CurrentArmConnector = EmptyMergeArmConnectorConflict(id),
                        Key = connector.Key,
                        HasConflict = true

                    };
                    armConnectorConflicts.Add(armConnector);
                }
            }
        }

        void AddArmConnectors(List<IArmConnectorConflict> armConnectorConflicts, ConflictTreeNode treeItem, Guid id)
        {
            var armConnectors = treeItem.Activity.ArmConnectors();
            foreach (var connector in armConnectors)
            {
                var mergeArmConnectorConflict = new MergeArmConnectorConflict(connector.Description, connector.SourceUniqueId, connector.DestinationUniqueId, connector.Key);
                mergeArmConnectorConflict.OnChecked += ArmCheck;
                var armConnector = new ArmConnectorConflict
                {
                    UniqueId = id,
                    CurrentArmConnector = mergeArmConnectorConflict,
                    DifferentArmConnector = EmptyMergeArmConnectorConflict(id),
                    Key = connector.Key,
                    HasConflict = true
                };
                armConnector.CurrentArmConnector.IsArmSelectionAllowed = true;
                if (armConnectorConflicts.FirstOrDefault(s => s.UniqueId == id && s.Key == connector.Key) == null)
                {
                    armConnectorConflicts.Add(armConnector);
                }
            }
        }

        static MergeArmConnectorConflict EmptyMergeArmConnectorConflict(Guid uniqueId)
        {
            return new MergeArmConnectorConflict
            {
                SourceUniqueId = uniqueId.ToString(),
                DestinationUniqueId = uniqueId.ToString()
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
            var conflictsMatch = Conflicts.All(check);
            var childrenMatch = true;
            foreach (var completeConflict in Conflicts)
            {
                if (completeConflict is IToolConflict toolConflict)
                {
                    childrenMatch &= toolConflict.All(check);
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
            WorkflowDesignerViewModel.AddItem(model);
            WorkflowDesignerViewModel.SelectedItem = model.ModelItem;
        }
#pragma warning disable S1450 // Private fields only used as local variables in methods should become local variables
        bool _canSave;
#pragma warning restore S1450 // Private fields only used as local variables in methods should become local variables
        readonly IEnumerator<IConflict> _conflictEnumerator;
        IDataListViewModel _dataListViewModel;

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
                    _conflictEnumerator.MoveNext();
                }
                var completeConflict = Conflicts.First.Value as IToolConflict;
                completeConflict.IsMergeExpanderEnabled = completeConflict.HasConflict;
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
            get
            {
                return _dataListViewModel;
            }
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
                if (!args.IsMergeChecked)
                {
                    return;
                }

                if (!HasMergeStarted)
                {
                    HasMergeStarted = true;
                }
                if (sender is IMergeToolModel previousToolValue)
                {
                    args.Container.IsChecked = args.Container.IsChecked || previousToolValue.IsMergeChecked;
                    if (!previousToolValue.IsMergeChecked && args.IsMergeChecked)
                    {
                        if (args.Container.CurrentViewModel == args)
                        {
                            var index = _conflicts.IndexOf(args.Container);
                            if(index!= -1 && index+1 < _conflicts.Count -1)
                            {
                                var restOfItems = _conflicts.Skip(index+1).ToList();
                                foreach(var conf in restOfItems)
                                {
                                    if(conf is IToolConflict tool)
                                    {
                                        if (tool.HasConflict)
                                        {
                                            if (tool.DiffViewModel != null)
                                            {
                                                tool.DiffViewModel.DisableEvents();
                                                tool.DiffViewModel.IsMergeChecked = false;
                                                tool.DiffViewModel.EnableEvents();
                                            }
                                            if (tool.CurrentViewModel != null)
                                            {
                                                tool.CurrentViewModel.DisableEvents();
                                                tool.CurrentViewModel.IsMergeChecked = false;
                                                tool.CurrentViewModel.EnableEvents();
                                            }
                                        }
                                    }
                                }
                            }
                            WorkflowDesignerViewModel.RemoveItem(args.Container.DiffViewModel);
                        }
                        if (args.Container.DiffViewModel == args)
                        {
                            var index = _conflicts.IndexOf(args.Container);
                            if (index != -1 && index + 1 < _conflicts.Count - 1)
                            {
                                var restOfItems = _conflicts.Skip(index+1).ToList();
                                foreach(var conf in restOfItems)
                                {
                                    if(conf is IToolConflict tool)
                                    {
                                        if (tool.HasConflict)
                                        {
                                            if (tool.DiffViewModel != null)
                                            {
                                                tool.DiffViewModel.DisableEvents();
                                                tool.DiffViewModel.IsMergeChecked = false;
                                                tool.DiffViewModel.EnableEvents();
                                            }
                                            if (tool.CurrentViewModel != null)
                                            {
                                                tool.CurrentViewModel.DisableEvents();
                                                tool.CurrentViewModel.IsMergeChecked = false;
                                                tool.CurrentViewModel.EnableEvents();
                                            }
                                        }
                                    }
                                }
                            }
                            WorkflowDesignerViewModel.RemoveItem(args.Container.CurrentViewModel);
                        }
                    }
                }
                args.Container.IsMergeExpanderEnabled = args.Container.HasConflict;
                AddActivity(args);
                if (!args.Container.IsChecked)
                {
                    var conflict = UpdateNextEnabledState();
                    if (conflict is IToolConflict nextConflict)
                    {
                        UpdateNextToolState(nextConflict);
                    }
                    if (conflict is IArmConnectorConflict nextArmConflict)
                    {
                        UpdateNextArmState(nextArmConflict);
                    }
                }
                args.Container.IsChecked = args.IsMergeChecked;
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, ex.Message);
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

        void ArmCheck(bool isChecked, string sourceUniqueId, string destionationUniqueId, string key)
        {
            if (isChecked)
            {
                WorkflowDesignerViewModel.LinkTools(sourceUniqueId, destionationUniqueId, key);

                var conflict = UpdateNextEnabledState();
                if (conflict is IArmConnectorConflict nextArmConflict)
                {
                    UpdateNextArmState(nextArmConflict);
                }
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

        IConflict GetNextConflict()
        {
            var current = _conflictEnumerator.Current;
            if (current != null)
            {                
                if (_conflictEnumerator.MoveNext())
                {
                    current = _conflictEnumerator.Current;
                    return current;
                }
            }
            _conflictEnumerator.MoveNext();
            return _conflictEnumerator.Current;
        }

        IConflict UpdateNextEnabledState()
        {
            if (Conflicts == null)
            {
                return null;
            }

            var conflict = GetNextConflict();
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
                    nextArmConflict.CurrentArmConnector.IsArmSelectionAllowed = nextArmConflict.HasConflict;
                    nextArmConflict.DifferentArmConnector.IsArmSelectionAllowed = nextArmConflict.HasConflict;
                }
                return nextArmConflict;
            }
            return nextConflict;
        }

        

        public LinkedList<IConflict> Conflicts { get; set; }

        public IWorkflowDesignerViewModel WorkflowDesignerViewModel { get; set; }

        public IConflictModelFactory CurrentConflictModel { get; set; }
        public IConflictModelFactory DifferenceConflictModel { get; set; }

        public void Save()
        {
            try
            {
                if (HasWorkflowNameConflict)
                {
                    var resourceName = CurrentConflictModel.IsWorkflowNameChecked ? CurrentConflictModel.WorkflowName : DifferenceConflictModel.WorkflowName;
                    _resourceModel.Environment.ExplorerRepository.UpdateManagerProxy.Rename(_resourceModel.ID, resourceName);
                }
                if (HasVariablesConflict)
                {
                    _resourceModel.DataList = CurrentConflictModel.IsVariablesChecked ? CurrentConflictModel.DataListViewModel.WriteToResourceModel() : DifferenceConflictModel.DataListViewModel.WriteToResourceModel();
                }
                _resourceModel.WorkflowXaml = WorkflowDesignerViewModel.ServiceDefinition;
                _resourceModel.Environment.ResourceRepository.Save(_resourceModel);
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
            get => All(conflict => conflict.IsChecked);
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
                _hasMergeStarted = value;
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
            _conflictEnumerator.Dispose();
        }

        public void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel?.UpdateHelpText(helpText);
        }
    }
}
