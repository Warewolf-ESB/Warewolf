using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.Workflow;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Dev2.Common;
using System.Text;
using System.Activities.Presentation.Model;

namespace Dev2.ViewModels.Merge
{
    public class MergeWorkflowViewModel : BindableBase, IMergeWorkflowViewModel
    {
        private readonly IServiceDifferenceParser _serviceDifferenceParser;
        private string _displayName;
        private string _serverName;
        private bool _hasMergeStarted;
        private bool _hasWorkflowNameConflict;
        private bool _hasVariablesConflict;
        private bool _isVariablesEnabled;
        private bool _isMergeExpanderEnabled;
        private bool _dirty;

        public MergeWorkflowViewModel(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel, bool isGitMerge)
            : this(CustomContainer.Get<IServiceDifferenceParser>())
        {
            WorkflowDesignerViewModel = new WorkflowDesignerViewModel(currentResourceModel, false);
            WorkflowDesignerViewModel.CreateBlankWorkflow();

            var currentChanges = _serviceDifferenceParser.GetDifferences(currentResourceModel, differenceResourceModel, isGitMerge);

            Conflicts = new ObservableCollection<ICompleteConflict>();
            foreach (var currentChange in currentChanges)
            {
                var conflict = new CompleteConflict { UniqueId = currentChange.uniqueId };
                var factoryA = new ConflictModelFactory(currentChange.currentNode.CurrentActivity, currentResourceModel);
                var factoryB = new ConflictModelFactory(currentChange.differenceNode.CurrentActivity, differenceResourceModel);
                conflict.CurrentViewModel = factoryA.GetModel();
                conflict.CurrentViewModel.FlowNode = currentChange.currentNode.CurrentFlowStep;
                conflict.CurrentViewModel.NodeLocation = currentChange.currentNode.NodeLocation;
                conflict.CurrentViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
                foreach (var child in conflict.CurrentViewModel.Children)
                {
                    child.SomethingModelToolChanged += SourceOnModelToolChanged;
                }

                conflict.DiffViewModel = factoryB.GetModel();
                conflict.DiffViewModel.FlowNode = currentChange.differenceNode.CurrentFlowStep;
                conflict.DiffViewModel.NodeLocation = currentChange.differenceNode.NodeLocation;
                conflict.DiffViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;
                foreach (var child in conflict.DiffViewModel.Children)
                {
                    child.SomethingModelToolChanged += SourceOnModelToolChanged;
                }

                conflict.HasConflict = currentChange.hasConflict;
                conflict.HasConflict = true;//Tests failing will b resolved once this is removed
                conflict.IsMergeExpanded = false;
                conflict.IsMergeExpanderEnabled = false;
                AddChildren(conflict, conflict.CurrentViewModel, conflict.DiffViewModel);
                Conflicts.Add(conflict);
            }

            var firstConflict = Conflicts.FirstOrDefault();

            var currResourceName = currentResourceModel.ResourceName;
            if (CurrentConflictModel == null)
            {
                CurrentConflictModel = new ConflictModelFactory();
                if (firstConflict?.CurrentViewModel != null)
                {
                    CurrentConflictModel.Model = firstConflict.CurrentViewModel;
                }
                CurrentConflictModel.WorkflowName = currResourceName;
                CurrentConflictModel.GetDataList();
                CurrentConflictModel.SomethingConflictModelChanged += SourceOnConflictModelChanged;
            }

            var diffResourceName = differenceResourceModel.ResourceName;
            if (DifferenceConflictModel == null)
            {
                DifferenceConflictModel = new ConflictModelFactory();

                if (firstConflict?.DiffViewModel != null)
                {
                    DifferenceConflictModel.Model = firstConflict.DiffViewModel;
                }
                DifferenceConflictModel.WorkflowName = diffResourceName;
                DifferenceConflictModel.GetDataList();
                DifferenceConflictModel.SomethingConflictModelChanged += SourceOnConflictModelChanged;
            }

            HasMergeStarted = false;

            //HasVariablesConflict = !CommonEqualityOps.AreObjectsEqual(((ConflictModelFactory)CurrentConflictModel).DataListViewModel, ((ConflictModelFactory)DifferenceConflictModel).DataListViewModel); //MATCH DATALISTS
            HasVariablesConflict = true;
            HasWorkflowNameConflict = currResourceName != diffResourceName;
            IsVariablesEnabled = !HasWorkflowNameConflict;
            IsMergeExpanderEnabled = !IsVariablesEnabled;

            SetServerName(currentResourceModel);
            DisplayName = "Merge" + _serverName;
            CanSave = false;

            WorkflowDesignerViewModel.CanViewWorkflowLink = false;
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
            WorkflowDesignerViewModel.BringMergeToView(model.FlowNode);
        }


        private IMergeToolModel _previousParent;
        private void SourceOnConflictModelChanged(object sender, IConflictModelFactory args)
        {
            try
            {
                if (args.IsWorkflowNameChecked)
                {
                    HasMergeStarted = true;
                    IsVariablesEnabled = HasVariablesConflict;
                }
                else if (args.IsVariablesChecked)
                {
                    HasMergeStarted = true;
                    IsMergeExpanderEnabled = true;
                }
                OnPropertyChanged(() => IsDirty);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void SourceOnModelToolChanged(object sender, IMergeToolModel args)
        {
            try
            {

                if (!(args is MergeToolModel mergeToolModel) || !mergeToolModel.IsMergeChecked)
                {
                    return;
                }

                if (mergeToolModel.Parent != null && mergeToolModel.Parent.IsMergeChecked)
                {
                    return;
                }

                HasMergeStarted = true;
                AddActivity(mergeToolModel);
                if (mergeToolModel.Children.Count < 1)
                {
                    return;
                }

                //mergeToolModel.Children.Flatten(a => a.Children).Apply(a => a.IsMergeChecked = true);
                OnPropertyChanged(() => IsDirty);
            }
            catch (Exception ex)
            {
                // ignored
                Dev2Logger.Error(ex, ex.Message);
            }
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
                remoteCopy = difChildChildren;
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
                        remoteCopy.Remove(childDifferent);
                        completeConflict.UniqueId = currentChildChild.UniqueId;
                        completeConflict.CurrentViewModel = childCurrent;
                        completeConflict.DiffViewModel = childDifferent;
                        if (childNodes.TryGetValue(currentChildChild.UniqueId.ToString(), out (ModelItem leftItem, ModelItem rightItem) item))
                        {
                            completeConflict.DiffViewModel.FlowNode = item.rightItem;
                            completeConflict.CurrentViewModel.FlowNode = item.leftItem;
                        };
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
                                };
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
                    };
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
                    };
                }
            }
            IMergeToolModel GetMergeToolItem(IEnumerable<IMergeToolModel> collection, Guid uniqueId)
            {
                var mergeToolModel = collection.FirstOrDefault(model => model.UniqueId.Equals(uniqueId));//
                return mergeToolModel;
            }
        }

        public ObservableCollection<ICompleteConflict> Conflicts { get; set; }

        public WorkflowDesignerViewModel WorkflowDesignerViewModel { get; set; }

        public IConflictModelFactory CurrentConflictModel { get; set; }
        public IConflictModelFactory DifferenceConflictModel { get; set; }

        public void Save()
        {
            try
            {
                Save();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                var isDirty = IsDirty;
                SetDisplayName(isDirty);
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

        public bool CanSave { get; set; }

        public bool IsDirty
        {
            get
            {
                try
                {
                    _dirty = ValidWorkflowName();
                    if (!_dirty)
                    {
                        _dirty = ValidVariables();
                    }
                    if (!_dirty)
                    {
                        var completeConflicts = Conflicts.Flatten(conflict => conflict.Children);
                        _dirty = completeConflicts.Any(conflict => conflict.IsMergeExpanded);
                    }

                    CanSave = _dirty;
                    SetDisplayName(_dirty);
                    return _dirty;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        private bool ValidWorkflowName()
        {
            var isWorkflowNameChecked = CurrentConflictModel != null && CurrentConflictModel.IsWorkflowNameChecked;
            var workflowNameChecked = DifferenceConflictModel != null && DifferenceConflictModel.IsWorkflowNameChecked;
            return isWorkflowNameChecked || workflowNameChecked;
        }

        private bool ValidVariables()
        {
            var isVariablesChecked = CurrentConflictModel != null && CurrentConflictModel.IsVariablesChecked;
            var variablesChecked = DifferenceConflictModel != null && DifferenceConflictModel.IsVariablesChecked;
            return isVariablesChecked || variablesChecked;
        }

        private void SetServerName(IContextualResourceModel resourceModel)
        {
            if (resourceModel.Environment == null || resourceModel.Environment.IsLocalHost)
            {
                _serverName = string.Empty;
            }
            else if (!resourceModel.Environment.IsLocalHost)
            {
                _serverName = " - " + resourceModel.Environment.Name;
            }
        }

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
