
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Studio.Core.ViewModels.Base;

namespace Dev2.Studio.Core.ViewModels.Navigation
{
    public abstract class NavigationItemViewModelBase : BaseViewModel
    {
        #region Class Members

        private readonly Func<NavigationItemViewModelBase, bool> _childCountPredicate;
        private readonly ObservableCollection<NavigationItemViewModelBase> _children;
        private readonly object _dataContext;
        private string _iconPath;
        private bool _isChecked;
        private bool _isExpanded;
        private bool _isSelected;
        private string _name;
        private NavigationItemViewModelBase _parent;

        #endregion Class Members

        #region Properties
        public event ObjectSelectedEventHandler ItemSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;

                if (_isSelected)
                {
                    if (ItemSelected != null)
                    {
                        ItemSelected(DataContext);
                    }
                }

                NotifyOfPropertyChange(() => IsSelected);
            }
        }

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    NotifyOfPropertyChange(() => IsExpanded);
                }

                // Expand all the way up to the root.
                if (_isExpanded && _parent != null)
                {
                    _parent.IsExpanded = true;
                }
            }
        }

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked == value) return;

                _isChecked = value;
                NotifyOfPropertyChange(() => IsChecked);
                OnChecked();
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        public string IconPath
        {
            get { return _iconPath; }
            set
            {
                _iconPath = value;
                NotifyOfPropertyChange(() => IconPath);
                NotifyOfPropertyChange(() => IconPathObject);
            }
        }

        public NavigationItemViewModelBase IconPathObject
        {
            get { return this; }
        }

        public NavigationItemViewModelBase Parent
        {
            get { return _parent; }
            set
            {
                _parent = value;
                NotifyOfPropertyChange(() => Parent);
            }
        }

        public object DataContext
        {
            get { return _dataContext; }
        }

        public ObservableCollection<NavigationItemViewModelBase> Children
        {
            get { return _children; }
        }

        public long ChildCount
        {
            get { return GetChildCount(_childCountPredicate); }
        }

        #endregion Properties

        #region Events

        #region Checked

        public event EventHandler Checked;

        protected void OnChecked()
        {
            if (Checked != null)
            {
                Checked(this, new EventArgs());
            }
        }

        #endregion Checked

        #endregion Events

        #region Constructors

        protected NavigationItemViewModelBase(string name,
                                              string iconPath,
                                              NavigationItemViewModelBase parent,
                                              object dataContext,
                                              bool isExpanded = false,
                                              bool isSelected = false,
                                              Func<NavigationItemViewModelBase, bool> childCountPredicate = null,
                                              bool isChecked = false)
        {
            _childCountPredicate = childCountPredicate;
            _children = new ObservableCollection<NavigationItemViewModelBase>();
            _dataContext = dataContext;

            Parent = parent;
            Name = name;
            IconPath = iconPath;
            IsSelected = isSelected;
            IsExpanded = isExpanded;
            IsChecked = isChecked;
        }

        #endregion Constructors


        #region Methods

        public void SilentlyCheck(bool isChecked)
        {
            _isChecked = isChecked;
            NotifyOfPropertyChange(() => IsChecked);
        }

        /// <summary>
        ///     Gets all children recursively that match the predicate
        /// </summary>
        public List<NavigationItemViewModelBase> GetChildren(Func<NavigationItemViewModelBase, bool> predicate)
        {
            if (predicate == null)
            {
                predicate = n => true;
            }

            var children = new List<NavigationItemViewModelBase>(Children.Where(predicate));

            foreach (var child in Children)
            {
                children.AddRange(child.GetChildren(predicate));
            }

            return children;
        }

        /// <summary>
        ///     Tries to find a child recursively based on the provided predicate
        /// </summary>
        public NavigationItemViewModelBase GetChild(Func<NavigationItemViewModelBase, bool> predicate)
        {
            var item = (predicate != null) ? Children.FirstOrDefault(predicate) : null;

            if (item == null)
            {
                var count = 0;
                while (count < Children.Count && item == null)
                {
                    item = Children[count].GetChild(predicate);
                    count++;
                }
            }

            return item;
        }

        /// <summary>
        ///     Gets the number of children recursively based on the predicate
        /// </summary>
        public long GetChildCount(Func<NavigationItemViewModelBase, bool> predicate)
        {
            return ((predicate != null) ? Children.Where(predicate).Count() : 0)
                   + Children.Sum(child => child.GetChildCount(predicate));
        }

        /// <summary>
        ///     Signals the the child count properties should be manually updated
        /// </summary>
        public void UpdateChildCount()
        {
            NotifyOfPropertyChange(() => ChildCount);
        }

        #endregion Methods
    }
}
