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

namespace Dev2.ViewModels.Merge
{
    public class MergeWorkflowViewModel : BindableBase, IMergeWorkflowViewModel
    {
        public MergeWorkflowViewModel()
        {
            CurrentConflictViewModel = new CurrentConflictViewModel();
            CurrentConflictViewModel.WorkflowName = "Current WorkflowName";
            CurrentConflictViewModel.WorkflowLocation = "Current WorkflowLocation";
            DifferenceConflictViewModel = new DifferenceConflictViewModel();
            DifferenceConflictViewModel.WorkflowName = "Difference WorkflowName";
            DifferenceConflictViewModel.WorkflowLocation = "Difference WorkflowLocation";
        }
        
        public IConflictViewModel CurrentConflictViewModel { get; set; }
        public IConflictViewModel DifferenceConflictViewModel { get; set; }
    }

    public class CurrentConflictViewModel : BindableBase, ICurrentConflictViewModel
    {
        private ObservableCollection<DataListHeaderItemModel> _baseCollection;
        private string _workflowName;
        private string _workflowLocation;

        //public List<ActivityDesignerViewModel> CurrentMergeList { get; set; }
        public CurrentConflictViewModel()
        {
            //DataListSingleton.SetDataList(new DataListViewModel());
            //var act = new DsfMultiAssignActivity();
            //act.FieldsCollection.Add(new ActivityDTO("[[a]]", "1", 1));
            //CurrentMergeList = new List<ActivityDesignerViewModel>
            //{
            //    new MultiAssignDesignerViewModel(ModelItemUtils.CreateModelItem(act))
            //    {
            //        ShowLarge = true
            //    },
            //    new CommentDesignerViewModel(ModelItemUtils.CreateModelItem(new DsfCommentActivity())),
            //    new DecisionDesignerViewModel(ModelItemUtils.CreateModelItem(new DsfFlowDecisionActivity()))
            //};

            MergeViewModel = new MergeViewModel();
            MergeViewModel.IsMergeExpanded = true;
            MergeViewModel.IsMergeExpanderEnabled = true;
            MergeViewModel.MergeDescription = "Current Assign (0)";
            MergeViewModel.SetMergeIcon(typeof(DsfMultiAssignActivity));

            MergeConflicts = new ObservableCollection<IMergeViewModel>();
            MergeConflicts.Add(MergeViewModel);
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
        public string WorkflowLocation
        {
            get { return _workflowLocation; }
            set
            {
                _workflowLocation = value;
                OnPropertyChanged(() => WorkflowLocation);
            }
        }
        public ObservableCollection<DataListHeaderItemModel> BaseCollection
        {
            get { return _baseCollection; }
            set
            {
                _baseCollection = value;
                OnPropertyChanged(() => BaseCollection);
            }
        }
        public IMergeViewModel MergeViewModel { get; set; }
        public ObservableCollection<IMergeViewModel> MergeConflicts { get; set; }
    }

    public class DifferenceConflictViewModel : BindableBase, IDifferenceConflictViewModel
    {
        private ObservableCollection<DataListHeaderItemModel> _baseCollection;
        private string _workflowName;
        private string _workflowLocation;

        //public List<ActivityDesignerViewModel> DifferenceMergeList { get; set; }
        public DifferenceConflictViewModel()
        {
            //DataListSingleton.SetDataList(new DataListViewModel());
            //var act = new DsfMultiAssignActivity();
            //act.FieldsCollection.Add(new ActivityDTO("[[a]]", "1", 1));
            //DifferenceMergeList = new List<ActivityDesignerViewModel>
            //{
            //    new MultiAssignDesignerViewModel(ModelItemUtils.CreateModelItem(act))
            //    {
            //        ShowLarge = true
            //    },
            //    new CommentDesignerViewModel(ModelItemUtils.CreateModelItem(new DsfCommentActivity())),
            //    new DecisionDesignerViewModel(ModelItemUtils.CreateModelItem(new DsfFlowDecisionActivity()))
            //};

            MergeViewModel = new MergeViewModel();
            MergeViewModel.IsMergeExpanded = true;
            MergeViewModel.IsMergeExpanderEnabled = true;
            MergeViewModel.MergeDescription = "Difference Assign (0)";
            MergeViewModel.SetMergeIcon(typeof(DsfMultiAssignActivity));

            MergeConflicts = new ObservableCollection<IMergeViewModel>();
            MergeConflicts.Add(MergeViewModel);
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
        public string WorkflowLocation
        {
            get { return _workflowLocation; }
            set
            {
                _workflowLocation = value;
                OnPropertyChanged(() => WorkflowLocation);
            }
        }
        public ObservableCollection<DataListHeaderItemModel> BaseCollection
        {
            get { return _baseCollection; }
            set
            {
                _baseCollection = value;
                OnPropertyChanged(() => BaseCollection);
            }
        }
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
    }
}
