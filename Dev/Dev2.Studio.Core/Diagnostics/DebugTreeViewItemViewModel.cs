/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Dev2.Studio.Interfaces;

namespace Dev2.Studio.Core
{
    public abstract class DebugTreeViewItemViewModel<TContent> : IDebugTreeViewItemViewModel
        where TContent : class
    {
        readonly ObservableCollection<IDebugTreeViewItemViewModel> _children;

        bool? _hasError = false;
        bool? _hasNoError = false;
        bool _isExpanded;
        bool _isSelected;
        TContent _content;
        IDebugTreeViewItemViewModel _parent;
        bool? _mockSelected;
        string _testDescription;

        protected DebugTreeViewItemViewModel()
        {
            _children = new ObservableCollection<IDebugTreeViewItemViewModel>();
            IsTestView = false;
        }

        public TContent Content
        {
            get => _content;
            set
            {
                if(value != _content)
                {
                    _content = value;
                    Initialize(value);
                    OnPropertyChanged(nameof(Content));
                }
            }
        }

        protected abstract void Initialize(TContent value);

        public int Depth => _parent?.Depth + 1 ?? 0;

        public ObservableCollection<IDebugTreeViewItemViewModel> Children => _children;

        public IDebugTreeViewItemViewModel Parent
        {
            get => _parent;
            set
            {
                if(value != _parent)
                {
                    _parent = value;
                    _parent?.VerifyErrorState();
                    OnPropertyChanged(nameof(Parent));
                    OnPropertyChanged(nameof(Depth));
                }
            }
        }

        public bool? HasError
        {
            get => _hasError;
            set
            {
                _hasError = value;
                _parent?.VerifyErrorState();
                OnPropertyChanged(nameof(HasError));
            }
        }
        public bool? HasNoError
        {
            get => _hasNoError;
            set
            {
                _hasNoError = value;
                OnPropertyChanged(nameof(HasNoError));
            }
        }
        public bool? MockSelected
        {
            get => _mockSelected;
            set
            {
                _mockSelected = value;
                OnPropertyChanged(nameof(MockSelected));
            }
        }
        public string ActivityTypeName { get; set; }
        public bool IsTestView { get; set; }

        public string TestDescription
        {
            get => _testDescription;
            set
            {
                _testDescription = value;
                OnPropertyChanged(nameof(TestDescription));
            }
        }

        public void VerifyErrorState()
        {
            if(HasError == true)
            {
                return;
            }

            if(_children.Any(c => c.HasError == true || c.HasError == null))
            {
                HasError = null;
            }
            else
            {
                HasError = false;
            }
            OnPropertyChanged(nameof(HasError));
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if(value != _isExpanded)
                {
                    _isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }

                // Expand all the way up to the root.
                if(_isExpanded && _parent != null)
                {
                    _parent.IsExpanded = true;
                }
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if(value != _isSelected)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        T IDebugTreeViewItemViewModel.As<T>()
        {
            if (this is T debugTreeView)
            {
                return debugTreeView;
            }
            return null;
        }
    }
}
