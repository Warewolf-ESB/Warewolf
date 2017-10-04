using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using Newtonsoft.Json;
using System.Activities.Statements;
using System.Activities.Presentation.Model;
using System.Windows;
using Caliburn.Micro;
using Dev2.Common;

namespace Dev2.ViewModels.Merge
{
    public class MergeToolModel : BindableBase, IMergeToolModel
    {
        private ImageSource _mergeIcon;
        private string _mergeDescription;
        private bool _isMergeChecked;
        private bool _isMergeEnabled;
        private ObservableCollection<IMergeToolModel> _children;
        private string _parentDescription;
        private bool _hasParent;
        private Guid _uniqueId;
        private FlowNode _activityType;
        private IMergeToolModel _parent;

        public MergeToolModel()
        {
            Children = new ObservableCollection<IMergeToolModel>();
        }

        public ActivityDesignerViewModel ActivityDesignerViewModel { get; set; }

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
                OnPropertyChanged(() => IsMergeChecked);
                SomethingModelToolChanged?.Invoke(this, this);
                if (Parent == null)
                {
                    Children?.Flatten(a => a.Children).Apply(a => a.IsMergeChecked = true);
                }
            }
        }

        public bool IsMergeEnabled
        {
            get => _isMergeEnabled;
            set
            {
                _isMergeEnabled = value;
                OnPropertyChanged(() => IsMergeEnabled);
            }
        }

        public IMergeToolModel Parent
        {
            get => _parent;
            set
            {
                _parent = value;
                OnPropertyChanged(() => Parent);
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
            get => _uniqueId;
            set
            {
                _uniqueId = value;
                OnPropertyChanged(() => UniqueId);
            }
        }

        public FlowNode ActivityType
        {
            get => _activityType;
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

        public ModelItem FlowNode { get; set; }
        public Point NodeLocation { get; set; }

        public event ModelToolChanged SomethingModelToolChanged;
    }
}