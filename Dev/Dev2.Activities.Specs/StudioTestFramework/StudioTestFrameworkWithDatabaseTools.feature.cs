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
namespace Dev2.Activities.Specs.StudioTestFramework
{
    using TechTalk.SpecFlow;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute()]
    public partial class StudioTestFrameworkWithDatabaseToolsFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private Microsoft.VisualStudio.TestTools.UnitTesting.TestContext _testContext;
        
        private static string[] featureTags = new string[] {
                "StudioTestFrameworkWithDatabaseTools"};
        
#line 1 "StudioTestFrameworkWithDatabaseTools.feature"
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
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "StudioTestFramework", "StudioTestFrameworkWithDatabaseTools", "\tIn order to test workflows that contain database tools in warewolf \r\n\tAs a user\r" +
                    "\n\tI want to create, edit, delete and update tests in a test window", ProgrammingLanguage.CSharp, featureTags);
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute()]
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute()]
        public void TestInitialize()
        {
            if (((testRunner.FeatureContext != null) 
                        && (testRunner.FeatureContext.FeatureInfo.Title != "StudioTestFrameworkWithDatabaseTools")))
            {
                global::Dev2.Activities.Specs.StudioTestFramework.StudioTestFrameworkWithDatabaseToolsFeature.FeatureSetup(null);
            }
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute()]
        public void TestTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Microsoft.VisualStudio.TestTools.UnitTesting.TestContext>(_testContext);
        }
        
        public void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        public virtual void FeatureBackground()
        {
#line 8
#line hidden
#line 9
  testRunner.Given("test folder is cleaned", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table926 = new TechTalk.SpecFlow.Table(new string[] {
                        "Input Var Name"});
            table926.AddRow(new string[] {
                        "[[a]]"});
#line 10
  testRunner.And("I have \"Workflow 1\" with inputs as", ((string)(null)), table926, "And ");
#line hidden
            TechTalk.SpecFlow.Table table927 = new TechTalk.SpecFlow.Table(new string[] {
                        "Ouput Var Name"});
            table927.AddRow(new string[] {
                        "[[outputValue]]"});
#line 13
  testRunner.And("\"Workflow 1\" has outputs as", ((string)(null)), table927, "And ");
#line hidden
            TechTalk.SpecFlow.Table table928 = new TechTalk.SpecFlow.Table(new string[] {
                        "Input Var Name"});
            table928.AddRow(new string[] {
                        "[[rec().a]]"});
            table928.AddRow(new string[] {
                        "[[rec().b]]"});
#line 16
  testRunner.Given("I have \"Workflow 2\" with inputs as", ((string)(null)), table928, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table929 = new TechTalk.SpecFlow.Table(new string[] {
                        "Ouput Var Name"});
            table929.AddRow(new string[] {
                        "[[returnVal]]"});
#line 20
  testRunner.And("\"Workflow 2\" has outputs as", ((string)(null)), table929, "And ");
#line hidden
            TechTalk.SpecFlow.Table table930 = new TechTalk.SpecFlow.Table(new string[] {
                        "Input Var Name"});
            table930.AddRow(new string[] {
                        "[[A]]"});
            table930.AddRow(new string[] {
                        "[[B]]"});
            table930.AddRow(new string[] {
                        "[[C]]"});
#line 23
  testRunner.Given("I have \"Workflow 3\" with inputs as", ((string)(null)), table930, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table931 = new TechTalk.SpecFlow.Table(new string[] {
                        "Ouput Var Name"});
            table931.AddRow(new string[] {
                        "[[message]]"});
#line 28
  testRunner.And("\"Workflow 3\" has outputs as", ((string)(null)), table931, "And ");
#line hidden
            TechTalk.SpecFlow.Table table932 = new TechTalk.SpecFlow.Table(new string[] {
                        "Input Var Name"});
            table932.AddRow(new string[] {
                        "[[input]]"});
#line 31
  testRunner.Given("I have \"WorkflowWithTests\" with inputs as", ((string)(null)), table932, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table933 = new TechTalk.SpecFlow.Table(new string[] {
                        "Ouput Var Name"});
            table933.AddRow(new string[] {
                        "[[outputValue]]"});
#line 34
  testRunner.And("\"WorkflowWithTests\" has outputs as", ((string)(null)), table933, "And ");
#line hidden
            TechTalk.SpecFlow.Table table934 = new TechTalk.SpecFlow.Table(new string[] {
                        "TestName",
                        "AuthenticationType",
                        "Error",
                        "TestFailing",
                        "TestPending",
                        "TestInvalid",
                        "TestPassed"});
            table934.AddRow(new string[] {
                        "Test1",
                        "Windows",
                        "false",
                        "false",
                        "false",
                        "false",
                        "true"});
            table934.AddRow(new string[] {
                        "Test2",
                        "Windows",
                        "false",
                        "true",
                        "false",
                        "false",
                        "false"});
            table934.AddRow(new string[] {
                        "Test3",
                        "Windows",
                        "false",
                        "false",
                        "false",
                        "true",
                        "false"});
            table934.AddRow(new string[] {
                        "Test4",
                        "Windows",
                        "false",
                        "false",
                        "true",
                        "false",
                        "false"});
#line 37
  testRunner.And("\"WorkflowWithTests\" Tests as", ((string)(null)), table934, "And ");
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Test WF with MySql")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "StudioTestFrameworkWithDatabaseTools")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("StudioTestFrameworkWithDatabaseTools")]
        public void TestWFWithMySql()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Test WF with MySql", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 45
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 8
this.FeatureBackground();
#line hidden
#line 46
  testRunner.Given("I have a workflow \"MySqlTestWF\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
                TechTalk.SpecFlow.Table table935 = new TechTalk.SpecFlow.Table(new string[] {
                            "Input to Service",
                            "From Variable",
                            "Output from Service",
                            "To Variable"});
                table935.AddRow(new string[] {
                            "",
                            "",
                            "name",
                            "[[rec(*).name]]"});
                table935.AddRow(new string[] {
                            "",
                            "",
                            "email",
                            "[[rec(*).email]]"});
#line 47
   testRunner.And("\"MySqlTestWF\" contains a mysql database service \"MySqlEmail\" with mappings for te" +
                        "sting as", ((string)(null)), table935, "And ");
#line hidden
#line 51
  testRunner.And("I save workflow \"MySqlTestWF\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 52
  testRunner.Then("the test builder is open with \"MySqlTestWF\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 53
  testRunner.And("I click New Test", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 54
  testRunner.And("a new test is added", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 55
  testRunner.And("test name starts with \"Test 1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 56
  testRunner.And("I Add \"MySqlEmail\" as TestStep", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table936 = new TechTalk.SpecFlow.Table(new string[] {
                            "Variable Name",
                            "Condition",
                            "Value"});
                table936.AddRow(new string[] {
                            "[[MySqlEmail(1).name]]",
                            "=",
                            "Monk"});
                table936.AddRow(new string[] {
                            "[[MySqlEmail(1).email]]",
                            "=",
                            "dora@explorers.com"});
#line 57
  testRunner.And("I add StepOutputs as", ((string)(null)), table936, "And ");
#line hidden
#line 61
  testRunner.When("I save", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 62
  testRunner.And("I run the test", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 63
  testRunner.Then("test result is Passed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Test WF with Sql Server")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "StudioTestFrameworkWithDatabaseTools")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("StudioTestFrameworkWithDatabaseTools")]
        public void TestWFWithSqlServer()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Test WF with Sql Server", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 65
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 8
this.FeatureBackground();
#line hidden
#line 66
  testRunner.Given("I depend on a valid MSSQL server", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 67
  testRunner.And("I have a workflow \"SqlTestWF\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table937 = new TechTalk.SpecFlow.Table(new string[] {
                            "ParameterName",
                            "ParameterValue"});
                table937.AddRow(new string[] {
                            "Prefix",
                            "D"});
#line 68
  testRunner.And("\"SqlTestWF\" contains a sqlserver database service \"dbo.Pr_CitiesGetCountries\" wit" +
                        "h mappings for testing as", ((string)(null)), table937, "And ");
#line hidden
#line 71
  testRunner.And("I save workflow \"SqlTestWF\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 72
  testRunner.Then("the test builder is open with \"SqlTestWF\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 73
  testRunner.And("I click New Test", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 74
  testRunner.And("a new test is added", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 75
  testRunner.And("test name starts with \"Test 1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 76
  testRunner.And("I Add \"dbo.Pr_CitiesGetCountries\" as TestStep", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table938 = new TechTalk.SpecFlow.Table(new string[] {
                            "Variable Name",
                            "Condition",
                            "Value"});
                table938.AddRow(new string[] {
                            "[[dbo_Pr_CitiesGetCountries(2).CountryID]]",
                            "=",
                            "40"});
                table938.AddRow(new string[] {
                            "[[dbo_Pr_CitiesGetCountries(2).Description]]",
                            "=",
                            "Djibouti"});
#line 77
  testRunner.And("I add StepOutputs as", ((string)(null)), table938, "And ");
#line hidden
#line 81
  testRunner.When("I save", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 82
  testRunner.And("I run the test", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 83
  testRunner.Then("test result is Passed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Test WF with Oracle")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "StudioTestFrameworkWithDatabaseTools")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("StudioTestFrameworkWithDatabaseTools")]
        public void TestWFWithOracle()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Test WF with Oracle", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 85
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 8
this.FeatureBackground();
#line hidden
#line 86
  testRunner.Given("I have a workflow \"oracleTestWF\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
                TechTalk.SpecFlow.Table table939 = new TechTalk.SpecFlow.Table(new string[] {
                            "ParameterName",
                            "ParameterValue"});
                table939.AddRow(new string[] {
                            "P_DEPTNO",
                            "110"});
#line 87
   testRunner.And("\"oracleTestWF\" contains a oracle database service \"HR.GET_EMP_RS\" with mappings a" +
                        "s", ((string)(null)), table939, "And ");
#line hidden
#line 90
  testRunner.And("I save workflow \"oracleTestWF\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 91
  testRunner.Then("the test builder is open with \"oracleTestWF\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 92
  testRunner.And("I click New Test", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 93
  testRunner.And("a new test is added", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 94
  testRunner.And("test name starts with \"Test 1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 95
  testRunner.And("I Add \"HR.GET_EMP_RS\" as TestStep", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table940 = new TechTalk.SpecFlow.Table(new string[] {
                            "Variable Name",
                            "Condition",
                            "Value"});
                table940.AddRow(new string[] {
                            "[[HR_GET_EMP_RS(2).EMPLOYEE_ID]]",
                            "=",
                            "205"});
                table940.AddRow(new string[] {
                            "[[HR_GET_EMP_RS(2).FIRST_NAME]]",
                            "=",
                            "Shelley"});
                table940.AddRow(new string[] {
                            "[[HR_GET_EMP_RS(2).LAST_NAME]]",
                            "=",
                            "Higgins"});
                table940.AddRow(new string[] {
                            "[[HR_GET_EMP_RS(2).EMAIL]]",
                            "=",
                            "SHIGGINS"});
#line 96
  testRunner.And("I add StepOutputs as", ((string)(null)), table940, "And ");
#line hidden
#line 102
  testRunner.When("I save", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 103
  testRunner.And("I run the test", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 104
  testRunner.Then("test result is Passed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Test WF with PostGre Sql")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "StudioTestFrameworkWithDatabaseTools")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("StudioTestFrameworkWithDatabaseTools")]
        public void TestWFWithPostGreSql()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Test WF with PostGre Sql", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 106
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 8
this.FeatureBackground();
#line hidden
#line 107
  testRunner.Given("I depend on a valid PostgreSQL server", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 108
  testRunner.And("I have a workflow \"PostGreTestWF\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table941 = new TechTalk.SpecFlow.Table(new string[] {
                            "ParameterName",
                            "ParameterValue"});
                table941.AddRow(new string[] {
                            "Prefix",
                            "K"});
#line 109
  testRunner.And("\"PostGreTestWF\" contains a postgre tool using \"get_countries\" with mappings for t" +
                        "esting as", ((string)(null)), table941, "And ");
#line hidden
#line 112
  testRunner.And("I save workflow \"PostGreTestWF\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 113
  testRunner.Then("the test builder is open with \"PostGreTestWF\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 114
  testRunner.And("I click New Test", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 115
  testRunner.And("a new test is added", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 116
  testRunner.And("test name starts with \"Test 1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 117
  testRunner.And("I Add \"get_countries\" as TestStep", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table942 = new TechTalk.SpecFlow.Table(new string[] {
                            "Variable Name",
                            "Condition",
                            "Value"});
                table942.AddRow(new string[] {
                            "[[get_countries(1).id]]",
                            "=",
                            "2"});
#line 118
  testRunner.And("I add StepOutputs as", ((string)(null)), table942, "And ");
#line hidden
#line 121
  testRunner.When("I save", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 122
  testRunner.And("I run the test", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 123
  testRunner.Then("test result is Passed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Test WF with Decision")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "StudioTestFrameworkWithDatabaseTools")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("StudioTestFrameworkWithDatabaseTools")]
        public void TestWFWithDecision()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Test WF with Decision", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 125
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 8
this.FeatureBackground();
#line hidden
#line 126
  testRunner.Given("I have a workflow \"DecisionTestWF\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
                TechTalk.SpecFlow.Table table943 = new TechTalk.SpecFlow.Table(new string[] {
                            "variable",
                            "value"});
                table943.AddRow(new string[] {
                            "[[A]]",
                            "30"});
#line 127
  testRunner.And("\"DecisionTestWF\" contains an Assign \"TestAssign\" as", ((string)(null)), table943, "And ");
#line hidden
#line 130
  testRunner.And("a decision variable \"[[A]]\" value \"30\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 131
  testRunner.And("decide if \"[[A]]\" \"IsAlphanumeric\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 132
  testRunner.And("I save workflow \"DecisionTestWF\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 133
  testRunner.Then("the test builder is open with \"DecisionTestWF\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 134
  testRunner.And("I click New Test", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 135
  testRunner.And("a new test is added", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 136
  testRunner.And("test name starts with \"Test 1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 137
  testRunner.And("I Add \"TestDecision\" as TestStep", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table944 = new TechTalk.SpecFlow.Table(new string[] {
                            "Step Name",
                            "Output Variable",
                            "Output Value",
                            "Activity Type"});
                table944.AddRow(new string[] {
                            "If [[Name]] <> (Not Equal)",
                            "Flow Arm",
                            "True",
                            "Decision"});
#line 138
  testRunner.And("I add Assert steps as", ((string)(null)), table944, "And ");
#line hidden
#line 141
  testRunner.When("I save", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 142
  testRunner.And("I run the test", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 143
  testRunner.Then("test result is Passed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Test WF with SqlBulk Insert")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "StudioTestFrameworkWithDatabaseTools")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("StudioTestFrameworkWithDatabaseTools")]
        public void TestWFWithSqlBulkInsert()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Test WF with SqlBulk Insert", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 145
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 8
this.FeatureBackground();
#line hidden
#line 146
  testRunner.Given("I depend on a valid MSSQL server", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 147
  testRunner.And("I have a workflow \"SqlBulkTestWF\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table945 = new TechTalk.SpecFlow.Table(new string[] {
                            "Column",
                            "Mapping",
                            "IsNullable",
                            "DataTypeName",
                            "MaxLength",
                            "IsAutoIncrement"});
                table945.AddRow(new string[] {
                            "Name",
                            "Warewolf",
                            "false",
                            "varchar",
                            "50",
                            "false"});
                table945.AddRow(new string[] {
                            "Email",
                            "Warewolf@dev2.co.za",
                            "false",
                            "varchar",
                            "50",
                            "false"});
#line 148
  testRunner.And("\"SqlBulkTestWF\" contains an SQL Bulk Insert \"BulkInsert\" using database \"NewSqlBu" +
                        "lkInsertSource\" and table \"dbo.MailingList\" and KeepIdentity set \"true\" and Resu" +
                        "lt set \"[[result]]\" for testing as", ((string)(null)), table945, "And ");
#line hidden
#line 152
  testRunner.And("I save workflow \"SqlBulkTestWF\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 153
  testRunner.Then("the test builder is open with \"SqlBulkTestWF\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 154
  testRunner.And("I click New Test", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 155
  testRunner.And("a new test is added", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 156
  testRunner.And("test name starts with \"Test 1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 157
  testRunner.And("I Add \"BulkInsert\" as TestStep", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table946 = new TechTalk.SpecFlow.Table(new string[] {
                            "Variable Name",
                            "Condition",
                            "Value"});
                table946.AddRow(new string[] {
                            "[[result]]",
                            "=",
                            "Success"});
#line 158
  testRunner.And("I add StepOutputs as", ((string)(null)), table946, "And ");
#line hidden
#line 161
  testRunner.When("I save", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 162
  testRunner.And("I run the test", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 163
  testRunner.Then("test result is Passed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
