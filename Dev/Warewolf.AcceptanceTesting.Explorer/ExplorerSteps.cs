using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Linq;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Common.Interfaces.Toolbox;
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
    
    [Binding]    
    public class ExplorerSteps
    {
        [BeforeFeature]
        public static void SetupExplorerDependencies()
        {
            var bootstrapper = new UnityBootstrapperForExplorerTesting();
            bootstrapper.Run();
            FeatureContext.Current.Add("container", bootstrapper.Container);
            var explorerView = bootstrapper.Container.Resolve<IExplorerView>();
            var window = new Window();
            window.Content = explorerView;
            var app = Application.Current;
            app.MainWindow = window;
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                var explorerViewWindow = (ExplorerView)Application.Current.MainWindow.Content;
                Assert.IsNotNull(explorerViewWindow);
                Assert.IsNotNull(explorerViewWindow.DataContext);
                Assert.IsInstanceOfType(explorerViewWindow.DataContext, typeof(IExplorerViewModel));
                Application.Current.Shutdown();
            }));
            
            Application.Current.Run(Application.Current.MainWindow);
        }

 
        [BeforeScenario("Explorer")]
        public void SetupForExplorer()
        {
            var container = FeatureContext.Current.Get<IUnityContainer>("container");
            var explorerView = container.Resolve<IExplorerView>();
            ScenarioContext.Current.Add("explorerView", explorerView);
            
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
            var environmentViewModel = explorerView.OpenEnvironmentNode(serverName);
            Assert.IsNotNull(environmentViewModel);
            Assert.IsTrue(environmentViewModel.IsExpanded);
        }

        [Given(@"I open ""(.*)""")]
        [When(@"I open ""(.*)""")]
        public void WhenIOpen(string folderName)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView");
            var environmentViewModel = explorerView.OpenFolderNode(folderName);
            Assert.IsNotNull(environmentViewModel);
        }

        [Then(@"I should see ""(.*)"" folders")]
        public void ThenIShouldSeeFolders(int numberOfFoldersVisible)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView");
            var explorerItemViewModels = explorerView.GetFoldersVisible();
            Assert.AreEqual(numberOfFoldersVisible,explorerItemViewModels.Count);
        }

        [Then(@"I should see ""(.*)"" children for ""(.*)""")]
        public void ThenIShouldSeeChildrenFor(int expectedChildrenCount, string folderName)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView");
            var childrenCount = explorerView.GetVisibleChildrenCount(folderName);
            Assert.AreEqual(expectedChildrenCount,childrenCount);
        }

        [When(@"I rename ""(.*)"" to ""(.*)""")]
        public void WhenIRenameTo(string originalFolderName, string newFolderName)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView");
            explorerView.PerformFolderRename(originalFolderName,newFolderName);
            
        }

        [Then(@"I should not see ""(.*)""")]
        public void ThenIShouldNotSee(string folderName)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView");
            var environmentViewModel = explorerView.OpenFolderNode(folderName);
            Assert.IsNull(environmentViewModel);
        }

        [Then(@"I should see ""(.*)"" only")]
        public void ThenIShouldSee(string folderName)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView");
            var environmentViewModel = explorerView.OpenFolderNode(folderName);
            Assert.IsNotNull(environmentViewModel);
        }

        [When(@"I search for ""(.*)""")]
        public void WhenISearchFor(string searchTerm)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView");
            explorerView.PerformSearch(searchTerm);
        }

    }

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

            Container.RegisterType<IServer, ServerForTesting>(new InjectionConstructor());
            Container.RegisterInstance<IShellViewModel>(new ShellViewModel(Container, Container.Resolve<IRegionManager>(), Container.Resolve<IEventAggregator>()));
            Container.RegisterInstance<IExplorerViewModel>(new ExplorerViewModel(Container.Resolve<IShellViewModel>(), Container.Resolve<IEventAggregator>()));

            var explorerView = new ExplorerView();
            explorerView.DataContext = Container.Resolve<IExplorerViewModel>();
            Container.RegisterInstance<IExplorerView>(explorerView);

        }

    }


    public class ServerForTesting : Resource, IServer
    {
        public ServerForTesting()
        {
             var mockExplorerRepo = new Mock<IExplorerRepository>();
            mockExplorerRepo.Setup(repository => repository.Rename(It.IsAny<IExplorerItemViewModel>(), It.IsAny<string>())).Returns(true);
            _explorerProxy = mockExplorerRepo.Object;
        }

        private IExplorerRepository _explorerProxy;

        public ServerForTesting(IResource copy) : base(copy)
        {
        }

        public ServerForTesting(XElement xml) : base(xml)
        {
        }

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
            children.AddRange(CreateFolders(new[] { "Folder 1", "Folder 2", "Folder 3", "Folder 4", "Folder 5" }));
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
                mockIExplorerItem.Setup(item => item.Children).Returns(new List<IExplorerItem>());
                folders.Add(mockIExplorerItem.Object);
            }
            CreateChildrenForFolder(folders[1], new[] { "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1" });
            return folders;
        }

        private void CreateChildrenForFolder(IExplorerItem explorerItem, string[] childNames)
        {
            int i = 1;
            var resourceType = ResourceType.EmailSource;
            foreach (var name in childNames)
            {
                if (i % 2 == 0)
                {
                    resourceType = ResourceType.WorkflowService;
                }
                if (i % 3 == 0)
                {
                    resourceType = ResourceType.DbService;
                }
                if (i % 4 == 0)
                {
                    resourceType = ResourceType.WebSource;
                }
                var mockIExplorerItem = new Mock<IExplorerItem>();
                mockIExplorerItem.Setup(item => item.ResourceType).Returns(resourceType);
                mockIExplorerItem.Setup(item => item.DisplayName).Returns(explorerItem.DisplayName + " " + name);
                mockIExplorerItem.Setup(item => item.ResourceId).Returns(Guid.NewGuid());
                explorerItem.Children.Add(mockIExplorerItem.Object);
                i++;
            }
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
            get
            {
                if (_explorerProxy != null)
                {
                    return _explorerProxy;
                }
                return new Mock<IExplorerRepository>().Object;
            }
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
            get
            {
                return new List<IWindowsGroupPermission>{new WindowsGroupPermission
            {
                Administrator = true,
                IsServer = true,
                ResourceID = Guid.Empty
                
            }};
            }
        }
        
        public event PermissionsChanged PermissionsChanged;
        public event NetworkStateChanged NetworkStateChanged;

        public IStudioUpdateManager UpdateRepository
        {
            get { throw new NotImplementedException(); }
        }

        public string GetServerVersion()
        {
            throw new NotImplementedException();
        }
    }

}
