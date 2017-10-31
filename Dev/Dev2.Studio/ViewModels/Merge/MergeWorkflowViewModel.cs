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
using Dev2.Annotations;
using Dev2.Common.Common;
using Dev2.Studio.Interfaces.DataList;
using System.IO;
using System.Text;
using Dev2.Studio.Core.Activities.Utils;

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
                    //ServerName = differenceResourceModel.Environment.Name
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

        private List<ICompleteConflict> BuildConflicts(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel, (IConflictTree current, IConflictTree diff) currentChanges)
        {
            var conflicts = new List<ICompleteConflict>();

            var currentTree = currentChanges.current;
            var diffTree = currentChanges.diff;

            
            if (currentTree.Start != null)
            {
                var conflict = new CompleteConflict();
                var modelFactory = new ConflictModelFactory(currentResourceModel, currentTree.Start);
                var id = Guid.Parse(currentTree.Start.UniqueId);
                conflict.UniqueId = id;
                conflict.DiffViewModel = EmptyConflictViewModel(id);
                conflict.CurrentViewModel = modelFactory.Model;
                conflict.CurrentViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
                conflict.CurrentViewModel.Container = conflict;

                AddChildrenCurrent(conflict, modelFactory.Model, currentTree.Start.Children, modelFactory, conflicts);
                AddNextNodesCurrent(conflict, currentTree.Start.NextNodes, modelFactory, conflicts);                
                conflicts.Add(conflict);
            }

            if (diffTree.Start != null)
            {
                ICompleteConflict conflict = null;
                var node = diffTree.Start;
                var foundConflict = conflicts.FirstOrDefault(t => t.UniqueId.ToString() == node.UniqueId);
                var id = Guid.Parse(node.UniqueId);
                if (foundConflict == null)
                {
                    conflict = new CompleteConflict { UniqueId = id, CurrentViewModel = EmptyConflictViewModel(id) };
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

                AddChildrenDiff(conflict, currentFactory.Model, diffTree.Start.Children, currentFactory, conflicts);
                AddNextNodesDiff(conflict, diffTree.Start.NextNodes, currentFactory, conflicts);                                
            }
            
            //conflicts.AddRange(BuildChildrenConflictsCurrent(conflicts,currentTree.Start.Children, currentResourceModel));
            //conflicts.AddRange(BuildChildrenConflictsDiff(conflicts, diffTree.Start.Children, differenceResourceModel));
            #region OLD
            //foreach (var currentChange in currentChanges)
            //{
            //    var conflict = new CompleteConflict { UniqueId = currentChange.uniqueId };
            //    if (currentChange.currentNode != null)
            //    {
            //        var factoryA = new ConflictModelFactory(currentChange.currentNode, currentResourceModel);
            //        conflict.CurrentViewModel = factoryA.Model;
            //        factoryA.OnModelItemChanged += (modelItem, modelTool) =>
            //        {
            //            if (modelTool.IsMergeChecked)
            //            {
            //                WorkflowDesignerViewModel.UpdateModelItem(modelItem, modelTool);
            //                AddActivity(modelTool);
            //            }
            //        };
            //        conflict.CurrentViewModel.Container = conflict;
            //        conflict.CurrentViewModel.IsMergeVisible = currentChange.hasConflict;
            //        conflict.CurrentViewModel.FlowNode = currentChange.currentNode.CurrentFlowStep;
            //        conflict.CurrentViewModel.NodeLocation = currentChange.currentNode.NodeLocation;
            //        conflict.CurrentViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
            //    }
            //    else
            //    {
            //        conflict.CurrentViewModel = EmptyConflictViewModel(currentChange.uniqueId);
            //        conflict.CurrentViewModel.Container = conflict;
            //        conflict.CurrentViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
            //    }

            //    if (currentChange.differenceNode != null)
            //    {
            //        var factoryB = new ConflictModelFactory(currentChange.differenceNode, differenceResourceModel);
            //        factoryB.OnModelItemChanged += (modelItem, modelTool) =>
            //        {
            //            if (modelTool.IsMergeChecked)
            //            {
            //                var item = WorkflowDesignerViewModel.UpdateModelItem(modelItem, modelTool);

            //                var container = modelTool.Container;
            //                if (container.CurrentViewModel.IsMergeChecked)
            //                {
            //                    WorkflowDesignerViewModel.RemoveItem(container.CurrentViewModel);
            //                }
            //                if (container.DiffViewModel.IsMergeChecked)
            //                {
            //                    WorkflowDesignerViewModel.RemoveItem(container.DiffViewModel);
            //                }
            //                AddActivity(modelTool);
            //            }
            //        };
            //        conflict.DiffViewModel = factoryB.Model;
            //        conflict.DiffViewModel.Container = conflict;
            //        conflict.DiffViewModel.IsMergeVisible = currentChange.hasConflict;
            //        conflict.DiffViewModel.FlowNode = currentChange.differenceNode.CurrentFlowStep;
            //        conflict.DiffViewModel.NodeLocation = currentChange.differenceNode.NodeLocation;
            //        conflict.DiffViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
            //    }
            //    else
            //    {
            //        conflict.DiffViewModel = EmptyConflictViewModel(currentChange.uniqueId);
            //        conflict.DiffViewModel.Container = conflict;
            //        conflict.DiffViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
            //    }

            //    conflict.HasConflict = currentChange.hasConflict;
            //    conflict.IsMergeExpanded = false;
            //    conflict.IsMergeExpanderEnabled = false;

            //    AddChildren(conflict, conflict.CurrentViewModel, conflict.DiffViewModel);
            //    conflicts.Add(conflict);
            //}
            #endregion

            return conflicts;
        }

        private void AddChildrenDiff(ICompleteConflict conflict, IMergeToolModel model, List<(string uniqueId, IConflictTreeNode node)> children, ConflictModelFactory currentFactory, List<ICompleteConflict> conflicts)
        {
            if (children != null)
            {
                foreach (var child in children)
                {
                    ICompleteConflict childConflict = null;
                    var node = child.node;
                    var foundConflict = conflicts.Flatten(c=>c.Children).FirstOrDefault(t => t.UniqueId.ToString() == node.UniqueId);
                    var id = Guid.Parse(node.UniqueId);
                    if (foundConflict == null)
                    {
                        childConflict = new CompleteConflict { UniqueId = id, CurrentViewModel = EmptyConflictViewModel(id) };
                        conflict.Children.AddLast(childConflict);
                    }
                    else
                    {
                        childConflict = foundConflict;
                    }
                    childConflict.DiffViewModel = currentFactory.GetModel(ModelItemUtils.CreateModelItem(child.node.Activity), child.node, model, child.uniqueId);
                    childConflict.DiffViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
                    childConflict.DiffViewModel.Container = conflict;
                    childConflict.HasConflict = childConflict.HasConflict || node.IsInConflict;
                    AddChildrenDiff(childConflict, childConflict.DiffViewModel, child.node.Children, currentFactory, conflicts);
                    AddNextNodesDiff(childConflict, child.node.NextNodes, currentFactory, conflicts);
                }
            }
        }

        void AddChildrenCurrent(CompleteConflict conflict, IMergeToolModel model, List<(string uniqueId, IConflictTreeNode node)> children, ConflictModelFactory factory, List<ICompleteConflict> conflicts)
        {
            if (children != null)
            {
                foreach(var child in children)
                {
                    var childConflict = new CompleteConflict();
                    var id = Guid.Parse(child.node.Activity.UniqueID);
                    childConflict.UniqueId = id;
                    childConflict.DiffViewModel = EmptyConflictViewModel(id);
                    childConflict.CurrentViewModel = factory.GetModel(ModelItemUtils.CreateModelItem(child.node.Activity),child.node,model,child.uniqueId);
                    childConflict.CurrentViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
                    childConflict.HasNodeArmConflict = true;
                    childConflict.CurrentViewModel.Container = conflict;
                    conflict.Children.AddLast(childConflict);
                    AddChildrenCurrent(childConflict, childConflict.CurrentViewModel, child.node.Children, factory, conflicts);
                    AddNextNodesCurrent(childConflict, child.node.NextNodes, factory, conflicts);                    
                }
            }
        }

        private void AddNextNodesDiff(ICompleteConflict conflict , List<IConflictTreeNode> children, ConflictModelFactory currentFactory, List<ICompleteConflict> conflicts)
        {
            if (children != null)
            {
                foreach (var child in children)
                {
                    ICompleteConflict childConflict = null;
                    var foundConflict = conflicts.Flatten(c => c.Children).FirstOrDefault(t => t.UniqueId.ToString() == child.UniqueId);
                    var id = Guid.Parse(child.UniqueId);
                    if (foundConflict == null)
                    {
                        childConflict = new CompleteConflict { UniqueId = id, CurrentViewModel = EmptyConflictViewModel(id) };
                        conflicts.Add(childConflict);
                    }
                    else
                    {
                        childConflict = foundConflict;
                    }
                    childConflict.DiffViewModel = currentFactory.GetModel(ModelItemUtils.CreateModelItem(child.Activity), child, null, child.Activity.GetDisplayName());
                    childConflict.DiffViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
                    childConflict.DiffViewModel.Container = conflict;
                    childConflict.HasConflict = childConflict.HasConflict || child.IsInConflict;
                    AddNextNodesDiff(conflict, child.NextNodes, currentFactory, conflicts);
                }
            }
        }

        void AddNextNodesCurrent(CompleteConflict conflict, List<IConflictTreeNode> children, ConflictModelFactory factory, List<ICompleteConflict> conflicts)
        {
            if (children != null)
            {
                foreach (var child in children)
                {
                    var childConflict = new CompleteConflict();
                    var id = Guid.Parse(child.Activity.UniqueID);
                    childConflict.UniqueId = id;
                    childConflict.DiffViewModel = EmptyConflictViewModel(id);
                    childConflict.CurrentViewModel = factory.GetModel(ModelItemUtils.CreateModelItem(child.Activity), child, null, child.Activity.GetDisplayName());
                    childConflict.CurrentViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
                    childConflict.CurrentViewModel.Container = conflict;
                    conflicts.Add(childConflict);
                    AddNextNodesCurrent(conflict,  child.NextNodes, factory, conflicts);
                }
            }
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

        private bool All(Func<ICompleteConflict, bool> check)
        {
            var conflictsMatch = Conflicts.All(check);
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
            ICompleteConflict previousCurrentViewModel = null;
            if (linkedConflict != null)
            {
                var parents = SetPreviousModelTool(linkedConflict);
                if (model.Parent != null)
                {
                    previousCurrentViewModel = parents?.FirstOrDefault(x => x.UniqueId == model.Parent.UniqueId);
            
                }
                else
                {
                    previousCurrentViewModel = parents?.FirstOrDefault(x => x.UniqueId != linkedConflict.Previous.Value.UniqueId);
                }

                if (previousCurrentViewModel != null)
                {
                    if (previousCurrentViewModel.CurrentViewModel != null && previousCurrentViewModel.CurrentViewModel.IsMergeChecked)
                    {
                        previous = previousCurrentViewModel.CurrentViewModel;
                    }
                    else
                    {
                        if (previousCurrentViewModel.DiffViewModel != null && previousCurrentViewModel.DiffViewModel.IsMergeChecked)
                        {
                            previous = previousCurrentViewModel.DiffViewModel;
                        }
                    }
                }
                //previous = SetPreviousModelTool(linkedConflict)?.FirstOrDefault();
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

        [CanBeNull]
        static List<ICompleteConflict> SetPreviousModelTool(LinkedListNode<ICompleteConflict> linkedConflict)
        {
            //IMergeToolModel previous = null;
            var parents = new List<ICompleteConflict>();
            var previousValue = linkedConflict.Previous?.Value ?? linkedConflict.Value.Parent;            
            var previousCurrentViewModel = previousValue?.CurrentViewModel;
            if (previousCurrentViewModel != null)
            {                
                parents.Add(previousValue);
            }
            if (previousValue?.Parent != null)
            {
                parents.Add(previousValue?.Parent);
            }
            
            //if (previousCurrentViewModel != null)
            //{
            //    if (previousCurrentViewModel.IsMergeChecked)
            //    {
            //        previous = previousCurrentViewModel;
            //    }
            //    else
            //    {
            //        if (previousValue.DiffViewModel != null && previousValue.DiffViewModel.IsMergeChecked)
            //        {
            //            previous = previousValue.DiffViewModel;
            //        }
            //    }
            //}
            //parents.Add(previous);
            return parents;
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

        public IDataListViewModel DataListViewModel
        {
            get
            {
                return _dataListViewModel;
            }
            set
            {
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
                    if (nextConflict != null && (!nextConflict.HasConflict || nextConflict.IsContainerTool))
                    {
                        ExpandPreviousItems(nextConflict);
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

        private static void ExpandPreviousItems(ICompleteConflict nextConflict)
        {
            if (nextConflict.Parent != null)
            {
                nextConflict.Parent.IsMergeExpanded = true;
                ExpandPreviousItems(nextConflict.Parent);
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

        //void AddChildren(ICompleteConflict parent, IMergeToolModel currentChild, IMergeToolModel childDiff)
        //{
        //    var childNodes = _serviceDifferenceParser.GetAllNodes();
        //    if (currentChild == null && childDiff == null)
        //    {
        //        return;
        //    }

        //    if (currentChild != null && childDiff != null)
        //    {
        //        var currentChildChildren = currentChild.Children;
        //        var difChildChildren = childDiff.Children;
        //        if (currentChildChildren.Count < 1 && difChildChildren.Count < 1)
        //        {
        //            return;
        //        }
        //        var count = Math.Max(currentChildChildren.Count, difChildChildren.Count);
        //        var remoteCopy = new ObservableCollection<IMergeToolModel>();
        //        var copy = difChildChildren.ToArray().Clone();
        //        var arracyCopy = copy as IMergeToolModel[];
        //        remoteCopy = arracyCopy?.ToList().ToObservableCollection();
        //        for (var index = 0; index <= count; index++)
        //        {
        //            var completeConflict = new CompleteConflict();
        //            try
        //            {
        //                var currentChildChild = currentChildChildren[index];
        //                if (currentChildChild == null)
        //                {
        //                    continue;
        //                }
        //                if (parent.Children.Any(conflict => conflict.UniqueId.Equals(currentChildChild.UniqueId) && conflict.CurrentViewModel.ParentDescription.Equals(currentChildChild.ParentDescription)))
        //                {
        //                    continue;
        //                }

        //                var childCurrent = GetMergeToolItem(currentChildChildren, currentChildChild.UniqueId, currentChildChild.ParentDescription);
        //                var childDifferent = GetMergeToolItem(difChildChildren, currentChildChild.UniqueId, currentChildChild.ParentDescription);
        //                if (childNodes.TryGetValue(currentChildChild.UniqueId.ToString(), out (ModelItem leftItem, ModelItem rightItem) item))
        //                {
        //                    var local1 = currentChildChildren.Where(p => p.UniqueId == currentChildChild.UniqueId);
        //                    foreach (var c in local1)
        //                    {
        //                        c.FlowNode = item.leftItem;
        //                    }
        //                    var local2 = difChildChildren.Where(p => p.UniqueId == currentChildChild.UniqueId);
        //                    foreach (var c in local2)
        //                    {
        //                        c.FlowNode = item.leftItem;
        //                    }
        //                }

        //                completeConflict.UniqueId = currentChildChild.UniqueId;
        //                completeConflict.CurrentViewModel = childCurrent;

        //                if (completeConflict.CurrentViewModel != null)
        //                {
        //                    completeConflict.CurrentViewModel.Container = completeConflict;
        //                }
        //                completeConflict.DiffViewModel = childDifferent;
        //                if (completeConflict.DiffViewModel != null)
        //                {
        //                    completeConflict.DiffViewModel.Container = completeConflict;
        //                }
        //                else
        //                {
        //                    completeConflict.DiffViewModel = EmptyConflictViewModel(currentChildChild.UniqueId);
        //                    completeConflict.DiffViewModel.Container = completeConflict;
        //                }

        //                completeConflict.Parent = parent;

        //                completeConflict.IsContainerTool = completeConflict.ValidateContainerTool(parent.CurrentViewModel);

        //                completeConflict.HasConflict = completeConflict.DiffViewModel.FlowNode == null || _serviceDifferenceParser.NodeHasConflict(currentChildChild.UniqueId.ToString()) && !completeConflict.IsContainerTool;
        //                completeConflict.DiffViewModel.IsMergeVisible = completeConflict.HasConflict;
        //                completeConflict.CurrentViewModel.IsMergeVisible = completeConflict.HasConflict;
        //                if (parent.Children.Count == 0)
        //                {
        //                    parent.Children.AddFirst(completeConflict);
        //                }
        //                else
        //                {
        //                    parent.Children.AddLast(completeConflict);
        //                }
        //                completeConflict.CurrentViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
        //                completeConflict.DiffViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
        //                AddChildren(completeConflict, childCurrent, childDifferent);
        //            }
        //            catch (ArgumentOutOfRangeException)
        //            {
        //                if (difChildChildren.Any())
        //                {
        //                    foreach (var mergeToolModel in remoteCopy.ToList())
        //                    {
        //                        if (parent.Children.Any(conflict => conflict.UniqueId.Equals(mergeToolModel.UniqueId) && conflict.DiffViewModel.ParentDescription.Equals(mergeToolModel.ParentDescription)))
        //                        {
        //                            continue;
        //                        }
        //                        var conflictChild = new CompleteConflict
        //                        {
        //                            UniqueId = mergeToolModel.UniqueId,
        //                            CurrentViewModel = EmptyConflictViewModel(mergeToolModel.UniqueId),
        //                            DiffViewModel = mergeToolModel
        //                        };
        //                        conflictChild.DiffViewModel.Container = conflictChild;
        //                        conflictChild.CurrentViewModel.Container = conflictChild;
        //                        if (childNodes.TryGetValue(mergeToolModel.UniqueId.ToString(), out (ModelItem leftItem, ModelItem rightItem) item))
        //                        {
        //                            conflictChild.DiffViewModel.FlowNode = item.rightItem;
        //                        }

        //                        conflictChild.IsContainerTool = conflictChild.ValidateContainerTool(parent.DiffViewModel);
        //                        conflictChild.HasConflict = !conflictChild.IsContainerTool;
        //                        conflictChild.DiffViewModel.IsMergeVisible = completeConflict.HasConflict;
        //                        conflictChild.CurrentViewModel.IsMergeVisible = completeConflict.HasConflict;
        //                        conflictChild.Parent = parent;
        //                        if (parent.Children.Count == 0)
        //                        {
        //                            parent.Children.AddFirst(conflictChild);
        //                        }
        //                        else
        //                        {
        //                            parent.Children.AddLast(conflictChild);
        //                        }
        //                        remoteCopy.Remove(mergeToolModel);
        //                        conflictChild.CurrentViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
        //                        conflictChild.DiffViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
        //                        AddChildren(conflictChild, null, mergeToolModel);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    if (currentChild == null)
        //    {
        //        var difChildChildren = childDiff.Children;
        //        foreach (var diffChild in difChildChildren)
        //        {
        //            var model = GetMergeToolItem(difChildChildren, diffChild.UniqueId, diffChild.ParentDescription);
        //            var completeConflict = new CompleteConflict
        //            {
        //                UniqueId = diffChild.UniqueId,
        //                HasConflict = true,
        //                DiffViewModel = model
        //            };
        //            completeConflict.DiffViewModel.Container = completeConflict;
        //            completeConflict.CurrentViewModel = EmptyConflictViewModel(diffChild.UniqueId);

        //            if (childNodes.TryGetValue(model.UniqueId.ToString(), out (ModelItem leftItem, ModelItem rightItem) item))
        //            {
        //                completeConflict.DiffViewModel.FlowNode = item.rightItem;
        //            }
        //            if (parent.Children.Count == 0)
        //            {
        //                parent.Children.AddFirst(completeConflict);
        //            }
        //            else
        //            {
        //                parent.Children.AddLast(completeConflict);
        //            }
        //        }
        //    }
        //    if (childDiff == null)
        //    {
        //        var difChildChildren = currentChild.Children;
        //        foreach (var diffChild in difChildChildren)
        //        {
        //            var model = GetMergeToolItem(difChildChildren, diffChild.UniqueId, diffChild.ParentDescription);
        //            var completeConflict = new CompleteConflict
        //            {
        //                UniqueId = diffChild.UniqueId,
        //                HasConflict = true,
        //                CurrentViewModel = model
        //            };
        //            completeConflict.CurrentViewModel.Container = completeConflict;
        //            completeConflict.DiffViewModel = EmptyConflictViewModel(diffChild.UniqueId);

        //            if (childNodes.TryGetValue(model.UniqueId.ToString(), out (ModelItem leftItem, ModelItem rightItem) item))
        //            {
        //                completeConflict.CurrentViewModel.FlowNode = item.leftItem;
        //            }
        //            if (parent.Children.Count == 0)
        //            {
        //                parent.Children.AddFirst(completeConflict);
        //            }
        //            else
        //            {
        //                parent.Children.AddLast(completeConflict);
        //            }
        //        }
        //    }
        //    IMergeToolModel GetMergeToolItem(IEnumerable<IMergeToolModel> collection, Guid uniqueId, string parentDescription)
        //    {
        //        var mergeToolModel = collection.FirstOrDefault(model => model.UniqueId.Equals(uniqueId) && model.ParentDescription.Equals(parentDescription));
        //        return mergeToolModel;
        //    }
        //}

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
