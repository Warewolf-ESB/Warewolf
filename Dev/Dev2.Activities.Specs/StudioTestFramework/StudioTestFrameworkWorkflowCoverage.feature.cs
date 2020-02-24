﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.3.2.0
//      SpecFlow Generator Version:2.3.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace Dev2.Activities.Specs.StudioTestFramework
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.3.2.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute()]
    public partial class StudioTestFrameworkWorkflowCoverageFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private Microsoft.VisualStudio.TestTools.UnitTesting.TestContext _testContext;
        
#line 1 "StudioTestFrameworkWorkflowCoverage.feature"
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
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner(null, 0);
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "StudioTestFrameworkWorkflowCoverage", "\tIn order to able to tell which nodes of the workflow has coverage\r\n\tAs a warewol" +
                    "f user\r\n\tI want to be able to generate test coverage results", ProgrammingLanguage.CSharp, ((string[])(null)));
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
                        && (testRunner.FeatureContext.FeatureInfo.Title != "StudioTestFrameworkWorkflowCoverage")))
            {
                global::Dev2.Activities.Specs.StudioTestFramework.StudioTestFrameworkWorkflowCoverageFeature.FeatureSetup(null);
            }
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Microsoft.VisualStudio.TestTools.UnitTesting.TestContext>(TestContext);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Run an individual test to show partial coverage of nodes")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "StudioTestFrameworkWorkflowCoverage")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("StudioTestFrameworkWorkflowCoverage")]
        public virtual void RunAnIndividualTestToShowPartialCoverageOfNodes()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Run an individual test to show partial coverage of nodes", new string[] {
                        "StudioTestFrameworkWorkflowCoverage"});
#line 7
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "node"});
            table1.AddRow(new string[] {
                        "Assign(input)"});
            table1.AddRow(new string[] {
                        "Decision"});
            table1.AddRow(new string[] {
                        "Assign(error)"});
            table1.AddRow(new string[] {
                        "SQL"});
            table1.AddRow(new string[] {
                        "Assign(person)"});
            table1.AddRow(new string[] {
                        "SMTP Send"});
#line 8
  testRunner.Given("a workflow with below nodes", ((string)(null)), table1, "Given ");
#line 16
  testRunner.And("generate test coverage is selected", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 17
  testRunner.When("I run test \"Test Decision false branch\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 18
  testRunner.And("test coverage is generated", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "node"});
            table2.AddRow(new string[] {
                        "Assign(input)"});
            table2.AddRow(new string[] {
                        "Decision"});
            table2.AddRow(new string[] {
                        "False"});
            table2.AddRow(new string[] {
                        "Assign(error)"});
#line 19
  testRunner.Then("the covered nodes are", ((string)(null)), table2, "Then ");
#line 25
  testRunner.And("the test coverage is \"50%\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Run all tests to generate total nodes covered in workflow")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "StudioTestFrameworkWorkflowCoverage")]
        public virtual void RunAllTestsToGenerateTotalNodesCoveredInWorkflow()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Run all tests to generate total nodes covered in workflow", ((string[])(null)));
#line 27
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "name"});
            table3.AddRow(new string[] {
                        "Test Decision false branch"});
            table3.AddRow(new string[] {
                        "Test Decision true branch"});
#line 28
  testRunner.Given("saved test(s) below is run", ((string)(null)), table3, "Given ");
#line 32
  testRunner.And("generate test coverage is selected", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 33
  testRunner.When("I run all the tests", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "coverage"});
            table4.AddRow(new string[] {
                        "Test Decision false branch",
                        "35%"});
            table4.AddRow(new string[] {
                        "Test Decision true branch",
                        "50%"});
#line 34
  testRunner.And("the test coverage is", ((string)(null)), table4, "And ");
#line 38
  testRunner.Then("the total workflow test coverage is \"85%\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "node"});
            table5.AddRow(new string[] {
                        "assign(input)"});
            table5.AddRow(new string[] {
                        "Decision"});
            table5.AddRow(new string[] {
                        "False branch"});
            table5.AddRow(new string[] {
                        "Assign(error)"});
            table5.AddRow(new string[] {
                        "assign(input)"});
            table5.AddRow(new string[] {
                        "Decision"});
            table5.AddRow(new string[] {
                        "True branch"});
            table5.AddRow(new string[] {
                        "SQL"});
#line 39
  testRunner.And("the nodes covered are", ((string)(null)), table5, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Run all tests should show which nodes have no coverage reports")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "StudioTestFrameworkWorkflowCoverage")]
        public virtual void RunAllTestsShouldShowWhichNodesHaveNoCoverageReports()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Run all tests should show which nodes have no coverage reports", ((string[])(null)));
#line 50
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "name"});
            table6.AddRow(new string[] {
                        "Test Decision false branch"});
            table6.AddRow(new string[] {
                        "Test Decision true branch"});
#line 51
  testRunner.Given("saved test(s) below is run", ((string)(null)), table6, "Given ");
#line 55
  testRunner.And("I run all the tests with generate coverage selected", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                        "node"});
            table7.AddRow(new string[] {
                        "assign(person)"});
            table7.AddRow(new string[] {
                        "SMTP Send"});
#line 56
  testRunner.Then("the nodes not covered are", ((string)(null)), table7, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Test coverage summary view folders should have coverage of all workflows it conta" +
            "ins")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "StudioTestFrameworkWorkflowCoverage")]
        public virtual void TestCoverageSummaryViewFoldersShouldHaveCoverageOfAllWorkflowsItContains()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Test coverage summary view folders should have coverage of all workflows it conta" +
                    "ins", ((string[])(null)));
#line 61
this.ScenarioSetup(scenarioInfo);
#line 62
  testRunner.Given("a test coverage summary view is opened", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 63
  testRunner.When("a folder containing test coverage reports is loaded", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                        "total",
                        "passed",
                        "failed"});
            table8.AddRow(new string[] {
                        "1324",
                        "1300",
                        "24"});
#line 64
  testRunner.Then("information bar will have these values", ((string)(null)), table8, "Then ");
#line hidden
            TechTalk.SpecFlow.Table table9 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "coverage"});
            table9.AddRow(new string[] {
                        "Folder-one",
                        "70 %"});
            table9.AddRow(new string[] {
                        "Folder-two",
                        "warning: no coverage report found"});
#line 67
  testRunner.And("the per folder coverage summary is", ((string)(null)), table9, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Test coverage summary view workflows should have per workflow coverage")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "StudioTestFrameworkWorkflowCoverage")]
        public virtual void TestCoverageSummaryViewWorkflowsShouldHavePerWorkflowCoverage()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Test coverage summary view workflows should have per workflow coverage", ((string[])(null)));
#line 72
this.ScenarioSetup(scenarioInfo);
#line 73
  testRunner.Given("a test coverage summary view is opened", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 74
  testRunner.And("a folder containing test coverage reports is loaded", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table10 = new TechTalk.SpecFlow.Table(new string[] {
                        "total",
                        "passed",
                        "failed"});
            table10.AddRow(new string[] {
                        "1324",
                        "1300",
                        "24"});
#line 75
  testRunner.And("information bar will have these values", ((string)(null)), table10, "And ");
#line hidden
            TechTalk.SpecFlow.Table table11 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "coverage",
                        "branch_coverage"});
            table11.AddRow(new string[] {
                        "wf-one",
                        "85%",
                        "30%"});
            table11.AddRow(new string[] {
                        "wf-two",
                        "warning: no coverage report found",
                        "15%"});
#line 78
  testRunner.And("the per workflow coverage summary is", ((string)(null)), table11, "And ");
#line 82
  testRunner.When("I select \"wf-one\" within test coverage summary view", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table12 = new TechTalk.SpecFlow.Table(new string[] {
                        "passed",
                        "node"});
            table12.AddRow(new string[] {
                        "true",
                        "assign(input)"});
            table12.AddRow(new string[] {
                        "false",
                        "Decision"});
            table12.AddRow(new string[] {
                        "true",
                        "False branch"});
            table12.AddRow(new string[] {
                        "true",
                        "Assign(error)"});
            table12.AddRow(new string[] {
                        "true",
                        "assign(input)"});
            table12.AddRow(new string[] {
                        "true",
                        "Decision"});
            table12.AddRow(new string[] {
                        "true",
                        "True branch"});
            table12.AddRow(new string[] {
                        "true",
                        "SQL"});
            table12.AddRow(new string[] {
                        "true",
                        "assign(person)"});
            table12.AddRow(new string[] {
                        "true",
                        "SMTP Send"});
#line 83
  testRunner.Then("the workflow nodes will be as", ((string)(null)), table12, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion

