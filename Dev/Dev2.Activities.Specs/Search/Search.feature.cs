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
namespace Dev2.Activities.Specs.Search
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.3.2.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute()]
    public partial class SearchFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private Microsoft.VisualStudio.TestTools.UnitTesting.TestContext _testContext;
        
#line 1 "Search.feature"
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
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Search", "\tIn order to avoid silly mistakes\r\n\tAs a math idiot\r\n\tI want to be told the sum o" +
                    "f two numbers", ProgrammingLanguage.CSharp, new string[] {
                        "WarewolfSearch"});
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
                        && (testRunner.FeatureContext.FeatureInfo.Title != "Search")))
            {
                global::Dev2.Activities.Specs.Search.SearchFeature.FeatureSetup(null);
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
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Search Workflow Name")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Search")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WarewolfSearch")]
        public virtual void SearchWorkflowName()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Search Workflow Name", ((string[])(null)));
#line 7
this.ScenarioSetup(scenarioInfo);
#line 8
 testRunner.Given("I have a localhost server", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 9
 testRunner.And("I have the Search View open", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 10
 testRunner.And("I check the \"IsWorkflowNameSelected\" checkbox", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 11
 testRunner.When("I search for \"SearchWorkflowForSpecs\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table923 = new TechTalk.SpecFlow.Table(new string[] {
                        "ResourceId",
                        "Name",
                        "Path",
                        "Type",
                        "Match"});
            table923.AddRow(new string[] {
                        "c494711c-c6a4-44d5-abb9-c0339cd88bae",
                        "SearchWorkflowForSpecs",
                        "SearchFolderForSpecs\\SearchWorkflowForSpecs",
                        "WorkflowName",
                        "SearchWorkflowForSpecs"});
#line 12
 testRunner.Then("the search result contains", ((string)(null)), table923, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Search TestName Name")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Search")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WarewolfSearch")]
        public virtual void SearchTestNameName()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Search TestName Name", ((string[])(null)));
#line 16
this.ScenarioSetup(scenarioInfo);
#line 17
 testRunner.Given("I have a localhost server", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 18
 testRunner.And("I have the Search View open", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 19
 testRunner.And("I check the \"IsTestNameSelected\" checkbox", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 20
 testRunner.When("I search for \"TestForSearchSpecs\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table924 = new TechTalk.SpecFlow.Table(new string[] {
                        "ResourceId",
                        "Name",
                        "Path",
                        "Type",
                        "Match"});
            table924.AddRow(new string[] {
                        "c494711c-c6a4-44d5-abb9-c0339cd88bae",
                        "SearchWorkflowForSpecs",
                        "SearchFolderForSpecs\\SearchWorkflowForSpecs",
                        "TestName",
                        "TestForSearchSpecs"});
#line 21
 testRunner.Then("the search result contains", ((string)(null)), table924, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Search ScalarName Name")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Search")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WarewolfSearch")]
        public virtual void SearchScalarNameName()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Search ScalarName Name", ((string[])(null)));
#line 25
this.ScenarioSetup(scenarioInfo);
#line 26
 testRunner.Given("I have a localhost server", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 27
 testRunner.And("I have the Search View open", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 28
 testRunner.And("I check the \"IsScalarNameSelected\" checkbox", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 29
 testRunner.When("I search for \"SearchVar\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table925 = new TechTalk.SpecFlow.Table(new string[] {
                        "ResourceId",
                        "Name",
                        "Path",
                        "Type",
                        "Match"});
            table925.AddRow(new string[] {
                        "c494711c-c6a4-44d5-abb9-c0339cd88bae",
                        "SearchWorkflowForSpecs",
                        "SearchFolderForSpecs\\SearchWorkflowForSpecs",
                        "Scalar",
                        "SearchVar"});
#line 30
 testRunner.Then("the search result contains", ((string)(null)), table925, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Search ObjectName Name")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Search")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WarewolfSearch")]
        public virtual void SearchObjectNameName()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Search ObjectName Name", ((string[])(null)));
#line 34
this.ScenarioSetup(scenarioInfo);
#line 35
 testRunner.Given("I have a localhost server", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 36
 testRunner.And("I have the Search View open", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 37
 testRunner.And("I check the \"IsObjectNameSelected\" checkbox", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 38
 testRunner.When("I search for \"SearchObject\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table926 = new TechTalk.SpecFlow.Table(new string[] {
                        "ResourceId",
                        "Name",
                        "Path",
                        "Type",
                        "Match"});
            table926.AddRow(new string[] {
                        "c494711c-c6a4-44d5-abb9-c0339cd88bae",
                        "SearchWorkflowForSpecs",
                        "SearchFolderForSpecs\\SearchWorkflowForSpecs",
                        "Object",
                        "@SearchObject"});
#line 39
 testRunner.Then("the search result contains", ((string)(null)), table926, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Search RecSetName Name")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Search")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WarewolfSearch")]
        public virtual void SearchRecSetNameName()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Search RecSetName Name", ((string[])(null)));
#line 43
this.ScenarioSetup(scenarioInfo);
#line 44
 testRunner.Given("I have a localhost server", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 45
 testRunner.And("I have the Search View open", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 46
 testRunner.And("I check the \"IsRecSetNameSelected\" checkbox", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 47
 testRunner.When("I search for \"SearchRec\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table927 = new TechTalk.SpecFlow.Table(new string[] {
                        "ResourceId",
                        "Name",
                        "Path",
                        "Type",
                        "Match"});
            table927.AddRow(new string[] {
                        "c494711c-c6a4-44d5-abb9-c0339cd88bae",
                        "SearchWorkflowForSpecs",
                        "SearchFolderForSpecs\\SearchWorkflowForSpecs",
                        "RecordSet",
                        "SearchRec"});
#line 48
 testRunner.Then("the search result contains", ((string)(null)), table927, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Search ToolTitle Name")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Search")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WarewolfSearch")]
        public virtual void SearchToolTitleName()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Search ToolTitle Name", ((string[])(null)));
#line 52
this.ScenarioSetup(scenarioInfo);
#line 53
 testRunner.Given("I have a localhost server", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 54
 testRunner.And("I have the Search View open", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 55
 testRunner.And("I check the \"IsToolTitleSelected\" checkbox", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 56
 testRunner.When("I search for \"Search Tool\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table928 = new TechTalk.SpecFlow.Table(new string[] {
                        "ResourceId",
                        "Name",
                        "Path",
                        "Type",
                        "Match"});
            table928.AddRow(new string[] {
                        "c494711c-c6a4-44d5-abb9-c0339cd88bae",
                        "SearchWorkflowForSpecs",
                        "SearchFolderForSpecs\\SearchWorkflowForSpecs",
                        "ToolTitle",
                        "Search Tool"});
#line 57
 testRunner.Then("the search result contains", ((string)(null)), table928, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Search InputVariable Name")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Search")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WarewolfSearch")]
        public virtual void SearchInputVariableName()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Search InputVariable Name", ((string[])(null)));
#line 61
this.ScenarioSetup(scenarioInfo);
#line 62
 testRunner.Given("I have a localhost server", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 63
 testRunner.And("I have the Search View open", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 64
 testRunner.And("I check the \"IsInputVariableSelected\" checkbox", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 65
 testRunner.When("I search for \"SearchVar\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table929 = new TechTalk.SpecFlow.Table(new string[] {
                        "ResourceId",
                        "Name",
                        "Path",
                        "Type",
                        "Match"});
            table929.AddRow(new string[] {
                        "c494711c-c6a4-44d5-abb9-c0339cd88bae",
                        "SearchWorkflowForSpecs",
                        "SearchFolderForSpecs\\SearchWorkflowForSpecs",
                        "ScalarInput",
                        "SearchVar"});
#line 66
 testRunner.Then("the search result contains", ((string)(null)), table929, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Search OutputVariable Name")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Search")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WarewolfSearch")]
        public virtual void SearchOutputVariableName()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Search OutputVariable Name", ((string[])(null)));
#line 70
this.ScenarioSetup(scenarioInfo);
#line 71
 testRunner.Given("I have a localhost server", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 72
 testRunner.And("I have the Search View open", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 73
 testRunner.And("I check the \"IsOutputVariableSelected\" checkbox", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 74
 testRunner.When("I search for \"SearchRec\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table930 = new TechTalk.SpecFlow.Table(new string[] {
                        "ResourceId",
                        "Name",
                        "Path",
                        "Type",
                        "Match"});
            table930.AddRow(new string[] {
                        "c494711c-c6a4-44d5-abb9-c0339cd88bae",
                        "SearchWorkflowForSpecs",
                        "SearchFolderForSpecs\\SearchWorkflowForSpecs",
                        "RecordSetOutput",
                        "SearchRec"});
#line 75
 testRunner.Then("the search result contains", ((string)(null)), table930, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
