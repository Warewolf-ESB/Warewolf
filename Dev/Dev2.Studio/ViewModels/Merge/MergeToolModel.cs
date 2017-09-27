using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using Newtonsoft.Json;
using System.Windows.Input;
using System.Activities.Statements;

namespace Dev2.ViewModels.Merge
{
    public class MergeToolModel : BindableBase, IMergeToolModel
    {
        private bool _isMergeExpanderEnabled;
        private ImageSource _mergeIcon;
        private bool _isMergeExpanded;
        private string _mergeDescription;
        private bool _isMergeChecked;
        private ObservableCollection<IMergeToolModel> _children;
        private string _parentDescription;
        private bool _hasParent;
        private Guid _uniqueId;
        private FlowNode _activityType;

        public MergeToolModel()
        {
            Children = new ObservableCollection<IMergeToolModel>();
        }

        public ActivityDesignerViewModel ActivityDesignerViewModel { get; set; }

        public bool IsMergeExpanderEnabled
        {
            get => _isMergeExpanderEnabled;
            set
            {
                _isMergeExpanderEnabled = value;
                OnPropertyChanged(() => IsMergeExpanderEnabled);
            }
        }
        public bool IsMergeExpanded
        {
            get => _isMergeExpanded;
            set
            {
                _isMergeExpanded = value;
                OnPropertyChanged(() => IsMergeExpanded);
            }
        }
        [JsonIgnore]
        public ImageSource MergeIcon
        {
            get => _mergeIcon;
            set
            {
                _mergeIcon = value;
                OnPropertyChanged(() => MergeIcon);
            }
        }
        public string MergeDescription
        {
            get => _mergeDescription;
            set
            {
                _mergeDescription = value;
                OnPropertyChanged(() => MergeDescription);
            }
        }
        public bool IsMergeChecked
        {
            get => _isMergeChecked;
            set
            {
                _isMergeChecked = value;
                IsMergeExpanderEnabled = _isMergeChecked;
                OnPropertyChanged(() => IsMergeChecked);
                SomethingModelToolChanged?.Invoke(this, this);
            }
        }
        public ObservableCollection<IMergeToolModel> Children
        {
            get => _children;
            set
            {
                _children = value;
                OnPropertyChanged("Children");
            }
        }

        public Guid UniqueId
        {
            get { return _uniqueId; }
            set
            {
                _uniqueId = value;
                OnPropertyChanged(() => UniqueId);
            }
        }

        public FlowNode ActivityType
        {
            get { return _activityType; }
            set
            {
                _activityType = value;
                OnPropertyChanged(() => ActivityType);
            }
        }

        public string ParentDescription
        {
            get => _parentDescription;
            set
            {
                _parentDescription = value;
                OnPropertyChanged("ParentDescription");
            }
        }
        public bool HasParent
        {
            get => _hasParent;
            set
            {
                _hasParent = value;
                OnPropertyChanged("HasParent");
            }
        }

        public ICommand AddAnItem { get; set; }

        public event ModelToolChanged SomethingModelToolChanged;
    }
}