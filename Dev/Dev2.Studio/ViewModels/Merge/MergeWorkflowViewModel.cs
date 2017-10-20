using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.Workflow;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common;
using System.Activities.Presentation.Model;
using System.Windows;
using Caliburn.Micro;
using Dev2.Common.Common;
using Dev2.Studio.Interfaces.DataList;

namespace Dev2.ViewModels.Merge
{
    public class MergeWorkflowViewModel : BindableBase, IMergeWorkflowViewModel
    {
        private readonly IServiceDifferenceParser _serviceDifferenceParser;
        private string _displayName;
        private bool _hasMergeStarted;
        private bool _hasWorkflowNameConflict;
        private bool _hasVariablesConflict;
        private bool _isVariablesEnabled;
        private bool _isMergeExpanderEnabled;
        private readonly IContextualResourceModel _resourceModel;

        public MergeWorkflowViewModel(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel, bool loadworkflowFromServer)
        : this(CustomContainer.Get<IServiceDifferenceParser>())
        {
            WorkflowDesignerViewModel = new WorkflowDesignerViewModel(currentResourceModel, false);
            WorkflowDesignerViewModel.CreateBlankWorkflow();

            _resourceModel = currentResourceModel;

            var currentChanges = _serviceDifferenceParser.GetDifferences(currentResourceModel, differenceResourceModel, loadworkflowFromServer);

            var conflicts = BuildConflicts(currentResourceModel, differenceResourceModel, currentChanges);
            Conflicts = new LinkedList<ICompleteConflict>(conflicts);
            _conflictEnumerator = Conflicts.GetEnumerator();
            var firstConflict = Conflicts.FirstOrDefault();
            SetupBindings(currentResourceModel, differenceResourceModel, firstConflict);
        }

        private void SetupBindings(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel, ICompleteConflict firstConflict)
        {
            if (CurrentConflictModel == null)
            {
                CurrentConflictModel = new ConflictModelFactory
                {
                    Model = firstConflict?.CurrentViewModel ?? new MergeToolModel {IsMergeEnabled = false},
                    WorkflowName = currentResourceModel.ResourceName,
                    ServerName = currentResourceModel.Environment.DisplayName
                };
                CurrentConflictModel.GetDataList(currentResourceModel);
                CurrentConflictModel.SomethingConflictModelChanged += SourceOnConflictModelChanged;
            }

            if (DifferenceConflictModel == null)
            {
                DifferenceConflictModel = new ConflictModelFactory
                {
                    Model = firstConflict?.DiffViewModel ?? new MergeToolModel {IsMergeEnabled = false},
                    WorkflowName = differenceResourceModel.ResourceName,
                    ServerName = differenceResourceModel.Environment.DisplayName
                };
                DifferenceConflictModel.GetDataList(differenceResourceModel);
                DifferenceConflictModel.SomethingConflictModelChanged += SourceOnConflictModelChanged;
            }

            HasMergeStarted = false;

            HasVariablesConflict = currentResourceModel.DataList != differenceResourceModel.DataList;
            HasWorkflowNameConflict = currentResourceModel.ResourceName != differenceResourceModel.ResourceName;
            IsVariablesEnabled = !HasWorkflowNameConflict && HasVariablesConflict;
            IsMergeExpanderEnabled = !IsVariablesEnabled;

            DisplayName = "Merge";
            CanSave = false;

            WorkflowDesignerViewModel.CanViewWorkflowLink = false;
            WorkflowDesignerViewModel.IsTestView = true;
            CurrentConflictModel.IsWorkflowNameChecked = !HasWorkflowNameConflict;
            CurrentConflictModel.IsVariablesChecked = !HasVariablesConflict;
        }

        private List<ICompleteConflict> BuildConflicts(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel, List<(Guid uniqueId, IConflictNode currentNode, IConflictNode differenceNode, bool hasConflict)> currentChanges)
        {
            var conflicts = new List<ICompleteConflict>();
            foreach (var currentChange in currentChanges)
            {
                var conflict = new CompleteConflict { UniqueId = currentChange.uniqueId };
                if (currentChange.currentNode != null)
                {
                    var factoryA = new ConflictModelFactory(currentChange.currentNode, currentResourceModel);
                    conflict.CurrentViewModel = factoryA.Model;
                    factoryA.OnModelItemChanged += (modelItem, modelTool) =>
                    {
                        if (modelTool.IsMergeChecked)
                        {
                            WorkflowDesignerViewModel.UpdateModelItem(modelItem, modelTool);
                            //AddActivity(modelTool);
                        }
                    };
                    conflict.CurrentViewModel.Container = conflict;
                    conflict.CurrentViewModel.IsMergeVisible = currentChange.hasConflict;
                    conflict.CurrentViewModel.FlowNode = currentChange.currentNode.CurrentFlowStep;
                    conflict.CurrentViewModel.NodeLocation = currentChange.currentNode.NodeLocation;
                    conflict.CurrentViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
                    conflict.CurrentViewModel.Children?.Flatten(model => model.Children).ToList().Apply(model =>
                    {
                        if (model != null)
                        {
                            model.SomethingModelToolChanged += SourceOnModelToolChanged;
                        }
                    });
                }
                else
                {
                    conflict.CurrentViewModel = EmptyConflictViewModel(currentChange.uniqueId);
                    conflict.CurrentViewModel.Container = conflict;
                    conflict.CurrentViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
                }

                if (currentChange.differenceNode != null)
                {
                    var factoryB = new ConflictModelFactory(currentChange.differenceNode, differenceResourceModel);
                    factoryB.OnModelItemChanged += (modelItem, modelTool) =>
                    {
                        if (modelTool.IsMergeChecked)
                        {
                            var item = WorkflowDesignerViewModel.UpdateModelItem(modelItem, modelTool);

                            //var container = modelTool.Container;
                            //if (container.CurrentViewModel.IsMergeChecked)
                            //{
                            //    WorkflowDesignerViewModel.RemoveItem(container.CurrentViewModel);
                            //}
                            //if (container.DiffViewModel.IsMergeChecked)
                            //{
                            //    WorkflowDesignerViewModel.RemoveItem(container.DiffViewModel);
                            //}
                            //AddActivity(modelTool);
                        }
                    };
                    conflict.DiffViewModel = factoryB.Model;
                    conflict.DiffViewModel.Container = conflict;
                    conflict.DiffViewModel.IsMergeVisible = currentChange.hasConflict;
                    conflict.DiffViewModel.FlowNode = currentChange.differenceNode.CurrentFlowStep;
                    conflict.DiffViewModel.NodeLocation = currentChange.differenceNode.NodeLocation;
                    conflict.DiffViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
                    conflict.DiffViewModel.Children?.Flatten(model => model.Children).ToList().Apply(model =>
                    {
                        if (model != null)
                        {
                            model.SomethingModelToolChanged += SourceOnModelToolChanged;
                        }
                    });
                }
                else
                {
                    conflict.DiffViewModel = EmptyConflictViewModel(currentChange.uniqueId);
                    conflict.DiffViewModel.Container = conflict;
                    conflict.DiffViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
                }

                conflict.HasConflict = currentChange.hasConflict;
                conflict.IsMergeExpanded = false;
                conflict.IsMergeExpanderEnabled = false;

                if (conflict.CurrentViewModel.FlowNode == null && conflict.DiffViewModel?.Children?.Count > 0)
                {
                    
                }
                else if (conflict.DiffViewModel?.FlowNode == null && conflict.CurrentViewModel?.Children?.Count > 0)
                {
                    
                }
                else
                {
                    AddChildren(conflict, conflict.CurrentViewModel, conflict.DiffViewModel);
                }
                conflicts.Add(conflict);
            }
            return conflicts;
        }

        private static MergeToolModel EmptyConflictViewModel(Guid uniqueId)
        {
            return new MergeToolModel
            {
                FlowNode = null,
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

        private LinkedListNode<ICompleteConflict> Find(ICompleteConflict itemToFind)
        {
            var linkedConflict = Conflicts.Find(itemToFind);
            if (linkedConflict != null)
            {
                return linkedConflict;
            }
            foreach (var completeConflict in Conflicts)
            {
                var childItem = completeConflict.Find(itemToFind);
                if (childItem != null)
                {
                    return childItem;
                }
            }
            return null;
        }

        private bool All(Func<ICompleteConflict,bool> check)
        {
            var conflictsMatch =  Conflicts.All(check);
            var childrenMatch = true;
            foreach (var completeConflict in Conflicts)
            {
                childrenMatch &= completeConflict.All(check);
            }
            return conflictsMatch && childrenMatch;
        }

        private void AddActivity(IMergeToolModel model)
        {
            var conflict = Conflicts.FirstOrDefault();
            if (conflict != null && conflict.UniqueId == model.UniqueId)
            {
                WorkflowDesignerViewModel.RemoveStartNodeConnection();
            }
            var linkedConflict = Find(model.Container);
            IMergeToolModel previous = null;
            IMergeToolModel next = null;

            if (linkedConflict != null)
            {
                previous = SetPreviousModelTool(linkedConflict);
                next = SetNextModelTool(linkedConflict);
            }
            WorkflowDesignerViewModel.AddItem(previous, model, next);
            WorkflowDesignerViewModel.SelectedItem = model.FlowNode;
        }

        static IMergeToolModel SetNextModelTool(LinkedListNode<ICompleteConflict> linkedConflict)
        {
            IMergeToolModel next = null;
            var nextValue = linkedConflict.Next?.Value;
            var nextValueCurrentViewModel = nextValue?.CurrentViewModel;
            if (nextValue?.Parent != null && nextValueCurrentViewModel == null)
            {
                nextValue = nextValue.Parent;
                nextValueCurrentViewModel = nextValue?.CurrentViewModel;
            }
            if (nextValueCurrentViewModel != null)
            {
                if (nextValueCurrentViewModel.IsMergeChecked)
                {
                    next = nextValueCurrentViewModel;
                }
                else
                {
                    if (nextValue.DiffViewModel != null && nextValue.DiffViewModel.IsMergeChecked)
                    {
                        next = nextValue.DiffViewModel;
                    }
                }
            }
            return next;
        }

        static IMergeToolModel SetPreviousModelTool(LinkedListNode<ICompleteConflict> linkedConflict)
        {
            IMergeToolModel previous = null;
            var previousValue = linkedConflict.Previous?.Value ?? linkedConflict.Value.Parent;            
            var previousCurrentViewModel = previousValue?.CurrentViewModel;
            if (previousValue?.Parent != null && previousCurrentViewModel==null)
            {
                previousValue = previousValue.Parent;
                previousCurrentViewModel = previousValue?.CurrentViewModel;
            }
            if (previousCurrentViewModel != null)
            {
                if (previousCurrentViewModel.IsMergeChecked)
                {
                    previous = previousCurrentViewModel;
                }
                else
                {
                    if (previousValue.DiffViewModel != null && previousValue.DiffViewModel.IsMergeChecked)
                    {
                        previous = previousValue.DiffViewModel;
                    }
                }
            }
            return previous;
        }

        private bool _canSave;
        private readonly IEnumerator<ICompleteConflict> _conflictEnumerator;
        private IDataListViewModel _dataListViewModel;

        private void SourceOnConflictModelChanged(object sender, IConflictModelFactory args)
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
                IsMergeExpanderEnabled = true;
                var completeConflict = Conflicts.First.Value;
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

        public IDataListViewModel DataListViewModel { 
                get => _dataListViewModel;
            set{
                _dataListViewModel = value;
                OnPropertyChanged("DataListViewModel");
            }
        }

        private void SourceOnModelToolChanged(object sender, IMergeToolModel args)
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
                            WorkflowDesignerViewModel.RemoveItem(args.Container.DiffViewModel);
                        }
                        if (args.Container.DiffViewModel == args)
                        {
                            WorkflowDesignerViewModel.RemoveItem(args.Container.CurrentViewModel);
                        }
                    }
                }
                
                AddActivity(args);
                if (!args.Container.IsChecked)
                {
                    var nextConflict = UpdateNextEnabledState();
                    if (nextConflict != null && !nextConflict.HasConflict)
                    {
                        nextConflict.CurrentViewModel.IsMergeChecked = true;
                        nextConflict.CurrentViewModel.IsMergeEnabled = false;
                        nextConflict.DiffViewModel.IsMergeEnabled = false;
                    }
                }
                args.Container.IsChecked = args.IsMergeChecked;
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, ex.Message);
            }
        }

        private ICompleteConflict GetNextConflict()
        {
            var current = _conflictEnumerator.Current;
            if (current != null)
            {
                if (current.Children.Count > 0)
                {
                    var nextConflict = current.GetNextConflict();
                    if (nextConflict != null)
                    {
                        return nextConflict;
                    }
                }                
                if (_conflictEnumerator.MoveNext())
                {                    
                    current = _conflictEnumerator.Current;
                    return current;
                }
            }
            _conflictEnumerator.MoveNext();
            return _conflictEnumerator.Current;
        }

        private ICompleteConflict UpdateNextEnabledState()
        {
            if (Conflicts == null)
            {
                return null;
            }
           
            var nextCurrConflict = GetNextConflict();
            if (nextCurrConflict != null)
            {
                nextCurrConflict.CurrentViewModel.IsMergeEnabled = nextCurrConflict.HasConflict;
                nextCurrConflict.DiffViewModel.IsMergeEnabled = nextCurrConflict.HasConflict;
            }
            return nextCurrConflict;
        }

        void AddChildren(ICompleteConflict parent, IMergeToolModel currentChild, IMergeToolModel childDiff)
        {
            var childNodes = _serviceDifferenceParser.GetAllNodes();
            if (currentChild == null && childDiff == null)
            {
                return;
            }

            if (currentChild != null && childDiff != null)
            {
                var currentChildChildren = currentChild.Children;
                var difChildChildren = childDiff.Children;
                if (currentChildChildren.Count < 1 && difChildChildren.Count < 1)
                {
                    return;
                }
                var count = Math.Max(currentChildChildren.Count, difChildChildren.Count);
                var remoteCopy = new ObservableCollection<IMergeToolModel>();
                var copy = difChildChildren.ToArray().Clone();
                var arracyCopy = copy as IMergeToolModel[];
                remoteCopy = arracyCopy?.ToList().ToObservableCollection();
                for (var index = 0; index <= count; index++)
                {
                    var completeConflict = new CompleteConflict();
                    try
                    {
                        var currentChildChild = currentChildChildren[index];
                        if (currentChildChild == null)
                        {
                            continue;
                        }
                        if (parent.Children.Any(conflict => conflict.UniqueId.Equals(currentChildChild.UniqueId) && conflict.CurrentViewModel.ParentDescription.Equals(currentChildChild.ParentDescription)))
                        {
                            continue;
                        }

                        var childCurrent = GetMergeToolItem(currentChildChildren, currentChildChild.UniqueId, currentChildChild.ParentDescription);
                        var childDifferent = GetMergeToolItem(difChildChildren, currentChildChild.UniqueId, currentChildChild.ParentDescription);
                        if (childNodes.TryGetValue(currentChildChild.UniqueId.ToString(), out (ModelItem leftItem, ModelItem rightItem) item))
                        {
                            var local1 = currentChildChildren.Where(p => p.UniqueId == currentChildChild.UniqueId);
                            foreach (var c in local1)
                            {
                                c.FlowNode = item.leftItem;
                            }
                            var local2 = difChildChildren.Where(p => p.UniqueId == currentChildChild.UniqueId);
                            foreach (var c in local2)
                            {
                                c.FlowNode = item.leftItem;
                            }
                        }
                        
                        completeConflict.UniqueId = currentChildChild.UniqueId;
                        completeConflict.CurrentViewModel = childCurrent;
                        
                        if (completeConflict.CurrentViewModel != null)
                        {
                            completeConflict.CurrentViewModel.Container = completeConflict;
                        }
                        completeConflict.DiffViewModel = childDifferent;
                        if (completeConflict.DiffViewModel != null)
                        {
                            completeConflict.DiffViewModel.Container = completeConflict;
                           
                        }
                        else
                        {
                            completeConflict.DiffViewModel = EmptyConflictViewModel(currentChildChild.UniqueId);
                        }
                        
                        completeConflict.Parent = parent;
                        
                        completeConflict.HasConflict = completeConflict.DiffViewModel.FlowNode == null || _serviceDifferenceParser.NodeHasConflict(currentChildChild.UniqueId.ToString());
                        completeConflict.DiffViewModel.IsMergeVisible = completeConflict.HasConflict;
                        completeConflict.CurrentViewModel.IsMergeVisible = completeConflict.HasConflict;
                        if (parent.Children.Count == 0)
                        {
                            parent.Children.AddFirst(completeConflict);
                        }
                        else
                        {
                            parent.Children.AddLast(completeConflict);
                        }
                        AddChildren(completeConflict, childCurrent, childDifferent);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        if (difChildChildren.Any())
                        {
                            foreach (var mergeToolModel in remoteCopy.ToList())
                            {
                                if (parent.Children.Any(conflict => conflict.UniqueId.Equals(mergeToolModel.UniqueId) && conflict.DiffViewModel.ParentDescription.Equals(mergeToolModel.ParentDescription)))
                                {
                                    continue;
                                }
                                var conflictChild = new CompleteConflict
                                {
                                    UniqueId = mergeToolModel.UniqueId,
                                    CurrentViewModel = EmptyConflictViewModel(mergeToolModel.UniqueId),
                                    DiffViewModel = mergeToolModel
                                };
                                conflictChild.DiffViewModel.Container = conflictChild;
                                if (childNodes.TryGetValue(mergeToolModel.UniqueId.ToString(), out (ModelItem leftItem, ModelItem rightItem) item))
                                {
                                    conflictChild.DiffViewModel.FlowNode = item.rightItem;
                                }
                                
                                conflictChild.HasConflict = true;
                                if (parent.Children.Count == 0)
                                {
                                    parent.Children.AddFirst(conflictChild);
                                }
                                else
                                {
                                    parent.Children.AddLast(conflictChild);
                                }
                                conflictChild.DiffViewModel.IsMergeVisible = completeConflict.HasConflict;
                                conflictChild.CurrentViewModel.IsMergeVisible = completeConflict.HasConflict;
                                remoteCopy.Remove(mergeToolModel);
                                AddChildren(conflictChild, null, mergeToolModel);
                            }
                        }
                    }
                }
            }

            if (currentChild == null)
            {
                var difChildChildren = childDiff.Children;
                foreach (var diffChild in difChildChildren)
                {
                    var model = GetMergeToolItem(difChildChildren, diffChild.UniqueId, diffChild.ParentDescription);
                    var completeConflict = new CompleteConflict();
                    completeConflict.CurrentViewModel = EmptyConflictViewModel(diffChild.UniqueId);
                    completeConflict.UniqueId = diffChild.UniqueId;
                    completeConflict.DiffViewModel = model;
                    completeConflict.DiffViewModel.Container = completeConflict;
                    completeConflict.HasConflict = true;
                    if (childNodes.TryGetValue(model.UniqueId.ToString(), out (ModelItem leftItem, ModelItem rightItem) item))
                    {
                        completeConflict.DiffViewModel.FlowNode = item.rightItem;
                    }
                    if (parent.Children.Count == 0)
                    {
                        parent.Children.AddFirst(completeConflict);
                    }
                    else
                    {
                        parent.Children.AddLast(completeConflict);
                    }
                }
            }
            if (childDiff == null)
            {
                var difChildChildren = currentChild.Children;
                foreach (var diffChild in difChildChildren)
                {
                    var model = GetMergeToolItem(difChildChildren, diffChild.UniqueId, diffChild.ParentDescription);
                    var completeConflict = new CompleteConflict();
                    completeConflict.DiffViewModel = EmptyConflictViewModel(diffChild.UniqueId);
                    completeConflict.UniqueId = diffChild.UniqueId;
                    completeConflict.CurrentViewModel = model;
                    completeConflict.CurrentViewModel.Container = completeConflict;
                    completeConflict.HasConflict = true;
                    if (childNodes.TryGetValue(model.UniqueId.ToString(), out (ModelItem leftItem, ModelItem rightItem) item))
                    {
                        completeConflict.CurrentViewModel.FlowNode = item.leftItem;
                    }
                    if (parent.Children.Count == 0)
                    {
                        parent.Children.AddFirst(completeConflict);
                    }
                    else
                    {
                        parent.Children.AddLast(completeConflict);
                    }
                }
            }
            IMergeToolModel GetMergeToolItem(IEnumerable<IMergeToolModel> collection, Guid uniqueId, string parentDescription)
            {
                var mergeToolModel = collection.FirstOrDefault(model => model.UniqueId.Equals(uniqueId) && model.ParentDescription.Equals(parentDescription));
                return mergeToolModel;
            }
        }

        public LinkedList<ICompleteConflict> Conflicts { get; set; }

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

        private void SetDisplayName(bool isDirty)
        {
            if (isDirty)
            {
                if (!DisplayName.EndsWith(" *"))
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

        public bool IsMergeExpanderEnabled
        {
            get => _isMergeExpanderEnabled;
            set
            {
                _isMergeExpanderEnabled = value;
                OnPropertyChanged(() => IsMergeExpanderEnabled);
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
