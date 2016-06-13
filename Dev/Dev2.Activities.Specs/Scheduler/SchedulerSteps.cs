
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Windows.Controls;
using CubicOrange.Windows.Forms.ActiveDirectory;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Core.Tests.Utils;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Settings.Scheduler;
using Dev2.Studio.Controller;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.TaskScheduler.Wrappers;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.TaskScheduler;
using Moq;
using TechTalk.SpecFlow;
using Dev2.Activities.Specs.BaseTypes;
// ReSharper disable UnusedMember.Global

namespace Dev2.Activities.Specs.Scheduler
{
    [Binding]
    public class SchedulerSteps
    {
        private readonly ScenarioContext scenarioContext;
        private readonly CommonSteps _commonSteps;

        public SchedulerSteps(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
            _commonSteps = new CommonSteps(this.scenarioContext);
        }

        [Given(@"I have a schedule ""(.*)""")]
        public void GivenIHaveASchedule(string scheduleName)
        {
            ScenarioContext.Current.Add("ScheduleName", scheduleName);
        }

        [Given(@"""(.*)"" executes an Workflow ""(.*)""")]
        public void GivenExecutesAnWorkflow(string scheduleName, string workFlow)
        {
            ScenarioContext.Current.Add("WorkFlow", workFlow);
        }

        [Given(@"""(.*)"" has a username of ""(.*)"" and a Password of ""(.*)""")]
        public void GivenHasAUsernameOfAndAPasswordOf(string scheduleName, string userName, string password)
        {
            if(!AccountExists("LocalSchedulerAdmin"))
            {
                CreateLocalWindowsAccount("LocalSchedulerAdmin", "987Sched#@!", "Administrtators");
            }
            ScenarioContext.Current.Add("UserName", userName);
            ScenarioContext.Current.Add("Password", password);
        }

        [Given(@"""(.*)"" has a Schedule of")]
        public void GivenHasAScheduleOf(string scheduleName, Table table)
        {
            AppSettings.LocalHost = "http://localhost:3142";
            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockshell.Setup(a => a.LocalhostServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            SchedulerViewModel scheduler = new SchedulerViewModel(EventPublishers.Aggregator, new DirectoryObjectPickerDialog(), new PopupController(), AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, new Mock<Dev2.Common.Interfaces.IServer>().Object, a => new Mock<IEnvironmentModel>().Object);
            IEnvironmentModel environmentModel = EnvironmentRepository.Instance.Source;

            environmentModel.Connect();
            scheduler.ScheduledResourceModel = new ClientScheduledResourceModel(environmentModel,()=>{});
            scheduler.CurrentEnvironment = environmentModel;
            scheduler.CreateNewTask();
            scheduler.SelectedTask.Name = ScenarioContext.Current["ScheduleName"].ToString();
            scheduler.SelectedTask.OldName = "bob";
            scheduler.SelectedTask.UserName = ScenarioContext.Current["UserName"].ToString();
            scheduler.SelectedTask.Password = ScenarioContext.Current["Password"].ToString();
            scheduler.SelectedTask.WorkflowName = ScenarioContext.Current["WorkFlow"].ToString();
            scheduler.SelectedTask.NumberOfHistoryToKeep = (int)ScenarioContext.Current["HistoryCount"];
            scheduler.SelectedTask.Status = (SchedulerStatus)ScenarioContext.Current["TaskStatus"];
            scheduler.Errors.ClearErrors();
            var task = scheduler.SelectedTask;
            UpdateTrigger(task, table);

            PrivateObject po = new PrivateObject(scheduler.CurrentEnvironment);
            var mockAuth = new Mock<IAuthorizationService>();
            mockAuth.Setup(a => a.IsAuthorized(It.IsAny<AuthorizationContext>(), null)).Returns(true);
            po.SetFieldOrProperty("AuthorizationService", mockAuth.Object);
            ScenarioContext.Current["Scheduler"] = scheduler;
            try
            {
                scheduler.SaveCommand.Execute("");
            }
            catch(Exception e)
            {

                ScenarioContext.Current["Error"] = e.Message;
            }


        }

        void UpdateTrigger(IScheduledResource task, Table table)
        {
            var sched = table.Rows[0]["ScheduleType"];
            Trigger x;
            switch(sched)
            {
                case "At log on":
                    x = new LogonTrigger();
                    break;
                case "On a schedule":
                    x = CreateScheduleTrigger(table);
                    break;
                case "At Startup":
                    x = new BootTrigger();
                    break;
                case "On Idle":
                    x = new IdleTrigger();
                    break;
                default:
                    x = new DailyTrigger();
                    break;
            }
            task.Trigger.Trigger = new Dev2Trigger(new TaskServiceConvertorFactory(), x);
        }

        Trigger CreateScheduleTrigger(Table table)
        {
            var sched = table.Rows[0]["Interval"];
            switch(sched)
            {
                case "Daily":
                    return new DailyTrigger(Convert.ToInt16(table.Rows[0]["Recurs"])) { StartBoundary = DateTime.Parse(table.Rows[0]["StartDate"] + " " + table.Rows[0]["StartTime"]) };
                case "One Time":
                    return new TimeTrigger(DateTime.Parse(table.Rows[0]["StartDate"] + " " + table.Rows[0]["StartTime"]));
                case "Weekly":
                    return new WeeklyTrigger(GetDays(table.Rows[0]["Interval"].Split(new[] { ',' })));
                default:
                    return new DailyTrigger();
            }


        }

        DaysOfTheWeek GetDays(string[] split)
        {
            DaysOfTheWeek res;
            Enum.TryParse(split.First(), true, out res);

            foreach(var s in split.Except(new[] { split.First() }))
            {
                DaysOfTheWeek day;
                Enum.TryParse(s, true, out day);
                res &= day;

            }
            return res;
        }



        [Then(@"the schedule status is ""(.*)""")]
        public void ThenTheScheduleStatusIs(string status)
        {
            var scheduler = ScenarioContext.Current["Scheduler"] as SchedulerViewModel;
            if(scheduler != null)
            {
                scheduler.ActiveItem = new TabItem { Header = "History" };
                Thread.Sleep(12000);
                // ReSharper disable RedundantAssignment
                IList<IResourceHistory> x = scheduler.ScheduledResourceModel.CreateHistory(scheduler.SelectedTask).ToList();
                // ReSharper restore RedundantAssignment

                if(status == "Success")
                    Assert.AreEqual(ScheduleRunStatus.Success, x[0].TaskHistoryOutput.Success);
                else
                {
                    Assert.IsTrue(x[0].TaskHistoryOutput.Success == ScheduleRunStatus.Error || x[0].TaskHistoryOutput.Success == ScheduleRunStatus.Error);
                }
                ScenarioContext.Current["History"] = x;

            }
            else
            {
                throw new Exception("Where the scheduler");
            }
        }

        [Then(@"""(.*)"" has ""(.*)"" row of history")]
        public void ThenHasRowOfHistory(string scheduleName, int history)
        {
            ScenarioContext.Current["HistoryCount"] = history;
        }

        [Then(@"the history debug output for ""(.*)"" for row ""(.*)"" is")]
        public void ThenTheHistoryDebugOutputForForRowIs(string p0, int p1, Table table)
        {
            IList<IResourceHistory> resources = ScenarioContext.Current["History"] as IList<IResourceHistory>;
            // ReSharper disable AssignNullToNotNullAttribute
            var debug = resources.First().DebugOutput;
            // ReSharper restore AssignNullToNotNullAttribute
            var debugTocompare = debug.Last();
            _commonSteps.ThenTheDebugOutputAs(table, debugTocompare.Outputs.SelectMany(s => s.ResultsList).ToList(), true);
        }

        [Given(@"task history ""(.*)"" is ""(.*)""")]
        public void GivenTaskHistoryIs(string scheduleName, int history)
        {
            ScenarioContext.Current["HistoryCount"] = history;
        }

        [Given(@"the task status ""(.*)"" is ""(.*)""")]
        public void GivenTheTaskStatusIs(string schedule, string status)
        {
            ScenarioContext.Current["TaskStatus"] = status == "Enabled" ? SchedulerStatus.Enabled : SchedulerStatus.Disabled;
        }



        [Then(@"the Schedule task has ""(.*)"" error")]
        public void ThenTheScheduleTaskHasError(string error)
        {
            if(error == "AN" && ScenarioContext.Current["Error"] == null)
                Assert.Fail("Error Expected");
        }

        [When(@"the ""(.*)"" is executed ""(.*)"" times")]
        public void WhenTheIsExecutedTimes(string scheduleName, int times)
        {
            try
            {


                int i = 0;
                var x = new TaskService();
                x.GetFolder("Warewolf");
                var task = x.FindTask(scheduleName);
                do
                {
                    task.Run();


                    const int TimeOut = 10;
                    int time = 0;
                    while(task.State == TaskState.Running && time < TimeOut)
                    {
                        time++;
                        Thread.Sleep(1000);
                    }
                    i++;


                } while(i < times);
            }
            catch(Exception e)
            {

                ScenarioContext.Current["Error"] = e;
            }

        }

        public static bool AccountExists(string name)
        {
            bool accountExists = false;
            try
            {
                var id = GetUserSecurityIdentifier(name);
                accountExists = id.IsAccountSid();
            }
                // ReSharper disable EmptyGeneralCatchClause
            catch(Exception)
                // ReSharper restore EmptyGeneralCatchClause
            {
                /* Invalid user account */
            }
            return accountExists;
        }

        public static SecurityIdentifier GetUserSecurityIdentifier(string name)
        {
            NTAccount acct = new NTAccount(Environment.MachineName, name);
            SecurityIdentifier id = (SecurityIdentifier)acct.Translate(typeof(SecurityIdentifier));
            return id;
        }

        public static bool CreateLocalWindowsAccount(string username, string password, string groupName)
        {
            try
            {
                PrincipalContext context = new PrincipalContext(ContextType.Machine);
                UserPrincipal user = new UserPrincipal(context);
                user.SetPassword(password);
                user.DisplayName = username;
                user.Name = username;
                user.UserCannotChangePassword = true;
                user.PasswordNeverExpires = true;

                user.Save();
                AddUserToGroup(groupName, context, user);
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(@"error creating user" + ex.Message);
                return false;
            }
        }

        public static void AddUserToGroup(string groupName, PrincipalContext context, UserPrincipal user)
        {
            GroupPrincipal usersGroup = GroupPrincipal.FindByIdentity(context, groupName);
            if(usersGroup != null)
            {
                usersGroup.Members.Add(user);
                usersGroup.Save();
            }
        }
    }
}
