﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Dev2.Diagnostics;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Diagnostics;

namespace Dev2.Studio.ViewModels.Diagnostics
{
    public class DebugStringTreeViewItemViewModel : DebugTreeViewItemViewModel
    {
        #region Class Members

        readonly string _content;

        #endregion Class Members

        #region Constructor

        public DebugStringTreeViewItemViewModel(string content, DebugTreeViewItemViewModel parent = null, bool isExpanded = true, bool isSelected = false, bool addedAsParent = false)
            : base(parent)
        {
            _content = content;
            IsExpanded = isExpanded;
            IsSelected = isSelected;
            AddedAsParent = addedAsParent;
        }

        #endregion Constructor

        #region Properties

        public string Content
        {
            get
            {
                return _content;
            }
        }

        #endregion Properties
    }

    public class DebugStateTreeViewItemViewModel : DebugTreeViewItemViewModel
    {
        #region Class Members

        readonly IDebugState _content;

        #endregion Class Members

        #region Constructor

        public DebugStateTreeViewItemViewModel(IEnvironmentRepository environmentRepository, IDebugState content, DebugTreeViewItemViewModel parent = null, bool isExpanded = true, bool isSelected = false, bool addedAsParent = false)
            : base(parent)
        {
            _content = content;
            IsExpanded = isExpanded;
            IsSelected = isSelected;
            AddedAsParent = addedAsParent;
            Inputs = new List<object>();
            Outputs = new List<object>();

            if(environmentRepository != null && content != null)
            {
                Guid serverID;
                // check for remote server ID ;)
                bool isRemote = Guid.TryParse(content.Server, out serverID);
                if (serverID == Guid.Empty)
                {
                    serverID = content.ServerID;
                }

                var env = environmentRepository.All().FirstOrDefault(e => e.ID == serverID) ?? EnvironmentRepository.Instance.Source;
                
                if (Equals(env, EnvironmentRepository.Instance.Source) && isRemote)
                {
                    // We have an unknown remote server ;)
                    content.Server = "Unknown Remote Server";
                }
                else
                {
                    content.Server = env.Name;    
                }
            }

            if(content != null)
            {
                BuildBindableListFromDebugItems(content.Inputs, Inputs);
                BuildBindableListFromDebugItems(content.Outputs, Outputs);
            }
        }

        #endregion Constructor

        #region Properties

        public IDebugState Content
        {
            get
            {
                return _content;
            }
        }

        public List<object> Inputs { get; set; }
        public List<object> Outputs { get; set; }

        #endregion Properties

        #region Public Methods

        public void AppendError(string errorMessage)
        {
            if(Content.HasError)
            {
                var currentError = new StringBuilder(Content.ErrorMessage);
                currentError.Append(errorMessage);
                Content.ErrorMessage = currentError.ToString();
            }
            else Content.ErrorMessage = errorMessage;
            Content.HasError = true;
            OnPropertyChanged("Content.ErrorMessage");
            OnPropertyChanged("Content.HasError");
            OnPropertyChanged("Content");
            HasError = true;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        ///     Builds the bindable list from debug items.
        /// </summary>
        /// <param name="debugItems">The debug items.</param>
        /// <param name="destinationList">The destination list.</param>
        void BuildBindableListFromDebugItems(IEnumerable<IDebugItem> debugItems, ICollection<object> destinationList)
        {
            //
            // Build destinationList
            //
            destinationList.Clear();
            if(debugItems == null)
            {
                return;
            }

            foreach(var item in debugItems)
            {
                var list = new DebugLine();
                var groups = new Dictionary<string, DebugLineGroup>();
                foreach(var result in item.FetchResultsList())
                {
                    if(string.IsNullOrEmpty(result.GroupName))
                    {
                        list.LineItems.Add(new DebugLineItem(result));
                    }
                    else
                    {
                        DebugLineGroup group;
                        if(!groups.TryGetValue(result.GroupName, out group))
                        {
                            group = new DebugLineGroup(result.GroupName)
                            {
                                MoreLink = result.MoreLink
                            };

                            groups.Add(group.GroupName, group);
                            list.LineItems.Add(group);
                        }

                        DebugLineGroupRow row;
                        if(!group.Rows.TryGetValue(result.GroupIndex, out row))
                        {
                            row = new DebugLineGroupRow();
                            group.Rows.Add(result.GroupIndex, row);
                        }
                        row.LineItems.Add(new DebugLineItem(result));
                    }
                }

                destinationList.Add(list);
            }
        }

        #endregion Private Methods
    }

    public abstract class DebugTreeViewItemViewModel : INotifyPropertyChanged
    {
        #region Fields

        readonly ObservableCollection<DebugTreeViewItemViewModel> _children;
        readonly DebugTreeViewItemViewModel _parent;

        bool _addedAsParent;
        bool? _hasError = false;
        bool _isExpanded;
        bool _isSelected;

        #endregion

        #region Ctor

        protected DebugTreeViewItemViewModel(DebugTreeViewItemViewModel parent = null)
        {
            _parent = parent;
            _children = new ObservableCollection<DebugTreeViewItemViewModel>();
        }

        #endregion

        #region Content Properties

        public ObservableCollection<DebugTreeViewItemViewModel> Children
        {
            get
            {
                return _children;
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
                if(Parent != null)
                {
                    Parent.VerifyErrorState();
                }
                OnPropertyChanged("HasError");
            }
        }

        public void VerifyErrorState()
        {
            if(HasError == true)
            {
                return;
            }
            if(Children.Any(c => c.HasError == true || c.HasError == null))
            {
                HasError = null;
            }
            else
            {
                HasError = false;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets/sets whether the TreeViewItem
        ///     associated with this object is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                if(value != _isExpanded)
                {
                    _isExpanded = value;
                    OnPropertyChanged("IsExpanded");
                }

                // Expand all the way up to the root.
                if(_isExpanded && _parent != null)
                {
                    _parent.IsExpanded = true;
                }
            }
        }

        /// <summary>
        ///     Gets/sets whether the TreeViewItem
        ///     associated with this object is selected.
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if(value != _isSelected)
                {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the item was added as a parent.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the item was added as a parent; otherwise, <c>false</c>.
        /// </value>
        public bool AddedAsParent
        {
            get
            {
                return _addedAsParent;
            }
            set
            {
                _addedAsParent = value;
                OnPropertyChanged("AddedAsParent");
            }
        }

        public DebugTreeViewItemViewModel Parent
        {
            get
            {
                return _parent;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        ///     Returns the first item which matches a predicate form the node it's self and all it's children recursively.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        public DebugTreeViewItemViewModel FindSelfOrChild(Func<DebugTreeViewItemViewModel, bool> predicate)
        {
            if(predicate == null)
            {
                return null;
            }

            //
            // Check the current node agains the predicate
            //
            if(predicate(this))
            {
                return this;
            }

            //
            // Recursively check all children against the predicate
            //
            foreach(DebugTreeViewItemViewModel child in Children)
            {
                DebugTreeViewItemViewModel recursiveMatch = child.FindSelfOrChild(predicate);
                if(recursiveMatch != null)
                {
                    return recursiveMatch;
                }
            }

            return null;
        }

        /// <summary>
        ///     Gets the depth.
        /// </summary>
        /// <returns></returns>
        public int GetDepth()
        {
            DebugTreeViewItemViewModel currentViewModel = this;

            int depth = 0;
            while(currentViewModel.Parent != null)
            {
                currentViewModel = currentViewModel.Parent;
                depth++;
            }

            return depth;
        }

        #endregion Methods

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}