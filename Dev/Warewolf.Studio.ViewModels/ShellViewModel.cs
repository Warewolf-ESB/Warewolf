using System.Collections.Generic;
using System.Linq;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Studio;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Common.Interfaces.Toolbox;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using Warewolf.Studio.Core;
using Warewolf.Studio.Core.View_Interfaces;

namespace Warewolf.Studio.ViewModels
{
    public class ShellViewModel:IShellViewModel
    {
        readonly IUnityContainer _unityContainer;
        readonly IRegionManager _regionManager;

        public ShellViewModel(IUnityContainer unityContainer, IRegionManager regionManager)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "unityContainer", unityContainer }, { "regionManager", regionManager } });
            _unityContainer = unityContainer;
            _regionManager = regionManager;
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

        public void NewResource(ResourceType? type)
        {
        }
    }
}