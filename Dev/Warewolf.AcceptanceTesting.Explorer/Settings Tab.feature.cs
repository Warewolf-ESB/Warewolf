﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.9.0.77
//      SpecFlow Generator Version:1.9.0.0
//      Runtime Version:4.0.30319.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace Warewolf.AcceptanceTesting.Explorer
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.9.0.77")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute()]
    public partial class SettingsTabFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "Settings Tab.feature"
#line hidden
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static void FeatureSetup(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Settings Tab", "In order to avoid silly mistakes\r\nAs a math idiot\r\nI want to be told the sum of t" +
                    "wo numbers", ProgrammingLanguage.CSharp, ((string[])(null)));
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
            if (((TechTalk.SpecFlow.FeatureContext.Current != null) 
                        && (TechTalk.SpecFlow.FeatureContext.Current.FeatureInfo.Title != "Settings Tab")))
            {
                Warewolf.AcceptanceTesting.Explorer.SettingsTabFeature.FeatureSetup(null);
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
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Settings Opened")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Settings Tab")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Settings")]
        public virtual void SettingsOpened()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Settings Opened", new string[] {
                        "Settings"});
#line 7
this.ScenarioSetup(scenarioInfo);
#line 8
 testRunner.Given("I have settings tab opened", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 9
 testRunner.And("\"server\" selected as \"localhost (Connected)\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 10
 testRunner.And("Server edit is \"Disabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 11
 testRunner.And("server connection is \"Disabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 12
 testRunner.And("Security is \"Selected\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 13
 testRunner.And("Logging is \"Unselected\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Windows Group",
                        "Edit Group",
                        "Deploy To",
                        "Deploy From",
                        "Administrator",
                        "View",
                        "Execute",
                        "Contribute",
                        "Delete Row",
                        "Row"});
            table1.AddRow(new string[] {
                        "Warewolf Administrator",
                        "Disabeled",
                        "Yes",
                        "Yes",
                        "Yes",
                        "Yes",
                        "Yes",
                        "Yes",
                        "Disabled",
                        "Disabled"});
            table1.AddRow(new string[] {
                        "Public",
                        "Disabeled",
                        "\"\"",
                        "\"\"",
                        "\"\"",
                        "\"\"",
                        "\"\"",
                        "\"\"",
                        "\"\"",
                        "Enabled"});
            table1.AddRow(new string[] {
                        "",
                        "Enabled",
                        "",
                        "",
                        "",
                        "",
                        "",
                        "",
                        "",
                        "Enabled"});
#line 14
 testRunner.And("Server Permissions is \"Visible\"", ((string)(null)), table1, "And ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Resources",
                        "REdit",
                        "Windows Group",
                        "WEdit",
                        "View",
                        "Execute",
                        "Contribute"});
            table2.AddRow(new string[] {
                        "",
                        "Enabled",
                        "",
                        "Enabled",
                        "",
                        "",
                        ""});
#line 19
 testRunner.And("Resource Permissions is \"Visible\"", ((string)(null)), table2, "And ");
#line 22
 testRunner.And("Save is \"Disabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Selecting Admin rights for public")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Settings Tab")]
        public virtual void SelectingAdminRightsForPublic()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Selecting Admin rights for public", ((string[])(null)));
#line 25
this.ScenarioSetup(scenarioInfo);
#line 26
 testRunner.Given("I have settings tab opened", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 27
 testRunner.And("\"server\" selected as \"localhost (Connected)\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 28
 testRunner.And("Security is \"Selected\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 29
 testRunner.And("Save is \"Disabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 30
 testRunner.And("Server Permissions is \"Visible\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 31
 testRunner.And("Resource Permissions is \"Visible\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "Windows Group",
                        "Edit Group",
                        "Deploy To",
                        "Deploy From",
                        "Administrator",
                        "View",
                        "Execute",
                        "Contribute",
                        "Delete Row",
                        "Row"});
            table3.AddRow(new string[] {
                        "Warewolf Administrator",
                        "Disabeled",
                        "Yes",
                        "Yes",
                        "Yes",
                        "Yes",
                        "Yes",
                        "Yes",
                        "Disabled",
                        "Disabled"});
            table3.AddRow(new string[] {
                        "Public",
                        "Disabeled",
                        "Yes",
                        "Yes",
                        "Yes",
                        "Yes",
                        "Yes",
                        "Yes",
                        "Disabled",
                        "Enabled"});
            table3.AddRow(new string[] {
                        "",
                        "Enabled",
                        "",
                        "",
                        "",
                        "",
                        "",
                        "",
                        "",
                        "Enabled"});
#line 32
 testRunner.When("i select server \"Public\" as \"Administrator\"", ((string)(null)), table3, "When ");
#line 37
 testRunner.Then("Save is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 38
 testRunner.When("I save the settings", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 39
 testRunner.Then("settings saved \"Successfull\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 40
 testRunner.And("the \"Settings\" has validation error \"False\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Selecting Resource Permissions")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Settings Tab")]
        public virtual void SelectingResourcePermissions()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Selecting Resource Permissions", ((string[])(null)));
#line 43
this.ScenarioSetup(scenarioInfo);
#line 44
 testRunner.Given("I have settings tab opened", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 45
 testRunner.And("\"server\" selected as \"localhost (Connected)\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 46
 testRunner.And("Save is \"Disabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 47
 testRunner.And("Security is \"Selected\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 48
 testRunner.And("Logging is \"Unselected\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "Windows Group",
                        "Edit Group",
                        "Deploy To",
                        "Deploy From",
                        "Administrator",
                        "View",
                        "Execute",
                        "Contribute",
                        "Delete Row",
                        "Row"});
            table4.AddRow(new string[] {
                        "Warewolf Administrator",
                        "Disabeled",
                        "Yes",
                        "Yes",
                        "Yes",
                        "Yes",
                        "Yes",
                        "Yes",
                        "Disabled",
                        "Disabled"});
            table4.AddRow(new string[] {
                        "Public",
                        "Disabeled",
                        "\"\"",
                        "\"\"",
                        "\"\"",
                        "\"\"",
                        "\"\"",
                        "\"\"",
                        "\"\"",
                        "Enabled"});
            table4.AddRow(new string[] {
                        "",
                        "Enabled",
                        "",
                        "",
                        "",
                        "",
                        "",
                        "",
                        "",
                        "Enabled"});
#line 49
 testRunner.And("Server Permissions is \"Visible\"", ((string)(null)), table4, "And ");
#line 54
 testRunner.And("Resource Permissions is \"Visible\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "Resources",
                        "REdit",
                        "Windows Group",
                        "WEdit",
                        "View",
                        "Execute",
                        "Contribute"});
            table5.AddRow(new string[] {
                        "WORKFLOWS\\My Category\\Dice Roll",
                        "Enabled",
                        "Public",
                        "Enabled",
                        "Yes",
                        "Yes",
                        "Yes"});
            table5.AddRow(new string[] {
                        "",
                        "Enabled",
                        "",
                        "Enabled",
                        "",
                        "",
                        ""});
#line 55
 testRunner.When("i select resource \"Resource Permissions\"", ((string)(null)), table5, "When ");
#line 59
    testRunner.Then("Save is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 60
 testRunner.When("I save the settings", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 61
 testRunner.Then("settings saved \"Successfull\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 62
 testRunner.And("the \"Settings\" has validation error \"False\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Warewolf is not allowing to save Duplicate server permissions")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Settings Tab")]
        public virtual void WarewolfIsNotAllowingToSaveDuplicateServerPermissions()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Warewolf is not allowing to save Duplicate server permissions", ((string[])(null)));
#line 65
this.ScenarioSetup(scenarioInfo);
#line 66
 testRunner.Given("I have settings tab opened", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 67
 testRunner.And("\"server\" selected as \"localhost (Connected)\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 68
 testRunner.And("Save is \"Disabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 69
 testRunner.And("Security is \"Selected\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 70
 testRunner.And("Logging is \"Unselected\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "Windows Group",
                        "Edit Group",
                        "Deploy To",
                        "Deploy From",
                        "Administrator",
                        "View",
                        "Execute",
                        "Contribute",
                        "Delete Row",
                        "Row"});
            table6.AddRow(new string[] {
                        "Warewolf Administrator",
                        "Disabeled",
                        "Yes",
                        "Yes",
                        "Yes",
                        "Yes",
                        "Yes",
                        "Yes",
                        "Disabled",
                        "Disabled"});
            table6.AddRow(new string[] {
                        "Public",
                        "Disabeled",
                        "Yes",
                        "Yes",
                        "Yes",
                        "Yes",
                        "Yes",
                        "Yes",
                        "Disabled",
                        "Disabled"});
            table6.AddRow(new string[] {
                        "Public",
                        "Enabled",
                        "Yes",
                        "Yes",
                        "Yes",
                        "Yes",
                        "Yes",
                        "Yes",
                        "Disabled",
                        "Enabled"});
#line 71
 testRunner.And("Server Permissions is \"Visible\"", ((string)(null)), table6, "And ");
#line 76
 testRunner.And("Resource Permissions is \"Visible\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 77
    testRunner.Then("Save is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 78
 testRunner.When("I save the settings", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 79
 testRunner.Then("settings saved \"UnSuccessfull\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 80
 testRunner.And("the \"Settings\" has validation error \"True\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Warewolf is not allowing to save Duplicate Resource permissions")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Settings Tab")]
        public virtual void WarewolfIsNotAllowingToSaveDuplicateResourcePermissions()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Warewolf is not allowing to save Duplicate Resource permissions", ((string[])(null)));
#line 82
this.ScenarioSetup(scenarioInfo);
#line 83
 testRunner.Given("I have settings tab opened", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 84
 testRunner.And("\"server\" selected as \"localhost (Connected)\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 85
 testRunner.And("Save is \"Disabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 86
 testRunner.And("Security is \"Selected\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 87
 testRunner.And("Logging is \"Unselected\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 88
 testRunner.And("Server Permissions is \"Visible\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                        "Resources",
                        "REdit",
                        "Windows Group",
                        "WEdit",
                        "View",
                        "Execute",
                        "Contribute"});
            table7.AddRow(new string[] {
                        "WORKFLOWS\\My Category\\Dice Roll",
                        "Enabled",
                        "Public",
                        "Enabled",
                        "Yes",
                        "Yes",
                        "Yes"});
            table7.AddRow(new string[] {
                        "WORKFLOWS\\My Category\\Dice Roll",
                        "Enabled",
                        "Public",
                        "Enabled",
                        "Yes",
                        "Yes",
                        "Yes"});
#line 89
 testRunner.When("i select resource \"Resource Permissions\"", ((string)(null)), table7, "When ");
#line 93
 testRunner.And("Resource Permissions is \"Visible\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 94
    testRunner.Then("Save is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 95
 testRunner.When("I save the settings", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 96
 testRunner.Then("settings saved \"UnSuccessfull\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 97
 testRunner.And("the \"Settings\" has validation error \"True\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Selecting Logging is showing Server and Studio log settings")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Settings Tab")]
        public virtual void SelectingLoggingIsShowingServerAndStudioLogSettings()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Selecting Logging is showing Server and Studio log settings", ((string[])(null)));
#line 100
this.ScenarioSetup(scenarioInfo);
#line 101
 testRunner.Given("I have settings tab opened", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 102
 testRunner.And("\"server\" selected as \"localhost (Connected)\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 103
 testRunner.And("Server edit is \"Disabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 104
 testRunner.And("server connection is \"Disabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 105
 testRunner.And("Save is \"Disabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 106
 testRunner.And("Security is \"Selected\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 107
 testRunner.And("Logging is \"Unselected\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 108
 testRunner.And("Server Permissions is \"Visible\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 109
 testRunner.And("Resource Permissions is \"Visible\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 110
 testRunner.When("I select \"Logging\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 111
 testRunner.Then("Server System Logs is \"Visible\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 112
 testRunner.And("Studio Logs is \"Visible\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 113
 testRunner.And("Server Permissions is \"InVisible\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 114
 testRunner.And("Resource Permissions is \"InVisible\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 115
 testRunner.And("Save is \"Disabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
