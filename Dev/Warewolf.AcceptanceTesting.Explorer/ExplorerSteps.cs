using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.PopupController;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Explorer;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.AcceptanceTesting.Core;
using Warewolf.Studio.Core.Popup;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;

namespace Warewolf.AcceptanceTesting.Explorer
{
    
    [Binding]    
    // ReSharper disable UnusedMember.Global
    public class ExplorerSteps
      
    {
        [BeforeFeature("Explorer")]
        public static void SetupExplorerDependencies()
        {
            var bootstrapper = new UnityBootstrapperForExplorerTesting();

            bootstrapper.Run();
            FeatureContext.Current.Add("container", bootstrapper.Container);
            FeatureContext.Current.Add("bootstrapper", bootstrapper);
            var explorerView = bootstrapper.Container.Resolve<IExplorerView>();
            var window = new Window { Content = explorerView };
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

        [When(@"I Connected to Remote Server ""(.*)""")]
        public void WhenIConnectedToRemoteServer(string name)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView");
            Assert.IsNotNull(explorerView.DataContext);    
            IExplorerViewModel explorerViewModel = (IExplorerViewModel)explorerView.DataContext;
            var server = new ServerForTesting(new Mock<IExplorerRepository>());
            server.ResourceName = name;
            explorerViewModel.ConnectControlViewModel.Connect(server);

            
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

        [Given(@"I should see ""(.*)"" folders")]
        [When(@"I should see ""(.*)"" folders")]
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

        [Then(@"Conflict error message is occurs")]
        public void ThenConflictErrorMessageIsOccurs()
        {
            var boot = FeatureContext.Current.Get<UnityBootstrapperForExplorerTesting>("bootstrapper");
            boot.PopupController.Verify(a => a.Show(It.IsAny<PopupMessage>()));

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




        [Then(@"I should see ""(.*)"" resources in ""(.*)""")]
        public void ThenIShouldSeeResourcesIn(int numberOfresources, string path)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView");
            var explorerItemViewModels = explorerView.GetResourcesVisible(path);
            Assert.AreEqual(numberOfresources, explorerItemViewModels);
        }


        [When(@"I search for ""(.*)""")]
        public void WhenISearchFor(string searchTerm)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView");
            explorerView.PerformSearch(searchTerm);
        }

        [When(@"I search for ""(.*)"" in explorer")]
        public void WhenISearchForInExplorer(string searchTerm)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView");
            explorerView.PerformSearch(searchTerm);
        }

        [When(@"I clear ""(.*)"" Filter")]
        public void WhenIClearFilter(string p0)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView");
            explorerView.PerformSearch("");
        }

        [Then(@"I should see the path ""(.*)""")]
        public void ThenIShouldSeeThePath(string path)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView");
            explorerView.VerifyItemExists(path);
        }
        [Then(@"I should not see the path ""(.*)""")]
        public void ThenIShouldNotSeeThePath(string path)
        {
            bool found = false;
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView") as ExplorerView;
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
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView");
            explorerView.AddResources(resourceNumber, path,"WorkflowService","Resource");
        }

        [When(@"I Add  ""(.*)"" ""(.*)"" to be returned for ""(.*)""")]
        public void WhenIAddToBeReturnedFor(int count, string type, string path)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView");
            explorerView.AddResources(count, path, type, "Resource");
        }

        [When(@"I Setup a resource  ""(.*)"" ""(.*)"" to be returned for ""(.*)"" called ""(.*)""")]
        public void WhenISetupAResourceToBeReturnedForCalled(int count, string type, string path, string name)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView");
            explorerView.AddResources(count, path, type,name);
        }



        [Then(@"""(.*)"" Context menu  should be ""(.*)"" for ""(.*)""")]
        public void ThenContextMenuShouldBeFor(string option, string visibility, string path)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView") as ExplorerView;
            // ReSharper disable PossibleNullReferenceException
            explorerView.ExplorerViewTestClass.VerifyContextMenu(option, visibility, path);
            // ReSharper restore PossibleNullReferenceException
        }


        [Then(@"I Create ""(.*)"" resources of Type ""(.*)"" in ""(.*)""")]
        public void ThenICreateResourcesOfTypeIn(int resourceNumber, string type, string path)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView");
            explorerView.AddResources(resourceNumber, path, type, "Resource");
        }

        [When(@"I Show Version History for ""(.*)""")]
        public void WhenIShowVersionHistoryFor(string path)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView") as ExplorerView;
            var boot = FeatureContext.Current.Get<UnityBootstrapperForExplorerTesting>("bootstrapper");
            
          
            if(explorerView != null)
            {
                // ReSharper disable MaximumChainedReferences
                boot.ExplorerRepository.Setup(a => a.GetVersions(It.IsAny<Guid>())).Returns(new List<IVersionInfo>
                {
                    new VersionInfo(DateTime.Now,"bob","Leon","3",Guid.Empty,Guid.Empty),
                    new VersionInfo(DateTime.Now,"bob","Leon","2",Guid.Empty,Guid.Empty),
                    new VersionInfo(DateTime.Now,"bob","Leon","1",Guid.Empty,Guid.Empty)
                });
                // ReSharper restore MaximumChainedReferences
                explorerView.ExplorerViewTestClass.ShowVersionHistory(path);
            }
        }


        [Then(@"I should see ""(.*)"" versions with ""(.*)"" Icons in ""(.*)""")]
        public void ThenIShouldSeeVersionsWithIconsIn(int count, string iconVisible, string path)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView") as ExplorerView;
            if(explorerView != null)
            {
                var node = explorerView.ExplorerViewTestClass.VerifyItemExists(path);
                Assert.AreEqual(node.Nodes.Count,count);
                foreach(var node1 in node.Nodes)
                {
                    var itm = node1.Data as ExplorerItemViewModel;
                    // ReSharper disable PossibleNullReferenceException
                    Assert.IsFalse(itm.CanExecute);
                    Assert.AreEqual(itm.ResourceType,ResourceType.Version);
                    Assert.IsFalse(itm.CanEdit);
                    // ReSharper restore PossibleNullReferenceException
                }
            }
        }
        [When(@"I Make ""(.*)"" the current version of ""(.*)""")]
        public void WhenIMakeTheCurrentVersionOf(string versionPath, string resourcePath)
        {
             var boot = FeatureContext.Current.Get<UnityBootstrapperForExplorerTesting>("bootstrapper");
            // ReSharper disable once MaximumChainedReferences
             boot.ExplorerRepository.Setup(a => a.Rollback(Guid.Empty, "1")).Returns(new RollbackResult
             {
                    DisplayName = "Resource 1" , 
                     VersionHistory = new List<IExplorerItem>()
                 }
             );
            // ReSharper disable once MaximumChainedReferences
             boot.ExplorerRepository.Setup(a => a.GetVersions(It.IsAny<Guid>())).Returns(new List<IVersionInfo>
             {
                     new VersionInfo(DateTime.Now,"bob","Leon","4",Guid.Empty,Guid.Empty),
                    new VersionInfo(DateTime.Now,"bob","Leon","3",Guid.Empty,Guid.Empty),
                    new VersionInfo(DateTime.Now,"bob","Leon","2",Guid.Empty,Guid.Empty),
                    new VersionInfo(DateTime.Now,"bob","Leon","1",Guid.Empty,Guid.Empty)
                });
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView") as ExplorerView;
            if(explorerView != null)
            {
                var tester = explorerView.ExplorerViewTestClass;
                tester.PerformVersionRollback(versionPath);
            }
        }


        [When(@"I Delete Version ""(.*)""")]
        public void WhenIDeleteVersion(string versionPath)
        {
            var boot = FeatureContext.Current.Get<UnityBootstrapperForExplorerTesting>("bootstrapper");
            boot.ExplorerRepository.Setup(a => a.Delete(It.IsAny<IExplorerItemViewModel>()));
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView") as ExplorerView;
            if (explorerView != null)
            {
                var tester = explorerView.ExplorerViewTestClass;
                tester.PerformVersionDelete(versionPath);
            }

    
        }



        [Then(@"I Setup  ""(.*)"" Versions to be returned for ""(.*)""")]
        public void ThenISetupVersionsToBeReturnedFor(int count, string path)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView") as ExplorerView;
            // ReSharper disable once PossibleNullReferenceException
            var tester = explorerView.ExplorerViewTestClass;
            tester.CreateChildNodes(count, path);
            ScenarioContext.Current.Add("versions", count);
        }


        [Then(@"I Setup  ""(.*)"" resources of Type ""(.*)"" in ""(.*)""")]
        public void ThenISetupResourcesOfTypeIn(int count, string path, string type)
        {
            ScenarioContext.Current.Pending();
        }



        [When(@"I delete ""(.*)"" in ""(.*)"" server")]
        public void WhenIDeleteInServer(string p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I delete ""(.*)""")]
        public void WhenIDelete(string path)
        {
            var boot = FeatureContext.Current.Get<UnityBootstrapperForExplorerTesting>("bootstrapper");
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView");
            if(ScenarioContext.Current.ContainsKey("popupResult"))
            {
                var popupResult = ScenarioContext.Current.Get<string>("popupResult");
                if(popupResult.ToLower()=="cancel")
                {
                    // ReSharper disable once MaximumChainedReferences
                    boot.PopupController.Setup(a => a.Show(It.IsAny<IPopupMessage>())).Returns(MessageBoxResult.Cancel);
                }
                else
                    // ReSharper disable once MaximumChainedReferences
                    boot.PopupController.Setup(a => a.Show(It.IsAny<IPopupMessage>())).Returns(MessageBoxResult.OK);
            }
            else
                // ReSharper disable once MaximumChainedReferences
                boot.PopupController.Setup(a => a.Show(It.IsAny<IPopupMessage>())).Returns(MessageBoxResult.OK);
           
            
            
            // ReSharper disable MaximumChainedReferences
      
            // ReSharper restore MaximumChainedReferences
            explorerView.DeletePath(path);
        }

        [Then(@"I choose to ""(.*)"" Any Popup Messages")]
        public void ThenIChooseToAnyPopupMessages(string result)
        {
            if (!ScenarioContext.Current.ContainsKey("popupResult")) 
                    ScenarioContext.Current.Add("popupResult",result);
            else
            {
                ScenarioContext.Current["popupResult"] = result;
            }
        }

        [When(@"I create ""(.*)""")]
        public void WhenICreate(string path)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView");
            explorerView.AddNewFolderFromPath(path);
        }
        [When(@"I add ""(.*)"" in ""(.*)""")]
        public void WhenIAddIn(string folder , string server)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView");
            explorerView.AddNewFolder(server, folder);
        }

        [When(@"I create the ""(.*)"" of type ""(.*)""")]
        public void WhenICreateTheOfType(string path, string type)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView");
            explorerView.AddNewResource(path, type);
        }

        [AfterScenario]
        public void AfterScenario()
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView") as ExplorerView;
            // ReSharper disable once PossibleNullReferenceException
            var tester = explorerView.ExplorerViewTestClass;
            tester.Reset();
        }

    }
}
// ReSharper restore UnusedMember.Global