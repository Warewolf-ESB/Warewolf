using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Runtime.Configuration.ViewModels.Base;
using System.Collections.ObjectModel;
using Dev2.Studio.Core.Activities.Utils;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities;
using Dev2.Common;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Communication;
using Dev2.Data.SystemTemplates.Models;

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
                var completeConflict = new CompleteConflict()
                {
                    CurrentViewModel = child,
                    UniqueId = child.UniqueId,

                };
                parent.Children.Add(completeConflict);
                foreach (var mergeToolModel in child.Children)
                {
                    AddChild(completeConflict, mergeToolModel);
                }

            }

            foreach (var curr in currentChanges)
            {
                var completeConflicts = Conflicts.Flatten(completeConflict => completeConflict.Children ?? new ObservableCollection<ICompleteConflict>());
                if (completeConflicts.Any(a => a.UniqueId == curr.uniqueId)) continue;
                var conflict = new CompleteConflict { UniqueId = curr.uniqueId };
                //if (curr.current.GetCurrentValue() is DsfDecision dec)
                {
                    //if (curr.conflict)
                    {

                        CurrentConflictViewModel = new ConflictViewModel(curr.current, currentResourceModel);
                        if (CurrentConflictViewModel?.MergeToolModel != null)
                        {
                            conflict.CurrentViewModel = CurrentConflictViewModel.MergeToolModel;
                            foreach (var child in CurrentConflictViewModel.MergeToolModel.Children)
                            {
                                AddChild(conflict, child);
                            }
                        }

                        if (curr.conflict)
                        {
                            DifferenceConflictViewModel = new ConflictViewModel(curr.difference, differenceResourceModel);
                            if (DifferenceConflictViewModel?.MergeToolModel != null)
                            {
                                conflict.DiffViewModel = DifferenceConflictViewModel.MergeToolModel;
                                foreach (var child in DifferenceConflictViewModel.MergeToolModel.Children)
                                {
                                    AddChild(conflict, child);
                                }
                            }
                        }


                        /*if (dec.TrueArm != null)
                        {
                            var mergeToolModels = CurrentConflictViewModel.MergeToolModel?.Children?? new ObservableCollection<IMergeToolModel>();
                            foreach (var mergeToolModel in mergeToolModels)
                            {
                                var trueArmConflict = new CompleteConflict();
                                if (mergeToolModel != null)
                                {
                                    trueArmConflict.CurrentViewModel = mergeToolModel;
                                    trueArmConflict.CurrentViewModel.HasParent = true;
                                    trueArmConflict.CurrentViewModel.ParentDescription = dec.Conditions.TrueArmText;
                                    conflict.Children.Add(trueArmConflict);
                                }
                              
                            }
                        }

                        if (dec.FalseArm != null)
                        {
                            var mergeToolModels = CurrentConflictViewModel.MergeToolModel?.Children ?? new ObservableCollection<IMergeToolModel>();
                            foreach (var mergeToolModel in mergeToolModels)
                            {
                                var falseArmConflict = new CompleteConflict();
                                if (mergeToolModel != null)
                                {
                                    falseArmConflict.CurrentViewModel = mergeToolModel;
                                    falseArmConflict.CurrentViewModel.HasParent = true;
                                    falseArmConflict.CurrentViewModel.ParentDescription = dec.Conditions.FalseArmText;
                                    conflict.Children.Add(falseArmConflict);
                                }
                               
                            }
                        }

                        if (curr.conflict)
                        {
                            if (dec.TrueArm != null)
                            {
                                var deTrueArm = dec.TrueArm.Flatten(p => p.NextNodes ?? new List<IDev2Activity>());
                                foreach (var dev2Activity in deTrueArm)
                                {
                                    var trueArmConflict = new CompleteConflict();
                                    var differenceConflictViewModel =
                                        new ConflictViewModel(ModelItemUtils.CreateModelItem(dev2Activity),
                                            currentResourceModel);
                                    if (differenceConflictViewModel?.MergeToolModel != null)
                                    {
                                        trueArmConflict.DiffViewModel = differenceConflictViewModel.MergeToolModel;
                                        trueArmConflict.DiffViewModel.HasParent = true;
                                        trueArmConflict.DiffViewModel.ParentDescription = dec.Conditions.TrueArmText;
                                    }
                                    conflict.Children.Add(trueArmConflict);
                                }
                            }

                            if (dec.FalseArm != null)
                            {
                                var deTrueArm = dec.FalseArm.Flatten(p => p.NextNodes ?? new List<IDev2Activity>());
                                foreach (var dev2Activity in deTrueArm)
                                {
                                    var falseArmConflict = new CompleteConflict();
                                    var differenceConflictViewModel =
                                        new ConflictViewModel(ModelItemUtils.CreateModelItem(dev2Activity),
                                            differenceResourceModel);
                                    if (differenceConflictViewModel?.MergeToolModel != null)
                                    {
                                        falseArmConflict.DiffViewModel = differenceConflictViewModel.MergeToolModel;
                                        falseArmConflict.DiffViewModel.HasParent = true;
                                        falseArmConflict.DiffViewModel.ParentDescription = dec.Conditions.FalseArmText;
                                    }
                                    conflict.Children.Add(falseArmConflict);
                                }
                            }
                        }*/


                        Conflicts.Add(conflict);
                    }
                }
                //else
                {
                    if (curr.conflict)
                    {
                        CurrentConflictViewModel = new ConflictViewModel(curr.current, currentResourceModel);
                        if (CurrentConflictViewModel?.MergeToolModel != null)
                        {
                            conflict.CurrentViewModel = CurrentConflictViewModel.MergeToolModel;
                        }

                        DifferenceConflictViewModel = new ConflictViewModel(curr.difference, differenceResourceModel);
                        if (DifferenceConflictViewModel?.MergeToolModel != null)
                        {
                            conflict.DiffViewModel = DifferenceConflictViewModel.MergeToolModel;
                        }
                        Conflicts.Add(conflict);
                    }
                }
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
            //HasVariablesConflict = false;

            HasWorkflowNameConflict = CurrentConflictViewModel?.WorkflowName != DifferenceConflictViewModel?.WorkflowName;

            SetServerName(currentResourceModel);
            DisplayName = "Merge Conflicts" + _serverName;


            AddAnItem = new DelegateCommand(o =>
            {
                //var step = new FlowStep { Action = act };
                //WorkflowDesignerViewModel.AddItem(step);
            });
            WorkflowDesignerViewModel.CanViewWorkflowLink = false;
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
