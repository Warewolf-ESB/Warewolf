using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Warewolf.Studio.AntiCorruptionLayer.ViewModels
{
    public abstract class DebugTreeViewItemViewModel<TContent> : IDebugTreeViewItemViewModel
        where TContent : class
    {
        readonly ObservableCollection<IDebugTreeViewItemViewModel> _children;

        bool? _hasError = false;
        bool _isExpanded;
        bool _isSelected;
        TContent _content;
        IDebugTreeViewItemViewModel _parent;

        protected DebugTreeViewItemViewModel()
        {
            _children = new ObservableCollection<IDebugTreeViewItemViewModel>();
        }

        public TContent Content
        {
            get { return _content; }
            set
            {
                if (value != _content)
                {
                    _content = value;
                    Initialize(value);
                    OnPropertyChanged("Content");
                }
            }
        }

        abstract protected void Initialize(TContent value);

        public int Depth
        {
            get
            {
                return _parent == null ? 0 : _parent.Depth + 1;
            }
        }

        public ObservableCollection<IDebugTreeViewItemViewModel> Children
        {
            get
            {
                return _children;
            }
        }

        public IDebugTreeViewItemViewModel Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                if (value != _parent)
                {
                    _parent = value;
                    if (_parent != null)
                    {
                        _parent.VerifyErrorState();
                    }
                    OnPropertyChanged("Parent");
                    OnPropertyChanged("Depth");
                }
            }
        }

        public bool? HasError
        {
            get
            {
                return _hasError;
            }
            set
            {
                _hasError = value;
                if (_parent != null)
                {
                    _parent.VerifyErrorState();
                }
                OnPropertyChanged("HasError");
            }
        }

        public void VerifyErrorState()
        {
            if (HasError == true)
            {
                return;
            }

            if (_children.Any(c => c.HasError == true || c.HasError == null))
            {
                HasError = null;
            }
            else
            {
                HasError = false;
            }
            OnPropertyChanged("HasError");
        }

        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    OnPropertyChanged("IsExpanded");
                }

                // Expand all the way up to the root.
                if (_isExpanded && _parent != null)
                {
                    _parent.IsExpanded = true;
                }
            }
        }

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}