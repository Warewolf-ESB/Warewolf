using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.Enums;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.Util;
using Infragistics.Themes;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.Studio.Core.View_Interfaces;
using Warewolf.Studio.Themes.Luna;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;

namespace Warewolf.AcceptanceTesting.Explorer
{

    public class UnityBootstrapperForExplorerTesting : UnityBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return new DependencyObject();
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            AppSettings.LocalHost = "http://myserver:3124/";
            // ReSharper disable ObjectCreationAsStatement
            new Application();
            // ReSharper restore ObjectCreationAsStatement
            ThemeManager.ApplicationTheme = new LunaTheme();

            Container.RegisterType<IServer, ServerForTesting>();
            Container.RegisterInstance<IShellViewModel>(new ShellViewModel(Container, Container.Resolve<IRegionManager>(), Container.Resolve<IEventAggregator>()));
            Container.RegisterInstance<IExplorerViewModel>(new ExplorerViewModel(Container.Resolve<IShellViewModel>()));

            var explorerView = new ExplorerView();
            explorerView.DataContext = Container.Resolve<IExplorerViewModel>();
            Container.RegisterInstance<IExplorerView>(explorerView);           
        }
    }


    public class ServerForTesting:Resource,IServer
    {
        public Task<bool> Connect()
        {
            return Task.FromResult(true);
        }

        public List<IResource> Load()
        {
            return CreateResources();
        }

        private List<IResource> CreateResources()
        {
            return new List<IResource>();
        }

        public Task<IExplorerItem> LoadExplorer()
        {
            return Task.FromResult(CreateExplorerItems());
        }

        private IExplorerItem CreateExplorerItems()
        {
            var mockExplorerItem = new Mock<IExplorerItem>();
            mockExplorerItem.Setup(item => item.DisplayName).Returns("Level 0");
            var children = new List<IExplorerItem>();
            children.AddRange(CreateFolders(new[] {"Folder 1","Folder 2", "Folder 3","Folder 4","Folder 5"}));
            mockExplorerItem.Setup(item => item.Children).Returns(children);
            return mockExplorerItem.Object;
        }

        private IEnumerable<IExplorerItem> CreateFolders(string[] names)
        {
            var folders = new List<IExplorerItem>();
            foreach (var name in names)
            {
                var mockIExplorerItem = new Mock<IExplorerItem>();
                mockIExplorerItem.Setup(item => item.ResourceType).Returns(ResourceType.Folder);
                mockIExplorerItem.Setup(item => item.DisplayName).Returns(name);
                folders.Add(mockIExplorerItem.Object);
            }
            return folders;
        }

        public IList<IServer> GetServerConnections()
        {
            return null;
        }

        public IList<IToolDescriptor> LoadTools()
        {
            throw new NotImplementedException();
        }

        public IExplorerRepository ExplorerRepository
        {
            get { return new Mock<IExplorerRepository>().Object; }
        }

        public bool IsConnected()
        {
            return true;
        }

        public void ReloadTools()
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public void Edit()
        {
            throw new NotImplementedException();
        }

        public List<IWindowsGroupPermission> Permissions
        {
            get { return new List<IWindowsGroupPermission>{new WindowsGroupPermission
            {
                Administrator = true,
                IsServer = true,
                ResourceID = Guid.Empty
                
            }};}
        }

        public event PermissionsChanged PermissionsChanged;
        public event NetworkStateChanged NetworkStateChanged;
    }

    [Binding]
    public class ExplorerSteps
    {
        [BeforeFeature]
        public static void SetupExplorerDependencies()
        {
            var bootstrapper = new UnityBootstrapperForExplorerTesting();
            bootstrapper.Run();
            FeatureContext.Current.Add("container", bootstrapper.Container);
        }

        [BeforeScenario("Explorer")]
        public void SetupForExplorer()
        {
            var container = FeatureContext.Current.Get<IUnityContainer>("container");
                var explorerView = container.Resolve<IExplorerView>();
                ScenarioContext.Current.Add("explorerView",explorerView);
            
        }

        [Given(@"the explorer is visible")]
        public void GivenTheExplorerIsVisible()
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView");
            Assert.IsNotNull(explorerView);
            Assert.IsNotNull(explorerView.DataContext);            
        }

        [Given(@"I open ""(.*)"" server")]
        [When(@"I open ""(.*)"" server")]
        public void WhenIOpenServer(string serverName)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView");
            var environmentViewModel = explorerView.GetEnvironmentNode(serverName);
            Assert.IsNotNull(environmentViewModel);
            environmentViewModel.IsExpanded = true;
        }

        [When(@"I open ""(.*)""")]
        public void WhenIOpen(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I should see ""(.*)"" folders")]
        public void ThenIShouldSeeFolders(int numberOfFoldersVisible)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView");
            var explorerItemViewModels = explorerView.GetFoldersVisible();
            Assert.AreEqual(numberOfFoldersVisible,explorerItemViewModels.Count);
        }

        [Then(@"I should see ""(.*)"" children for ""(.*)""")]
        public void ThenIShouldSeeChildrenFor(int p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }
    }
}
