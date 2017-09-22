using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Runtime.Configuration.ViewModels.Base;
using System.Collections.ObjectModel;
using System;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Studio.Core.Activities.Utils;

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
            var differenceStore = currentChanges.differenceStore;
            var currentChart = currentChanges.current;
            var differenceChart = currentChanges.difference;

            void AddChild(ICompleteConflict parent, IMergeToolModel child, IMergeToolModel childDiff, bool isConfilct)
            {
                var completeConflict = new CompleteConflict()
                {
                    UniqueId = child.UniqueId,
                };
                if (isConfilct)
                {
                    completeConflict.DiffViewModel = childDiff;
                }
                else
                {
                    completeConflict.CurrentViewModel = child;
                }
                parent.Children.Add(completeConflict);
                foreach (var mergeToolModel in child.Children)
                {
                    var keyValuePair = differenceStore.SingleOrDefault(pair => pair.Key.Equals(mergeToolModel.UniqueId));
                    var diffModel = childDiff.Children.SingleOrDefault(model => model.UniqueId ==  keyValuePair.Key);
                    AddChild(completeConflict, mergeToolModel, diffModel, keyValuePair.Value);
                   
                }
            }



            var conflict = new CompleteConflict {UniqueId = currentResourceModel.ID};
            CurrentConflictViewModel = new ConflictViewModel(currentChart, currentResourceModel);
            DifferenceConflictViewModel = new ConflictViewModel(differenceChart, differenceResourceModel);
            if (CurrentConflictViewModel?.MergeToolModel != null)
            {
                conflict.CurrentViewModel = CurrentConflictViewModel.MergeToolModel;
                var keyValuePair = differenceStore.SingleOrDefault(pair => pair.Key.Equals(CurrentConflictViewModel.MergeToolModel.UniqueId));
               

                foreach (var child in CurrentConflictViewModel.MergeToolModel.Children)
                {
                   
                    if (keyValuePair.Value)
                    {
                        if (DifferenceConflictViewModel.MergeToolModel.UniqueId == keyValuePair.Key)
                        {
                            AddChild(conflict, child, DifferenceConflictViewModel.MergeToolModel, true);
                        }
                    }
                    else
                    {
                        AddChild(conflict, child,null,false);
                    }
                   
                }
            }

            if (CurrentConflictViewModel.Children.Any())
            {
                foreach (var mergeToolModel in CurrentConflictViewModel.Children)
                {
                    var keyValuePair = differenceStore.SingleOrDefault(pair => pair.Key.Equals(mergeToolModel.UniqueId));
                    var toolModel = DifferenceConflictViewModel?.Children?.SingleOrDefault(model => model.UniqueId == keyValuePair.Key);

                    var childConflict = new CompleteConflict
                    {
                        UniqueId = mergeToolModel.UniqueId,
                        CurrentViewModel = mergeToolModel,
                        DiffViewModel = toolModel,
                       
                    };
                    foreach (var child in mergeToolModel.Children)
                    {
                        var keyValuePairChild = differenceStore.SingleOrDefault(pair => pair.Key.Equals(child.UniqueId));
                        var toolModelChild = DifferenceConflictViewModel?.Children?.SingleOrDefault(model => model.UniqueId == keyValuePairChild.Key);
                        AddChild(childConflict, child, toolModelChild, keyValuePairChild.Value);
                      
                    }
                    Conflicts.Add(childConflict);

                }
            }

            Conflicts.Add(conflict);

            if (CurrentConflictViewModel != null)
            {
                CurrentConflictViewModel.WorkflowName = currentResourceModel.ResourceName;
                CurrentConflictViewModel.GetDataList();
            }
            if (DifferenceConflictViewModel != null)
            {
                DifferenceConflictViewModel.WorkflowName = differenceResourceModel.ResourceName;
                DifferenceConflictViewModel.GetDataList();
            }
            HasVariablesConflict = true;
            HasWorkflowNameConflict = true;
            SetServerName(currentResourceModel);
            DisplayName = "Merge Conflicts" + _serverName;


            AddAnItem = new DelegateCommand(o =>
            {
                //var step = new FlowStep { Action = act };
                //WorkflowDesignerViewModel.AddItem(step);
            });
            WorkflowDesignerViewModel.CanViewWorkflowLink = false;
            Conflicts = Conflicts.Reverse().ToObservableCollection();
        }

        public ObservableCollection<ICompleteConflict> Conflicts { get; set; }

        public System.Windows.Input.ICommand AddAnItem { get; set; }

        public WorkflowDesignerViewModel WorkflowDesignerViewModel { get; set; }

        public IConflictViewModel CurrentConflictViewModel { get; set; }
        public IConflictViewModel DifferenceConflictViewModel { get; set; }

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
