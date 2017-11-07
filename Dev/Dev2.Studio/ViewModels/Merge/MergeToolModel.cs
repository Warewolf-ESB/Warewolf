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

namespace Dev2.ViewModels.Merge
{
    public class MergeToolModel : BindableBase, IMergeToolModel
    {
        private ImageSource _mergeIcon;
        private string _mergeDescription;
        private bool _isMergeChecked;
        private bool _isMergeEnabled;
        private bool _isMergeVisible;
        private ObservableCollection<IMergeToolModel> _children;
        private string _parentDescription;
        private bool _hasParent;
        private Guid _uniqueId;
        private FlowNode _flowNode;
        private IMergeToolModel _parent;
        private string _nodeArmDescription;
        private bool _processEvents;

        public MergeToolModel()
        {
            Children = new ObservableCollection<IMergeToolModel>();
            _processEvents = true;
        }

        public ActivityDesignerViewModel ActivityDesignerViewModel { get; set; }

        [JsonIgnore]
        public ImageSource MergeIcon
        {
            set
            {
                _mergeIcon = value;
                OnPropertyChanged(() => MergeIcon);
           
            }
            get => _mergeIcon;
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
                var current = MemberwiseClone();
                _isMergeChecked = value;
                if (_processEvents)
                {
                    SomethingModelToolChanged?.Invoke(current, this);
                }
                OnPropertyChanged(() => IsMergeChecked);
                
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

        public bool IsMergeVisible
        {
            get => _isMergeVisible;
            set
            {
                _isMergeVisible = value;
                OnPropertyChanged(() => IsMergeVisible);
            }
        }

        public IToolConflict Container { get; set; }

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
                OnPropertyChanged(() => Children);
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

        public FlowNode FlowNode
        {
            get => _flowNode;
            set
            {
                _flowNode = value;
                OnPropertyChanged(() => FlowNode);
            }
        }

        public string ParentDescription
        {
            get => _parentDescription;
            set
            {
                _parentDescription = value;
                OnPropertyChanged(() => ParentDescription);
            }
        }
        public bool HasParent
        {
            get => _hasParent;
            set
            {
                _hasParent = value;
                OnPropertyChanged(() => HasParent);
            }
        }

        public ModelItem ModelItem { get; set; }
        public Point NodeLocation { get; set; }

        public bool IsTrueArm { get; set; }
        public string NodeArmDescription
        {
            get => _nodeArmDescription;
            set
            {
                _nodeArmDescription = value;
                OnPropertyChanged(() => NodeArmDescription);
            }
        }

        public event ModelToolChanged SomethingModelToolChanged;

        public void DisableEvents()
        {
            IsMergeChecked = false;
            _processEvents = false;
        }

        public void EnableEvents() => _processEvents = true;
    }
}