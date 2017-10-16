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
                CurrentConflictModel = new ConflictModelFactory();
                CurrentConflictModel.Model = firstConflict?.CurrentViewModel ?? new MergeToolModel {IsMergeEnabled = false};
                CurrentConflictModel.WorkflowName = currentResourceModel.ResourceName;
                CurrentConflictModel.ServerName = currentResourceModel.Environment.DisplayName;
                CurrentConflictModel.GetDataList();
                CurrentConflictModel.SomethingConflictModelChanged += SourceOnConflictModelChanged;
            }

            if (DifferenceConflictModel == null)
            {
                DifferenceConflictModel = new ConflictModelFactory();
                DifferenceConflictModel.Model = firstConflict?.DiffViewModel ?? new MergeToolModel { IsMergeEnabled = false };
                DifferenceConflictModel.WorkflowName = differenceResourceModel.ResourceName;
                DifferenceConflictModel.ServerName = differenceResourceModel.Environment.DisplayName;
                DifferenceConflictModel.GetDataList();
                DifferenceConflictModel.SomethingConflictModelChanged += SourceOnConflictModelChanged;
            }

            HasMergeStarted = false;

            HasVariablesConflict = !CommonEqualityOps.AreObjectsEqual(((ConflictModelFactory)CurrentConflictModel).DataListViewModel, ((ConflictModelFactory)DifferenceConflictModel).DataListViewModel); //MATCH DATALISTS
            HasWorkflowNameConflict = currentResourceModel.ResourceName != differenceResourceModel.ResourceName;
            IsVariablesEnabled = !HasWorkflowNameConflict && HasVariablesConflict;
            IsMergeExpanderEnabled = !IsVariablesEnabled;

            DisplayName = "Merge";
            CanSave = false;

            WorkflowDesignerViewModel.CanViewWorkflowLink = false;
            WorkflowDesignerViewModel.IsTestView = true;

            if (!HasWorkflowNameConflict)
            {
                CurrentConflictModel.IsWorkflowNameChecked = true;
            }
            if (!HasVariablesConflict)
            {
                CurrentConflictModel.IsVariablesChecked = true;
            }

            if (!HasWorkflowNameConflict && !HasVariablesConflict)
            {
                var conflict = Conflicts?.FirstOrDefault();
                _conflictEnumerator.MoveNext();
                if (conflict != null && !conflict.HasConflict)
                {
                    conflict.CurrentViewModel.IsMergeChecked = true;
                }
            }
        }

        private List<ICompleteConflict> BuildConflicts(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel, List<(Guid uniqueId, IConflictNode currentNode, IConflictNode differenceNode, bool hasConflict)> currentChanges)
        {
            var conflicts = new List<ICompleteConflict>();
            foreach (var currentChange in currentChanges)
            {
                var conflict = new CompleteConflict { UniqueId = currentChange.uniqueId };
                if (currentChange.currentNode != null)
                {
                    var factoryA = new ConflictModelFactory(currentChange.currentNode.CurrentActivity, currentResourceModel);
                    conflict.CurrentViewModel = factoryA.GetModel();
                    conflict.CurrentViewModel.Container = conflict;
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
                    conflict.CurrentViewModel = EmptyConflictViewModel(currentChange);
                    conflict.CurrentViewModel.Container = conflict;
                    conflict.CurrentViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
                }

                if (currentChange.differenceNode != null)
                {
                    var factoryB = new ConflictModelFactory(currentChange.differenceNode.CurrentActivity, differenceResourceModel);
                    conflict.DiffViewModel = factoryB.GetModel();
                    conflict.DiffViewModel.Container = conflict;
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
                    conflict.DiffViewModel = EmptyConflictViewModel(currentChange);
                    conflict.DiffViewModel.Container = conflict;
                    conflict.DiffViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
                }

                conflict.HasConflict = currentChange.hasConflict;
                conflict.IsMergeExpanded = false;
                conflict.IsMergeExpanderEnabled = false;
                AddChildren(conflict, conflict.CurrentViewModel, conflict.DiffViewModel);
                conflicts.Add(conflict);
            }
            return conflicts;
        }

        private static MergeToolModel EmptyConflictViewModel((Guid uniqueId, IConflictNode currentNode, IConflictNode differenceNode, bool hasConflict) currentChange)
        {
            return new MergeToolModel
            {
                FlowNode = null,
                NodeLocation = new Point(),
                IsMergeEnabled = false,
                IsMergeVisible = false,
                UniqueId = currentChange.uniqueId
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
            //WorkflowDesignerViewModel.RemoveItem(model);
            WorkflowDesignerViewModel.AddItem(previous, model, next);
            WorkflowDesignerViewModel.SelectedItem = model.FlowNode;
        }

        private static IMergeToolModel SetNextModelTool(LinkedListNode<ICompleteConflict> linkedConflict)
        {
            IMergeToolModel next = null;
            var nextValue = linkedConflict.Next?.Value;
            var nextValueCurrentViewModel = nextValue?.CurrentViewModel;
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

        private static IMergeToolModel SetPreviousModelTool(LinkedListNode<ICompleteConflict> linkedConflict)
        {
            IMergeToolModel previous = null;
            var previousValue = linkedConflict.Previous?.Value;
            var previousCurrentViewModel = previousValue?.CurrentViewModel;
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

        private void SourceOnConflictModelChanged(object sender, IConflictModelFactory args)
        {
            try
            {
                var argsIsVariablesChecked = args.IsVariablesChecked || !HasVariablesConflict;

                if (!HasMergeStarted)
                {
                    HasMergeStarted = args.IsWorkflowNameChecked || argsIsVariablesChecked;
                }
                IsVariablesEnabled = HasVariablesConflict;

                IsMergeExpanderEnabled = argsIsVariablesChecked;
                Conflicts.First.Value.DiffViewModel.IsMergeEnabled = Conflicts.First.Value.HasConflict && argsIsVariablesChecked;
                Conflicts.First.Value.CurrentViewModel.IsMergeEnabled = Conflicts.First.Value.HasConflict && argsIsVariablesChecked;
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, ex.Message);
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


        public ICompleteConflict GetNextConflict()
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
                var count = Math.Max(currentChildChildren.Count, difChildChildren.Count);
                ObservableCollection<IMergeToolModel> remoteCopy = new ObservableCollection<IMergeToolModel>();
                var copy = difChildChildren.ToArray().Clone();
                var arracyCopy = copy as IMergeToolModel[];
                remoteCopy = arracyCopy?.ToList().ToObservableCollection();
                for (var index = 0; index < count; index++)
                {
                    var completeConflict = new CompleteConflict();
                    try
                    {
                        var currentChildChild = currentChildChildren[index];
                        if (currentChildChild == null)
                        {
                            continue;
                        }
                        var childCurrent = GetMergeToolItem(currentChildChildren, currentChildChild.UniqueId);
                        var childDifferent = GetMergeToolItem(difChildChildren, currentChildChild.UniqueId);
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
                        remoteCopy.Remove(childDifferent);
                        completeConflict.UniqueId = currentChildChild.UniqueId;
                        completeConflict.CurrentViewModel = childCurrent;
                        completeConflict.DiffViewModel = childDifferent;
                        completeConflict.CurrentViewModel.Container = completeConflict;
                        completeConflict.DiffViewModel.Container = completeConflict;
                        completeConflict.Parent = parent;

                        if (parent.Children.Any(conflict => conflict.UniqueId.Equals(currentChild.UniqueId)))
                        {
                            continue;
                        }
                        completeConflict.HasConflict = true;
                        completeConflict.HasConflict = _serviceDifferenceParser.NodeHasConflict(currentChildChild.UniqueId.ToString());
                        parent.Children.AddLast(completeConflict);
                        AddChildren(completeConflict, childCurrent, childDifferent);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        if (difChildChildren.Any())
                        {
                            foreach (var mergeToolModel in remoteCopy)
                            {
                                completeConflict.UniqueId = mergeToolModel.UniqueId;
                                completeConflict.CurrentViewModel = null;
                                completeConflict.DiffViewModel = mergeToolModel;
                                completeConflict.DiffViewModel.Container = completeConflict;
                                if (childNodes.TryGetValue(mergeToolModel.UniqueId.ToString(), out (ModelItem leftItem, ModelItem rightItem) item))
                                {
                                    completeConflict.DiffViewModel.FlowNode = item.rightItem;
                                    completeConflict.CurrentViewModel.FlowNode = item.leftItem;
                                }
                                if (parent.Children.Any(conflict => conflict.UniqueId.Equals(currentChild.UniqueId)))
                                {
                                    continue;
                                }
                                completeConflict.HasConflict = true;
                                completeConflict.HasConflict = _serviceDifferenceParser.NodeHasConflict(mergeToolModel.UniqueId.ToString());
                                if (parent.Children.Count == 0)
                                {
                                    parent.Children.AddFirst(completeConflict);
                                }
                                else
                                {
                                    parent.Children.AddLast(completeConflict);
                                }
                                AddChildren(completeConflict, null, mergeToolModel);
                            }
                        }
                    }
                }
            }

            if (childDiff == null)
            {
                var difChildChildren = currentChild.Children;
                var completeConflict = new CompleteConflict();
                foreach (var diffChild in difChildChildren)
                {
                    var model = GetMergeToolItem(difChildChildren, diffChild.UniqueId);
                    completeConflict.UniqueId = diffChild.UniqueId;
                    completeConflict.DiffViewModel = model;
                    completeConflict.DiffViewModel.Container = completeConflict;
                    if (childNodes.TryGetValue(model.UniqueId.ToString(), out (ModelItem leftItem, ModelItem rightItem) item))
                    {
                        completeConflict.DiffViewModel.FlowNode = item.rightItem;
                        completeConflict.HasConflict = _serviceDifferenceParser.NodeHasConflict(diffChild.UniqueId.ToString());
                    }
                }
            }
            if (currentChild == null)
            {
                var difChildChildren = childDiff.Children;
                var completeConflict = new CompleteConflict();
                foreach (var diffChild in difChildChildren)
                {
                    var model = GetMergeToolItem(difChildChildren, diffChild.UniqueId);
                    completeConflict.UniqueId = diffChild.UniqueId;
                    completeConflict.CurrentViewModel = model;
                    completeConflict.CurrentViewModel.Container = completeConflict;
                    if (childNodes.TryGetValue(model.UniqueId.ToString(), out (ModelItem leftItem, ModelItem rightItem) item))
                    {
                        completeConflict.CurrentViewModel.FlowNode = item.leftItem;
                        completeConflict.HasConflict = _serviceDifferenceParser.NodeHasConflict(diffChild.UniqueId.ToString());
                    }
                }
            }
            IMergeToolModel GetMergeToolItem(IEnumerable<IMergeToolModel> collection, Guid uniqueId)
            {
                var mergeToolModel = collection.FirstOrDefault(model => model.UniqueId.Equals(uniqueId));
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
