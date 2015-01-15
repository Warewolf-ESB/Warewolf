using System;
using System.Collections.Generic;
using System.Linq;
using Dev2;
using Dev2.Common.Interfaces.Studio;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Common.Interfaces.Toolbox;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.Core;
using Warewolf.Studio.Core.View_Interfaces;

// ReSharper disable RedundantTypeArgumentsOfMethod

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ShellViewModelTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ShellViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShellViewModel_Constructor_NullContainer_NullExceptionThrown()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            new ShellViewModel(null, new Mock<IRegionManager>().Object);
            //------------Assert Results-------------------------
        } 
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ShellViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShellViewModel_Constructor_NullRegionManager_NullExceptionThrown()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            new ShellViewModel(new Mock<IUnityContainer>().Object, null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ShellViewModel_Constructor")]
        public void ShellViewModel_Constructor_Should_AddViewsToRegions()
        {
            //------------Setup for test--------------------------
            var testContainer = new UnityContainer();
            var testRegionManager = new RegionManager();
            testRegionManager.Regions.Add("Explorer",new Region());
            testRegionManager.Regions.Add("Toolbox",new Region());
            testRegionManager.Regions.Add("Menu",new Region());
            testContainer.RegisterInstance<IExplorerView>(new Mock<IExplorerView>().Object);
            testContainer.RegisterInstance<IToolboxView>(new Mock<IToolboxView>().Object);
            testContainer.RegisterInstance<IMenuView>(new Mock<IMenuView>().Object);

            testContainer.RegisterInstance<IExplorerViewModel>(new Mock<IExplorerViewModel>().Object);
            testContainer.RegisterInstance<IToolboxViewModel>(new Mock<IToolboxViewModel>().Object);
            testContainer.RegisterInstance<IMenuViewModel>(new Mock<IMenuViewModel>().Object);
            //------------Execute Test---------------------------
            var shellViewModel = new ShellViewModel(testContainer, testRegionManager);
            //------------Assert Results-------------------------
            Assert.IsNotNull(shellViewModel);
            Assert.IsTrue(shellViewModel.RegionHasView("Explorer"));
            Assert.IsTrue(shellViewModel.RegionHasView("Toolbox"));
            Assert.IsTrue(shellViewModel.RegionHasView("Menu"));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ShellViewModel_Constructor")]
        public void ShellViewModel_Constructor_Should_SetViewModelAsDataContextForRegionViews()
        {
            //------------Setup for test--------------------------
            var testContainer = new UnityContainer();
            var testRegionManager = new RegionManager();
            testRegionManager.Regions.Add("Explorer",new Region());
            testRegionManager.Regions.Add("Toolbox",new Region());
            testRegionManager.Regions.Add("Menu",new Region());
            testContainer.RegisterInstance<IExplorerViewModel>(new Mock<IExplorerViewModel>().Object);
            testContainer.RegisterInstance<IToolboxViewModel>(new Mock<IToolboxViewModel>().Object);
            testContainer.RegisterInstance<IMenuViewModel>(new Mock<IMenuViewModel>().Object);

            var mockExplorerView = new Mock<IExplorerView>();
            mockExplorerView.SetupProperty(view => view.DataContext);
            testContainer.RegisterInstance<IExplorerView>(mockExplorerView.Object);
            var mockToolBoxView = new Mock<IToolboxView>();
            mockToolBoxView.SetupProperty(view => view.DataContext);
            testContainer.RegisterInstance<IToolboxView>(mockToolBoxView.Object);
            var mockMenuView = new Mock<IMenuView>();
            mockMenuView.SetupProperty(view => view.DataContext);
            testContainer.RegisterInstance<IMenuView>(mockMenuView.Object);
            //------------Execute Test---------------------------
            var shellViewModel = new ShellViewModel(testContainer, testRegionManager);
            //------------Assert Results-------------------------
            Assert.IsNotNull(shellViewModel);
            Assert.IsTrue(shellViewModel.RegionViewHasDataContext("Explorer"));
            Assert.IsTrue(shellViewModel.RegionViewHasDataContext("Toolbox"));
            Assert.IsTrue(shellViewModel.RegionViewHasDataContext("Menu"));
        }
    }

    public class ShellViewModel
    {
        readonly IUnityContainer _unityContainer;
        readonly IRegionManager _regionManager;

        public ShellViewModel(IUnityContainer unityContainer, IRegionManager regionManager)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "unityContainer", unityContainer }, { "regionManager", regionManager } });
            _unityContainer = unityContainer;
            _regionManager = regionManager;
            Initialize();
        }

        void Initialize()
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
        }

        public bool RegionHasView(string regionName)
        {
            var region = _regionManager.Regions[regionName]; //get the region
            return region.Views.Any();
        }

        public bool RegionViewHasDataContext(string regionName)
        {
            var region = _regionManager.Regions[regionName]; //get the region
            var view = region.GetView(regionName);
            var userView = view as IView;
            if (userView != null)
            {
                return userView.DataContext != null;
            }
            return false;
        }
    }
}
