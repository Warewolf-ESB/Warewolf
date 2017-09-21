using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Runtime.Configuration.ViewModels.Base;
using System.Collections.ObjectModel;
using Dev2.Studio.Core.Activities.Utils;
using System;
using System.Activities.Statements;
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

            foreach (var curr in currentChanges)
            {
                var conflict = new CompleteConflict();
                if (curr.current.ItemType == typeof(FlowDecision))
                {
                    //if (curr.conflict)
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

                        var currDec = curr.current.GetCurrentValue<FlowDecision>();
                        var trueArmConflict = new CompleteConflict();
                        var falseArmConflict = new CompleteConflict();

                        var currActivity = (DsfFlowNodeActivity<bool>)currDec.Condition;
                        var currExpression = currActivity.ExpressionText;
                        var currEval = Dev2DecisionStack.ExtractModelFromWorkflowPersistedData(currExpression);

                        var ser = new Dev2JsonSerializer();
                        var currDds = ser.Deserialize<Dev2DecisionStack>(currEval);

                        if (currDec.True != null)
                        {
                            var currentConflictViewModel = new ConflictViewModel(ModelItemUtils.CreateModelItem(currDec.True), currentResourceModel);
                            if (currentConflictViewModel?.MergeToolModel != null)
                            {
                                trueArmConflict.CurrentViewModel = currentConflictViewModel.MergeToolModel;
                                trueArmConflict.CurrentViewModel.HasParent = true;
                                trueArmConflict.CurrentViewModel.ParentDescription = currDds.TrueArmText;
                            }
                        }
                        if (currDec.False != null)
                        {
                            var currentConflictViewModel = new ConflictViewModel(ModelItemUtils.CreateModelItem(currDec.False), currentResourceModel);
                            if (currentConflictViewModel?.MergeToolModel != null)
                            {
                                falseArmConflict.CurrentViewModel = currentConflictViewModel.MergeToolModel;
                                falseArmConflict.CurrentViewModel.HasParent = true;
                                falseArmConflict.CurrentViewModel.ParentDescription = currDds.FalseArmText;
                            }
                        }

                        var diffDec = curr.difference.GetCurrentValue<FlowDecision>();

                        var diffActivity = (DsfFlowNodeActivity<bool>)diffDec.Condition;
                        var diffExpression = diffActivity.ExpressionText;
                        var diffEval = Dev2DecisionStack.ExtractModelFromWorkflowPersistedData(diffExpression);

                        var diffDds = ser.Deserialize<Dev2DecisionStack>(diffEval);

                        if (diffDec.True != null)
                        {
                            var differenceConflictViewModel = new ConflictViewModel(ModelItemUtils.CreateModelItem(diffDec.True), currentResourceModel);
                            if (differenceConflictViewModel?.MergeToolModel != null)
                            {
                                trueArmConflict.DiffViewModel = differenceConflictViewModel.MergeToolModel;
                                trueArmConflict.DiffViewModel.HasParent = true;
                                trueArmConflict.DiffViewModel.ParentDescription = currDds.TrueArmText;
                            }
                        }
                        if (diffDec.False != null)
                        {
                            var differenceConflictViewModel = new ConflictViewModel(ModelItemUtils.CreateModelItem(diffDec.False), differenceResourceModel);
                            if (differenceConflictViewModel?.MergeToolModel != null)
                            {
                                falseArmConflict.DiffViewModel = differenceConflictViewModel.MergeToolModel;
                                falseArmConflict.DiffViewModel.HasParent = true;
                                falseArmConflict.DiffViewModel.ParentDescription = currDds.FalseArmText;
                            }
                        }
                        conflict.Children.Add(trueArmConflict);
                        conflict.Children.Add(falseArmConflict);
                        Conflicts.Add(conflict);
                    }
                }
                else
                {
                    //if (curr.conflict)
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
            HasVariablesConflict = true;
            //HasWorkflowNameConflict = CurrentConflictViewModel?.WorkflowName != DifferenceConflictViewModel?.WorkflowName;
            HasWorkflowNameConflict = true;
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
