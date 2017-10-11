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

            Conflicts = new ObservableCollection<ICompleteConflict>();
            BuildConflicts(currentResourceModel, differenceResourceModel, currentChanges);
            var firstConflict = Conflicts.FirstOrDefault();
            SetupBindings(currentResourceModel, differenceResourceModel, firstConflict);
        }

        private void SetupBindings(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel, ICompleteConflict firstConflict)
        {
            if (CurrentConflictModel == null)
            {
                CurrentConflictModel = new ConflictModelFactory();
                if (firstConflict?.CurrentViewModel != null)
                {
                    CurrentConflictModel.Model = firstConflict.CurrentViewModel;
                }
                else
                {
                    CurrentConflictModel.Model = new MergeToolModel {IsMergeEnabled = false};
                }
                CurrentConflictModel.WorkflowName = currentResourceModel.DisplayName;
                CurrentConflictModel.ServerName = currentResourceModel.Environment.DisplayName;
                CurrentConflictModel.GetDataList();
                CurrentConflictModel.SomethingConflictModelChanged += SourceOnConflictModelChanged;
            }

            if (DifferenceConflictModel == null)
            {
                DifferenceConflictModel = new ConflictModelFactory();

                if (firstConflict?.DiffViewModel != null)
                {
                    DifferenceConflictModel.Model = firstConflict.DiffViewModel;
                }
                DifferenceConflictModel.WorkflowName = differenceResourceModel.DisplayName;
                DifferenceConflictModel.ServerName = differenceResourceModel.Environment.DisplayName;
                DifferenceConflictModel.GetDataList();
                DifferenceConflictModel.SomethingConflictModelChanged += SourceOnConflictModelChanged;
            }

            HasMergeStarted = false;

            //HasVariablesConflict = !CommonEqualityOps.AreObjectsEqual(((ConflictModelFactory)CurrentConflictModel).DataListViewModel, ((ConflictModelFactory)DifferenceConflictModel).DataListViewModel); //MATCH DATALISTS
            HasVariablesConflict = true;
            //HasWorkflowNameConflict = currentResourceModel.DisplayName != differenceResourceModel.DisplayName;
            HasWorkflowNameConflict = true;
            IsVariablesEnabled = !HasWorkflowNameConflict;
            IsMergeExpanderEnabled = !IsVariablesEnabled;

            DisplayName = "Merge";
            CanSave = false;

            WorkflowDesignerViewModel.CanViewWorkflowLink = false;
            WorkflowDesignerViewModel.IsTestView = true;

            if (!HasWorkflowNameConflict && !HasVariablesConflict)
            {
                var conflict = Conflicts?.FirstOrDefault();
                if (conflict != null && !conflict.HasConflict)
                {
                    AddActivity(conflict.DiffViewModel);
                }
            }
        }

        private void BuildConflicts(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel, List<(Guid uniqueId, IConflictNode currentNode, IConflictNode differenceNode, bool hasConflict)> currentChanges)
        {
            foreach (var currentChange in currentChanges)
            {
                var conflict = new CompleteConflict { UniqueId = currentChange.uniqueId };
                if (currentChange.currentNode != null)
                {
                    var factoryA = new ConflictModelFactory(currentChange.currentNode.CurrentActivity, currentResourceModel);
                    conflict.CurrentViewModel = factoryA.GetModel();
                    conflict.CurrentViewModel.FlowNode = currentChange.currentNode.CurrentFlowStep;
                    conflict.CurrentViewModel.NodeLocation = currentChange.currentNode.NodeLocation;
                    conflict.CurrentViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
                    foreach (var child in conflict.CurrentViewModel.Children)
                    {
                        child.SomethingModelToolChanged += SourceOnModelToolChanged;
                    }
                }
                else
                {
                    conflict.CurrentViewModel = EmptyConflictViewModel(currentChange);
                    conflict.CurrentViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
                }

                if (currentChange.differenceNode != null)
                {
                    var factoryB = new ConflictModelFactory(currentChange.differenceNode.CurrentActivity, differenceResourceModel);
                    conflict.DiffViewModel = factoryB.GetModel();
                    conflict.DiffViewModel.FlowNode = currentChange.differenceNode.CurrentFlowStep;
                    conflict.DiffViewModel.NodeLocation = currentChange.differenceNode.NodeLocation;
                    conflict.DiffViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
                    foreach (var child in conflict.DiffViewModel.Children)
                    {
                        child.SomethingModelToolChanged += SourceOnModelToolChanged;
                    }
                }
                else
                {
                    conflict.DiffViewModel = EmptyConflictViewModel(currentChange);
                    conflict.DiffViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
                }

                conflict.HasConflict = currentChange.hasConflict;
                conflict.HasConflict = true;//Tests failing will b resolved once this is removed
                conflict.IsMergeExpanded = false;
                conflict.IsMergeExpanderEnabled = false;
                AddChildren(conflict, conflict.CurrentViewModel, conflict.DiffViewModel);
                Conflicts.Add(conflict);
            }
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

        private void AddActivity(IMergeToolModel model)
        {
            WorkflowDesignerViewModel.RemoveItem(model);
            WorkflowDesignerViewModel.AddItem(_previousParent, model);
            _previousParent = model;
            WorkflowDesignerViewModel.SelectedItem = model.FlowNode;
        }

        private IMergeToolModel _previousParent;
        private bool _canSave;

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
                Conflicts[0].DiffViewModel.IsMergeEnabled = argsIsVariablesChecked;
                Conflicts[0].CurrentViewModel.IsMergeEnabled = argsIsVariablesChecked;
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
                if (args.Parent != null && args.Parent.IsMergeChecked)
                {
                    return;
                }

                var nextConflict = UpdateNextEnabledState(args);
                if (!HasMergeStarted)
                {
                    HasMergeStarted = true;
                }
                AddActivity(args);
                if (nextConflict != null && !nextConflict.HasConflict)
                {
                    AddActivity(nextConflict.CurrentViewModel);
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, ex.Message);
            }
        }

        private ICompleteConflict UpdateNextEnabledState(IMergeToolModel args)
        {
            if (Conflicts == null)
            {
                return null;
            }

            var argsUniqueId = args.UniqueId;
            var completeConflict = Conflicts.FirstOrDefault(conflict => conflict.CurrentViewModel.UniqueId == argsUniqueId || conflict.DiffViewModel.UniqueId == argsUniqueId);
            if (completeConflict == null)
            {
                return null;
            }

            completeConflict.Children?.Flatten(a => a.Children ?? new ObservableCollection<ICompleteConflict>())
                .Apply(a =>
                {
                    if (a?.CurrentViewModel != null)
                    {
                        a.CurrentViewModel.IsMergeEnabled = true;
                    }
                });
            completeConflict.Children?.Flatten(a => a.Children ?? new ObservableCollection<ICompleteConflict>())
                .Apply(a =>
                {
                    if (a?.DiffViewModel != null)
                    {
                        a.DiffViewModel.IsMergeEnabled = true;
                    }
                });

            var currIndex = 0;
            if (Conflicts.Count > 1)
            {
                currIndex = Conflicts.IndexOf(completeConflict) + 1;
            }
            ICompleteConflict nextCurrConflict;
            try
            {
                nextCurrConflict = Conflicts[currIndex];
            }
            catch (ArgumentOutOfRangeException)
            {
                nextCurrConflict = Conflicts.LastOrDefault();
            }

            if (nextCurrConflict == null)
            {
                return null;
            }

            if (nextCurrConflict.CurrentViewModel != null)
            {
                nextCurrConflict.CurrentViewModel.IsMergeEnabled = true;
                nextCurrConflict.CurrentViewModel.Children?.Flatten(a => a.Children ?? new ObservableCollection<IMergeToolModel>()).Apply(a => a.IsMergeEnabled = true);
            }
            if (nextCurrConflict.DiffViewModel != null)
            {
                nextCurrConflict.DiffViewModel.IsMergeEnabled = true;
                nextCurrConflict.DiffViewModel.Children?.Flatten(a => a.Children ?? new ObservableCollection<IMergeToolModel>()).Apply(a => a.IsMergeEnabled = true);
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

                        if (parent.Children.Any(conflict => conflict.UniqueId.Equals(currentChild.UniqueId)))
                        {
                            continue;
                        }
                        completeConflict.HasConflict = true;
                        parent.Children.Add(completeConflict);
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
                                parent.Children.Add(completeConflict);
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
                    if (childNodes.TryGetValue(model.UniqueId.ToString(), out (ModelItem leftItem, ModelItem rightItem) item))
                    {
                        completeConflict.DiffViewModel.FlowNode = item.rightItem;
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
                    if (childNodes.TryGetValue(model.UniqueId.ToString(), out (ModelItem leftItem, ModelItem rightItem) item))
                    {
                        completeConflict.CurrentViewModel.FlowNode = item.leftItem;
                    }
                }
            }
            IMergeToolModel GetMergeToolItem(IEnumerable<IMergeToolModel> collection, Guid uniqueId)
            {
                var mergeToolModel = collection.FirstOrDefault(model => model.UniqueId.Equals(uniqueId));
                return mergeToolModel;
            }
        }

        public ObservableCollection<ICompleteConflict> Conflicts { get; set; }

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
            get => ValidateCanSave();
            set
            {
                _canSave = value;
                OnPropertyChanged(() => CanSave);
            }
        }

        private bool ValidateCanSave()
        {
            var conflict = Conflicts?.LastOrDefault();
            if (conflict != null)
            {
                var currMergeToolModels = conflict.CurrentViewModel.Children?.Flatten(a => a.Children ?? new ObservableCollection<IMergeToolModel>());
                var currToolModels = currMergeToolModels as IList<IMergeToolModel> ?? currMergeToolModels?.ToList();
                if (currToolModels != null)
                {
                    var currDefault = currToolModels.LastOrDefault();
                    _canSave = currDefault != null && currDefault.IsMergeChecked;
                }
                var diffMergeToolModels = conflict.DiffViewModel.Children?.Flatten(a => a.Children ?? new ObservableCollection<IMergeToolModel>());
                var diffToolModels = diffMergeToolModels as IList<IMergeToolModel> ?? diffMergeToolModels?.ToList();
                if (diffToolModels != null)
                {
                    var diffToolModel = diffToolModels.LastOrDefault();
                    _canSave = diffToolModel != null && diffToolModel.IsMergeChecked;
                }
            }
            return _canSave;
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

        }

        public void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel?.UpdateHelpText(helpText);
        }
    }
}
