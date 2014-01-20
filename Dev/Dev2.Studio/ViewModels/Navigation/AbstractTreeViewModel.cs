using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Caliburn.Micro;
using Dev2.Providers.Logs;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.ViewModels.Navigation;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.ViewModels.Navigation
{
    /// <summary>
    ///     Abstract class representing all treeview nodes in Both The Explorer and Deploy Tabs/Views
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/01/23</date>
    public abstract class AbstractTreeViewModel : SimpleBaseViewModel, ITreeNode, IContextCommands
    {
        #region private fields

        // ReSharper disable InconsistentNaming
        protected ObservableCollection<ITreeNode> _children;
        // ReSharper restore InconsistentNaming
        private RelayCommand _deployCommand;
        private string _filterText;
        private ICollectionView _filteredChildren;
        private bool _hasUnfilteredExpandStateBeenSet;
        private string _iconPath;
        private bool? _isChecked = false;
        private bool _isExpanded;
        private bool _isFiltered;
        private bool _isSelected;
        private ITreeNode _treeParent;
        private bool? _unfilteredExpandState;
        bool _isRefreshing;
        int _serverRenameProgress;
        bool _isNew;
        protected readonly IEventAggregator EventPublisher;
        bool _isOverwrite;

        #endregion

        #region ctor + init
        //, IWizardEngine wizardEngine
        protected AbstractTreeViewModel(IEventAggregator eventPublisher, ITreeNode parent)
        {
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            EventPublisher = eventPublisher;

            if(parent != null)
            {
                parent.Add(this);
            }
        }

        #endregion ctor + init

        #region properties

        #region public

        public bool IsOverwrite
        {
            get
            {
                return _isOverwrite;
            }
            set
            {
                _isOverwrite = value;
                NotifyOfPropertyChange(() => IsOverwrite);
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is refreshing.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is refreshing; otherwise, <c>false</c>.
        /// </value>
        /// <author>Massimo.Guerrera</author>
        /// <date>2013/06/20</date>
        public bool IsRefreshing
        {
            get
            {
                return _isRefreshing;
            }
            set
            {
                _isRefreshing = value;
                NotifyOfPropertyChange(() => IsRefreshing);
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is new - controls appropraite animations
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is refreshing; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.Smit</author>
        /// <date>2013/07/08</date>
        public bool IsNew
        {
            get
            {
                return _isNew;
            }
            set
            {
                _isNew = value;
                NotifyOfPropertyChange(() => IsNew);
            }
        }

        /// <summary>
        ///     Gets or sets the wizard engine to use.
        /// </summary>
        /// <value>
        ///     The wizard engine.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        // public IWizardEngine WizardEngine { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether this instance is connected, by walking the tree to the environment node.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.Smit</author>
        /// <date>2013/01/28</date>
        public virtual bool IsConnected
        {
            get
            {
                if(TreeParent != null)
                {
                    return TreeParent.IsConnected;
                }
                return false;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is filtered from the tree.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is filtered; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public virtual bool IsFiltered
        {
            get { return _isFiltered; }
            set
            {
                if(_isFiltered == value)
                {
                    return;
                }

                _isFiltered = value;
                NotifyOfPropertyChange(() => IsFiltered);
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is selected in the tree.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is selected; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if(_isSelected == value)
                {
                    return;
                }

                _isSelected = value;
                NotifyOfPropertyChange(() => IsSelected);
            }
        }

        /// <summary>
        ///     Gets the tree parent for this node.
        /// </summary>
        /// <value>
        ///     The tree parent.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public ITreeNode TreeParent
        {
            get { return _treeParent; }
            set
            {
                if(_treeParent == value)
                {
                    return;
                }

                _treeParent = value;
                NotifyOfPropertyChange(() => TreeParent);
                NotifyOfPropertyChange(() => ChildrenCount);
            }
        }

        public string FilterText
        {
            get { return _filterText; }
            set
            {
                _filterText = value;
                SetFilter(_filterText, true);
            }
        }

        /// <summary>
        ///     Gets the children for this node.
        /// </summary>
        /// <value>
        ///     The children.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public virtual ObservableCollection<ITreeNode> Children
        {
            get
            {
                if(_children == null)
                {
                    _children = new ObservableCollection<ITreeNode>();
                    _children.CollectionChanged += ChildrenOnCollectionChanged;
                }
                return _children;
            }
            set
            {
                if(_children == value) return;

                _children = value;
                _children.CollectionChanged -= ChildrenOnCollectionChanged;
                _children.CollectionChanged += ChildrenOnCollectionChanged;
            }
        }

        /// <summary>
        ///     Gets the recursive count of the children. (ie, inlcuding childrens' childrencount)
        /// </summary>
        /// <value>
        ///     The children count.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public virtual int ChildrenCount
        {
            get { return Children.Where(c => !c.IsFiltered).Sum(c => c.ChildrenCount); }
        }

        /// <summary>
        ///     Gets/sets the state of the associated UI toggle (ex. CheckBox).
        ///     The return value is calculated based on the check state of all
        ///     child ITreeNodes.  Setting this property to true or false
        ///     will set all children to the same check state, and setting it
        ///     to any value will cause the parent to verify its check state.
        /// </summary>
        /// <value>
        ///     The IsChecked value - null indicates that some children, not all, has been Checked
        /// </value>
        public bool? IsChecked
        {
            get { return _isChecked; }
            set { SetIsChecked(value, true, true); }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is expanded in the tree.
        ///     Also expands parent items so that it is visible
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is expanded; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public virtual bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if(_isExpanded != value)
                {
                    _isExpanded = value;
                    NotifyOfPropertyChange(() => IsExpanded);
                }

                // Expand all the way up to the root.
                if(value && TreeParent != null)
                {
                    TreeParent.IsExpanded = true;
                }
            }
        }

        public int ServerRenameProgress
        {
            get
            {
                return _serverRenameProgress;
            }
            set
            {
                if(_serverRenameProgress == value)
                {
                    return;
                }
                _serverRenameProgress = value;

                NotifyOfPropertyChange(() => ServerRenameProgress);
            }
        }

        #endregion public

        #region virtual

        public virtual bool HasExecutableCommands
        {
            get
            {
                return true;
            }
        }

        public virtual bool CanSelectDependencies
        {
            get { return false; }
        }

        public virtual bool CanBuild
        {
            get { return false; }
        }

        public virtual bool CanDebug
        {
            get { return false; }
        }

        public virtual bool CanEdit
        {
            get { return false; }
        }

        public virtual bool IsRenaming
        {
            get
            {
                return false;
            }
            // ReSharper disable once ValueParameterNotUsed
            set
            {
            }
        }

        public virtual bool CanManualEdit
        {
            get { return false; }
        }

        public virtual bool CanRun
        {
            get { return false; }
        }

        public virtual bool CanDelete
        {
            get { return false; }
        }

        public virtual bool CanHelp
        {
            get { return false; }
        }

        public virtual bool CanShowDependencies
        {
            get { return false; }
        }

        public virtual bool CanShowProperties
        {
            get { return false; }
        }

        public virtual bool CanCreateWizard
        {
            get { return false; }
        }

        public virtual bool CanEditWizard
        {
            get { return false; }
        }

        public virtual bool CanDeploy
        {
            get { return EnvironmentModel != null && EnvironmentModel.IsConnected; }
        }

        public virtual bool CanRename
        {
            get { return false; }
        }

        public virtual string DeployTitle
        {
            get
            {
                return "Deploy";
            }
        }

        public virtual string NewWorkflowTitle
        {
            get
            {
                return "New Workflow   (Ctrl+W)";
            }
        }

        public virtual string NewServiceTitle
        {
            get
            {
                return "New Service";
            }
        }

        public virtual string AddSourceTitle
        {
            get
            {
                return "Add Source";
            }
        }

        public virtual bool CanRemove
        {
            get { return false; }
        }

        public virtual bool CanDisconnect
        {
            get { return false; }
        }

        public virtual bool CanConnect
        {
            get { return false; }
        }

        public virtual bool HasFileMenu
        {
            get { return false; }
        }

        public virtual bool HasNewWorkflowMenu
        {
            get { return false; }
        }

        public virtual bool HasNewServiceMenu
        {
            get { return false; }
        }

        public virtual bool HasNewSourceMenu
        {
            get { return false; }
        }

        public virtual string IconPath
        {
            get { return _iconPath; }
            set
            {
                if(_iconPath == value)
                {
                    return;
                }

                _iconPath = value;
                NotifyOfPropertyChange(() => IconPath);
            }
        }

        public virtual bool CanDuplicate
        {
            get { return false; }
        }

        public virtual bool CanMoveRename
        {
            get { return false; }
        }

        public virtual bool CanRefresh
        {
            get { return false; }
        }

        #endregion virtual

        #region abstract

        public abstract IEnvironmentModel EnvironmentModel { get; protected set; }

        #endregion abstract

        #endregion properties

        #region Commands

        public virtual ICommand BuildCommand
        {
            get { return null; }
        }

        public virtual ICommand DebugCommand
        {
            get { return null; }
        }

        public virtual ICommand EditCommand
        {
            get { return null; }
        }

        public virtual ICommand ManualEditCommand
        {
            get { return null; }
        }

        public virtual ICommand RunCommand
        {
            get { return null; }
        }

        public virtual ICommand DeleteCommand
        {
            get { return null; }
        }

        public virtual ICommand HelpCommand
        {
            get { return null; }
        }

        public virtual ICommand ShowDependenciesCommand
        {
            get { return null; }
        }

        public virtual ICommand ShowPropertiesCommand
        {
            get { return null; }
        }

        public virtual ICommand CreateWizardCommand
        {
            get { return null; }
        }

        public virtual ICommand EditWizardCommand
        {
            get { return null; }
        }

        public virtual ICommand DeployCommand
        {
            get
            {
                return _deployCommand ??
                       (_deployCommand =
                        new RelayCommand(param =>
                        {
                            this.TraceInfo("Publish message of type - " + typeof(DeployResourcesMessage));
                            EventPublisher.Publish(new DeployResourcesMessage(this));
                        },
                            o => CanDeploy));
            }
        }

        public virtual ICommand RenameCommand
        {
            get { return null; }
        }

        public virtual ICommand PreviewKeyUpCommand
        {
            get { return null; }
        }

        public virtual ICommand NewResourceCommand
        {
            get { return null; }
        }

        public virtual ICommand RemoveCommand
        {
            get { return null; }
        }

        public virtual ICommand DisconnectCommand
        {
            get { return null; }
        }

        public virtual ICommand ConnectCommand
        {
            get { return null; }
        }

        public virtual ICommand DuplicateCommand
        {
            get { return null; }
        }

        public virtual ICommand MoveRenameCommand
        {
            get { return null; }
        }

        public virtual ICommand RefreshCommand
        {
            get { return null; }
        }

        #endregion Commands

        #region public methods

        public INavigationContext FindRootNavigationViewModel()
        {
            var root = this as RootTreeViewModel;
            if(root == null)
            {
                return TreeParent.FindRootNavigationViewModel();
            }

            var parent = root.Parent as INavigationContext;
            return parent;
        }

        /// <summary>
        /// Sets the filter text used to set The IsFilteredProperty.
        /// </summary>
        /// <param name="filterText">The filter text.</param>
        /// <param name="updateChildren">if set to <c>true</c> [update children].</param>
        /// <date>2013/01/23</date>
        /// <author>
        /// Jurie.smit
        /// </author>
        public virtual void SetFilter(string filterText, bool updateChildren)
        {
            bool originalFilter = IsFiltered;

            Children.ToList().ForEach(c => c.SetFilter(filterText, true));

            IsFiltered = Children.All(c => c.IsFiltered);

            //Notify parent to verify filterstate
            if(TreeParent != null && originalFilter != IsFiltered)
            {
                TreeParent.SetFilter(filterText, false);
            }

            //Notify parent to update check status
            if(TreeParent != null)
            {
                TreeParent.VerifyCheckState();
            }
        }

        public virtual void NotifyOfFilterPropertyChanged(bool updateParent = false)
        {
            Children.ToList().ForEach(c => c.NotifyOfFilterPropertyChanged(false));

            Dispatcher.CurrentDispatcher.Invoke(
                () => FilteredChildren.Refresh());
            NotifyOfPropertyChange("ChildrenCount");
            VerifyCheckState();

            if(updateParent && TreeParent != null)
            {
                TreeParent.NotifyOfFilterPropertyChanged(true);
            }
        }

        /// <summary>
        ///     Sets the IsChecked Property and updates children and parent
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="updateChildren">
        ///     if set to <c>true</c> [update children].
        /// </param>
        /// <param name="updateParent">
        ///     if set to <c>true</c> [update parent].
        /// </param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            bool? preState = _isChecked; ;
            if(value == _isChecked)
            {
                return;
            }

            if(_isChecked == true && value == false)
            {
                TreeParent.IsOverwrite = false;
                IsOverwrite = false;
            }

            _isChecked = value;

            if(updateChildren && _isChecked.HasValue)
            {
                //Do not check filtered children
                foreach(var c in Children)
                {
                    if(!c.IsFiltered) c.SetIsChecked(_isChecked, true, false);
                }
            }

            if(updateParent && _treeParent != null)
            {
                TreeParent.VerifyCheckState();
            }

            NotifyOfPropertyChange(() => IsChecked);
            Logger.TraceInfo("Publish message of type - " + typeof(ResourceCheckedMessage));
            ResourceTreeViewModel rstvm = this as ResourceTreeViewModel;
            if(rstvm != null)
            {
                EventPublisher.Publish(new ResourceCheckedMessage { PreCheckedState = preState, PostCheckedState = value, ResourceModel = rstvm.DataContext });
            }
            EventPublisher.Publish(new ResourceCheckedMessage { PreCheckedState = preState, PostCheckedState = value });
        }

        /// <summary>
        ///     Verifies the state of the IsChecked property by taking the childrens IsChecked State into account
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public virtual void VerifyCheckState()
        {
            if(!FilteredChildren.OfType<ITreeNode>().Any())
            {
                SetIsChecked(false, false, true);
                return;
            }

            bool? state = null;
            var count = FilteredChildren.OfType<ITreeNode>().Count();
            for(int i = 0; i < count; ++i)
            {
                bool? current = FilteredChildren.OfType<ITreeNode>().ToArray()[i].IsChecked;
                if(i == 0)
                {
                    state = current;
                }
                else if(state != current)
                {
                    state = null;
                    break;
                }
            }
            SetIsChecked(state, false, true);
        }

        /// <summary>
        ///     Gets all children recursively that match the predicate
        /// </summary>
        public IEnumerable<ITreeNode> GetChildren(Func<ITreeNode, bool> predicate)
        {
            if(predicate == null)
            {
                predicate = n => true;
            }

            var children = new List<ITreeNode>(Children.Where(predicate));

            foreach(ITreeNode child in Children)
            {
                children.AddRange(child.GetChildren(predicate));
            }

            return children;
        }

        /// <summary>
        ///     Updates the node's expansion states according to the current filter.
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2/25/2013</date>
        public void UpdateFilteredNodeExpansionStates(string filterText)
        {
            //If no filter set unfiltered state
            if(string.IsNullOrWhiteSpace(filterText))
            {
                if(_unfilteredExpandState != null)
                    IsExpanded = _unfilteredExpandState.Value;
                _hasUnfilteredExpandStateBeenSet = false;
                foreach(ITreeNode treeNode in Children)
                {
                    treeNode.UpdateFilteredNodeExpansionStates(filterText);
                }

                VerifyCheckState();
                return;
            }

            //toggle boolean indicating wheter unfiltered state has been set
            if(!_hasUnfilteredExpandStateBeenSet)
            {
                _unfilteredExpandState = IsExpanded;
                _hasUnfilteredExpandStateBeenSet = true;
            }

            //set expanded state according to current filter
            if(!IsFiltered || Children.Any(c => !c.IsFiltered))
            {
                IsExpanded = true;
            }
            foreach(ITreeNode treeNode in Children)
            {
                treeNode.UpdateFilteredNodeExpansionStates(filterText);
            }

            VerifyCheckState();
        }

        /// <summary>
        ///     Finds the child containing a specific resource (or environmentmodel).
        ///     This looks recursively in all the children
        ///     Implemented by inheriting nodes when they have a specific resouretype to return
        /// </summary>
        /// <typeparam name="T">Type of the resource to find</typeparam>
        /// <param name="resourceToFind">The resource to find.</param>
        /// <returns></returns>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public virtual ITreeNode FindChild<T>(T resourceToFind)
        {
            return Children.Select(treeNode => treeNode.FindChild(resourceToFind))
                           .FirstOrDefault(toReturn => toReturn != null);
        }

        /// <summary>
        ///     Tries to find a sepcific node among the children
        /// </summary>
        /// <param name="childToFind">The child to find.</param>
        /// <returns></returns>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public ITreeNode FindChild(ITreeNode childToFind)
        {
            ITreeNode toFind = Children.FirstOrDefault(c => ReferenceEquals(childToFind, c));
            if(toFind != null)
            {
                return toFind;
            }
            foreach(ITreeNode child in Children)
            {
                toFind = child.FindChild(childToFind);
                if(toFind == null)
                {
                    continue;
                }
                return toFind;
            }
            return null;
        }

        /// <summary>
        ///     Adds the specified child to children.
        /// </summary>
        /// <param name="child">The child.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public virtual void Add(ITreeNode child)
        {
            child.TreeParent = this;
            Children.Add(child);
        }

        /// <summary>
        ///     Removes the specified child from the children.
        /// </summary>
        /// <param name="child">The child.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public void Remove(ITreeNode child)
        {
            if(child != null && child.TreeParent != null && child.TreeParent.TreeParent != null)
            {
                child.TreeParent.Children.Remove(child);

                if(child.TreeParent.Children.Count == 0 && child.TreeParent.TreeParent.TreeParent != null)
                {
                    Remove(child.TreeParent);
                }

                child.TreeParent = null;
            }
        }

        public bool GetIsFiltered(string filterText)
        {
            return !(DisplayName.ToUpper().Contains(filterText.ToUpper()) ||
                     IsChecked.HasValue &&
                     IsChecked.Value);
        }

        /// <summary>
        ///     Raises the property changed for the commands.
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public void RaisePropertyChangedForCommands()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion

        #region private methods

        /// <summary>
        ///     Handler for the CollectionChanged event of the Children OBservable Collection.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">
        ///     The <see cref="NotifyCollectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        protected void ChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            NotifyOfPropertyChange(() => ChildrenCount);
            if(args.NewItems != null && args.NewItems.Count > 0)
            {
                args.NewItems.Cast<ITreeNode>()
                    .ToList()
                    .ForEach(c => c.PropertyChanged += ChildPropertyChanged);
            }
            if(args.OldItems != null && args.OldItems.Count > 0)
            {
                args.OldItems.Cast<ITreeNode>()
                    .ToList()
                    .ForEach(c => c.PropertyChanged -= ChildPropertyChanged);
            }
        }

        /// <summary>
        ///     Escalates the property changed notification up the tree for the ChildrenCount
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        ///     The <see cref="PropertyChangedEventArgs" /> instance containing the event data.
        /// </param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        private void ChildPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //Specifically used switch for extensibility - propertyName is a magic string
            switch(e.PropertyName)
            {
                case "ChildrenCount":
                    NotifyOfPropertyChange("ChildrenCount");
                    break;
            }
        }

        #endregion

        #region Implementation of IComparable

        /// <summary>
        ///     Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <returns>
        ///     A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes
        ///     <paramref
        ///         name="obj" />
        ///     in the sort order. Zero This instance occurs in the same position in the sort order as
        ///     <paramref
        ///         name="obj" />
        ///     . Greater than zero This instance follows <paramref name="obj" /> in the sort order.
        /// </returns>
        /// <param name="obj">An object to compare with this instance. </param>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="obj" /> is not the same type as this instance.
        /// </exception>
        public virtual int CompareTo(object obj)
        {
            return 0;
        }

        #endregion

        public ICollectionView FilteredChildren
        {
            get
            {
                if(_filteredChildren == null)
                {
                    _filteredChildren = CollectionViewSource.GetDefaultView(Children);
                    _filteredChildren.Filter += Filter;
                }
                return _filteredChildren;
            }
        }

        private bool Filter(object o)
        {
            var vm = o as ITreeNode;
            if(vm == null) return false;
            if(!vm.IsFiltered) return true;
            return false;
        }


        protected virtual void Reparent(string parentName)
        {
            if(string.IsNullOrEmpty(parentName))
            {
                return;
            }

            var newParent = TreeParent.Children.FirstOrDefault(c => parentName.Equals(c.DisplayName, StringComparison.InvariantCultureIgnoreCase));
            // ReSharper disable ConvertIfStatementToNullCoalescingExpression
            if(newParent == null)
            // ReSharper restore ConvertIfStatementToNullCoalescingExpression
            {
                newParent = CreateParent(parentName);
            }

            ServerRenameProgress = 0;
            foreach(var child in Children)
            {
                newParent.Add(child);//reparent child resource
                ServerRenameProgress += 100 / Children.Count;
            }

            TreeParent.Remove(this);//remove old parent
        }

        protected abstract ITreeNode CreateParent(string displayName);
    }


    public abstract class AbstractTreeViewModel<T> : AbstractTreeViewModel, ITreeNode<T>
    {
        private T _dataContext;

        protected AbstractTreeViewModel(IEventAggregator eventPublisher, ITreeNode parent)
            : base(eventPublisher, parent)
        {
        }

        public virtual T DataContext
        {
            get { return _dataContext; }
            set
            {
                if(value.Equals(_dataContext))
                    return;

                _dataContext = value;
                NotifyOfPropertyChange(() => DataContext);
            }
        }
    }
}