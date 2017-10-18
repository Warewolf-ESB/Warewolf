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
                localTaskService.GetFolder("Warewolf").DeleteTask("UILoadTest" + i.ToString());
            }
            localTaskService.Dispose();
        }

        [Given(@"I start the timer")]
        [When(@"I start the timer")]
        public void StartTimer()
        {
            if (!ScenarioContext.Current.ContainsKey("StartTime"))
            {
                ScenarioContext.Current.Add("StartTime", System.DateTime.Now);
            }
            else
            {
                ScenarioContext.Current.Set(System.DateTime.Now, "StartTime");
            }
        }

        [Then(@"the timer duration is less than ""(.*)"" seconds")]
        public void StopTimer(string duration)
        {
            var startTime = ScenarioContext.Current.Get<System.DateTime>("StartTime");
            double totalSeconds = (System.DateTime.Now - startTime).TotalSeconds;
            Assert.IsTrue(totalSeconds < int.Parse(duration), "Load test failed. Duration of " + totalSeconds.ToString() + " seconds is greater than " + duration + " seconds");
            Console.WriteLine("timer stopped after " + totalSeconds + " seconds.");
        }

        [Given(@"I have ""(.*)"" new workflow tabs open")]
        [When(@"I open ""(.*)"" new workflow tabs")]
        public void OpenManyNewWorkflowTabs(string numberOfTabs)
        {
            for(var i = int.Parse(numberOfTabs); i > 0; i--)
            {
                UIMap.Click_NewWorkflow_RibbonButton();
            }
        }

        [Given(@"I have ""(.*)"" scheduled tasks")]
        public void ManyTasks(string numberOfTasks)
        {
            TaskService localTaskService = new TaskService();
            for (var i = int.Parse(numberOfTasks); i > 0; i--)
            {
                TaskDefinition td = localTaskService.NewTask();
                td.RegistrationInfo.Description = "Does something";
                td.Triggers.Add(new DailyTrigger { DaysInterval = 2 });
                td.Actions.Add(new ExecAction("cmd.exe", "/c echo WarewolfAgent.exe", null));
                localTaskService.GetFolder("Warewolf").RegisterTaskDefinition(@"UILoadTest" + i.ToString(), td);
            }
            ScenarioContext.Current.Add("localTaskService", localTaskService);
            ScenarioContext.Current.Add("numberOfTasks", numberOfTasks);
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
