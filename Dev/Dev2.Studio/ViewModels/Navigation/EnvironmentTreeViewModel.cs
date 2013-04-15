#region

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.InterfaceImplementors.WizardResourceKeys;
using Dev2.Studio.Webs;
using Unlimited.Applications.BusinessDesignStudio.Views;

#endregion

namespace Dev2.Studio.ViewModels.Navigation
{
    /// <summary>
    /// The viewmodel for a treenode representing an environment
    /// </summary>
    /// <date>2013/01/23</date>
    /// <author>
    /// Jurie.smit
    /// </author>
    public sealed class EnvironmentTreeViewModel : AbstractTreeViewModel,IHandle<CloseWizardMessage>
    {
        #region private fields

        private RelayCommand _connectCommand;
        private RelayCommand _disconnectCommand;
        private IEnvironmentModel _environmentModel;
        private RelayCommand _removeCommand;
        private RelayCommand<string> _newResourceCommand;
        WebPropertyEditorWindow _win;

        #endregion

        #region ctor + init

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentTreeViewModel" /> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="environmentModel">The environment model.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public EnvironmentTreeViewModel(ITreeNode parent,
            IEnvironmentModel environmentModel) 
            : base(null)
        {
            EnvironmentModel = environmentModel;
            IsExpanded = true;
            if (parent != null)
            {
                parent.Add(this);
            }
        }

        #endregion

        #region public properties

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

        /// <summary>
        /// Gets or sets the environment model for this instance.
        /// </summary>
        /// <value>
        /// The environment model.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override sealed IEnvironmentModel EnvironmentModel 
        {
            get { return _environmentModel; }
            protected set
            {
                _environmentModel = value;
                NotifyOfPropertyChange(() => EnvironmentModel);
                NotifyOfPropertyChange(() => IsConnected);
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
            get { return EnvironmentModel != null 
                && !EnvironmentModel.IsConnected; }
        }

        public override bool HasFileMenu
        {
            get { return EnvironmentModel != null 
                && EnvironmentModel.IsConnected; }
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
                if (_children == null)
                {
                    _children = new SortedObservableCollection<ITreeNode>();
                    _children.CollectionChanged += ChildrenOnCollectionChanged;
                }
                return _children;
            }
            set
            {
                if (_children == value) return;

                _children = value;
                _children.CollectionChanged -= ChildrenOnCollectionChanged;
                _children.CollectionChanged += ChildrenOnCollectionChanged;
            }
        }
        #endregion public properties

        #region Commands

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
                       (_newResourceCommand = new RelayCommand<string>(NewResource, o => HasFileMenu));
            }
        }

        void NewResource(string resourceType)
        {
            IContextualResourceModel resourceModel = ResourceModelFactory.CreateResourceModel(EnvironmentModel, resourceType,
                                                                                             resourceType);
            var resourceViewModel = new ResourceWizardViewModel(resourceModel);
            if (RootWebSite.ShowDialog(resourceModel))
            {
                return;
            }

            bool doesServiceExist =
                EnvironmentModel.ResourceRepository.Find(r => r.ResourceName == "Dev2ServiceDetails").Count > 0;

            if (doesServiceExist)
            {
                // Travis.Frisinger: 07.90.2012 - Amended to convert studio resources into server resources
                string resName =
                    StudioToWizardBridge.ConvertStudioToWizardType(resourceType.ToString(CultureInfo.InvariantCulture),
                        resourceModel.ServiceDefinition,
                        resourceModel.Category);
                //string requestUri = string.Format("{0}/services/{1}?{2}={3}&Dev2NewService=1", MainViewModel.CurrentWebServer, StudioToWizardBridge.SelectWizard(resourceModel), ResourceKeys.Dev2ServiceType, resName);

                Uri requestUri;
                if (
                    !Uri.TryCreate(EnvironmentModel.Connection.WebServerUri,
                        BuildUri(resourceModel, resName), out requestUri))
                {
                    requestUri = new Uri(new Uri(StringResources.Uri_WebServer), BuildUri(resourceModel, resName));
                }

                try
                {
                    _win = new WebPropertyEditorWindow(resourceViewModel, requestUri.AbsoluteUri)
                    {
                        Width = 850,
                        Height = 600
                    };
                    _win.ShowDialog();
                }
                catch
                {
                }
            }
        }

        public void Handle(CloseWizardMessage message)
        {
            if (_win != null)
            {
                _win.Close();
            }
        }


        static string BuildUri(IContextualResourceModel resourceModel, string resName)
        {
            var uriString = "/services/" + StudioToWizardBridge.SelectWizard(resourceModel);
            if (resourceModel.ResourceType == ResourceType.WorkflowService || resourceModel.ResourceType == ResourceType.Service)
            {
                uriString += "?" + ResourceKeys.Dev2ServiceType + "=" + resName;
            }
            return uriString;
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
        ///     Raises the property changed for the commands.
        /// </summary>
        /// <date>2013/01/23</date>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override void RaisePropertyChangedForCommands()
        {
            NotifyOfPropertyChange(() => CanDisconnect);
            NotifyOfPropertyChange(() => CanConnect);
            NotifyOfPropertyChange(() => IsConnected);
            base.RaisePropertyChangedForCommands();
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
            if (resourceToFind is IEnvironmentModel)
                if (EnvironmentModelEqualityComparer.Current.Equals(EnvironmentModel, resourceToFind))
                    return this;
            return base.FindChild(resourceToFind);
        }

        #endregion

        #region private methods

        /// <summary>
        /// Removes this instance.
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        private void Remove()
        {
            if (EnvironmentModel == null) return;
            EventAggregator.Publish(new RemoveServerFromExplorerMessage(EnvironmentModel));
            //Mediator.SendMessage(MediatorMessages.RemoveServerFromExplorer, EnvironmentModel);

            RaisePropertyChangedForCommands();
        }

        /// <summary>
        /// Connects this instance.
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        private void Connect()
        {
            if (EnvironmentModel.IsConnected) return;

            EnvironmentModel.Connect();
            RaisePropertyChangedForCommands();
        }

        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        private void Disconnect()
        {
            if (!EnvironmentModel.IsConnected) return;

            EnvironmentModel.Disconnect();
            RaisePropertyChangedForCommands();
        }
        #endregion
    }

    
}
