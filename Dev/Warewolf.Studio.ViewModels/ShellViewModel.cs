using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Deploy;
using Dev2.Common.Interfaces.ErrorHandling;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.PopupController;
using Dev2.Common.Interfaces.Runtime.ServiceModel;
using Dev2.Common.Interfaces.ServerDialogue;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Util;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using Moq;
using Warewolf.Studio.Core;
using Warewolf.Studio.Core.Popup;
using Warewolf.Studio.Core.View_Interfaces;
using Warewolf.Studio.Models.Help;
using Warewolf.Studio.Models.Toolbox;
using Warewolf.Studio.ViewModels.ToolBox;
using IVariableListViewModel = Dev2.Common.Interfaces.DataList.DatalistView.IVariableListViewModel;

namespace Warewolf.Studio.ViewModels
{
    public class ShellViewModel : BindableBase, IShellViewModel
    {
        readonly IUnityContainer _unityContainer;
        readonly IRegionManager _regionManager;
        readonly IEventAggregator _aggregator;
        IExceptionHandler _handler;
        IPopupController _popupController;
        object _activeItem;
        IServer _activeServer;
        double _menuPanelWidth;
        bool _menuExpanded;

        public ShellViewModel(IUnityContainer unityContainer, IRegionManager regionManager, IEventAggregator aggregator)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "unityContainer", unityContainer }, { "regionManager", regionManager } });
            _unityContainer = unityContainer;
            _regionManager = regionManager;
            _aggregator = aggregator;
            var localHostString = AppSettings.LocalHost;
            var localhostUri = new Uri(localHostString);
            LocalhostServer = unityContainer.Resolve<IServer>(new ParameterOverrides { { "uri", localhostUri } });
            LocalhostServer.ResourceName = "localhost (" + localHostString + ")";
            ActiveServer = LocalhostServer;

            _menuPanelWidth = 60;
            _menuExpanded = false;
        }

        public void Initialize()
        {

            _unityContainer.RegisterInstance<IToolboxViewModel>(new ToolboxViewModel(new ToolboxModel(LocalhostServer, LocalhostServer, new Mock<IPluginProxy>().Object), new ToolboxModel(LocalhostServer, LocalhostServer, new Mock<IPluginProxy>().Object)));
           
            InitializeRegion<IExplorerView,IExplorerViewModel>(RegionNames.Explorer);
            InitializeRegion<IToolboxView, IToolboxViewModel>(RegionNames.Toolbox);
            InitializeRegion<IMenuView, IMenuViewModel>(RegionNames.Menu);
            InitializeRegion<IVariableListView, IVariableListViewModel>(RegionNames.VariableList);
            InitializeRegion<IHelpView, IHelpWindowViewModel>(RegionNames.Help);

            _handler = _unityContainer.Resolve<IExceptionHandler>();
            _popupController = _unityContainer.Resolve<IPopupController>();
            _handler.AddHandler(typeof(WarewolfInvalidPermissionsException), () => { _popupController.Show(PopupMessages.GetInvalidPermissionException()); });

        }

        public void InitializeRegion<T,TU>(string regionName) where T:IView
        {
            var region = _regionManager.Regions[regionName];
            var view = _unityContainer.Resolve<T>();
            view.DataContext = _unityContainer.Resolve<TU>();
            region.Add(view, regionName);
            region.Activate(view);
        }

        public bool RegionHasView(string regionName)
        {
           return GetRegionViews(regionName).Any();
        }



        public async Task<bool> CheckForNewVersion()
        {
            var versionChecker = _unityContainer.Resolve<IVersionChecker>();
            var hasNewVersion = await versionChecker.GetNewerVersionAsync();
            return hasNewVersion;
        }
        
        public async void DisplayDialogForNewVersion()
        {
            var hasNewVersion = await CheckForNewVersion();
            if (hasNewVersion)
            {
                var dialog = _unityContainer.Resolve<IWebLatestVersionDialog>();
                dialog.ShowDialog();                
            }
        }

        public void OpenVersion(Guid resourceId, string versionNumber)
        {
            //todo:
        }

        public void ExecuteOnDispatcher(Action action)
        {
            Application.Current.Dispatcher.Invoke(action);
        }

        public void ServerSourceAdded(IServerSource source)
        {
            if (source != null && _aggregator != null)
            {
                _aggregator.GetEvent<ServerAddedEvent>().Publish(source); 
            }
        }

        public IViewsCollection GetRegionViews(string regionName)
        {
            var region = GetRegion(regionName);
            return region.Views;
        }

        IRegion GetRegion(string regionName)
        {
            var region = _regionManager.Regions[regionName]; //get the region
            return region;
        }

        public bool RegionViewHasDataContext(string regionName)
        {
            var region = GetRegion(regionName);
            var view = region.GetView(regionName);
            var userView = view as IView;
            if (userView != null)
            {
                return userView.DataContext != null;
            }
            return false;
        }

        public void AddService(IResource resource)
        {

            var region = GetRegion(RegionNames.Workspace);
            var foundViewModel = region.Views.FirstOrDefault(o =>
            {
                var viewModel = o as IServiceDesignerViewModel;
                if (viewModel == null)
                {
                    return false;
                }
                return Equals(viewModel.Resource, resource);
            });
            if (foundViewModel == null)
            {
                if (resource.ResourceType == ResourceType.WorkflowService)
                {
                    foundViewModel = _unityContainer.Resolve<IWorkflowServiceDesignerViewModel>(new ParameterOverrides { { "resource", resource } });
                }
                else
                {
                    foundViewModel = _unityContainer.Resolve<IServiceDesignerViewModel>(new ParameterOverrides { { "resource", resource } });
                }
                region.Add(foundViewModel); //add the viewModel
            }
            region.Activate(foundViewModel); //active the viewModel
        }


        public void DeployService(IExplorerItemViewModel resourceToDeploy)
        {
            VerifyArgument.IsNotNull("resourceToDeploy", resourceToDeploy);
            var region = GetRegion(RegionNames.Workspace);
            var vm = region.Views.FirstOrDefault(a => a is IDeployViewModel);

            if (vm == null)
            {
                var deployVm = _unityContainer.Resolve<IDeployViewModel>();
                deployVm.SelectSourceItem(resourceToDeploy);
                region.Add(deployVm);
                region.Activate(deployVm); 
            }
            else
            {
                var deploy = vm as IDeployViewModel;
                // ReSharper disable once PossibleNullReferenceException
                deploy.SelectSourceItem(resourceToDeploy);
                region.Activate(deploy); //active the viewModel
            }
            
        }

        public void UpdateHelpDescriptor(IHelpDescriptor helpDescriptor)
        {
            //!!!!!!!/// Reviewer please note this could go either way here. Used the event aggregator to ensure order of events... but could go the other way as well cos the shell view model owns this
             VerifyArgument.IsNotNull("helpDescriptor", helpDescriptor);
              _aggregator.GetEvent<HelpChangedEvent>().Publish(helpDescriptor);
            
        }
       

        public double MenuPanelWidth
        {
            get
            {
                return _menuPanelWidth;
            }
            set
            {
                _menuPanelWidth = value;
                OnPropertyChanged(() => MenuPanelWidth);
            }
        }

        public void NewResource(ResourceType? type,Guid selectedId)
        {
            if (type == null)
                return;
            switch (type.Value)
            {
                case ResourceType.ServerSource:
                    CreateNewServerSource(selectedId);
                    break;
                case ResourceType.DbSource:
                    CreateDatabaseSource();
                    break;
                default: return;

            }
        }

        private void CreateDatabaseSource()
        {
            var mockDbSourceViewModel = new ManageDatabaseSourceViewModel();
            GetRegion("Workspace").Add(mockDbSourceViewModel);
        }

        void CreateNewServerSource(Guid selectedId)
        {
            var server = new NewServerViewModel(new ServerSource() { UserName = "", Address = "", AuthenticationType = AuthenticationType.Windows, ID = Guid.NewGuid(), Name = "", Password = "", ResourcePath = "" }, ActiveServer.UpdateRepository, new RequestServiceNameViewModel(new EnvironmentViewModel(LocalhostServer, this, _unityContainer.Resolve<IExplorerViewModel>()), _unityContainer.Resolve<IRequestServiceNameView>(), selectedId), this,
                ActiveServer.ResourceName.Substring(0,ActiveServer.ResourceName.IndexOf("(", System.StringComparison.Ordinal)),selectedId) { ServerSource = new ServerSource() { UserName = "", Address = "", AuthenticationType = AuthenticationType.Windows, ID = Guid.NewGuid(), Name = "", Password = "", ResourcePath = "" } };
            GetRegion("Workspace").Add(server);

        }

        public void SaveService()
        {
        }

        public void ExecuteService()
        {
        }

        public void OpenScheduler()
        {
        }

        public void OpenSettings()
        {
        }

        public IServer ActiveServer
        {
            get
            {
                return _activeServer;
            }
            set
            {
                if (!value.Equals(_activeServer))
                {
                    _activeServer = value;
                    RaiseActiveServerChanged();
                }
            }
        }

        void RaiseActiveServerChanged()
        {
            if (ActiveServerChanged != null)
            {
                ActiveServerChanged();
            }
        }

        public object ActiveItem
        {
            get
            {
                return _activeItem;
            }
            set
            {
                _activeItem = value;
                RaiseActiveItemChanged();
            }
        }

        void RaiseActiveItemChanged()
        {
            if (ActiveItemChanged != null)
            {
                ActiveItemChanged();
            }
        }

        public IServer LocalhostServer { get; set; }

        public void Handle(Exception err)
        {
            _handler.Handle(err);
        }

        public bool ShowPopup(IPopupMessage msg)
        {
            var res = _popupController.Show(msg);
            return res == MessageBoxResult.OK || res == MessageBoxResult.Yes;
        }

        public void RemoveServiceFromExplorer(IExplorerItemViewModel explorerItemViewModel)
        {
            var explorervm = _unityContainer.Resolve<IExplorerViewModel>();
            explorervm.RemoveItem(explorerItemViewModel);
        }

        public event Action ActiveServerChanged;
        public event Action ActiveItemChanged;




        public bool MenuExpanded
        {
            get
            {
                return _menuExpanded;
            }
            set
            {
                _menuExpanded = value;
                OnPropertyChanged(() => MenuExpanded);
            }
        }
    }
}