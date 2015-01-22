using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Deploy;
using Dev2.Common.Interfaces.ErrorHandling;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.PopupController;
using Dev2.Common.Interfaces.Studio;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Util;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using Warewolf.Studio.Core;
using Warewolf.Studio.Core.Popup;
using Warewolf.Studio.Core.View_Interfaces;
using Warewolf.Studio.Models.Help;

namespace Warewolf.Studio.ViewModels
{
    public class ShellViewModel:IShellViewModel
    {
        readonly IUnityContainer _unityContainer;
        readonly IRegionManager _regionManager;
        readonly IEventAggregator _aggregator;
        IExceptionHandler _handler;
        IPopupController _popupController;
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
        }

        public void Initialize()
        {
            var explorerRegion = _regionManager.Regions[RegionNames.Explorer];
            var explorerView = _unityContainer.Resolve<IExplorerView>();
            explorerView.DataContext = _unityContainer.Resolve<IExplorerViewModel>();
            explorerRegion.Add(explorerView, RegionNames.Explorer);
            explorerRegion.Activate(explorerView);

            var toolboxRegion = _regionManager.Regions[RegionNames.Toolbox];
            var toolBoxView = _unityContainer.Resolve<IToolboxView>();
            toolBoxView.DataContext = _unityContainer.Resolve<IToolboxViewModel>();
            toolboxRegion.Add(toolBoxView, RegionNames.Toolbox);
            toolboxRegion.Activate(toolBoxView);

            var menuRegion = _regionManager.Regions[RegionNames.Menu];
            var menuView = _unityContainer.Resolve<IMenuView>();
            menuView.DataContext = _unityContainer.Resolve<IMenuViewModel>();
            menuRegion.Add(menuView, RegionNames.Menu);
            menuRegion.Activate(menuView);
            _handler = _unityContainer.Resolve<IExceptionHandler>();
            _popupController = _unityContainer.Resolve<IPopupController>();
            _handler.AddHandler(typeof(WarewolfInvalidPermissionsException), () => { _popupController.Show(PopupMessages.GetInvalidPermissionException()); });
            var variableListRegion = _regionManager.Regions[RegionNames.VariableList];

            var variableList = _unityContainer.Resolve<IVariableListView>();
            variableList.DataContext = _unityContainer.Resolve<Dev2.Common.Interfaces.DataList.DatalistView.IVariableListViewModel>();
            variableListRegion.Add(variableList, RegionNames.VariableList);

        }

        public bool RegionHasView(string regionName)
        {
           return GetRegionViews(regionName).Any();
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
            if(foundViewModel==null)
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
            VerifyArgument.IsNotNull("resourceToDeploy",resourceToDeploy);
            var region = GetRegion(RegionNames.Workspace);
            var vm = region.Views.FirstOrDefault(a=>a is IDeployViewModel);

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
       

        public void NewResource(ResourceType? type)
        {
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

  
    }
}