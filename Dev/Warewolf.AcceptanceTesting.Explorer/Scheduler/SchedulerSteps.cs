using System;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;

namespace Warewolf.AcceptanceTesting.Explorer.Scheduler
{
    [Binding]
    public class SchedulerSteps
    {
        [BeforeFeature("Scheduler")]
        public static void SetupForFeature()
        {
            var bootStrapper = new UnityBootstrapperForDatabaseSourceConnectorTesting();
            bootStrapper.Run();
            var view = new Mock<ISchedulerView>();
            var viewModel = new Mock<ISchedulerViewModel>();
            view.Object.DataContext = viewModel;
            Utils.ShowTheViewForTesting(view.Object);
            FeatureContext.Current.Add(Utils.ViewNameKey, view.Object);
            FeatureContext.Current.Add(Utils.ViewModelNameKey, viewModel.Object);
        }

        [BeforeScenario("Scheduler")]
        public void SetupForScenerio()
        {
            ScenarioContext.Current.Add(Utils.ViewNameKey, FeatureContext.Current.Get<ISchedulerView>(Utils.ViewNameKey));
            ScenarioContext.Current.Add(Utils.ViewModelNameKey, FeatureContext.Current.Get<ISchedulerViewModel>(Utils.ViewModelNameKey));
        }

        [Given(@"I have Scheduler tab opened")]
        public void GivenIHaveSchedulerTabOpened()
        {
            var view = Utils.GetView<ISchedulerView>();
            Assert.IsNotNull(view);
        }

        [Given(@"selected server is ""(.*)""")]
        public void GivenSelectedServerIs(string serverName)
        {
            var view = Utils.GetView<ISchedulerView>();
            var selectedServerName = view.GetSelectedServerName();
            Assert.AreEqual(serverName,selectedServerName);
        }

        [Given(@"the saved tasks are")]
        public void GivenTheSavedTasksAre(Table table)
        {
            var viewModel = Utils.GetViewModel<ISchedulerViewModel>();
            var rows = table.Rows;
            foreach(var tableRow in rows)
            {
                viewModel.AddNewSchedule(tableRow["Name"], tableRow["Status"]);
            }
        }

        [Given(@"task settings are")]
        [Then(@"task settings are")]
        public void GivenTaskSettingsAre(Table table)
        {
            var view = Utils.GetView<ISchedulerView>();
            view.GetCurrentSchedule();
        }

        [Given(@"username is as ""(.*)""")]
        public void GivenUsernameIsAs(string userName)
        {
            var view = Utils.GetView<ISchedulerView>();
            view.EnterUsername(userName);
        }

        [Given(@"Password is as ""(.*)""")]
        public void GivenPasswordIsAs(string password)
        {
            var view = Utils.GetView<ISchedulerView>();
            view.EnterPassword(password);
        }

        [Given(@"""(.*)"" task is selected")]
        [When(@"I select ""(.*)"" task")]
        public void GivenTaskIsSelected(string taskName)
        {
            var view = Utils.GetView<ISchedulerView>();
            view.SelectTask(taskName);
        }

        [When(@"I create new schedule")]
        public void WhenICreateNewSchedule()
        {
            var view = Utils.GetView<ISchedulerView>();
            view.CreateNewTask();
        }

        [When(@"I save the Task")]
        public void WhenISaveTheTask()
        {
            var view = Utils.GetView<ISchedulerView>();
            view.Save();
        }

        [When(@"I Delete ""(.*)""")]
        public void WhenIDelete(string taskName)
        {
            var view = Utils.GetView<ISchedulerView>();
            view.DeleteTask(taskName);
        }
        
        [When(@"I edit the task settings to")]
        public void WhenIEditTheTaskSettingsTo(Table table)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"username is ""(.*)""")]
        public void ThenUsernameIs(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"Password is ""(.*)""")]
        public void ThenPasswordIs(string p0)
        {
            ScenarioContext.Current.Pending();
        }
       

        [Then(@"Task ""(.*)"" is saved")]
        public void ThenTaskIsSaved(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"Task ""(.*)"" is Deleted")]
        public void ThenTaskIsDeleted(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"the saved tasks are")]
        public void ThenTheSavedTasksAre(Table table)
        {
            ScenarioContext.Current.Pending();
        }
    }

    public interface ISchedulerViewModel
    {
        void AddNewSchedule(string s, string s1);
    }

    public interface ISchedulerView:IView
    {
        string GetSelectedServerName();

        void GetCurrentSchedule();

        void EnterUsername(string userName);

        void EnterPassword(string password);

        void SelectTask(string taskName);

        void CreateNewTask();

        void Save();

        void DeleteTask(string taskName);
    }
}
