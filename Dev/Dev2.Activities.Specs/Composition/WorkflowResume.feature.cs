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
namespace Dev2.Activities.Specs.Composition
{
    using TechTalk.SpecFlow;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute()]
    public partial class WorkflowResumeFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private Microsoft.VisualStudio.TestTools.UnitTesting.TestContext _testContext;
        
        private string[] _featureTags = ((string[])(null));
        
#line 1 "WorkflowResume.feature"
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
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Composition", "WorkflowResume", "\tWhen a workflow execution is suspended\r\n\tI want to Resume", ProgrammingLanguage.CSharp, ((string[])(null)));
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
                        && (testRunner.FeatureContext.FeatureInfo.Title != "WorkflowResume")))
            {
                global::Dev2.Activities.Specs.Composition.WorkflowResumeFeature.FeatureSetup(null);
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
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("When Resuming a Workflow it will always resume from the latest Version")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "WorkflowResume")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("ResumeWorkflowExecution")]
        public virtual void WhenResumingAWorkflowItWillAlwaysResumeFromTheLatestVersion()
        {
            string[] tagsOfScenario = new string[] {
                    "ResumeWorkflowExecution"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("When Resuming a Workflow it will always resume from the latest Version", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 6
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
#line 7
 testRunner.Given("I have a workflow \"ResumeWorkflowFromVersion\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
                TechTalk.SpecFlow.Table table845 = new TechTalk.SpecFlow.Table(new string[] {
                            "variable",
                            "value"});
                table845.AddRow(new string[] {
                            "[[rec().a]]",
                            "New"});
                table845.AddRow(new string[] {
                            "[[rec().a]]",
                            "Test"});
#line 8
 testRunner.And("\"ResumeWorkflowFromVersion\" contains an Assign \"VarsAssign\" as", ((string)(null)), table845, "And ");
#line hidden
#line 12
 testRunner.When("workflow \"ResumeWorkflowFromVersion\" is saved \"1\" time", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 13
 testRunner.And("Resource \"ResumeWorkflowFromVersion\" has rights \"Execute\" for \"SecuritySpecsUser\"" +
                        " with password \"ASfas123@!fda\" in \"Users\" group", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 14
 testRunner.Then("workflow \"ResumeWorkflowFromVersion\" has \"0\" Versions in explorer", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 15
 testRunner.When("\"ResumeWorkflowFromVersion\" is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 16
 testRunner.Then("the workflow execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
                TechTalk.SpecFlow.Table table846 = new TechTalk.SpecFlow.Table(new string[] {
                            "#",
                            ""});
                table846.AddRow(new string[] {
                            "1",
                            "[[rec(1).a]] = New"});
                table846.AddRow(new string[] {
                            "2",
                            "[[rec(2).a]] = Test"});
#line 17
 testRunner.And("the \"VarsAssign\" in Workflow \"ResumeWorkflowFromVersion\" debug outputs as", ((string)(null)), table846, "And ");
#line hidden
                TechTalk.SpecFlow.Table table847 = new TechTalk.SpecFlow.Table(new string[] {
                            "variable",
                            "value"});
                table847.AddRow(new string[] {
                            "[[variable]]",
                            "NewlyAddedVariable"});
#line 21
 testRunner.Then("I update \"ResumeWorkflowFromVersion\" by adding \"AnotherVarsAssign\" as", ((string)(null)), table847, "Then ");
#line hidden
#line 24
 testRunner.When("workflow \"ResumeWorkflowFromVersion\" is saved \"1\" time", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table848 = new TechTalk.SpecFlow.Table(new string[] {
                            "variable",
                            "value"});
                table848.AddRow(new string[] {
                            "[[ThirdAssignVariable]]",
                            "ThirdAssignVariable"});
#line 25
 testRunner.Then("I update \"ResumeWorkflowFromVersion\" by adding \"ThirVarAssign\" as", ((string)(null)), table848, "Then ");
#line hidden
#line 28
 testRunner.When("workflow \"ResumeWorkflowFromVersion\" is saved \"1\" time", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 29
 testRunner.And("I reload Server resources", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 30
 testRunner.And("I resume the workflow \"ResumeWorkflowFromVersion\" at \"VarsAssign\" from version \"2" +
                        "\"  with user \"SecuritySpecsUser\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 31
 testRunner.Then("the workflow execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
                TechTalk.SpecFlow.Table table849 = new TechTalk.SpecFlow.Table(new string[] {
                            "#",
                            ""});
                table849.AddRow(new string[] {
                            "1",
                            "[[rec(1).a]] = New"});
                table849.AddRow(new string[] {
                            "2",
                            "[[rec(2).a]] = Test"});
#line 32
 testRunner.And("the \"VarsAssign\" in Workflow \"ResumeWorkflowFromVersion\" debug outputs as", ((string)(null)), table849, "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Resuming a workflow with an Invalid User Returns Authentication Error")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "WorkflowResume")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("ResumeWorkflowExecution")]
        public virtual void ResumingAWorkflowWithAnInvalidUserReturnsAuthenticationError()
        {
            string[] tagsOfScenario = new string[] {
                    "ResumeWorkflowExecution"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Resuming a workflow with an Invalid User Returns Authentication Error", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 38
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
#line 39
 testRunner.Given("I have a server at \"localhost\" with workflow \"Hello World\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 40
 testRunner.And("Resource \"Hello World\" has rights \"Execute\" for \"SecuritySpecsUser\" with password" +
                        " \"ASfas123@!fda\" in \"Users\" group", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 41
 testRunner.And("Workflow \"Hello World\" has \"Assign a value to Name if blank (1)\" activity", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 42
 testRunner.Then("Resume workflow \"Hello World\" at \"Assign a value to Name if blank (1)\" tool with " +
                        "user \"InvalidUsername\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 43
 testRunner.Then("Resume has \"AN\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 44
 testRunner.Then("Resume message is \"Authentication Error resuming. User InvalidUsername is not aut" +
                        "horized to execute the workflow.\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
