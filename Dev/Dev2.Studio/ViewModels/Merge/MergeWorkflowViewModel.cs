using System.Activities.Presentation.Model;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Runtime.Configuration.ViewModels.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;      
using System.Linq;
using Dev2.Studio.Core.Activities.Utils;
using System;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using System.Activities;
using Warewolf;

namespace Dev2.ViewModels.Merge
{
    public class MergeWorkflowViewModel : BindableBase, IMergeWorkflowViewModel
    {
        private string _displayName;
        private string _serverName;

        public MergeWorkflowViewModel(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel)
        {
            #region TO DELETE
            //var assignId = Guid.NewGuid();
            //var foreachId = Guid.NewGuid();
            //var dsfMultiAssignActivity = new DsfMultiAssignActivity()
            //{
            //    UniqueID = assignId.ToString(),
            //    FieldsCollection = new List<ActivityDTO>()
            //    {
            //        new ActivityDTO("a","a",1),
            //        new ActivityDTO("a","a",2)
            //    }
            //};
            //var dsfMultiAssignActivity1 = new DsfMultiAssignActivity()
            //{
            //    UniqueID = assignId.ToString(),
            //    FieldsCollection = new List<ActivityDTO>()
            //    {
            //        new ActivityDTO("a","b",1),
            //        new ActivityDTO("a","a",2)
            //    }
            //};
            //var dsfForEachActivity = new DsfForEachActivity()
            //{
            //    UniqueID = foreachId.ToString(),
            //    DataFunc = new ActivityFunc<string, bool>()
            //    {
            //        Handler = new DsfDateTimeActivity()
            //    }
            //};
            //var dsfForEachActivity1 = new DsfForEachActivity()
            //{
            //    UniqueID = foreachId.ToString(),
            //    DataFunc = new ActivityFunc<string, bool>()
            //    {
            //        Handler = new DsfDateTimeActivity()
            //    }
            //};
            //var assignOne = ModelItemUtils.CreateModelItem(dsfMultiAssignActivity);
            //var assign2 = ModelItemUtils.CreateModelItem(dsfMultiAssignActivity1);
            //var forEach = ModelItemUtils.CreateModelItem(dsfForEachActivity);
            //var forEach1 = ModelItemUtils.CreateModelItem(dsfForEachActivity1);

            //var currentChanges = new List<ModelItem>()
            //{
            //    assignOne,forEach
            //};
            //var differenceChanges = new List<ModelItem>()
            //{
            //    assign2,forEach1
            //};
            #endregion

            WorkflowDesignerViewModel = new WorkflowDesignerViewModel(currentResourceModel,false);
            WorkflowDesignerViewModel.CreateBlankWorkflow();
            var mergeParser = CustomContainer.Get<IParseServiceForDifferences>();
            var currentChanges = mergeParser.GetDifferences(currentResourceModel, differenceResourceModel);           
            
            Conflicts = new ObservableCollection<CompleteConflict>();

            foreach (var curr in currentChanges)
            {
                var conflict = new CompleteConflict();
                CurrentConflictViewModel = new CurrentConflictViewModel(curr.current,currentResourceModel);
                conflict.CurrentViewModel = CurrentConflictViewModel.MergeToolModel;

                var uniqueId = curr.uniqueId;
                if (curr.conflict)
                {
                    DifferenceConflictViewModel = new DifferenceConflictViewModel(curr.difference, differenceResourceModel);
                    conflict.DiffViewModel = DifferenceConflictViewModel.MergeToolModel;
                }
                
                Conflicts.Add(conflict);
            }

            CurrentConflictViewModel.WorkflowName = currentResourceModel.ResourceName;
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
