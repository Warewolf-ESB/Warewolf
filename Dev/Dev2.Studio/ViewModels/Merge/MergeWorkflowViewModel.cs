using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Runtime.Configuration.ViewModels.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Studio.Core.Activities.Utils;
using System.Collections.ObjectModel;
using System;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using System.Activities;
using Warewolf;
using System.Activities.Statements;
using Dev2.Common;
using Microsoft.Practices.Prism;

namespace Dev2.ViewModels.Merge
{
    public class MergeWorkflowViewModel : BindableBase, IMergeWorkflowViewModel
    {
        private string _displayName;
        private string _serverName;

        public MergeWorkflowViewModel(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel)
        {
            WorkflowDesignerViewModel = new WorkflowDesignerViewModel(currentResourceModel, false);
            WorkflowDesignerViewModel.CreateBlankWorkflow();
            var mergeParser = CustomContainer.Get<IParseServiceForDifferences>();
            var currentChanges = mergeParser.GetDifferences(currentResourceModel, differenceResourceModel);

            Conflicts = new ObservableCollection<CompleteConflict>();

            foreach (var curr in currentChanges)
            {
                var completeConflicts = Conflicts.Flatten(completeConflict =>
                    completeConflict.Children ?? new ObservableCollection<CompleteConflict>());
                if (completeConflicts.Any(p => p.CurrentViewModel?.UniqueId == curr.uniqueId)) continue;
                var conflict = new CompleteConflict();
                if (curr.current.ItemType == typeof(FlowDecision))
                {
                    CurrentConflictViewModel = new CurrentConflictViewModel(curr.current, currentResourceModel);
                    if (CurrentConflictViewModel?.MergeToolModel != null)
                    {
                        conflict.CurrentViewModel = CurrentConflictViewModel.MergeToolModel;

                        foreach (var child in CurrentConflictViewModel.MergeToolModel.Children)
                        {
                            conflict.Children.Add(new CompleteConflict
                            {
                                CurrentViewModel = child
                            });
                        }
                    }

                    if (curr.conflict)
                    {
                        DifferenceConflictViewModel = new DifferenceConflictViewModel(curr.difference, differenceResourceModel);
                        if (DifferenceConflictViewModel?.MergeToolModel != null)
                        {
                            conflict.DiffViewModel = DifferenceConflictViewModel.MergeToolModel;
                        }
                        foreach (var child in CurrentConflictViewModel.Children)
                        {
                            conflict.Children.Add(new CompleteConflict()
                            {
                                CurrentViewModel = child
                            });
                        }

                    }
                    Conflicts.Add(conflict);
                }
                else
                {
                    CurrentConflictViewModel = new CurrentConflictViewModel(curr.current, currentResourceModel);
                    if (CurrentConflictViewModel?.MergeToolModel != null)
                    {
                        conflict.CurrentViewModel = CurrentConflictViewModel.MergeToolModel;
                    }
                    if (curr.conflict)
                    {
                        DifferenceConflictViewModel = new DifferenceConflictViewModel(curr.difference, differenceResourceModel);
                        if (DifferenceConflictViewModel?.MergeToolModel != null)
                        {
                            conflict.DiffViewModel = DifferenceConflictViewModel.MergeToolModel;
                        }

                    }
                    Conflicts.Add(conflict);
                }
            }

            if (CurrentConflictViewModel != null)
                CurrentConflictViewModel.WorkflowName = currentResourceModel.ResourceName;
            if (DifferenceConflictViewModel != null)
                DifferenceConflictViewModel.WorkflowName = differenceResourceModel.ResourceName;

            SetServerName(currentResourceModel);
            DisplayName = "Merge Conflicts" + _serverName;


            AddAnItem = new DelegateCommand(o =>
            {
                //var step = new FlowStep { Action = act };
                //WorkflowDesignerViewModel.AddItem(step);
            });
            WorkflowDesignerViewModel.CanViewWorkflowLink = false;
        }

        public ObservableCollection<CompleteConflict> Conflicts { get; set; }

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
