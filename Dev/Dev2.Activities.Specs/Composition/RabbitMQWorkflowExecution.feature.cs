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
    public partial class RabbitMQWorkflowExecutionFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private Microsoft.VisualStudio.TestTools.UnitTesting.TestContext _testContext;
        
        private string[] _featureTags = ((string[])(null));
        
#line 1 "RabbitMQWorkflowExecution.feature"
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
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Composition", "RabbitMQWorkflowExecution", "\tIn order to execute a workflow\r\n\tAs a Warewolf user\r\n\tI want to be able to build" +
                    " workflows and execute them against the server", ProgrammingLanguage.CSharp, ((string[])(null)));
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
                        && (testRunner.FeatureContext.FeatureInfo.Title != "RabbitMQWorkflowExecution")))
            {
                global::Dev2.Activities.Specs.Composition.RabbitMQWorkflowExecutionFeature.FeatureSetup(null);
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
        
        public virtual void FeatureBackground()
        {
#line 6
#line hidden
#line 7
   testRunner.Given("Debug events are reset", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 8
   testRunner.And("Debug states are cleared", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("WF with DsfRabbitMq Consume timeout 5")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "RabbitMQWorkflowExecution")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("RabbitMQWorkflowExecution")]
        public virtual void WFWithDsfRabbitMqConsumeTimeout5()
        {
            string[] tagsOfScenario = new string[] {
                    "RabbitMQWorkflowExecution"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("WF with DsfRabbitMq Consume timeout 5", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 11
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
#line 6
this.FeatureBackground();
#line hidden
#line 12
 testRunner.Given("I depend on a valid RabbitMQ server", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 13
 testRunner.And("I have a workflow \"RabbitMqConsume5mintimeout\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 14
 testRunner.And("\"RabbitMqConsume5mintimeout\" contains DsfRabbitMQPublish and Queue1 \"DsfPublishRa" +
                        "bbitMQActivity\" into \"[[result1]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 15
 testRunner.And("\"RabbitMqConsume5mintimeout\" contains RabbitMQConsume \"DsfConsumeRabbitMQActivity" +
                        "\" with timeout 5 seconds into \"[[result]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 16
 testRunner.When("\"RabbitMqConsume5mintimeout\" is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 17
    testRunner.Then("the workflow execution has \"No\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 18
 testRunner.And("the \"RabbitMqConsume5mintimeout\" has a start and end duration", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 19
 testRunner.And("\"RabbitMqConsume5mintimeout\" Duration is greater or equal to 5 seconds", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("WF with RabbitMq Consume timeout 5")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "RabbitMQWorkflowExecution")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("RabbitMQWorkflowExecution")]
        public virtual void WFWithRabbitMqConsumeTimeout5()
        {
            string[] tagsOfScenario = new string[] {
                    "RabbitMQWorkflowExecution"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("WF with RabbitMq Consume timeout 5", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
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
#line 6
this.FeatureBackground();
#line hidden
#line 23
 testRunner.Given("I depend on a valid RabbitMQ server", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 24
 testRunner.And("I have a workflow \"RabbitMqConsume5mintimeout\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 25
 testRunner.And("\"RabbitMqConsume5mintimeout\" contains RabbitMQPublish and Queue1 - CorrelationID " +
                        "\"PublishRabbitMQActivity\" into \"[[result1]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 26
 testRunner.And("\"RabbitMqConsume5mintimeout\" contains RabbitMQConsume \"DsfConsumeRabbitMQActivity" +
                        "\" with timeout 5 seconds into \"[[result]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 27
 testRunner.When("\"RabbitMqConsume5mintimeout\" is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 28
    testRunner.Then("the workflow execution has \"No\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 29
 testRunner.And("the \"RabbitMqConsume5mintimeout\" has a start and end duration", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 30
 testRunner.And("\"RabbitMqConsume5mintimeout\" Duration is greater or equal to 5 seconds", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("WF with RabbitMq Consume with no timeout")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "RabbitMQWorkflowExecution")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("RabbitMQWorkflowExecution")]
        public virtual void WFWithRabbitMqConsumeWithNoTimeout()
        {
            string[] tagsOfScenario = new string[] {
                    "RabbitMQWorkflowExecution"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("WF with RabbitMq Consume with no timeout", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 33
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
#line 6
this.FeatureBackground();
#line hidden
#line 34
 testRunner.Given("I have a workflow \"RabbitMqConsumeNotimeout\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 35
 testRunner.And("\"RabbitMqConsumeNotimeout\" contains RabbitMQConsume \"DsfConsumeRabbitMQActivity\" " +
                        "with timeout -1 seconds into \"[[result]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 36
 testRunner.When("\"RabbitMqConsumeNotimeout\" is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 37
    testRunner.Then("the workflow execution has \"No\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 38
 testRunner.And("the \"RabbitMqConsumeNotimeout\" has a start and end duration", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 39
 testRunner.And("\"RabbitMqConsumeNotimeout\" Duration is less or equal to 60 seconds", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("COM DLL service execute")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "RabbitMQWorkflowExecution")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("RabbitMQWorkflowExecution")]
        public virtual void COMDLLServiceExecute()
        {
            string[] tagsOfScenario = new string[] {
                    "RabbitMQWorkflowExecution"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("COM DLL service execute", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 42
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
#line 6
this.FeatureBackground();
#line hidden
#line 43
 testRunner.Given("I have a server at \"localhost\" with workflow \"Testing COM DLL Activity Execute\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 44
 testRunner.When("\"localhost\" is the active environment used to execute \"Testing COM DLL Activity E" +
                        "xecute\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 45
    testRunner.Then("the workflow execution has \"No\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
                TechTalk.SpecFlow.Table table617 = new TechTalk.SpecFlow.Table(new string[] {
                            ""});
                table617.AddRow(new string[] {
                            "[[PrimitiveReturnValue]] = 0"});
#line 46
 testRunner.And("the \"Com DLL\" in Workflow \"Testing COM DLL Activity Execute\" debug outputs is", ((string)(null)), table617, "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Workflow with ForEach and Manual Loop")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "RabbitMQWorkflowExecution")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("NestedForEachExecution")]
        public virtual void WorkflowWithForEachAndManualLoop()
        {
            string[] tagsOfScenario = new string[] {
                    "NestedForEachExecution"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Workflow with ForEach and Manual Loop", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 51
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
#line 6
this.FeatureBackground();
#line hidden
#line 52
      testRunner.Given("I have a workflow \"WFWithForEachWithManualLoop\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
                TechTalk.SpecFlow.Table table618 = new TechTalk.SpecFlow.Table(new string[] {
                            "variable",
                            "value"});
                table618.AddRow(new string[] {
                            "[[counter]]",
                            "0"});
#line 53
   testRunner.And("\"WFWithForEachWithManualLoop\" contains an Assign \"Setup Counter\" as", ((string)(null)), table618, "And ");
#line hidden
                TechTalk.SpecFlow.Table table619 = new TechTalk.SpecFlow.Table(new string[] {
                            "variable",
                            "value"});
                table619.AddRow(new string[] {
                            "[[counter]]",
                            "=[[counter]]+1"});
#line 56
   testRunner.And("\"WFWithForEachWithManualLoop\" contains an Assign \"Increment Counter\" as", ((string)(null)), table619, "And ");
#line hidden
#line 59
   testRunner.And("\"WFWithForEachWithManualLoop\" contains a Foreach \"ForEachTest\" as \"NumOfExecution" +
                        "\" executions \"2\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table620 = new TechTalk.SpecFlow.Table(new string[] {
                            "variable",
                            "value"});
                table620.AddRow(new string[] {
                            "[[rec().a]]",
                            "Test"});
#line 60
   testRunner.And("\"ForEachTest\" contains an Assign \"MyAssign\" as", ((string)(null)), table620, "And ");
#line hidden
                TechTalk.SpecFlow.Table table621 = new TechTalk.SpecFlow.Table(new string[] {
                            "ItemToCheck",
                            "Condition",
                            "ValueToCompareTo",
                            "TrueArmToolName",
                            "FalseArmToolName"});
                table621.AddRow(new string[] {
                            "[[counter]]",
                            "=",
                            "3",
                            "End Result",
                            "Increment Counter"});
#line 63
   testRunner.And("\"WFWithForEachWithManualLoop\" contains a Decision \"Check Counter\" as", ((string)(null)), table621, "And ");
#line hidden
                TechTalk.SpecFlow.Table table622 = new TechTalk.SpecFlow.Table(new string[] {
                            "variable",
                            "value"});
                table622.AddRow(new string[] {
                            "[[result]]",
                            "DONE"});
#line 66
   testRunner.And("\"WFWithForEachWithManualLoop\" contains an Assign \"End Result\" as", ((string)(null)), table622, "And ");
#line hidden
#line 69
      testRunner.When("\"WFWithForEachWithManualLoop\" is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 70
   testRunner.Then("the workflow execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
                TechTalk.SpecFlow.Table table623 = new TechTalk.SpecFlow.Table(new string[] {
                            "",
                            "Number"});
                table623.AddRow(new string[] {
                            "No. of Executes",
                            "2"});
#line 71
   testRunner.And("the \"ForEachTest\" number \'1\' in WorkFlow \"WFWithForEachWithManualLoop\" debug inpu" +
                        "ts as", ((string)(null)), table623, "And ");
#line hidden
#line 74
      testRunner.And("the \"ForEachTest\" number \'1\' in WorkFlow \"WFWithForEachWithManualLoop\" has \"2\" ne" +
                        "sted children", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table624 = new TechTalk.SpecFlow.Table(new string[] {
                            "#",
                            "Variable",
                            "New Value"});
                table624.AddRow(new string[] {
                            "1",
                            "[[rec().a]] =",
                            "Test"});
#line 75
   testRunner.And("the \"MyAssign\" in step 1 for \"ForEachTest\" number \'1\' debug inputs as", ((string)(null)), table624, "And ");
#line hidden
                TechTalk.SpecFlow.Table table625 = new TechTalk.SpecFlow.Table(new string[] {
                            "#",
                            ""});
                table625.AddRow(new string[] {
                            "1",
                            "[[rec(1).a]] = Test"});
#line 78
   testRunner.And("the \"MyAssign\" in step 1 for \"ForEachTest\" number \'1\' debug outputs as", ((string)(null)), table625, "And ");
#line hidden
                TechTalk.SpecFlow.Table table626 = new TechTalk.SpecFlow.Table(new string[] {
                            "#",
                            "Variable",
                            "New Value"});
                table626.AddRow(new string[] {
                            "1",
                            "[[rec().a]] =",
                            "Test"});
#line 81
   testRunner.And("the \"MyAssign\" in step 2 for \"ForEachTest\" number \'1\' debug inputs as", ((string)(null)), table626, "And ");
#line hidden
                TechTalk.SpecFlow.Table table627 = new TechTalk.SpecFlow.Table(new string[] {
                            "#",
                            ""});
                table627.AddRow(new string[] {
                            "1",
                            "[[rec(2).a]] = Test"});
#line 84
   testRunner.And("the \"MyAssign\" in step 2 for \"ForEachTest\" number \'1\' debug outputs as", ((string)(null)), table627, "And ");
#line hidden
                TechTalk.SpecFlow.Table table628 = new TechTalk.SpecFlow.Table(new string[] {
                            "",
                            "Number"});
                table628.AddRow(new string[] {
                            "No. of Executes",
                            "2"});
#line 87
   testRunner.And("the \"ForEachTest\" number \'2\' in WorkFlow \"WFWithForEachWithManualLoop\" debug inpu" +
                        "ts as", ((string)(null)), table628, "And ");
#line hidden
#line 90
      testRunner.And("the \"ForEachTest\" number \'2\' in WorkFlow \"WFWithForEachWithManualLoop\" has \"2\" ne" +
                        "sted children", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table629 = new TechTalk.SpecFlow.Table(new string[] {
                            "#",
                            "Variable",
                            "New Value"});
                table629.AddRow(new string[] {
                            "1",
                            "[[rec().a]] =",
                            "Test"});
#line 91
   testRunner.And("the \"MyAssign\" in step 1 for \"ForEachTest\" number \'2\' debug inputs as", ((string)(null)), table629, "And ");
#line hidden
                TechTalk.SpecFlow.Table table630 = new TechTalk.SpecFlow.Table(new string[] {
                            "#",
                            ""});
                table630.AddRow(new string[] {
                            "1",
                            "[[rec(3).a]] = Test"});
#line 94
   testRunner.And("the \"MyAssign\" in step 1 for \"ForEachTest\" number \'2\' debug outputs as", ((string)(null)), table630, "And ");
#line hidden
                TechTalk.SpecFlow.Table table631 = new TechTalk.SpecFlow.Table(new string[] {
                            "#",
                            "Variable",
                            "New Value"});
                table631.AddRow(new string[] {
                            "1",
                            "[[rec().a]] =",
                            "Test"});
#line 97
   testRunner.And("the \"MyAssign\" in step 2 for \"ForEachTest\" number \'2\' debug inputs as", ((string)(null)), table631, "And ");
#line hidden
                TechTalk.SpecFlow.Table table632 = new TechTalk.SpecFlow.Table(new string[] {
                            "#",
                            ""});
                table632.AddRow(new string[] {
                            "1",
                            "[[rec(4).a]] = Test"});
#line 100
   testRunner.And("the \"MyAssign\" in step 2 for \"ForEachTest\" number \'2\' debug outputs as", ((string)(null)), table632, "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
