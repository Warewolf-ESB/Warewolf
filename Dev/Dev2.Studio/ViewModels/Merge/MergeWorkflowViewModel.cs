using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Runtime.Configuration.ViewModels.Base;
using System.Collections.ObjectModel;
using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Communication;
using Dev2.Studio.Core.Activities.Utils;
using System.Activities.Statements;

namespace Dev2.ViewModels.Merge
{
    public class MergeWorkflowViewModel : BindableBase, IMergeWorkflowViewModel
    {
        private string _displayName;
        private string _serverName;
        private bool _hasMergeStarted;
        private bool _hasWorkflowNameConflict;
        private bool _hasVariablesConflict;

        public MergeWorkflowViewModel(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel)
        {
            WorkflowDesignerViewModel = new WorkflowDesignerViewModel(currentResourceModel, false);
            WorkflowDesignerViewModel.CreateBlankWorkflow();
            var mergeParser = CustomContainer.Get<IServiceDifferenceParser>();
            
            var currentChanges = mergeParser.GetDifferences(currentResourceModel, differenceResourceModel);

            
            Conflicts = new ObservableCollection<ICompleteConflict>();
            foreach (var currentChange in currentChanges)
            {
                var conflict = new CompleteConflict { UniqueId = currentChange.uniqueId };
                var factoryA = new ConflictModelFactory(currentChange.current, currentResourceModel);
                var factoryB = new ConflictModelFactory(currentChange.difference, differenceResourceModel);
                conflict.CurrentViewModel = factoryA.GetModel();
                conflict.CurrentViewModel.AddAnItem = new DelegateCommand(o =>
                {
                    var model = conflict.CurrentViewModel as MergeToolModel;
                    AddActivity(model);
                });
                conflict.CurrentViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;

                conflict.DiffViewModel = factoryB.GetModel();
                conflict.DiffViewModel.AddAnItem = new DelegateCommand(o =>
                {
                    var model = conflict.DiffViewModel as MergeToolModel;
                    AddActivity(model);
                });
                conflict.DiffViewModel.SomethingModelToolChanged += SourceOnModelToolChanged;

                conflict.HasConflict = currentChange.hasConflict;
                AddChildren(conflict, conflict.CurrentViewModel, conflict.DiffViewModel);
                Conflicts.Add(conflict);
            }
           
            Conflicts = Conflicts.Reverse().ToObservableCollection();
            var firstConflict = Conflicts.FirstOrDefault();

            if (CurrentConflictModel == null)
            {
                CurrentConflictModel = new ConflictModelFactory();
                if (firstConflict?.CurrentViewModel != null)
                {
                    CurrentConflictModel.Model = firstConflict.CurrentViewModel;
                }
            }

            CurrentConflictModel.WorkflowName = currentResourceModel.ResourceName;
            CurrentConflictModel.GetDataList();
            if (DifferenceConflictModel == null)
            {
                DifferenceConflictModel = new ConflictModelFactory();
                if (firstConflict?.DiffViewModel != null)
                {
                    DifferenceConflictModel.Model = firstConflict.DiffViewModel;
                }
            }
            DifferenceConflictModel.WorkflowName = differenceResourceModel.ResourceName;
            DifferenceConflictModel.GetDataList();
            HasMergeStarted = false;
            HasVariablesConflict = true;
            HasWorkflowNameConflict = true;
            SetServerName(currentResourceModel);
            DisplayName = "Merge" + _serverName;

            WorkflowDesignerViewModel.CanViewWorkflowLink = false;

            CurrentConflictModel.SomethingConflictModelChanged += SourceOnConflictModelChanged;
            DifferenceConflictModel.SomethingConflictModelChanged += SourceOnConflictModelChanged;
        }

        private void AddActivity(MergeToolModel model)
        {
            if (model.ActivityType.DisplayName == "Decision")
            {
                //WorkflowDesignerViewModel.AddDecisionItem(model.ActivityType);
            }
            else if (model.ActivityType.DisplayName == "Switch")
            {

            }
            else
            {
                var step = new FlowStep { Action = model.ActivityType };
                WorkflowDesignerViewModel.AddItem(step);
            }
        }

        private void SourceOnConflictModelChanged(object sender, IConflictModelFactory args)
        {
            try
            {
                if (args.IsWorkflowNameChecked)
                {
                    
                }
                else if (args.IsVariablesChecked)
                {
                    
                }
            }
            catch (Exception e)
            {
                
            }
            finally
            {
                
            }
        }

        private void SourceOnModelToolChanged(object sender, IMergeToolModel args)
        {
            try
            {
                if (args.IsMergeChecked)
                {
                    
                }
            }
            catch (Exception e)
            {

            }
            finally
            {

            }
        }

        void AddChildren(ICompleteConflict parent, IMergeToolModel currentChild, IMergeToolModel childDiff)
        {
            if (currentChild == null && childDiff == null) return;
            if (currentChild != null && childDiff != null)
            {
                var completeConflict = new CompleteConflict();
                var currentChildChildren = currentChild.Children;
                var difChildChildren = childDiff.Children;
                foreach (var currentChildChild in currentChildChildren)
                {
                    if (currentChildChild == null) continue;

                    var childCurrent = GetMergeToolItem(currentChildChildren, currentChildChild.UniqueId);
                    var childDifferent = GetMergeToolItem(difChildChildren, currentChildChild.UniqueId);
                    completeConflict.UniqueId = currentChild.UniqueId;
                    completeConflict.CurrentViewModel = childCurrent;
                    completeConflict.DiffViewModel = childDifferent;
                    parent.Children.Add(completeConflict);
                    AddChildren(completeConflict, childCurrent, childDifferent);
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
                }
            }
            IMergeToolModel GetMergeToolItem(IEnumerable<IMergeToolModel> collection, Guid uniqueId)
            {
                var mergeToolModel = collection.FirstOrDefault(model => model.UniqueId.Equals(uniqueId));//
                return mergeToolModel;
            }
        }

        public ObservableCollection<ICompleteConflict> Conflicts { get; set; }

        public System.Windows.Input.ICommand AddAnItem { get; set; }

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
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
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
