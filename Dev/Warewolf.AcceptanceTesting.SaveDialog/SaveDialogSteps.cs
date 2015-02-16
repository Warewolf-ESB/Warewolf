using System;
using System.Windows;
using System.Windows.Threading;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.SaveDialog;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Warewolf.Studio.Views;

namespace Warewolf.AcceptanceTesting.SaveDialog
{
    [Binding]
    public class SaveDialogSteps
    {
        [BeforeFeature("SaveDialog")]
        public static void SetupSaveDialogDependencies()
        {
            var bootstrapper = new UnityBootstrapperForSaveDialogTesting();
            bootstrapper.Run();
            FeatureContext.Current.Add("container", bootstrapper.Container);
            var view = bootstrapper.Container.Resolve<IRequestServiceNameView>();
            var window = new Window {Content = view};
            var app = Application.Current;
            app.MainWindow = window;
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                var viewWindow = (RequestServiceNameView)Application.Current.MainWindow.Content;
                Assert.IsNotNull(viewWindow);
                Assert.IsNotNull(viewWindow.DataContext);
                Assert.IsInstanceOfType(viewWindow.DataContext, typeof(IRequestServiceNameViewModel));
                Application.Current.Shutdown();
            }));

            Application.Current.Run(Application.Current.MainWindow);
        }


        [BeforeScenario("SaveDialog")]
        public void SetupForSave()
        {
            var container = FeatureContext.Current.Get<IUnityContainer>("container");
            var view = container.Resolve<IRequestServiceNameView>();
            ScenarioContext.Current.Add("saveView", view);
            ScenarioContext.Current.Add("explorerView",view.GetExplorerView());

        }

        [Given(@"the Save Dialog is opened")]
        public void GivenTheSaveDialogIsOpened()
        {
            IRequestServiceNameView saveView;
            var gotView = ScenarioContext.Current.TryGetValue("saveView", out saveView);
            Assert.IsTrue(gotView);
            Assert.IsNotNull(saveView);
        }

        [Given(@"the ""(.*)"" server is visible in save dialog")]
        public void GivenTheServerIsVisibleInSaveDialog(string serverName)
        {
            IRequestServiceNameView saveView;
            var gotView = ScenarioContext.Current.TryGetValue("saveView", out saveView);
            Assert.IsTrue(gotView);
            saveView.HasServer(serverName);
        }

        [Given(@"I should see ""(.*)"" folders")]
        [When(@"I should see ""(.*)"" folders")]
        [Then(@"I should see ""(.*)"" folders")]
        public void GivenIShouldSeeFolders(int numberOfFoldersVisible)
        {
            IRequestServiceNameView saveView;
            ScenarioContext.Current.TryGetValue("saveView", out saveView);
            Assert.IsNotNull(saveView);
            var explorerItemViewModels = saveView.GetFoldersVisible();
            Assert.AreEqual(numberOfFoldersVisible, explorerItemViewModels.Count);
        }

        [Given(@"I should see ""(.*)"" folders in ""(.*)"" save dialog")]
        public void GivenIShouldSeeFoldersInSaveDialog(int p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"I have an ""(.*)"" workflow open")]
        public void GivenIHaveAnWorkflowOpen(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"I have an New workflow ""(.*)"" is open")]
        public void GivenIHaveAnNewWorkflowIsOpen(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"I open ""(.*)"" in save dialog")]
        [When(@"I open ""(.*)"" in save dialog")]
        [Then(@"I open ""(.*)"" in save dialog")]
        public void WhenIOpenInSaveDialog(string folderName)
        {
            IRequestServiceNameView saveView;
            ScenarioContext.Current.TryGetValue("saveView", out saveView);
            Assert.IsNotNull(saveView);
            saveView.OpenFolder(folderName);

        }

        //[When(@"I create ""(.*)"" in ""(.*)""")]
        //public void WhenICreateIn(string p0, string p1)
        //{
        //    ScenarioContext.Current.Pending();
        //}
        [When(@"I create ""(.*)"" in ""(.*)""")]
        public void WhenICreateIn(string newFolderName, string rootPath)
        {
            IRequestServiceNameView saveView;
            ScenarioContext.Current.TryGetValue("saveView", out saveView);
            Assert.IsNotNull(saveView);

            saveView.CreateNewFolder(newFolderName, rootPath);


        }

        [When(@"I should see ""(.*)"" folders in ""(.*)"" save dialog")]
        public void WhenIShouldSeeFoldersInSaveDialog(int p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I right click on ""(.*)""")]
        public void WhenIRightClickOn(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I save ""(.*)"" in ""(.*)""")]
        public void WhenISaveIn(string p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I enter name ""(.*)""")]
        public void WhenIEnterName(string serviceName)
        {
            IRequestServiceNameView saveView;
            ScenarioContext.Current.TryGetValue("saveView", out saveView);
            IUnityContainer container;
            FeatureContext.Current.TryGetValue("container", out container);
            Assert.IsNotNull(saveView);
            var requestServiceNameViewModel = container.Resolve<IRequestServiceNameViewModel>();
            Assert.IsNotNull(requestServiceNameViewModel);
            saveView.EnterName(serviceName);
            Assert.AreEqual(serviceName,requestServiceNameViewModel.Name);
        }


        [When(@"I cancel the save dialog")]
        public void WhenICancelTheSaveDialog()
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"the Save Dialog is opened")]
        public void WhenTheSaveDialogIsOpened()
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"I select ""(.*)""")]
        public void WhenISelectSaveDialog(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I save ""(.*)""")]
        public void WhenISave(string p0)
        {
            
        }

        
        [When(@"click ""(.*)""")]
        public void WhenClick(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I edit ""(.*)""")]
        public void WhenIEdit(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I should see ""(.*)"" in ""(.*)""")]
        public void ThenIShouldSeeIn(string p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }


        [Then(@"I shouldn't see ""(.*)""")]
        public void ThenIShouldnTSee(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"""(.*)"" is visible in ""(.*)""")]
        public void ThenIsVisibleIn(string p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"save button is ""(.*)""")]
        public void ThenSaveButtonIs(string enabledString)
        {
            var enabled = enabledString == "Enabled";
            IRequestServiceNameView saveView;
            ScenarioContext.Current.TryGetValue("saveView", out saveView);
            Assert.IsNotNull(saveView);
            var isSaveButtonEnabled = saveView.IsSaveButtonEnabled();
            Assert.AreEqual(enabled,isSaveButtonEnabled);
        }

        [Then(@"validation message is ""(.*)""")]
        public void ThenValidationMessageIs(string expectedValidationMessage)
        {
            IRequestServiceNameView saveView;
            ScenarioContext.Current.TryGetValue("saveView", out saveView);
            Assert.IsNotNull(saveView);
            var currentValidationMessage = saveView.GetValidationMessage();
            Assert.AreEqual(expectedValidationMessage,currentValidationMessage);
        }

        [Then(@"validation message is thrown ""(.*)""")]
        public void ThenValidationMessageIsThrown(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"save dilog localhost is refreshed ""(.*)""")]
        public void ThenSaveDilogLocalhostIsRefreshed(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"the ""(.*)"" workflow is saved ""(.*)""")]
        public void ThenTheWorkflowIsSaved(string p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"cancel button is ""(.*)""")]
        public void ThenCancelButtonIs(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"the save dialog is closed")]
        public void ThenTheSaveDialogIsClosed()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"the Save Dialog is opened")]
        public void ThenTheSaveDialogIsOpened()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"the ""(.*)"" server is visible in save dialog")]
        public void ThenTheServerIsVisibleInSaveDialog(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I should see ""(.*)"" folders in ""(.*)"" save dialog")]
        public void ThenIShouldSeeFoldersInSaveDialog(int p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"save textbox  name is ""(.*)""")]
        public void ThenSaveTextboxNameIs(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"the path in the title as ""(.*)""")]
        public void ThenThePathInTheTitleAs(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"the New workflow ""(.*)"" is open with Star notation")]
        public void ThenTheNewWorkflowIsOpenWithStarNotation(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"the New workflow ""(.*)"" is open without Star notation")]
        public void ThenTheNewWorkflowIsOpenWithoutStarNotation(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I have an ""(.*)"" workflow open with Star notation")]
        public void ThenIHaveAnWorkflowOpenWithStarNotation(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I have an ""(.*)"" workflow without Star notation")]
        public void ThenIHaveAnWorkflowWithoutStarNotation(string p0)
        {
            ScenarioContext.Current.Pending();
        }
    }
}
