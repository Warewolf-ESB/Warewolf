using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.Win32.TaskScheduler;
using TechTalk.SpecFlow;
using Warewolf.UI.Tests;
using Microsoft.VisualStudio.TestTools.UITesting;
using System.Diagnostics;

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
            try
            {
                for (var i = int.Parse(numberOfTasks); i > 0; i--)
                {
                    TaskDefinition td = localTaskService.NewTask();
                    td.RegistrationInfo.Description = "Does something";
                    td.Triggers.Add(new DailyTrigger { DaysInterval = 2 });
                    td.Actions.Add(new ExecAction("cmd.exe", "/c echo WarewolfAgent.exe", null));
                    TaskFolder localWarewolfFolder = localTaskService.GetFolder("Warewolf");
                    if (localWarewolfFolder != null)
                    {
                        localWarewolfFolder.RegisterTaskDefinition(@"UILoadTest" + i.ToString(), td);
                    }
                    else
                    {
                        Assert.Fail("Task scheduler has no Warewolf folder.");
                    }
                }
            }
            finally
            {
                ScenarioContext.Current.Add("localTaskService", localTaskService);
                ScenarioContext.Current.Add("numberOfTasks", numberOfTasks);
            }
        }

        [When(@"I close the studio")]
        public void CloseStudio()
        {
            Mouse.Click(UIMap.MainStudioWindow.CloseStudioButton);
            var studioProcess = Process.GetProcessesByName("Warewolf Studio");
            if (studioProcess != null && studioProcess.Length > 0)
            {
                ScenarioContext.Current.Add("studioProcess", studioProcess[0]);
                studioProcess[0].WaitForExit();
            }
        }

        [When("I start the Studio")]
        public void StartStudio()
        {
            var studioProcess = ScenarioContext.Current.Get<Process>("studioProcess");
            studioProcess.Start();
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
