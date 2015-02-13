using System;
using System.Windows;
using System.Windows.Threading;
using Dev2.Common.Interfaces.PopupController;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.Studio.Core.View_Interfaces;
using Warewolf.Studio.Views;

namespace Warewolf.AcceptanceTesting.Explorer
{
    
    [Binding]    
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

        //[When(@"I create ""(.*)"" in ""(.*)""")]
        //public void WhenICreateIn(string folder, string server)
        //{
        //    var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView");
        //    explorerView.AddNewFolder(folder,server);
        //}

        //[Then(@"I should see ""(.*)"" in ""(.*)"" server")]
        //public void ThenIShouldSeeInServer(string folder, string server)
        //{
        //    //var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView");
        //    //explorerView.VerifyItemExists(folder, server);
     
        //}

        [Then(@"I should see the path ""(.*)""")]
        public void ThenIShouldSeeThePath(string path)
        {
            var explorerView = ScenarioContext.Current.Get<IExplorerView>("explorerView");
            explorerView.VerifyItemExists(path);
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
            ScenarioContext.Current.Add("popupResult",result);
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


    }
}
