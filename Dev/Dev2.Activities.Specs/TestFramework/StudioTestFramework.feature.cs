﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.1.0.0
//      SpecFlow Generator Version:2.0.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace Dev2.Activities.Specs.TestFramework
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.1.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute()]
    public partial class StudioTestFrameworkFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "StudioTestFramework.feature"
#line hidden
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static void FeatureSetup(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner(null, 0);
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "StudioTestFramework", "\tIn order to test workflows in warewolf \r\n\tAs a user\r\n\tI want to create, edit, de" +
                    "lete and update tests in a test window", ProgrammingLanguage.CSharp, ((string[])(null)));
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
                        && (testRunner.FeatureContext.FeatureInfo.Title != "StudioTestFramework")))
            {
                Dev2.Activities.Specs.TestFramework.StudioTestFrameworkFeature.FeatureSetup(null);
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
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        public virtual void FeatureBackground()
        {
#line 7
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Input Var Name"});
            table1.AddRow(new string[] {
                        "[[a]]"});
#line 8
   testRunner.Given("I have \"Workflow 1\" with inputs as", ((string)(null)), table1, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Ouput Var Name"});
            table2.AddRow(new string[] {
                        "[[outputValue]]"});
#line 11
   testRunner.And("\"Workflow 1\" has outputs as", ((string)(null)), table2, "And ");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "Input Var Name"});
            table3.AddRow(new string[] {
                        "[[rec().a]]"});
            table3.AddRow(new string[] {
                        "[[rec().b]]"});
#line 14
   testRunner.Given("I have \"Workflow 2\" with inputs as", ((string)(null)), table3, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "Ouput Var Name"});
            table4.AddRow(new string[] {
                        "[[returnVal]]"});
#line 18
   testRunner.And("\"Workflow 2\" has outputs as", ((string)(null)), table4, "And ");
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "Input Var Name"});
            table5.AddRow(new string[] {
                        "[[A]]"});
            table5.AddRow(new string[] {
                        "[[B]]"});
            table5.AddRow(new string[] {
                        "[[C]]"});
#line 21
    testRunner.Given("I have \"Workflow 3\" with inputs as", ((string)(null)), table5, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "Ouput Var Name"});
            table6.AddRow(new string[] {
                        "[[message]]"});
#line 26
   testRunner.And("\"Workflow 3\" has outputs as", ((string)(null)), table6, "And ");
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Create New Test")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "StudioTestFramework")]
        public virtual void CreateNewTest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Create New Test", ((string[])(null)));
#line 30
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 31
 testRunner.Given("the test builder is open with \"Workflow 1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 32
 testRunner.And("Tab Header is \"Workflow 1 - Tests\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 33
 testRunner.And("there are no tests", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 34
 testRunner.When("I click New Test", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 35
 testRunner.Then("a new test is added", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 37
 testRunner.And("test name starts with \"Test 1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 38
 testRunner.And("username is blank", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 39
 testRunner.And("password is blank", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                        "Variable Name",
                        "Value"});
            table7.AddRow(new string[] {
                        "a",
                        ""});
#line 40
 testRunner.And("inputs are", ((string)(null)), table7, "And ");
#line hidden
            TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                        "Variable Name",
                        "Value"});
            table8.AddRow(new string[] {
                        "outputValue",
                        ""});
#line 43
 testRunner.And("outputs as", ((string)(null)), table8, "And ");
#line 46
 testRunner.And("save is enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 47
 testRunner.And("test status is pending", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 48
 testRunner.And("test is enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Create New Test with Service that as recordset inputs")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "StudioTestFramework")]
        public virtual void CreateNewTestWithServiceThatAsRecordsetInputs()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Create New Test with Service that as recordset inputs", ((string[])(null)));
#line 50
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 51
 testRunner.Given("the test builder is open with \"Workflow 2\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 52
 testRunner.And("Tab Header is \"Workflow 2 - Tests\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 53
 testRunner.And("there are no tests", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 54
 testRunner.When("I click New Test", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 55
 testRunner.Then("a new test is added", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 57
 testRunner.And("test name starts with \"Test 1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 58
 testRunner.And("username is blank", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 59
 testRunner.And("password is blank", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table9 = new TechTalk.SpecFlow.Table(new string[] {
                        "Variable Name",
                        "Value"});
            table9.AddRow(new string[] {
                        "rec(1).a",
                        ""});
            table9.AddRow(new string[] {
                        "rec(1).b",
                        ""});
#line 60
 testRunner.And("inputs are", ((string)(null)), table9, "And ");
#line hidden
            TechTalk.SpecFlow.Table table10 = new TechTalk.SpecFlow.Table(new string[] {
                        "Variable Name",
                        "Value"});
            table10.AddRow(new string[] {
                        "rec(1).a",
                        "val1"});
            table10.AddRow(new string[] {
                        "rec(1).b",
                        ""});
#line 64
 testRunner.When("I updated the inputs as", ((string)(null)), table10, "When ");
#line hidden
            TechTalk.SpecFlow.Table table11 = new TechTalk.SpecFlow.Table(new string[] {
                        "Variable Name",
                        "Value"});
            table11.AddRow(new string[] {
                        "rec(1).a",
                        "val1"});
            table11.AddRow(new string[] {
                        "rec(1).b",
                        ""});
            table11.AddRow(new string[] {
                        "rec(2).a",
                        ""});
            table11.AddRow(new string[] {
                        "rec(2).b",
                        ""});
#line 68
 testRunner.Then("inputs are", ((string)(null)), table11, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Save a New Test")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "StudioTestFramework")]
        public virtual void SaveANewTest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Save a New Test", ((string[])(null)));
#line 75
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 76
 testRunner.Given("the test builder is open with \"Workflow 1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 77
 testRunner.And("Tab Header is \"Workflow 1 - Tests\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 78
 testRunner.And("there are no tests", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 79
 testRunner.And("I click New Test", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 80
 testRunner.Then("a new test is added", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 82
 testRunner.And("test name starts with \"Test 1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table12 = new TechTalk.SpecFlow.Table(new string[] {
                        "Variable Name",
                        "Value"});
            table12.AddRow(new string[] {
                        "a",
                        ""});
#line 83
 testRunner.And("inputs are", ((string)(null)), table12, "And ");
#line hidden
            TechTalk.SpecFlow.Table table13 = new TechTalk.SpecFlow.Table(new string[] {
                        "Variable Name",
                        "Value"});
            table13.AddRow(new string[] {
                        "outputValue",
                        ""});
#line 86
 testRunner.And("outputs as", ((string)(null)), table13, "And ");
#line 89
 testRunner.And("save is enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 90
 testRunner.When("I save", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 91
 testRunner.Then("Tab Header is \"Workflow 1 - Tests\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 92
 testRunner.And("I close the test builder", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 93
 testRunner.When("the test builder is open with \"Workflow 1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 94
 testRunner.Then("there are 2 tests", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 95
 testRunner.And("\"Test 1\" is selected", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 96
 testRunner.And("test name starts with \"Test 1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table14 = new TechTalk.SpecFlow.Table(new string[] {
                        "Variable Name",
                        "Value"});
            table14.AddRow(new string[] {
                        "a",
                        ""});
#line 97
 testRunner.And("inputs are", ((string)(null)), table14, "And ");
#line hidden
            TechTalk.SpecFlow.Table table15 = new TechTalk.SpecFlow.Table(new string[] {
                        "Variable Name",
                        "Value"});
            table15.AddRow(new string[] {
                        "outputValue",
                        ""});
#line 100
 testRunner.And("outputs as", ((string)(null)), table15, "And ");
#line 103
 testRunner.And("save is enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Edit existing test")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "StudioTestFramework")]
        public virtual void EditExistingTest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Edit existing test", ((string[])(null)));
#line 107
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 108
 testRunner.Given("the test builder is open with \"Workflow 3\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 109
 testRunner.And("Tab Header is \"Workflow 3 - Tests\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 110
 testRunner.And("there are no tests", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 111
 testRunner.And("I click New Test", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table16 = new TechTalk.SpecFlow.Table(new string[] {
                        "TestName",
                        "AuthenticationType",
                        "Error"});
            table16.AddRow(new string[] {
                        "Test1",
                        "Windows",
                        "true"});
#line 112
 testRunner.And("I set Test Values as", ((string)(null)), table16, "And ");
#line 115
 testRunner.Then("NoErrorExpected is \"false\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 116
 testRunner.And("save is enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 117
 testRunner.When("I save", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 118
 testRunner.Then("Tab Header is \"Workflow 3 - Tests\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 119
 testRunner.When("I click New Test", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table17 = new TechTalk.SpecFlow.Table(new string[] {
                        "TestName",
                        "AuthenticationType",
                        "Error"});
            table17.AddRow(new string[] {
                        "Test2",
                        "Windows",
                        "true"});
#line 120
 testRunner.And("I set Test Values as", ((string)(null)), table17, "And ");
#line 123
 testRunner.And("save is enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 124
 testRunner.When("I save", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 125
 testRunner.Then("Tab Header is \"Workflow 3 - Tests\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 126
 testRunner.And("I close the test builder", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 127
 testRunner.When("the test builder is open with \"Workflow 3\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 128
 testRunner.Then("there are 3 tests", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 129
 testRunner.And("I select \"Test2\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table18 = new TechTalk.SpecFlow.Table(new string[] {
                        "TestName",
                        "AuthenticationType",
                        "Error"});
            table18.AddRow(new string[] {
                        "Test2",
                        "Public",
                        "true"});
#line 130
 testRunner.And("I set Test Values as", ((string)(null)), table18, "And ");
#line 133
 testRunner.When("I save", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 134
 testRunner.Then("Tab Header is \"Workflow 3 - Tests\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 135
 testRunner.And("I close the test builder", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 136
 testRunner.When("the test builder is open with \"Workflow 2\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 137
 testRunner.Then("there are 3 tests", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 138
 testRunner.And("I select \"Test2\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 139
 testRunner.And("Test name is \"Test2\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 140
 testRunner.And("Authentication is Public", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Loading exisiting Tests")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "StudioTestFramework")]
        public virtual void LoadingExisitingTests()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Loading exisiting Tests", ((string[])(null)));
#line 142
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 143
 testRunner.Given("the test builder is open with \"Workflow 3\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 144
 testRunner.And("Tab Header is \"Workflow 3 - Tests\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 145
 testRunner.And("there are no tests", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 146
 testRunner.And("I click New Test", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table19 = new TechTalk.SpecFlow.Table(new string[] {
                        "TestName",
                        "AuthenticationType",
                        "Error"});
            table19.AddRow(new string[] {
                        "Test1",
                        "Windows",
                        "true"});
#line 147
 testRunner.And("I set Test Values as", ((string)(null)), table19, "And ");
#line 150
 testRunner.Then("NoErrorExpected is \"false\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 151
 testRunner.And("save is enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 152
 testRunner.When("I save", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 153
 testRunner.Then("Tab Header is \"Workflow 3 - Tests\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 154
 testRunner.When("I click New Test", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table20 = new TechTalk.SpecFlow.Table(new string[] {
                        "TestName",
                        "AuthenticationType",
                        "Error"});
            table20.AddRow(new string[] {
                        "Test2",
                        "Windows",
                        "true"});
#line 155
 testRunner.And("I set Test Values as", ((string)(null)), table20, "And ");
#line 158
 testRunner.And("save is enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 159
 testRunner.When("I save", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 160
 testRunner.Then("Tab Header is \"Workflow 3 - Tests\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 161
 testRunner.And("I close the test builder", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 162
 testRunner.When("the test builder is open with \"Workflow 3\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 163
 testRunner.Then("there are 3 tests", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Delete an Enabled Test")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "StudioTestFramework")]
        public virtual void DeleteAnEnabledTest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Delete an Enabled Test", ((string[])(null)));
#line 166
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 167
 testRunner.Given("the test builder is open with \"Workflow 3\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 168
 testRunner.And("Tab Header is \"Workflow 3 - Tests\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 169
 testRunner.And("there are no tests", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 170
 testRunner.And("I click New Test", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table21 = new TechTalk.SpecFlow.Table(new string[] {
                        "TestName",
                        "AuthenticationType",
                        "Error"});
            table21.AddRow(new string[] {
                        "Test1",
                        "Windows",
                        "true"});
#line 171
 testRunner.And("I set Test Values as", ((string)(null)), table21, "And ");
#line 174
 testRunner.Then("NoErrorExpected is \"false\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 175
 testRunner.And("save is enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 176
 testRunner.When("I save", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 177
 testRunner.Then("Tab Header is \"Workflow 3 - Tests\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 178
 testRunner.And("there are 1 tests", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 179
 testRunner.When("I disable \"Test1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 180
 testRunner.Then("Delete is enabled for \"Test1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 181
 testRunner.When("I enable \"Test1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 182
 testRunner.Then("Delete is disabled for \"Test1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 183
 testRunner.When("I delete \"Test1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 184
 testRunner.When("The Confirmation popup is shown I click Ok", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 185
 testRunner.Then("there are no tests", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 186
 testRunner.And("I close the test builder", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 187
 testRunner.When("the test builder is open with \"Workflow 3\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 188
 testRunner.Then("there are no tests", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Duplicate a test")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "StudioTestFramework")]
        public virtual void DuplicateATest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Duplicate a test", ((string[])(null)));
#line 190
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 191
 testRunner.Given("the test builder is open with \"Workflow 1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 192
 testRunner.And("Tab Header is \"Workflow 1 - Tests\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 193
 testRunner.And("there are no tests", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 194
 testRunner.And("I click New Test", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 195
 testRunner.Then("a new test is added", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 196
 testRunner.And("Tab Header is \"Workflow 1 - Tests *\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 197
 testRunner.And("test name starts with \"Test 1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table22 = new TechTalk.SpecFlow.Table(new string[] {
                        "Variable Name",
                        "Value"});
            table22.AddRow(new string[] {
                        "a",
                        ""});
#line 198
 testRunner.And("inputs are", ((string)(null)), table22, "And ");
#line hidden
            TechTalk.SpecFlow.Table table23 = new TechTalk.SpecFlow.Table(new string[] {
                        "Variable Name",
                        "Value"});
            table23.AddRow(new string[] {
                        "outputValue",
                        ""});
#line 201
 testRunner.And("outputs as", ((string)(null)), table23, "And ");
#line 204
 testRunner.And("save is enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 205
 testRunner.When("I save", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 206
 testRunner.Then("Tab Header is \"Workflow 1 - Tests\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 207
 testRunner.And("there are 1 tests", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 208
 testRunner.When("I right click \"Test 1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 209
 testRunner.Then("Duplicate Test is visible", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 210
 testRunner.When("I click duplicate", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 211
 testRunner.Then("there are 2 tests", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 212
 testRunner.And("the duplicated tests is \"Test 1_dup\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 213
 testRunner.When("I right click \"Test 1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 214
 testRunner.Then("Duplicate Test in not Visible", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
