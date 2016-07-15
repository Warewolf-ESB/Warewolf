using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.PopupController;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Explorer;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.Studio.Controller;
using Dev2.Studio.Core.Interfaces;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.AcceptanceTesting.Core;
using Warewolf.Studio.Core;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;
using Warewolf.Testing;

namespace Warewolf.AcceptanceTesting.Explorer
{    
    [Binding]    
    // ReSharper disable UnusedMember.Global
    public class ExplorerSteps      
    {
        [BeforeFeature("Explorer")]
        public static void SetupExplorerDependencies()
        {
            Utils.SetupResourceDictionary();
            var explorerView = new ExplorerView();
            var mockShellViewModel = new Mock<IShellViewModel>();
            var mockExplorerRepository = new Mock<IExplorerRepository>();
            var popupController = new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>();
            CustomContainer.Register(popupController.Object);
            mockExplorerRepository.Setup(repository => repository.CreateFolder(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()));
            mockExplorerRepository.Setup(repository => repository.Rename(It.IsAny<IExplorerItemViewModel>(), It.IsAny<string>())).Returns(true);
            mockShellViewModel.Setup(model => model.LocalhostServer).Returns(new ServerForTesting(mockExplorerRepository));
            mockShellViewModel.Setup(model => model.OpenResource(It.IsAny<Guid>(),It.IsAny<IServer>())).Verifiable();
            var mockEventAggregator = new Mock<IEventAggregator>();
            mockEventAggregator.Setup(aggregator => aggregator.GetEvent<ServerAddedEvent>()).Returns(new ServerAddedEvent());
            var explorerViewModel = new ExplorerViewModel(mockShellViewModel.Object,mockEventAggregator.Object );
            explorerView.DataContext = explorerViewModel;

            Utils.ShowTheViewForTesting(explorerView);
            FeatureContext.Current.Add(Utils.ViewNameKey, explorerView);
            FeatureContext.Current.Add("popupController", popupController);
            FeatureContext.Current.Add(Utils.ViewModelNameKey, explorerViewModel);
            FeatureContext.Current.Add("mockShellViewModel", mockShellViewModel);
            FeatureContext.Current.Add("mockExplorerRepository", mockExplorerRepository);
        }
         
        [BeforeScenario("Explorer")]
        public void SetupForExplorer()
        {
            var explorerView = FeatureContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            var explorerViewModel = FeatureContext.Current.Get<IExplorerViewModel>(Utils.ViewModelNameKey);
            ScenarioContext.Current.Add(Utils.ViewNameKey, explorerView);
            ScenarioContext.Current.Add(Utils.ViewModelNameKey, explorerViewModel);
            var mainViewModelMock = new Mock<IMainViewModel>();
            ScenarioContext.Current.Add("mainViewModel",mainViewModelMock);
            ScenarioContext.Current.Add("mockShellViewModel", FeatureContext.Current.Get<Mock<IShellViewModel>>("mockShellViewModel"));
            ScenarioContext.Current.Add("mockExplorerRepository", FeatureContext.Current.Get<Mock<IExplorerRepository>>("mockExplorerRepository"));
        }

        [Given(@"the explorer is visible")]
        public void GivenTheExplorerIsVisible()
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            Assert.IsNotNull(explorerView);
            Assert.IsNotNull(explorerView.DataContext);            
        }

        [When(@"I Connected to Remote Server ""(.*)""")]
        public void WhenIConnectedToRemoteServer(string name)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            Assert.IsNotNull(explorerView.DataContext);    
            IExplorerViewModel explorerViewModel = (IExplorerViewModel)explorerView.DataContext;
            var server = new ServerForTesting(new Mock<IExplorerRepository>());
            server.ResourceName = name;
            explorerViewModel.ConnectControlViewModel.Connect(server);            
        }

        [Given(@"I connect to ""(.*)"" server")]
        [When(@"I connect to ""(.*)"" server")]
        [Then(@"I connect to ""(.*)"" server")]
        public void WhenIConnectToServer(string serverName)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            Assert.IsNotNull(explorerView.DataContext);
            CustomContainer.Register<IShellViewModel>(new Mock<IShellViewModel>().Object);
            IExplorerViewModel explorerViewModel = (IExplorerViewModel)explorerView.DataContext;
            var explorerRepository = new Mock<IExplorerRepository>();
            explorerRepository.Setup(repository => repository.CreateFolder(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()));
            explorerRepository.Setup(repository => repository.Rename(It.IsAny<IExplorerItemViewModel>(), It.IsAny<string>())).Returns(true);
            var server = new ServerForTesting(explorerRepository);
            server.ResourceName = serverName;
            explorerViewModel.ConnectControlViewModel.Connect(server);       
            ScenarioContext.Current.Add("mockRemoteExplorerRepository",explorerRepository);
        }

        [When(@"I open the server ""(.*)"" server and the permissions are ""(.*)""")]
        public void WhenIOpenTheServerServerAndThePermissionsAre(string serverName, string permission)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            Assert.IsNotNull(explorerView.DataContext);
            CustomContainer.Register<IShellViewModel>(new Mock<IShellViewModel>().Object);
            IExplorerViewModel explorerViewModel = (IExplorerViewModel)explorerView.DataContext;
            var explorerRepository = new Mock<IExplorerRepository>();
            explorerRepository.Setup(repository => repository.CreateFolder(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()));
            explorerRepository.Setup(repository => repository.Rename(It.IsAny<IExplorerItemViewModel>(), It.IsAny<string>())).Returns(true);
            var server = new ServerForTesting(explorerRepository, new WindowsGroupPermission(){ResourceID = Guid.Empty,IsServer = true,View = permission.ToLower().Contains("view"), Execute = permission.ToLower().Contains("execute"), Administrator = false});
            server.ResourceName = serverName;
            explorerViewModel.ConnectControlViewModel.Connect(server);
            ScenarioContext.Current.Add("mockRemoteExplorerRepository", explorerRepository);
            var environmentViewModel = explorerView.OpenEnvironmentNode(serverName);
        }

        [Then(@"the option to ""(.*)"" is ""(.*)""")]
        public void ThenTheOptionToIs(string servername, string permissions)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            var environmentViewModel = explorerView.OpenEnvironment(servername);
            Assert.IsTrue(environmentViewModel.AsList().Where(a=> a.ResourceType != "Folder").All(a=>a.CanView &&!a.CanEdit && !a.CanExecute));
        }

        [Then(@"the option to ""(.*)"" is ""(.*)"" on server '(.*)'")]
        public void ThenTheOptionToIsOnServer(string permission, string state, string servername)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            var environmentViewModel = explorerView.OpenEnvironment(servername);
            var resources = environmentViewModel.AsList().Where(a => a.ResourceType != "Folder" && a.ResourceType != "DbService" && a.ResourceType != "PluginService" && a.ResourceType != "WebService");
            var resources2 = environmentViewModel.AsList().Where(a => a.ResourceType == "WorkflowService").ToList();
            resources2.Clear();
            if (state.ToLower().Contains("enabled"))
            {
                if (permission.ToLower().Contains("view"))
                    Assert.IsTrue(resources.All(a => a.CanView));
                if (permission.ToLower().Contains("execute"))
                    Assert.IsTrue(resources.Where(a=> a.ResourceType == "WorkflowService").All(a => a.CanExecute));
                if (permission.ToLower().Contains("debug"))
                    Assert.IsTrue(resources.Where(a => a.ResourceType == "WorkflowService").All(a => a.CanExecute));
            }
            if (state.ToLower().Contains("disabled"))
            {
                if (permission.ToLower().Contains("view"))
                    Assert.IsTrue(!resources.Any(a => a.CanView));
                if (permission.ToLower().Contains("execute"))
                    Assert.IsTrue(!resources.Any(a => a.CanExecute));
                if (permission.ToLower().Contains("debug"))
                    Assert.IsTrue(!resources.Any(a => a.CanExecute));
            }
        }

        [Given(@"I open ""(.*)"" server")]
        [When(@"I open ""(.*)"" server")]
        public void WhenIOpenServer(string serverName)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            var environmentViewModel = explorerView.OpenEnvironmentNode(serverName);
            Assert.IsNotNull(environmentViewModel);
            Assert.IsTrue(environmentViewModel.IsExpanded);
        }

        [When(@"""(.*)"" permissions are ""(.*)""")]
        public void WhenPermissionsAre(string serverName, string permission)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            var environmentViewModel = explorerView.OpenEnvironmentNode(serverName);
            Assert.IsNotNull(environmentViewModel);
            Assert.IsTrue(environmentViewModel.IsExpanded);
        }

        [When(@"I select ""(.*)""")]
        public void WhenISelect(string serverName)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            var environmentViewModel = explorerView.OpenEnvironmentNode(serverName);
            Assert.IsNotNull(environmentViewModel);
            Assert.IsTrue(environmentViewModel.IsExpanded);
        }

        [Given(@"I open Resource ""(.*)""")]
        [When(@"I open Resource ""(.*)""")]
        [Then(@"I open Resource ""(.*)""")]
        public void WhenIOpenResource(string folderName)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            var environmentViewModel = explorerView.OpenFolderNode(folderName);
            Assert.IsNotNull(environmentViewModel);
        }

        [Given(@"I open ""(.*)"" in ""(.*)""")]
        [When(@"I open ""(.*)"" in ""(.*)""")]
        public void WhenIOpenIn(string resourceName, string folderName)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            var environmentViewModel = explorerView.OpenItem(resourceName, folderName);
            Assert.IsNotNull(environmentViewModel);            
        }       

        [When(@"""(.*)"" tab is opened")]
        [Then(@"""(.*)"" tab is opened")]
        public void WhenTabIsOpened(string resourceName)
        {
            var mockShellViewModel = ScenarioContext.Current.Get<Mock<IShellViewModel>>("mockShellViewModel");
            mockShellViewModel.Verify();
        }

        [Given(@"I should see ""(.*)"" folders")]
        [When(@"I should see ""(.*)"" folders")]
        [Then(@"I should see ""(.*)"" folders")]
        public void ThenIShouldSeeFolders(int numberOfFoldersVisible)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            var explorerItemViewModels = explorerView.GetFoldersVisible();
            Assert.AreEqual(numberOfFoldersVisible,explorerItemViewModels.Count);
        }

        [Then(@"I should see ""(.*)"" children for ""(.*)""")]
        public void ThenIShouldSeeChildrenFor(int expectedChildrenCount, string folderName)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            var childrenCount = explorerView.GetVisibleChildrenCount(folderName);
            Assert.AreEqual(expectedChildrenCount,childrenCount);
        }

        [When(@"I rename ""(.*)"" to ""(.*)""")]
        public void WhenIRenameTo(string originalFolderName, string newFolderName)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            explorerView.PerformFolderRename(originalFolderName,newFolderName);            
        }

        [Then(@"I should not see ""(.*)""")]
        public void ThenIShouldNotSee(string folderName)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            var environmentViewModel = explorerView.OpenFolderNode(folderName);
            Assert.IsNull(environmentViewModel);
        }

        [Then(@"I should see ""(.*)"" only")]
        public void ThenIShouldSee(string itemName)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            explorerView.VerifyItemExists(itemName);
        }

        [Then(@"I should see ""(.*)"" resources in ""(.*)""")]
        public void ThenIShouldSeeResourcesIn(int numberOfresources, string path)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            var explorerItemViewModels = explorerView.GetResourcesVisible(path);
            Assert.AreEqual(numberOfresources, explorerItemViewModels);
        }

        [When(@"I search for ""(.*)""")]
        public void WhenISearchFor(string searchTerm)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            explorerView.PerformSearch(searchTerm);
        }

        [When(@"I search for ""(.*)"" in explorer")]
        public void WhenISearchForInExplorer(string searchTerm)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            explorerView.PerformSearch(searchTerm);
        }

        [When(@"I clear ""(.*)"" Filter")]
        public void WhenIClearFilter(string p0)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            explorerView.PerformSearch("");
        }

        [Then(@"I should see the path ""(.*)""")]
        public void ThenIShouldSeeThePath(string path)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            explorerView.VerifyItemExists(path);
        }

        [Then(@"the path ""(.*)"" should exist")]
        public void ThenThePathShouldExist(string p0)
        {
           var vm =FeatureContext.Current.Get<IExplorerViewModel>(Utils.ViewModelNameKey);
           var env =p0.Substring(0,p0.IndexOf("\\", StringComparison.Ordinal));
           var environment = vm.Environments.FirstOrDefault(a => a.DisplayName == env);
           Assert.IsTrue( environment != null && environment.AsList().Any(a=>a.ResourcePath==  p0.Substring(p0.IndexOf("\\", StringComparison.Ordinal)+1)));
        }

        [Then(@"I should not see the path ""(.*)""")]
        public void ThenIShouldNotSeeThePath(string path)
        {
            bool found = false;
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey) as ExplorerView;
            try
            {
                // ReSharper disable once PossibleNullReferenceException
                explorerView.ExplorerViewTestClass.VerifyItemExists(path);
            }
            catch(Exception e)
            {
                if (e.Message.Contains("Folder or environment not found. Name"))
                    found = true;
            
            }
           Assert.IsTrue(found);
        }

        [Then(@"I setup (.*) resources in ""(.*)""")]
        public void ThenISetupResourcesIn(int resourceNumber, string path)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            explorerView.AddResources(resourceNumber, path,"WorkflowService","Resource");
        }

        [When(@"I Add  ""(.*)"" ""(.*)"" to be returned for ""(.*)""")]
        public void WhenIAddToBeReturnedFor(int count, string type, string path)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            explorerView.AddResources(count, path, type, "Resource");
        }

        [When(@"I Setup a resource  ""(.*)"" ""(.*)"" to be returned for ""(.*)"" called ""(.*)""")]
        public void WhenISetupAResourceToBeReturnedForCalled(int count, string type, string path, string name)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            explorerView.AddResources(count, path, type,name);
        }

        [Then(@"""(.*)"" Context menu  should be ""(.*)"" for ""(.*)""")]
        public void ThenContextMenuShouldBeFor(string option, string visibility, string path)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey) as ExplorerView;
            // ReSharper disable PossibleNullReferenceException
            explorerView.ExplorerViewTestClass.VerifyContextMenu(option, visibility, path);
            // ReSharper restore PossibleNullReferenceException
        }

        [Then(@"I Create ""(.*)"" resources of Type ""(.*)"" in ""(.*)""")]
        public void ThenICreateResourcesOfTypeIn(int resourceNumber, string type, string path)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            explorerView.AddResources(resourceNumber, path, type, "Resource");
        }

        [When(@"I Show Version History for ""(.*)""")]
        public void WhenIShowVersionHistoryFor(string path)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey) as ExplorerView;
            if(explorerView != null)
            {
                explorerView.ExplorerViewTestClass.ShowVersionHistory(path);
            }
        }

        [Then(@"I should see ""(.*)"" versions with ""(.*)"" Icons in ""(.*)""")]
        public void ThenIShouldSeeVersionsWithIconsIn(int count, string iconVisible, string path)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey) as ExplorerView;
            if(explorerView != null)
            {
                var node = explorerView.ExplorerViewTestClass.VerifyItemExists(path);
                Assert.AreEqual(count,node.Nodes.Count);
                foreach(var node1 in node.Nodes)
                {
                    var itm = node1.Data as ExplorerItemViewModel;
                    // ReSharper disable PossibleNullReferenceException
                    Assert.IsFalse(itm.CanExecute);
                    Assert.AreEqual(itm.ResourceType,"Version");
                    Assert.IsFalse(itm.CanEdit);
                    // ReSharper restore PossibleNullReferenceException
                }
            }
        }

        [When(@"I Make ""(.*)"" the current version of ""(.*)""")]
        public void WhenIMakeTheCurrentVersionOf(string versionPath, string resourcePath)
        {
            var mockRepo = ScenarioContext.Current.Get<Mock<IExplorerRepository>>("mockExplorerRepository");
            if (versionPath.Contains("Remote Connection Integration"))
            {
                mockRepo = ScenarioContext.Current.Get<Mock<IExplorerRepository>>("mockRemoteExplorerRepository");
            }
            // ReSharper disable once MaximumChainedReferences
            mockRepo.Setup(a => a.Rollback(Guid.Empty, "1")).Returns(new RollbackResult
             {
                    DisplayName = "Resource 1" , 
                     VersionHistory = new List<IExplorerItem>()
                 }
             );
            // ReSharper disable once MaximumChainedReferences
            mockRepo.Setup(a => a.GetVersions(It.IsAny<Guid>())).Returns(new List<IVersionInfo>
             {
                     new VersionInfo(DateTime.Now,"bob","Leon","4",Guid.Empty,Guid.Empty),
                    new VersionInfo(DateTime.Now,"bob","Leon","3",Guid.Empty,Guid.Empty),
                    new VersionInfo(DateTime.Now,"bob","Leon","2",Guid.Empty,Guid.Empty),
                    new VersionInfo(DateTime.Now,"bob","Leon","1",Guid.Empty,Guid.Empty)
                });
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey) as ExplorerView;
            if(explorerView != null)
            {
                var tester = explorerView.ExplorerViewTestClass;
                tester.PerformVersionRollback(versionPath);
            }
        }

        [When(@"I Delete Version ""(.*)""")]
        public void WhenIDeleteVersion(string versionPath)
        {
            var popup = FeatureContext.Current.Get<Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>>("popupController");
            popup.Setup(a => a.ShowDeleteVersionMessage(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockRepo = ScenarioContext.Current.Get<Mock<IExplorerRepository>>("mockExplorerRepository");
            if (versionPath.Contains("Remote Connection Integration"))
            {
                mockRepo = ScenarioContext.Current.Get<Mock<IExplorerRepository>>("mockRemoteExplorerRepository");
            }
            mockRepo.Setup(a => a.Delete(It.IsAny<IExplorerItemViewModel>()));
            // ReSharper disable once MaximumChainedReferences
            // ReSharper disable once MaximumChainedReferences
            mockRepo.Setup(a => a.GetVersions(It.IsAny<Guid>())).Returns(new List<IVersionInfo>
             {
                    new VersionInfo(DateTime.Now,"bob","Leon","3",Guid.Empty,Guid.Empty),
                    new VersionInfo(DateTime.Now,"bob","Leon","2",Guid.Empty,Guid.Empty),
                    new VersionInfo(DateTime.Now,"bob","Leon","1",Guid.Empty,Guid.Empty)
              });

            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey) as ExplorerView;
            if (explorerView != null)
            {
                var tester = explorerView.ExplorerViewTestClass;
                tester.PerformVersionDelete(versionPath);
            }    
        }

        [Given(@"I Setup  ""(.*)"" Versions to be returned for ""(.*)""")]
        [When(@"I Setup  ""(.*)"" Versions to be returned for ""(.*)""")]
        [Then(@"I Setup  ""(.*)"" Versions to be returned for ""(.*)""")]
        public void ThenISetupVersionsToBeReturnedFor(int count, string path)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey) as ExplorerView;
            var mockRepo = ScenarioContext.Current.Get<Mock<IExplorerRepository>>("mockExplorerRepository");
            if (path.Contains("Remote Connection Integration"))
            {
                mockRepo = ScenarioContext.Current.Get<Mock<IExplorerRepository>>("mockRemoteExplorerRepository");
            }
            if (explorerView != null)
            {
                var tester = explorerView.ExplorerViewTestClass;
                tester.CreateChildNodes(count, path);
                // ReSharper disable MaximumChainedReferences
                mockRepo.Setup(a => a.GetVersions(It.IsAny<Guid>())).Returns(new List<IVersionInfo>
                {
                    new VersionInfo(DateTime.Now,"bob","Leon","3",Guid.Empty,Guid.Empty),
                    new VersionInfo(DateTime.Now,"bob","Leon","2",Guid.Empty,Guid.Empty),
                    new VersionInfo(DateTime.Now,"bob","Leon","1",Guid.Empty,Guid.Empty)
                });                
            }
            ScenarioContext.Current.Add("versions", count);
        }

        [When(@"I delete ""(.*)""")]
        public void WhenIDelete(string path)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            var mainViewModelMock = ScenarioContext.Current.Get<Mock<IMainViewModel>>("mainViewModel");
            var repo = FeatureContext.Current.Get<Mock<IExplorerRepository>>("mockExplorerRepository");
            repo.Setup(a => a.Delete(It.IsAny<IExplorerItemViewModel>())).Returns(new DeletedFileMetadata() { IsDeleted = true });
            if (ScenarioContext.Current.ContainsKey("popupResult"))
            {
                var popupResult = ScenarioContext.Current.Get<string>("popupResult");
                if (popupResult.ToLower() == "cancel")
                {
                    mainViewModelMock.Setup(model => model.ShowDeleteDialogForFolder(It.IsAny<string>())).Returns(false);
                }
                else
                    // ReSharper disable once MaximumChainedReferences
                    mainViewModelMock.Setup(model => model.ShowDeleteDialogForFolder(It.IsAny<string>())).Returns(true);
            }
            else
            {
                mainViewModelMock.Setup(model => model.ShowDeleteDialogForFolder(It.IsAny<string>())).Returns(true);
            }
            CustomContainer.DeRegister<IMainViewModel>();
            CustomContainer.Register(mainViewModelMock.Object);
            explorerView.DeletePath(path, new Mock<IEnvironmentModel>().Object);
        }

        [Then(@"I choose to ""(.*)"" Any Popup Messages")]
        public void ThenIChooseToAnyPopupMessages(string result)
        {
            if(result.ToLower()=="ok")
            {
                var popup = FeatureContext.Current.Get<Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>>("popupController");
                popup.Setup(a => a.Show(It.IsAny<IPopupMessage>())).Returns(MessageBoxResult.Yes);
            }
            if (!ScenarioContext.Current.ContainsKey("popupResult")) 
                    ScenarioContext.Current.Add("popupResult",result);
            else
            {
                ScenarioContext.Current["popupResult"] = result;
            }
        }

        [When(@"I create ""(.*)""")]
        [Then(@"I create ""(.*)""")]
        public void WhenICreate(string path)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            explorerView.AddNewFolderFromPath(path);
        }

        [When(@"I add ""(.*)"" in ""(.*)""")]
        public void WhenIAddIn(string folder , string server)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            explorerView.AddNewFolder(server, folder);
        }

        [Given(@"I change path ""(.*)"" to ""(.*)""")]
        [When(@"I change path ""(.*)"" to ""(.*)""")]
        [Then(@"I move ""(.*)"" to ""(.*)""")]
        public void WhenIChangePathTo(string originalPath, string destinationPath)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            explorerView.Move(originalPath, destinationPath);
        }


        [Given(@"I create the ""(.*)"" of type ""(.*)""")]
        [When(@"I create the ""(.*)"" of type ""(.*)""")]
        [Then(@"I create the ""(.*)"" of type ""(.*)""")]
        public void WhenICreateTheOfType(string path, string type)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            explorerView.AddNewResource(path, type);
        }

        [AfterScenario("Explorer")]
        public void AfterScenario()
        {
            var mockExplorerRepository = new Mock<IExplorerRepository>();
            mockExplorerRepository.Setup(repository => repository.CreateFolder(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()));
            mockExplorerRepository.Setup(repository => repository.Rename(It.IsAny<IExplorerItemViewModel>(), It.IsAny<string>())).Returns(true);
            var mockEventAggregator = new Mock<IEventAggregator>();
            mockEventAggregator.Setup(aggregator => aggregator.GetEvent<ServerAddedEvent>()).Returns(new ServerAddedEvent());
            var mockShellViewModel = ScenarioContext.Current.Get<Mock<IShellViewModel>>("mockShellViewModel");
            var explorerViewModel = new ExplorerViewModel(mockShellViewModel.Object, mockEventAggregator.Object);
            var view = ScenarioContext.Current.Get<IExplorerView>(Utils.ViewNameKey);
            try
            {
                view.DataContext = explorerViewModel;
            }
            catch(Exception e)
            {
                view.DataContext = null;
                view.DataContext = explorerViewModel;
            }
            ScenarioContext.Current.Remove(Utils.ViewModelNameKey);
            ScenarioContext.Current.Add(Utils.ViewModelNameKey, explorerViewModel);
        }

        [Then(@"I Setup  ""(.*)"" resources of Type ""(.*)"" in ""(.*)""")]
        public void ThenISetupResourcesOfTypeIn(int count, string path, string type)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [When(@"I delete ""(.*)"" in ""(.*)"" server")]
        public void WhenIDeleteInServer(string p0, string p1)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Then(@"Conflict error message occurs")]
        public void ThenConflictErrorMessageOccurs()
        {
            var mockShellViewModel = ScenarioContext.Current.Get<Mock<IShellViewModel>>("mockShellViewModel");
            var popupController = new PopupController();
            mockShellViewModel.Object.ShowPopup(popupController.GetDuplicateMessage("Workflow1"));
        }

        [Then(@"""(.*)"" is Disconnected")]
        public void ThenIsDisconnected(string p0)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Then(@"""(.*)"" is visible")]
        public void ThenIsVisible(string p0)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Then(@"""(.*)"" is executed")]
        public void ThenIsExecuted(string p0)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Then(@"I select ""(.*)""")]
        public void ThenISelect(string p0)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [When(@"I drag ""(.*)"" onto the design surface")]
        public void WhenIDragOntoTheDesignSurface(string p0)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Then(@"""(.*)"" permissions are ""(.*)""")]
        public void ThenPermissionsAre(string p0, string p1)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [When(@"the ""(.*)"" window is opened")]
        public void WhenTheWindowIsOpened(string p0)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Then(@"""(.*)"" is Collapsed")]
        public void ThenIsCollapsed(string p0)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [When(@"resource shows that it cannot be placed in the design surface")]
        public void WhenResourceShowsThatItCannotBePlacedInTheDesignSurface()
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Then(@"""(.*)"" is not visible on design surface")]
        public void ThenIsNotVisibleOnDesignSurface(string p0)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Given(@"I have a workflow ""(.*)""")]
        public void GivenIHaveAWorkflow(string p0)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Then(@"the ""(.*)"" tab is opened")]
        public void ThenTheTabIsOpened(string p0)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Then(@"""(.*)"" is checked by default")]
        public void ThenIsCheckedByDefault(string p0)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Then(@"Nesting Levels equals ""(.*)"" equals ""(.*)""")]
        public void ThenNestingLevelsEqualsEquals(int p0, string p1)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Then(@"""(.*)"" is enabled")]
        public void ThenIsEnabled(string p0)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Then(@"""(.*)"" has no dependancies")]
        public void ThenHasNoDependancies(string p0)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Given(@"I select ""(.*)""")]
        public void GivenISelect(string p0)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Then(@"Nothing depends on ""(.*)""")]
        public void ThenNothingDependsOn(string p0)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Then(@"""(.*)"" is shown as the first level of dependancy")]
        public void ThenIsShownAsTheFirstLevelOfDependancy(string p0)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Then(@"""(.*)"" is shown as the second level of dependancy")]
        public void ThenIsShownAsTheSecondLevelOfDependancy(string p0)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [When(@"Nesting Levels ""(.*)"" equals ""(.*)"" only")]
        public void WhenNestingLevelsEqualsOnly(int p0, string p1)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Then(@"""(.*)"" is visible in the Explorer")]
        public void ThenIsVisibleInTheExplorer(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"""(.*)"" is invisible")]
        public void ThenIsInvisible(string p0)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [When(@"Nesting Levels equals ""(.*)"" equals ""(.*)""")]
        [Then(@"Nesting Levels ""(.*)"" equals ""(.*)""")]
        public void ThenNestingLevelsEquals(int p0, string p1)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [When(@"I double click ""(.*)""")]
        public void WhenIDoubleClick(string p0)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [When(@"I click Refresh")]
        public void WhenIClickRefresh()
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [When(@"I Show Dependencies of ""(.*)""")]
        public void WhenIShowDependenciesOf(string p0)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Then(@"the Debug output window is populated")]
        public void ThenTheDebugOutputWindowIsPopulated()
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Then(@"there are No errors")]
        public void ThenThereAreNoErrors()
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Then(@"""(.*)"" is merged with ""(.*)""")]
        public void ThenIsMergedWith(string p0, string p1)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

    }
}
// ReSharper restore UnusedMember.Global