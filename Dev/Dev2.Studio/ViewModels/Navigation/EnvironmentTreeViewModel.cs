using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Caliburn.Micro;
using Dev2.Network;
using Dev2.Providers.Logs;
using Dev2.Services.Security;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Threading;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.Navigation
{
    /// <summary>
    /// The viewmodel for a treenode representing an environment
    /// </summary>
    /// <date>2013/01/23</date>
    /// <author>
    /// Jurie.smit
    /// </author>
    public sealed class EnvironmentTreeViewModel : AbstractTreeViewModel
    //,IHandle<UpdateActiveEnvironmentMessage>
    {
        #region private fields

        private RelayCommand _connectCommand;
        private RelayCommand _disconnectCommand;
        private IEnvironmentModel _environmentModel;
        private RelayCommand _removeCommand;
        private RelayCommand<string> _newResourceCommand;
        private RelayCommand _refreshCommand;
        private readonly IAsyncWorker _asyncWorker;
        bool _isAuthorized;
        bool _isAuthorizeDeployFrom;
        bool _isAuthorizeDeployTo;

        #endregion

        #region ctor + init
        public EnvironmentTreeViewModel(IEventAggregator eventPublisher, ITreeNode parent, IEnvironmentModel environmentModel, IAsyncWorker asyncWorker)
            : base(eventPublisher, parent)
        {
            _asyncWorker = asyncWorker;
            EnvironmentModel = environmentModel;
            IsExpanded = true;
            _isAuthorized = IsAuthorized;
            _isAuthorizeDeployFrom = IsAuthorizedDeployFrom;
            _isAuthorizeDeployTo = IsAuthorizedDeployTo;

        }

        #endregion

        #region public properties

        public override bool CanRefresh
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the icon path.
        /// </summary>
        /// <value>
        /// The icon path.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override string IconPath
        {
            get { return StringResources.Navigation_Environment_Icon_Pack_Uri; }
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override string DisplayName
        {
            get
            {
                return EnvironmentModel == null
                           ? String.Empty
                           : string.Format("{0} ({1})", EnvironmentModel.Name,
                                           (EnvironmentModel.Connection.AppServerUri == null)
                                               ? String.Empty
                                               : EnvironmentModel.Connection.AppServerUri.AbsoluteUri);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is connected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override bool IsConnected
        {
            get { return (EnvironmentModel != null) && EnvironmentModel.IsConnected; }
        }

        public override bool IsAuthorized
        {
            get
            {
                return (EnvironmentModel != null) && EnvironmentModel.IsAuthorized;
            }
        }
        public override bool IsAuthorizedDeployFrom
        {
            get
            {
                return EnvironmentModel != null
                    && (EnvironmentModel.AuthorizationService != null
                    && EnvironmentModel.AuthorizationService.IsAuthorized(AuthorizationContext.DeployFrom, null));
            }
        }
        public override bool IsAuthorizedDeployTo
        {
            get
            {
                return EnvironmentModel != null
                    && (EnvironmentModel.AuthorizationService != null
                    && EnvironmentModel.AuthorizationService.IsAuthorized(AuthorizationContext.DeployTo, null));
            }
        }

        /// <summary>
        /// Gets or sets the environment model for this instance.
        /// </summary>
        /// <value>
        /// The environment model.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override IEnvironmentModel EnvironmentModel
        {
            get { return _environmentModel; }
            protected set
            {
                if(_environmentModel != null)
                {
                    // BUG 9940 - 2013.07.29 - TWR - added
                    _environmentModel.IsConnectedChanged -= OnEnvironmentModelIsConnectedChanged;
                    _environmentModel.Connection.PermissionsChanged -= OnEnvironmentModelPermissionsChanged;
                }
                _environmentModel = value;
                if(_environmentModel != null)
                {
                    // BUG 9940 - 2013.07.29 - TWR - added
                    _environmentModel.IsConnectedChanged += OnEnvironmentModelIsConnectedChanged;
                    _environmentModel.Connection.PermissionsChanged += OnEnvironmentModelPermissionsChanged;
                }
                NotifyOfPropertyChange(() => EnvironmentModel);
                NotifyOfPropertyChange(() => IsConnected);
            }
        }

        void OnEnvironmentModelPermissionsChanged(object sender, EventArgs eventArgs)
        {
            if(_environmentModel != null && !ServerProxy.IsShuttingDown)
            {
                NotifyOfPropertyChange(() => IsAuthorized);
                NotifyOfPropertyChange(() => IsAuthorizedDeployFrom);
                NotifyOfPropertyChange(() => IsAuthorizedDeployTo);

                DoUpdateBasedOnPermissions();
            }
        }

        public void DoUpdateBasedOnPermissions()
        {
            var rootNavigationViewModel = FindRootNavigationViewModel() as NavigationViewModel;
            if(rootNavigationViewModel != null)
            {
                switch(rootNavigationViewModel.NavigationViewModelType)
                {
                    case NavigationViewModelType.Explorer:
                        _isAuthorized = UpdateBasedOnPermission(rootNavigationViewModel, _isAuthorized, IsAuthorized);
                        break;
                    case NavigationViewModelType.DeployFrom:
                        _isAuthorizeDeployFrom = UpdateBasedOnPermission(rootNavigationViewModel, _isAuthorizeDeployFrom, IsAuthorizedDeployFrom);
                        break;
                    case NavigationViewModelType.DeployTo:
                        _isAuthorizeDeployTo = UpdateBasedOnPermission(rootNavigationViewModel, _isAuthorizeDeployTo, IsAuthorizedDeployTo);
                        break;
                }
            }
        }

        bool UpdateBasedOnPermission(NavigationViewModel rootNavigationViewModel, bool previousAuthorizations, bool isAuthorized)
        {
            if(previousAuthorizations != isAuthorized)
            {
                UpdateCollection(rootNavigationViewModel, isAuthorized);
                return isAuthorized;
            }
            return previousAuthorizations;
        }

        void UpdateCollection(NavigationViewModel rootNavigationViewModel, bool isAuthorized)
        {
            if(!isAuthorized)
            {
                PerformActionOnCorrectDispatcher(() => Children.Clear());
            }
            else
            {
                PerformActionOnCorrectDispatcher(() => rootNavigationViewModel.LoadEnvironmentResources(EnvironmentModel));
            }
        }

        void PerformActionOnCorrectDispatcher(System.Action actionToPerform)
        {
            try
            {
                if(Application.Current != null && Application.Current.Dispatcher != null)
                {
                    Application.Current.Dispatcher.Invoke(actionToPerform, DispatcherPriority.Send);
                }
                else
                {
                    actionToPerform();
                }
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch(Exception)
            {
                //TO PREVENT TESTING ISSUES
            }
        }

        void OnEnvironmentModelIsConnectedChanged(object sender, ConnectedEventArgs args)
        {
            // BUG 9940 - 2013.07.29 - TWR - added
            NotifyOfPropertyChange(() => IsConnected);

            if(IsRefreshing && !args.IsConnected)
            {
                var rootNavigationViewModel = FindRootNavigationViewModel() as NavigationViewModel;
                if(rootNavigationViewModel != null && !rootNavigationViewModel.IsRefreshing)
                {
                    IsRefreshing = false;
                }
            }

        }

        /// <summary>
        /// Gets a value indicating whether this instance can connect.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can connect; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override bool CanConnect
        {
            get
            {
                return EnvironmentModel != null
                    && (!EnvironmentModel.IsConnected || !EnvironmentModel.CanStudioExecute);
            }
        }

        public override bool HasFileMenu
        {
            get
            {
                return EnvironmentModel != null
                    && EnvironmentModel.IsConnected;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has executable commands.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has executable commands; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override bool HasExecutableCommands
        {
            get
            {
                return base.HasExecutableCommands ||
                    CanConnect || CanDisconnect || CanRemove;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can disconnect.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can disconnect; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override bool CanDisconnect
        {
            get
            {
                return EnvironmentModel != null &&
                       EnvironmentModel.Connection != null &&
                       EnvironmentModel.IsConnected &&
                       EnvironmentModel.Name != StringResources.DefaultEnvironmentName;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance can be removed.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance can remove; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override bool CanRemove
        {
            get { return EnvironmentModel.ID != Guid.Empty; }
        }

        public override ObservableCollection<ITreeNode> Children
        {
            get
            {
                if(_children == null)
                {
                    if(_children != null)
                    {
                        _children.CollectionChanged -= ChildrenOnCollectionChanged;
                    }
                    _children = new SortedObservableCollection<ITreeNode>();
                    _children.CollectionChanged += ChildrenOnCollectionChanged;
                }
                return _children;
            }
            set
            {
                if(_children == value) return;

                _children.CollectionChanged -= ChildrenOnCollectionChanged;
                _children = value;
                _children.CollectionChanged += ChildrenOnCollectionChanged;
            }
        }


        public override ICommand RenameCommand
        {
            get
            {
                //not implimented
                return null;
            }
        }

        #endregion public properties

        #region Commands

        /// <summary>
        /// Gets the refresh command.
        /// </summary>
        /// <value>
        /// The refresh command.
        /// </value>
        /// <author>Massimo.Guerrera</author>
        /// <date>2013/06/20</date>
        public override ICommand RefreshCommand
        {
            get
            {
                return _refreshCommand ??
                        (_refreshCommand = new RelayCommand(param => RefreshEnvironment(), o => CanRefresh));
            }
        }

        /// <summary>
        /// Gets the connect command.
        /// </summary>
        /// <value>
        /// The connect command.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override ICommand ConnectCommand
        {
            get
            {
                return _connectCommand ??
                       (_connectCommand = new RelayCommand(param => Connect(), o => CanConnect));
            }
        }

        public override ICommand NewResourceCommand
        {
            get
            {
                return _newResourceCommand ??
                       (_newResourceCommand = new RelayCommand<string>(s
                                                                       =>
                       {
                           this.TraceInfo("Publish message of type - " + typeof(ShowNewResourceWizard));
                           EventPublisher.Publish(new ShowNewResourceWizard(s));
                       }, o => HasFileMenu));
            }
        }

        /// <summary>
        /// Gets the disconnect command.
        /// </summary>
        /// <value>
        /// The disconnect command.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override ICommand DisconnectCommand
        {
            get
            {
                return _disconnectCommand ?? (_disconnectCommand = new RelayCommand(param =>
                                                                       Disconnect(), param => CanDisconnect));
            }
        }

        /// <summary>
        /// Gets the remove command.
        /// </summary>
        /// <value>
        /// The remove command.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override ICommand RemoveCommand
        {
            get { return _removeCommand ?? (_removeCommand = new RelayCommand(parm => Remove(), o => CanRemove)); }
        }

        #endregion Commands

        #region public methods

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is filtered from the tree.
        ///     Always false for environment node
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is filtered; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override bool IsFiltered
        {
            get
            {
                return false;
            }
            set
            {
                //Do Nothing
            }
        }

        /// <summary>
        ///     Finds the environmentmodel for the treeparent
        /// </summary>
        /// <typeparam name="T">Type of the resource to find</typeparam>
        /// <param name="resourceToFind">The resource to find.</param>
        /// <returns></returns>
        /// <date>2013/01/23</date>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override ITreeNode FindChild<T>(T resourceToFind)
        {
            if(resourceToFind is IEnvironmentModel)
                if(EnvironmentModelEqualityComparer.Current.Equals(EnvironmentModel, resourceToFind))
                    return this;
            return base.FindChild(resourceToFind);
        }

        #endregion

        #region private methods

        /// <summary>
        /// Refreshes the environment.
        /// </summary>
        /// <author>Massimo.Guerrera</author>
        /// <date>2013/06/20</date>
        void RefreshEnvironment()
        {
            var rootNavigationViewModel = FindRootNavigationViewModel() as NavigationViewModel;
            if(rootNavigationViewModel != null)
            {
                rootNavigationViewModel.LoadEnvironmentResources(EnvironmentModel);
            }
        }

        /// <summary>
        /// Removes this instance.
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        private void Remove()
        {
            if(EnvironmentModel == null) return;
            Disconnect();
            var rootVm = FindRootNavigationViewModel();
            var ctx = (rootVm == null) ? null : rootVm.Context;
            this.TraceInfo("Publish message of type - " + typeof(RemoveEnvironmentMessage));
            EventPublisher.Publish(new RemoveEnvironmentMessage(EnvironmentModel, ctx));
            RaisePropertyChangedForCommands();
        }

        /// <summary>
        /// Connects this instance.
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        private void Connect()
        {
            if(EnvironmentModel.IsConnected && EnvironmentModel.CanStudioExecute) return;

            EnvironmentModel.CanStudioExecute = true;
            _asyncWorker.Start(EnvironmentModel.Connect, () => { });
            EnvironmentModel.ForceLoadResources();
            var vm = FindRootNavigationViewModel();
            vm.LoadEnvironmentResources(EnvironmentModel);
            RaisePropertyChangedForCommands();
        }

        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        private void Disconnect()
        {
            if(!EnvironmentModel.IsConnected) return;

            EnvironmentModel.Disconnect();
            NotifyOfPropertyChange(() => IsConnected);
            RaisePropertyChangedForCommands();

            var vm = FindRootNavigationViewModel() as NavigationViewModel;
            if(vm != null)
            {
                vm.SelectLocalHost();
            }
        }

        #endregion

        public void Handle(CloseWizardMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            throw new NotImplementedException();
        }

        //#region Implementation of IHandle<UpdateActiveEnvironmentMessage>

        //Juries Removed, we shouldnt need to set the active environment 
        //as selected just because its currently active?
        //public void Handle(UpdateActiveEnvironmentMessage message)
        //{
        //    if(Equals(_environmentModel, message.EnvironmentModel))
        //    {
        //        IsSelected = true;
        //    }
        //}


        protected override ITreeNode CreateParent(string displayName)
        {
            throw new NotImplementedException();
        }



    }
}
