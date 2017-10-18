using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.Win32.TaskScheduler;
using TechTalk.SpecFlow;
using Warewolf.UI.Tests;


namespace Warewolf.UI.Load.Specs
{
    [Binding]
    class LoadSteps
    {
        [AfterScenario("SchedulerView")]
        public void RemoveScheduledTasks()
        {
            var localTaskService = ScenarioContext.Current.Get<TaskService>("localTaskService");
            var numberOfTasks = ScenarioContext.Current.Get<String>("numberOfTasks");
            for (var i = int.Parse(numberOfTasks); i > 0; i--)
            {
                localTaskService.RootFolder.DeleteTask("Test" + i.ToString());
            }
            localTaskService.Dispose();
        }

        [Given(@"I start the timer")]
        [When(@"I start the timer")]
        public void StartTimer()
        {
            ScenarioContext.Current.Add("StartTime", System.DateTime.Now);
        }

        [Then(@"the timer duration is less than ""(.*)"" seconds")]
        public void StopTimer(string duration)
        {
            var startTime = ScenarioContext.Current.Get<System.DateTime>("StopTime");
            Assert.IsTrue((startTime - System.DateTime.Now).TotalSeconds < int.Parse(duration), "Load test failed. Duration is greater than " + duration + " seconds");
        }

        [Given(@"I have ""(.*)"" new workflow tabs open")]
        [When(@"I open ""(.*)"" new workflow tabs")]
        public void OpenManyTabs(string numberOfTabs)
        {
            for(var i = int.Parse(numberOfTabs); i > 0; i--)
            {
                UIMap.Click_NewWorkflow_RibbonButton();
            }
        }

        [Given(@"I have ""(.*)"" scheduled tasks")]
        public void ManyTasks(string numberOfTasks)
        {
            for (var i = int.Parse(numberOfTasks); i > 0; i--)
            {
                TaskService localTaskService = new TaskService();
                TaskDefinition td = localTaskService.NewTask();
                td.RegistrationInfo.Description = "Does something";
                td.Triggers.Add(new DailyTrigger { DaysInterval = 2 });
                td.Actions.Add(new ExecAction("cmd.exe", "/c echo UI Load Testing", null));
                localTaskService.RootFolder.RegisterTaskDefinition(@"Test" + i.ToString(), td);
                ScenarioContext.Current.Add("localTaskService", localTaskService);
                ScenarioContext.Current.Add("numberOfTasks", numberOfTasks);
            }
        }

        #region Additional test attributes

        UIMap UIMap
        {
            get
            {
                if (_UIMap == null)
                {
                    _UIMap = new UIMap();
                }

                return _UIMap;
            }
        }

        private UIMap _UIMap; 

        #endregion
    }
}
