﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (https://www.specflow.org/).
//      SpecFlow Version:3.9.0.0
//      SpecFlow Generator Version:3.9.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace Dev2.Activities.Specs.Scheduler
{
    using TechTalk.SpecFlow;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute()]
    public partial class SchedulerFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private Microsoft.VisualStudio.TestTools.UnitTesting.TestContext _testContext;
        
        private string[] _featureTags = new string[] {
                "Scheduler"};
        
#line 1 "Scheduler.feature"
#line hidden
        
        public virtual Microsoft.VisualStudio.TestTools.UnitTesting.TestContext TestContext
        {
            get
            {
                return this._testContext;
            }
            set
            {
                this._testContext = value;
            }
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static void FeatureSetup(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Scheduler", "Scheduler", "\tIn order to schedule workflows\r\n\tAs a Warewolf user\r\n\tI want to setup schedules", ProgrammingLanguage.CSharp, new string[] {
                        "Scheduler"});
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute()]
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute()]
        public virtual void TestInitialize()
        {
            if (((testRunner.FeatureContext != null) 
                        && (testRunner.FeatureContext.FeatureInfo.Title != "Scheduler")))
            {
                global::Dev2.Activities.Specs.Scheduler.SchedulerFeature.FeatureSetup(null);
            }
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute()]
        public virtual void TestTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Microsoft.VisualStudio.TestTools.UnitTesting.TestContext>(_testContext);
        }
        
        public virtual void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Schedule with history")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Scheduler")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Scheduler")]
        public virtual void ScheduleWithHistory()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Schedule with history", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 8
 this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 9
  testRunner.Given("I have a schedule \"ScheduleWithHistory\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 10
  testRunner.And("\"ScheduleWithHistory\" executes an Workflow \"Hello World\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 11
  testRunner.And("task history \"Number of history records to load\" is \"2\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 12
  testRunner.And("the task status \"Status\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 13
  testRunner.And("\"Diceroll00\" has a username of \".\\LocalSchedulerAdmin\" and a Password of \"987Sche" +
                        "d#@!\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table916 = new TechTalk.SpecFlow.Table(new string[] {
                            "ScheduleType",
                            "Interval",
                            "StartDate",
                            "StartTime",
                            "Recurs",
                            "RecursInterval",
                            "Delay",
                            "DelayInterval",
                            "Repeat",
                            "RepeatInterval",
                            "ExpireDate",
                            "ExpireTime",
                            "ResourceId"});
                table916.AddRow(new string[] {
                            "On a schedule",
                            "Daily",
                            "2020/01/01",
                            "15:40:44",
                            "1",
                            "day",
                            "1",
                            "hour",
                            "1",
                            "hour",
                            "2020/01/02",
                            "15:40:15",
                            ""});
#line 14
  testRunner.And("\"ScheduleWithHistory\" has a Schedule of", ((string)(null)), table916, "And ");
#line hidden
#line 17
  testRunner.When("the \"ScheduleWithHistory\" is executed \"1\" times", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 18
  testRunner.Then("the Schedule task has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 19
  testRunner.Then("the schedule status is \"Success\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 20
  testRunner.And("\"ScheduleWithHistory\" has \"2\" row of history", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Creating task with schedule status is disabled")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Scheduler")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Scheduler")]
        public virtual void CreatingTaskWithScheduleStatusIsDisabled()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Creating task with schedule status is disabled", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 22
 this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 23
  testRunner.Given("I have a schedule \"Diceroll00\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 24
  testRunner.And("\"Diceroll00\" executes an Workflow \"Hello World\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 25
  testRunner.And("task history \"Number of history records to load\" is \"2\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 26
  testRunner.And("the task status \"Status\" is \"Disabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 27
  testRunner.And("\"Diceroll00\" has a username of \".\\LocalSchedulerAdmin\" and a Password of \"987Sche" +
                        "d#@!\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table917 = new TechTalk.SpecFlow.Table(new string[] {
                            "ScheduleType",
                            "Interval",
                            "StartDate",
                            "StartTime",
                            "Recurs",
                            "RecursInterval",
                            "Delay",
                            "DelayInterval",
                            "Repeat",
                            "RepeatInterval",
                            "ExpireDate",
                            "ExpireTime",
                            "ResourceId"});
                table917.AddRow(new string[] {
                            "On a schedule",
                            "\"Daily\"",
                            "2020/01/01",
                            "15:40:44",
                            "1",
                            "day",
                            "1",
                            "hour",
                            "1",
                            "hour",
                            "2020/01/02",
                            "15:40:15",
                            ""});
#line 28
  testRunner.And("\"Diceroll00\" has a Schedule of", ((string)(null)), table917, "And ");
#line hidden
#line 31
  testRunner.When("the \"Diceroll00\" is executed \"1\" times", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 32
  testRunner.Then("the Schedule task has \"AN\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Setting schedule task \"At log on\"")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Scheduler")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Scheduler")]
        public virtual void SettingScheduleTaskAtLogOn()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Setting schedule task \"At log on\"", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 34
 this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 35
  testRunner.Given("I have a schedule \"Diceroll1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 36
  testRunner.And("\"Diceroll1\" executes an Workflow \"Hello World\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 37
  testRunner.And("task history \"Number of history records to load\" is \"2\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 38
  testRunner.And("the task status \"Status\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 39
  testRunner.And("\"Diceroll1\" has a username of \".\\LocalSchedulerAdmin\" and a Password of \"987Sched" +
                        "#@!\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table918 = new TechTalk.SpecFlow.Table(new string[] {
                            "ScheduleType",
                            "Delay",
                            "DelayInterval",
                            "Repeat",
                            "RepeatInterval",
                            "ExpireDate",
                            "ExpireTime",
                            "ResourceId"});
                table918.AddRow(new string[] {
                            "At log on",
                            "1",
                            "hour",
                            "1",
                            "hour",
                            "2020/01/02",
                            "15:40:15",
                            ""});
#line 40
  testRunner.And("\"Diceroll1\" has a Schedule of", ((string)(null)), table918, "And ");
#line hidden
#line 43
  testRunner.Then("the Schedule task has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 44
  testRunner.When("the \"Diceroll1\" is executed \"1\" times", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 45
  testRunner.Then("the Schedule task has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 46
  testRunner.Then("the schedule status is \"Success\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 47
  testRunner.And("\"Diceroll1\" has \"2\" row of history", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Schedule the task with Incorrect username or password")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Scheduler")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Scheduler")]
        public virtual void ScheduleTheTaskWithIncorrectUsernameOrPassword()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Schedule the task with Incorrect username or password", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 49
 this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 50
  testRunner.Given("I have a schedule \"Diceroll1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 51
  testRunner.And("\"Diceroll1\" executes an Workflow \"Hello World\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 52
  testRunner.And("task history \"Number of history records to load\" is \"2\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 53
  testRunner.And("the task status \"Status\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 54
  testRunner.And("\"Diceroll1\" has a username of \"bobthebuilder\" and a Password of \"987Sched#@!\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table919 = new TechTalk.SpecFlow.Table(new string[] {
                            "ScheduleType",
                            "Delay",
                            "DelayInterval",
                            "Repeat",
                            "RepeatInterval",
                            "ExpireDate",
                            "ExpireTime",
                            "ResourceId"});
                table919.AddRow(new string[] {
                            "At log on",
                            "1",
                            "hour",
                            "1",
                            "hour",
                            "2020/01/02",
                            "15:40:15",
                            ""});
#line 55
  testRunner.And("\"Diceroll1\" has a Schedule of", ((string)(null)), table919, "And ");
#line hidden
#line 58
  testRunner.Then("the Schedule task has \"AN\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Schedule with LocalUser")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Scheduler")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Scheduler")]
        public virtual void ScheduleWithLocalUser()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Schedule with LocalUser", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 60
 this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 61
  testRunner.Given("I have a schedule \"LocalUserSchedule\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 62
  testRunner.And("\"LocalUserSchedule\" executes an Workflow \"Hello World\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 63
  testRunner.And("task history \"Number of history records to load\" is \"2\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 64
  testRunner.And("the task status \"Status\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 65
  testRunner.And("\"LocalUserSchedule\" has a username of \".\\LocalSchedulerAdmin\" and a Password of \"" +
                        "987Sched#@!\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table920 = new TechTalk.SpecFlow.Table(new string[] {
                            "ScheduleType",
                            "Interval",
                            "StartDate",
                            "StartTime",
                            "Recurs",
                            "RecursInterval",
                            "Delay",
                            "DelayInterval",
                            "Repeat",
                            "RepeatInterval",
                            "ExpireDate",
                            "ExpireTime",
                            "ResourceId"});
                table920.AddRow(new string[] {
                            "On a schedule",
                            "Daily",
                            "2020/01/01",
                            "15:40:44",
                            "1",
                            "day",
                            "1",
                            "hour",
                            "1",
                            "hour",
                            "2020/01/02",
                            "15:40:15",
                            ""});
#line 66
  testRunner.And("\"LocalUserSchedule\" has a Schedule of", ((string)(null)), table920, "And ");
#line hidden
#line 69
  testRunner.When("the \"LocalUserSchedule\" is executed \"1\" times", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 70
  testRunner.Then("the Schedule task has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 71
  testRunner.Then("the schedule status is \"Success\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 72
  testRunner.And("\"LocalUserSchedule\" has \"2\" row of history", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Schedule with ErrorInDebug")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Scheduler")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Scheduler")]
        public virtual void ScheduleWithErrorInDebug()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Schedule with ErrorInDebug", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 74
 this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 75
  testRunner.Given("I have a schedule \"ScheduleWithError\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 76
  testRunner.And("\"ScheduleWithError\" executes an Workflow \"moocowimpi\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 77
  testRunner.And("task history \"Number of history records to load\" is \"2\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 78
  testRunner.And("the task status \"Status\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 79
  testRunner.And("\"ScheduleWithError\" has a username of \".\\LocalSchedulerAdmin\" and a Password of \"" +
                        "987Sched#@!\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table921 = new TechTalk.SpecFlow.Table(new string[] {
                            "ScheduleType",
                            "Interval",
                            "StartDate",
                            "StartTime",
                            "Recurs",
                            "RecursInterval",
                            "Delay",
                            "DelayInterval",
                            "Repeat",
                            "RepeatInterval",
                            "ExpireDate",
                            "ExpireTime",
                            "ResourceId"});
                table921.AddRow(new string[] {
                            "On a schedule",
                            "Daily",
                            "2020/01/01",
                            "15:40:44",
                            "1",
                            "day",
                            "1",
                            "hour",
                            "1",
                            "hour",
                            "2020/01/02",
                            "15:40:15",
                            "acb75027-ddeb-47d7-814e-a54c37247ec2"});
#line 80
  testRunner.And("\"ScheduleWithError\" has a Schedule of", ((string)(null)), table921, "And ");
#line hidden
#line 83
  testRunner.When("the \"ScheduleWithError\" is executed \"1\" times", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 84
  testRunner.Then("the Schedule task has \"AN\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 85
  testRunner.Then("the schedule status is \"Failure\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Schedule Workflow with success")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Scheduler")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Scheduler")]
        public virtual void ScheduleWorkflowWithSuccess()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Schedule Workflow with success", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 87
 this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 88
  testRunner.Given("I have a schedule \"ScheduleAssignOutput\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 89
  testRunner.And("\"ScheduleAssignOutput\" executes an Workflow \"AssignOutput\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 90
  testRunner.And("task history \"Number of history records to load\" is \"2\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 91
  testRunner.And("the task status \"Status\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 92
  testRunner.And("\"ScheduleAssignOutput\" has a username of \".\\LocalSchedulerAdmin\" and a Password o" +
                        "f \"987Sched#@!\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table922 = new TechTalk.SpecFlow.Table(new string[] {
                            "ScheduleType",
                            "Interval",
                            "StartDate",
                            "StartTime",
                            "Recurs",
                            "RecursInterval",
                            "Delay",
                            "DelayInterval",
                            "Repeat",
                            "RepeatInterval",
                            "ExpireDate",
                            "ExpireTime",
                            "ResourceId"});
                table922.AddRow(new string[] {
                            "On a schedule",
                            "Daily",
                            "2020/01/01",
                            "15:40:44",
                            "1",
                            "day",
                            "1",
                            "hour",
                            "1",
                            "hour",
                            "2020/01/02",
                            "15:40:15",
                            "e7ea5196-33f7-4e0e-9d66-44bd67528a96"});
#line 93
  testRunner.And("\"ScheduleAssignOutput\" has a Schedule of", ((string)(null)), table922, "And ");
#line hidden
#line 96
  testRunner.When("the \"ScheduleAssignOutput\" is executed \"1\" times", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 97
  testRunner.Then("the Schedule task has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 98
  testRunner.Then("the schedule status is \"Success\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
