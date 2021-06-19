// ------------------------------------------------------------------------------
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
namespace Warewolf.UIBindingTests.Variables
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.3.2.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute()]
    public partial class VariableListFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private Microsoft.VisualStudio.TestTools.UnitTesting.TestContext _testContext;
        
#line 1 "VariableList.feature"
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
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "VariableList", "\tIn order to manage my variables\r\n\tAs a Warewolf user\r\n\tI want to be told shown a" +
                    "ll variables in my workflow service", ProgrammingLanguage.CSharp, new string[] {
                        "CannotParallelize",
                        "VariableList"});
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
                        && (testRunner.FeatureContext.FeatureInfo.Title != "VariableList")))
            {
                global::Warewolf.UIBindingTests.Variables.VariableListFeature.FeatureSetup(null);
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
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Variables adding in variable list and removing unused")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "VariableList")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("CannotParallelize")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("VariableList")]
        public virtual void VariablesAddingInVariableListAndRemovingUnused()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Variables adding in variable list and removing unused", new string[] {
                        "MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15." +
                            "1.dll",
                        "MSTest:DeploymentItem:Warewolf_Studio.exe",
                        "MSTest:DeploymentItem:Newtonsoft.Json.dll",
                        "MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll",
                        "MSTest:DeploymentItem:System.Windows.Interactivity.dll"});
#line 30
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Variable",
                        "Note",
                        "Input",
                        "Output",
                        "IsUsed"});
            table1.AddRow(new string[] {
                        "[[rec().a]]",
                        "This is recordset",
                        "",
                        "YES",
                        "YES"});
            table1.AddRow(new string[] {
                        "[[rec().b]]",
                        "",
                        "",
                        "",
                        ""});
            table1.AddRow(new string[] {
                        "[[mr()]]",
                        "",
                        "",
                        "",
                        "YES"});
            table1.AddRow(new string[] {
                        "[[Var]]",
                        "",
                        "YES",
                        "",
                        "YES"});
            table1.AddRow(new string[] {
                        "[[a]]",
                        "",
                        "",
                        "",
                        ""});
            table1.AddRow(new string[] {
                        "[[lr().a]]",
                        "",
                        "",
                        "",
                        ""});
#line 31
 testRunner.Given("I have variables as", ((string)(null)), table1, "Given ");
#line 39
 testRunner.Then("\"Variables\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 40
 testRunner.And("variables filter box is \"Visible\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 41
 testRunner.And("\"Filter Clear\" is \"Disabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 42
 testRunner.And("\"Delete Variables\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 43
 testRunner.And("\"Sort Variables\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Variable Name",
                        "Delete IsEnabled",
                        "Note Highlighted",
                        "Input",
                        "Output"});
            table2.AddRow(new string[] {
                        "Var",
                        "",
                        "",
                        "YES",
                        ""});
            table2.AddRow(new string[] {
                        "a",
                        "YES",
                        "",
                        "",
                        ""});
#line 44
 testRunner.And("the Variable Names are", ((string)(null)), table2, "And ");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "Recordset Name",
                        "Delete IsEnabled",
                        "Note Highlighted",
                        "Input",
                        "Output"});
            table3.AddRow(new string[] {
                        "rec()",
                        "",
                        "",
                        "",
                        ""});
            table3.AddRow(new string[] {
                        "rec().a",
                        "",
                        "YES",
                        "",
                        "YES"});
            table3.AddRow(new string[] {
                        "rec().b",
                        "YES",
                        "",
                        "",
                        ""});
            table3.AddRow(new string[] {
                        "mr()",
                        "",
                        "",
                        "",
                        ""});
            table3.AddRow(new string[] {
                        "lr()",
                        "YES",
                        "",
                        "",
                        ""});
            table3.AddRow(new string[] {
                        "lr().a",
                        "YES",
                        "",
                        "",
                        ""});
#line 48
 testRunner.And("the Recordset Names are", ((string)(null)), table3, "And ");
#line 56
 testRunner.When("I click \"Delete Variables\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "Variable Name",
                        "Delete IsEnabled",
                        "Note Highlighted",
                        "Input",
                        "Output"});
            table4.AddRow(new string[] {
                        "Var",
                        "",
                        "",
                        "YES",
                        ""});
#line 57
 testRunner.And("the Variable Names are", ((string)(null)), table4, "And ");
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "Recordset Name",
                        "Delete IsEnabled",
                        "Note Highlighted",
                        "Input",
                        "Output"});
            table5.AddRow(new string[] {
                        "mr()",
                        "",
                        "",
                        "",
                        ""});
            table5.AddRow(new string[] {
                        "rec()",
                        "",
                        "",
                        "",
                        ""});
            table5.AddRow(new string[] {
                        "rec().a",
                        "",
                        "YES",
                        "",
                        "YES"});
#line 60
 testRunner.And("the Recordset Names are", ((string)(null)), table5, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Sorting Variables in Variable list")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "VariableList")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("CannotParallelize")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("VariableList")]
        public virtual void SortingVariablesInVariableList()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sorting Variables in Variable list", new string[] {
                        "MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15." +
                            "1.dll",
                        "MSTest:DeploymentItem:Warewolf_Studio.exe",
                        "MSTest:DeploymentItem:Newtonsoft.Json.dll",
                        "MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll",
                        "MSTest:DeploymentItem:System.Windows.Interactivity.dll"});
#line 71
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "Variable",
                        "Note",
                        "Input",
                        "Output",
                        "IsUsed"});
            table6.AddRow(new string[] {
                        "[[rec().a]]",
                        "This is recordset",
                        "",
                        "YES",
                        "YES"});
            table6.AddRow(new string[] {
                        "[[rec().b]]",
                        "",
                        "",
                        "",
                        ""});
            table6.AddRow(new string[] {
                        "[[mr()]]",
                        "",
                        "",
                        "",
                        "YES"});
            table6.AddRow(new string[] {
                        "[[Var]]",
                        "",
                        "YES",
                        "",
                        "YES"});
            table6.AddRow(new string[] {
                        "[[a]]",
                        "",
                        "",
                        "",
                        ""});
            table6.AddRow(new string[] {
                        "[[lr().a]]",
                        "",
                        "",
                        "",
                        ""});
#line 72
 testRunner.Given("I have variables as", ((string)(null)), table6, "Given ");
#line 80
 testRunner.When("I Sort the variables", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                        "Variable Name",
                        "Delete IsEnabled",
                        "Note Highlighted",
                        "Input",
                        "Output"});
            table7.AddRow(new string[] {
                        "a",
                        "YES",
                        "",
                        "",
                        ""});
            table7.AddRow(new string[] {
                        "Var",
                        "",
                        "",
                        "YES",
                        ""});
#line 81
 testRunner.And("the Variable Names are", ((string)(null)), table7, "And ");
#line hidden
            TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                        "Recordset Name",
                        "Delete IsEnabled",
                        "Note Highlighted",
                        "Input",
                        "Output"});
            table8.AddRow(new string[] {
                        "lr()",
                        "YES",
                        "",
                        "",
                        ""});
            table8.AddRow(new string[] {
                        "lr().a",
                        "YES",
                        "",
                        "",
                        ""});
            table8.AddRow(new string[] {
                        "mr()",
                        "",
                        "",
                        "",
                        ""});
            table8.AddRow(new string[] {
                        "rec()",
                        "",
                        "",
                        "",
                        ""});
            table8.AddRow(new string[] {
                        "rec().a",
                        "",
                        "YES",
                        "",
                        "YES"});
            table8.AddRow(new string[] {
                        "rec().b",
                        "YES",
                        "",
                        "",
                        ""});
#line 85
 testRunner.And("the Recordset Names are", ((string)(null)), table8, "And ");
#line 93
 testRunner.When("I Sort the variables", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table9 = new TechTalk.SpecFlow.Table(new string[] {
                        "Variable Name",
                        "Delete IsEnabled",
                        "Note Highlighted",
                        "Input",
                        "Output"});
            table9.AddRow(new string[] {
                        "Var",
                        "",
                        "",
                        "YES",
                        ""});
            table9.AddRow(new string[] {
                        "a",
                        "YES",
                        "",
                        "",
                        ""});
#line 94
 testRunner.And("the Variable Names are", ((string)(null)), table9, "And ");
#line hidden
            TechTalk.SpecFlow.Table table10 = new TechTalk.SpecFlow.Table(new string[] {
                        "Recordset Name",
                        "Delete IsEnabled",
                        "Note Highlighted",
                        "Input",
                        "Output"});
            table10.AddRow(new string[] {
                        "rec()",
                        "",
                        "",
                        "",
                        ""});
            table10.AddRow(new string[] {
                        "rec().b",
                        "YES",
                        "",
                        "",
                        ""});
            table10.AddRow(new string[] {
                        "rec().a",
                        "",
                        "YES",
                        "",
                        "YES"});
            table10.AddRow(new string[] {
                        "mr()",
                        "",
                        "",
                        "",
                        ""});
            table10.AddRow(new string[] {
                        "lr()",
                        "YES",
                        "",
                        "",
                        ""});
            table10.AddRow(new string[] {
                        "lr().a",
                        "YES",
                        "",
                        "",
                        ""});
#line 98
 testRunner.And("the Recordset Names are", ((string)(null)), table10, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Variable Errors")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "VariableList")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("CannotParallelize")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("VariableList")]
        public virtual void VariableErrors()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Variable Errors", new string[] {
                        "MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15." +
                            "1.dll",
                        "MSTest:DeploymentItem:Warewolf_Studio.exe",
                        "MSTest:DeploymentItem:Newtonsoft.Json.dll",
                        "MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll",
                        "MSTest:DeploymentItem:System.Windows.Interactivity.dll"});
#line 112
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table11 = new TechTalk.SpecFlow.Table(new string[] {
                        "Variable",
                        "Error State",
                        "Delete IsEnabled",
                        "Error Tooltip"});
            table11.AddRow(new string[] {
                        "a",
                        "",
                        "",
                        ""});
            table11.AddRow(new string[] {
                        "1b",
                        "YES",
                        "",
                        "Variables must begin with alphabetical characters"});
            table11.AddRow(new string[] {
                        "b@",
                        "YES",
                        "",
                        "Variables contains invalid character"});
            table11.AddRow(new string[] {
                        "b1",
                        "",
                        "",
                        ""});
#line 113
 testRunner.Given("the Variable Names are", ((string)(null)), table11, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table12 = new TechTalk.SpecFlow.Table(new string[] {
                        "Recordset Name",
                        "Error State",
                        "Delete IsEnabled",
                        "Error Tooltip"});
            table12.AddRow(new string[] {
                        "1r()",
                        "YES",
                        "",
                        "Recordset names must begin with alphabetical characters"});
            table12.AddRow(new string[] {
                        "1r().a",
                        "",
                        "",
                        ""});
            table12.AddRow(new string[] {
                        "rec()",
                        "",
                        "",
                        ""});
            table12.AddRow(new string[] {
                        "rec().a",
                        "",
                        "",
                        ""});
            table12.AddRow(new string[] {
                        "rec().1a",
                        "YES",
                        "",
                        "Recordset fields must begin with alphabetical characters"});
            table12.AddRow(new string[] {
                        "rec().b",
                        "YES",
                        "",
                        "Duplicate Variable"});
            table12.AddRow(new string[] {
                        "rec().b",
                        "YES",
                        "",
                        "Duplicate Variable"});
#line 119
 testRunner.And("the Recordset Names are", ((string)(null)), table12, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Variables removed from design surface and list")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "VariableList")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("CannotParallelize")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("VariableList")]
        public virtual void VariablesRemovedFromDesignSurfaceAndList()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Variables removed from design surface and list", new string[] {
                        "MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15." +
                            "1.dll",
                        "MSTest:DeploymentItem:Warewolf_Studio.exe",
                        "MSTest:DeploymentItem:Newtonsoft.Json.dll",
                        "MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll",
                        "MSTest:DeploymentItem:System.Windows.Interactivity.dll"});
#line 134
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table13 = new TechTalk.SpecFlow.Table(new string[] {
                        "Variable",
                        "Note",
                        "Input",
                        "Output",
                        "IsUsed"});
            table13.AddRow(new string[] {
                        "[[rec().a]]",
                        "This is recordset",
                        "",
                        "YES",
                        "YES"});
            table13.AddRow(new string[] {
                        "[[rec().b]]",
                        "",
                        "",
                        "",
                        ""});
            table13.AddRow(new string[] {
                        "[[mr()]]",
                        "",
                        "",
                        "",
                        ""});
            table13.AddRow(new string[] {
                        "[[Var]]",
                        "",
                        "YES",
                        "",
                        "YES"});
            table13.AddRow(new string[] {
                        "[[a]]",
                        "",
                        "",
                        "",
                        ""});
            table13.AddRow(new string[] {
                        "[[lr().a]]",
                        "",
                        "",
                        "",
                        ""});
#line 135
 testRunner.Given("I have variables as", ((string)(null)), table13, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table14 = new TechTalk.SpecFlow.Table(new string[] {
                        "Variable Name",
                        "Delete IsEnabled",
                        "Note Highlighted",
                        "Input",
                        "Output"});
            table14.AddRow(new string[] {
                        "Var",
                        "",
                        "",
                        "YES",
                        ""});
            table14.AddRow(new string[] {
                        "a",
                        "YES",
                        "",
                        "",
                        ""});
#line 143
 testRunner.And("the Variable Names are", ((string)(null)), table14, "And ");
#line hidden
            TechTalk.SpecFlow.Table table15 = new TechTalk.SpecFlow.Table(new string[] {
                        "Recordset Name",
                        "Delete IsEnabled",
                        "Note Highlighted",
                        "Input",
                        "Output"});
            table15.AddRow(new string[] {
                        "rec()",
                        "",
                        "",
                        "",
                        ""});
            table15.AddRow(new string[] {
                        "rec().a",
                        "",
                        "YES",
                        "",
                        "YES"});
            table15.AddRow(new string[] {
                        "rec().b",
                        "YES",
                        "",
                        "",
                        ""});
            table15.AddRow(new string[] {
                        "mr()",
                        "",
                        "",
                        "",
                        ""});
            table15.AddRow(new string[] {
                        "lr()",
                        "YES",
                        "",
                        "",
                        ""});
            table15.AddRow(new string[] {
                        "lr().a",
                        "YES",
                        "",
                        "",
                        ""});
#line 147
 testRunner.And("the Recordset Names are", ((string)(null)), table15, "And ");
#line 155
 testRunner.And("I click delete for \"[[a]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 156
 testRunner.And("I click delete for \"mr()\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table16 = new TechTalk.SpecFlow.Table(new string[] {
                        "Variable Name",
                        "Delete IsEnabled",
                        "Note Highlighted",
                        "Input",
                        "Output"});
            table16.AddRow(new string[] {
                        "a",
                        "YES",
                        "",
                        "",
                        ""});
#line 157
 testRunner.And("the Variable Names are", ((string)(null)), table16, "And ");
#line hidden
            TechTalk.SpecFlow.Table table17 = new TechTalk.SpecFlow.Table(new string[] {
                        "Recordset Name",
                        "Delete IsEnabled",
                        "Note Highlighted",
                        "Input",
                        "Output"});
            table17.AddRow(new string[] {
                        "rec()",
                        "",
                        "",
                        "",
                        ""});
            table17.AddRow(new string[] {
                        "rec().a",
                        "",
                        "YES",
                        "",
                        "YES"});
            table17.AddRow(new string[] {
                        "rec().b",
                        "YES",
                        "",
                        "",
                        ""});
            table17.AddRow(new string[] {
                        "lr()",
                        "YES",
                        "",
                        "",
                        ""});
            table17.AddRow(new string[] {
                        "lr().a",
                        "YES",
                        "",
                        "",
                        ""});
#line 160
 testRunner.And("the Recordset Names are", ((string)(null)), table17, "And ");
#line 167
 testRunner.And("I change variable Name from \"a\" to \"\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 168
 testRunner.And("I change Recordset Name from \"rec()\" to \"this\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table18 = new TechTalk.SpecFlow.Table(new string[] {
                        "Variable Name",
                        "Delete IsEnabled",
                        "Note Highlighted",
                        "Input",
                        "Output"});
#line 169
 testRunner.And("the Variable Names are", ((string)(null)), table18, "And ");
#line hidden
            TechTalk.SpecFlow.Table table19 = new TechTalk.SpecFlow.Table(new string[] {
                        "Recordset Name",
                        "Delete IsEnabled",
                        "Note Highlighted",
                        "Input",
                        "Output"});
            table19.AddRow(new string[] {
                        "this()",
                        "YES",
                        "",
                        "",
                        ""});
            table19.AddRow(new string[] {
                        "this().a",
                        "YES",
                        "YES",
                        "",
                        "YES"});
            table19.AddRow(new string[] {
                        "this().b",
                        "YES",
                        "",
                        "",
                        ""});
            table19.AddRow(new string[] {
                        "lr()",
                        "YES",
                        "",
                        "",
                        ""});
            table19.AddRow(new string[] {
                        "lr().a",
                        "YES",
                        "",
                        "",
                        ""});
            table19.AddRow(new string[] {
                        "rec()",
                        "",
                        "",
                        "",
                        ""});
            table19.AddRow(new string[] {
                        "rec().a",
                        "",
                        "YES",
                        "",
                        "YES"});
#line 171
 testRunner.And("the Recordset Names are", ((string)(null)), table19, "And ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
