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
            var mergeParser = CustomContainer.Get<IParseServiceForDifferences>();
            var currentChanges = mergeParser.GetDifferences(currentResourceModel, differenceResourceModel);
            Conflicts = new ObservableCollection<ICompleteConflict>();

            void AddChild(ICompleteConflict parent, IMergeToolModel child)
            {
                var completeConflict = new CompleteConflict { UniqueId = child.UniqueId };
                completeConflict.CurrentViewModel = child;
                parent.Children.Add(completeConflict);
                foreach (var mergeToolModel in child.Children)
                {
                    AddChild(completeConflict, mergeToolModel);
                }
            }
            void AddDiffChild(ICompleteConflict parent, IMergeToolModel child)
            {
                var completeConflict = new CompleteConflict { UniqueId = child.UniqueId };
                completeConflict.DiffViewModel = child;
                parent.Children.Add(completeConflict);
                foreach (var mergeToolModel in child.Children)
                {
                    AddDiffChild(completeConflict, mergeToolModel);
                }
            }

            foreach (var curr in currentChanges)
            {
                var completeConflicts = Conflicts.Flatten(completeConflict => completeConflict.Children ?? new ObservableCollection<ICompleteConflict>());
                if (completeConflicts.Any(a => a.UniqueId == curr.uniqueId)) continue;
                var conflict = new CompleteConflict { UniqueId = curr.uniqueId };
                CurrentConflictViewModel = new ConflictViewModel(curr.current, currentResourceModel);
                if (CurrentConflictViewModel?.MergeToolModel != null)
                {
                    conflict.CurrentViewModel = CurrentConflictViewModel.MergeToolModel;
                    foreach (var child in CurrentConflictViewModel.MergeToolModel.Children)
                    {
                        AddChild(conflict, child);
                    }
                }
                if (CurrentConflictViewModel.Children.Any())
                {
                    foreach (var mergeToolModel in CurrentConflictViewModel.Children)
                    {
                        var childConflict = new CompleteConflict { UniqueId = curr.uniqueId };
                        childConflict.CurrentViewModel = mergeToolModel;
                        foreach (var child in mergeToolModel.Children)
                        {
                            AddChild(childConflict, child);
                        }
                        conflict.Children.Add(childConflict);
                    }
                }

                //if (curr.conflict)
                DifferenceConflictViewModel = new ConflictViewModel(curr.difference, differenceResourceModel);
                if (DifferenceConflictViewModel?.MergeToolModel != null)
                {
                    conflict.DiffViewModel = DifferenceConflictViewModel.MergeToolModel;
                    foreach (var child in DifferenceConflictViewModel.MergeToolModel.Children)
                    {
                        AddDiffChild(conflict, child);
                    }
                }

                if (DifferenceConflictViewModel.Children.Any())
                {
                    foreach (var mergeToolModel in DifferenceConflictViewModel.Children)
                    {
                        var childConflict = new CompleteConflict { UniqueId = curr.uniqueId };
                        childConflict.DiffViewModel = mergeToolModel;
                        foreach (var child in mergeToolModel.Children)
                        {
                            AddDiffChild(childConflict, child);
                        }
                        Conflicts.Add(childConflict);
                    }
                }

                Conflicts.Add(conflict);
            }

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
