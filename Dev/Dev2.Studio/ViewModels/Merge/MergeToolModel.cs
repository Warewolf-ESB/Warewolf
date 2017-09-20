using System.Collections.ObjectModel;
using System.Windows.Media;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using Newtonsoft.Json;

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

        public MergeToolModel()
        {
            Children = new ObservableCollection<IMergeToolModel>();
        }

        public ActivityDesignerViewModel ActivityDesignerViewModel { get; set; }

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

        public ObservableCollection<IMergeToolModel> Children
        {
            get => _children;
            set
            {
                _children = value;
                OnPropertyChanged("Children");
            }
        }

        public string ParentDescription
        {
            get { return _parentDescription; }
            set
            {
                _parentDescription = value;
                OnPropertyChanged(() => ParentDescription);
            }
        }
        public bool HasParent
        {
            get { return _hasParent; }
            set
            {
                _hasParent = value;
                OnPropertyChanged(() => HasParent);
            }
        }
    }
}