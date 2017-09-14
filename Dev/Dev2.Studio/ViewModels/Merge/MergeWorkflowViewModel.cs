using Dev2.Activities.Designers2.Comment;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Decision;
using Dev2.Activities.Designers2.MultiAssign;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.ViewModels.DataList;
using Microsoft.Practices.Prism.Mvvm;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using System.Collections.ObjectModel;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Runtime.Configuration.ViewModels.Base;
using System.Activities.Statements;
using Dev2.Studio.Factory;

namespace Dev2.ViewModels.Merge
{
    public class MergeWorkflowViewModel : BindableBase, IMergeWorkflowViewModel
    {
        public MergeWorkflowViewModel()
        {
            CurrentConflictViewModel = new CurrentConflictViewModel();
            CurrentConflictViewModel.WorkflowName = "Current WorkflowName";
            DifferenceConflictViewModel = new DifferenceConflictViewModel();
            DifferenceConflictViewModel.WorkflowName = "Difference WorkflowName";

            DataListSingleton.SetDataList(new DataListViewModel());
            var act = new DsfMultiAssignActivity();
            act.FieldsCollection.Add(new ActivityDTO("[[a]]", "1", 1));
            var mi = ModelItemUtils.CreateModelItem(act);
            CurrentMergeList = new List<ActivityDesignerViewModel>
            {
                new MultiAssignDesignerViewModel(mi)
                {
                    ShowLarge = true
                },
                new CommentDesignerViewModel(ModelItemUtils.CreateModelItem(new DsfCommentActivity())),
                new DecisionDesignerViewModel(ModelItemUtils.CreateModelItem(new DsfFlowDecisionActivity()))
            };

            string newWorflowName = NewWorkflowNames.Instance.GetNext();

            IContextualResourceModel tempResource = ResourceModelFactory.CreateResourceModel(CustomContainer.Get<IShellViewModel>().ActiveServer, @"WorkflowService",
                newWorflowName);
            tempResource.Category = @"Unassigned\" + newWorflowName;
            tempResource.ResourceName = newWorflowName;
            tempResource.DisplayName = newWorflowName;
            tempResource.IsNewWorkflow = true;

            WorkflowDesignerViewModel = new WorkflowDesignerViewModel(tempResource, true);
            AddAnItem = new DelegateCommand(o => {

                var step = new FlowStep { Action = act };
                WorkflowDesignerViewModel.AddItem(step);
            });
        }

        public System.Windows.Input.ICommand AddAnItem { get; set; }

        public List<ActivityDesignerViewModel> CurrentMergeList { get; set; }
        public WorkflowDesignerViewModel WorkflowDesignerViewModel { get; set; }

        public IConflictViewModel CurrentConflictViewModel { get; set; }
        public IConflictViewModel DifferenceConflictViewModel { get; set; }
    }

    public class CurrentConflictViewModel : BindableBase, ICurrentConflictViewModel
    {
        private string _workflowName;
        
        public CurrentConflictViewModel()
        {
            #region MockToolSetup
            MergeConflicts = new ObservableCollection<IMergeViewModel>();

            DataListSingleton.SetDataList(new DataListViewModel());
            var assign = new DsfMultiAssignActivity();
            assign.FieldsCollection.Add(new ActivityDTO("[[a]]", "1", 1));

            //ASSIGN
            MergeViewModel = new MergeViewModel();
            var mergeAssignVM = MergeViewModel as MergeViewModel;
            mergeAssignVM.IsMergeExpanded = true;
            mergeAssignVM.IsMergeExpanderEnabled = true;
            mergeAssignVM.MergeDescription = "Current Assign (0)";
            mergeAssignVM.SetMergeIcon(typeof(DsfMultiAssignActivity));
            mergeAssignVM.ActivityDesignerViewModel = new MultiAssignDesignerViewModel(ModelItemUtils.CreateModelItem(assign));

            MergeConflicts.Add(mergeAssignVM);

            //DECISION
            var decision = new DsfFlowDecisionActivity();
            MergeViewModel = new MergeViewModel();
            var mergeDecisionVM = MergeViewModel as MergeViewModel;
            mergeDecisionVM.IsMergeExpanded = true;
            mergeDecisionVM.IsMergeExpanderEnabled = true;
            mergeDecisionVM.MergeDescription = "Current Decision (0)";
            mergeDecisionVM.SetMergeIcon(typeof(DsfFlowDecisionActivity));
            mergeDecisionVM.ActivityDesignerViewModel = new DecisionDesignerViewModel(ModelItemUtils.CreateModelItem(decision));

            MergeConflicts.Add(mergeDecisionVM);

            #endregion

            string newWorflowName = NewWorkflowNames.Instance.GetNext();
            IContextualResourceModel tempResource = ResourceModelFactory.CreateResourceModel(CustomContainer.Get<IShellViewModel>().ActiveServer, @"WorkflowService",
                newWorflowName);
            tempResource.Category = @"Unassigned\" + newWorflowName;
            tempResource.ResourceName = newWorflowName;
            tempResource.DisplayName = newWorflowName;
            tempResource.IsNewWorkflow = true;

            DataListViewModel = DataListViewModelFactory.CreateDataListViewModel(tempResource) as DataListViewModel;
            DataListViewModel.ViewSortDelete = false;
        }

        public string WorkflowName
        {
            get { return _workflowName; }
            set
            {
                _workflowName = value;
                OnPropertyChanged(() => WorkflowName);
            }
        }
        
        public DataListViewModel DataListViewModel { get; set; }
        public IMergeViewModel MergeViewModel { get; set; }
        public ObservableCollection<IMergeViewModel> MergeConflicts { get; set; }
    }

    public class DifferenceConflictViewModel : BindableBase, IDifferenceConflictViewModel
    {
        private string _workflowName;
        
        public DifferenceConflictViewModel()
        {
            #region MockToolSetup
            MergeConflicts = new ObservableCollection<IMergeViewModel>();

            DataListSingleton.SetDataList(new DataListViewModel());
            var assign = new DsfMultiAssignActivity();
            assign.FieldsCollection.Add(new ActivityDTO("[[a]]", "1", 1));

            //ASSIGN
            MergeViewModel = new MergeViewModel();
            var mergeAssignVM = MergeViewModel as MergeViewModel;
            mergeAssignVM.IsMergeExpanded = true;
            mergeAssignVM.IsMergeExpanderEnabled = true;
            mergeAssignVM.MergeDescription = "Difference Assign (0)";
            mergeAssignVM.SetMergeIcon(typeof(DsfMultiAssignActivity));
            mergeAssignVM.ActivityDesignerViewModel = new MultiAssignDesignerViewModel(ModelItemUtils.CreateModelItem(assign));

            MergeConflicts.Add(mergeAssignVM);

            //DECISION
            var decision = new DsfFlowDecisionActivity();
            MergeViewModel = new MergeViewModel();
            var mergeDecisionVM = MergeViewModel as MergeViewModel;
            mergeDecisionVM.IsMergeExpanded = true;
            mergeDecisionVM.IsMergeExpanderEnabled = true;
            mergeDecisionVM.MergeDescription = "Difference Decision (0)";
            mergeDecisionVM.SetMergeIcon(typeof(DsfFlowDecisionActivity));
            mergeDecisionVM.ActivityDesignerViewModel = new DecisionDesignerViewModel(ModelItemUtils.CreateModelItem(decision));

            MergeConflicts.Add(mergeDecisionVM);

            #endregion

            string newWorflowName = NewWorkflowNames.Instance.GetNext();
            IContextualResourceModel tempResource = ResourceModelFactory.CreateResourceModel(CustomContainer.Get<IShellViewModel>().ActiveServer, @"WorkflowService",
                newWorflowName);
            tempResource.Category = @"Unassigned\" + newWorflowName;
            tempResource.ResourceName = newWorflowName;
            tempResource.DisplayName = newWorflowName;
            tempResource.IsNewWorkflow = true;

            DataListViewModel = DataListViewModelFactory.CreateDataListViewModel(tempResource) as DataListViewModel;
            DataListViewModel.ViewSortDelete = false;
        }

        public string WorkflowName
        {
            get { return _workflowName; }
            set
            {
                _workflowName = value;
                OnPropertyChanged(() => WorkflowName);
            }
        }
        
        public DataListViewModel DataListViewModel { get; set; }
        public IMergeViewModel MergeViewModel { get; set; }
        public ObservableCollection<IMergeViewModel> MergeConflicts { get; set; }
    }

    public class MergeViewModel : BindableBase, IMergeViewModel
    {
        private bool _isMergeExpanderEnabled;
        private ImageSource _mergeIcon;
        private bool _isMergeExpanded;
        private string _mergeDescription;
        private List<string> _fieldCollection;
        private bool _isMergeChecked;
        private bool _isVariablesChecked;

        public MergeViewModel()
        {
            
        }

        public void SetMergeIcon(Type type)
        {
            if (type == null)
            {
                return;
            }
            if (type == typeof(DsfActivity))
            {
                MergeIcon = Application.Current?.TryFindResource("Explorer-WorkflowService") as ImageSource;
            }
            else if (type.GetCustomAttributes().Any(a => a is ToolDescriptorInfo))
            {
                var desc = GetDescriptorFromAttribute(type);
                MergeIcon = Application.Current?.TryFindResource(desc.Icon) as ImageSource;
            }
            else
            {
                MergeIcon = null;
            }
        }

        public ActivityDesignerViewModel ActivityDesignerViewModel { get; set; }

        IToolDescriptor GetDescriptorFromAttribute(Type type)
        {
            var info = type.GetCustomAttributes(typeof(ToolDescriptorInfo)).First() as ToolDescriptorInfo;

            return new ToolDescriptor(info.Id, info.Designer, new WarewolfType(type.FullName, type.Assembly.GetName().Version, type.Assembly.Location), info.Name, info.Icon, type.Assembly.GetName().Version, true, info.Category, ToolType.Native, info.IconUri, info.FilterTag, info.ResourceToolTip, info.ResourceHelpText);
        }

        public List<string> FieldCollection
        {
            get { return _fieldCollection; }
            set
            {
                _fieldCollection = value;
                OnPropertyChanged(() => FieldCollection);
            }
        }
        public bool IsMergeExpanderEnabled
        {
            get { return _isMergeExpanderEnabled; }
            set
            {
                _isMergeExpanderEnabled = value;
                OnPropertyChanged(() => IsMergeExpanderEnabled);
            }
        }
        public bool IsMergeExpanded
        {
            get { return _isMergeExpanded; }
            set
            {
                _isMergeExpanded = value;
                OnPropertyChanged(() => IsMergeExpanded);
            }
        }
        [JsonIgnore]
        public ImageSource MergeIcon
        {
            get { return _mergeIcon; }
            set
            {
                _mergeIcon = value;
                OnPropertyChanged(() => MergeIcon);
            }
        }
        public string MergeDescription
        {
            get { return _mergeDescription; }
            set
            {
                _mergeDescription = value;
                OnPropertyChanged(() => MergeDescription);
            }
        }
        public bool IsMergeChecked
        {
            get { return _isMergeChecked; }
            set
            {
                _isMergeChecked = value;
                OnPropertyChanged(() => IsMergeChecked);
            }
        }
        public bool IsVariablesChecked
        {
            get { return _isVariablesChecked; }
            set
            {
                _isVariablesChecked = value;
                OnPropertyChanged(() => IsVariablesChecked);
            }
        }
    }
}
