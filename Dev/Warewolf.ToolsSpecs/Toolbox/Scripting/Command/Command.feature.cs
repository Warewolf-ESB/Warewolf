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
namespace Warewolf.ToolsSpecs.Toolbox.Scripting.Command
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.1.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute()]
    public partial class CommandFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "Command.feature"
#line hidden
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static void FeatureSetup(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner(null, 0);
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Command", "\tIn order to execute command line scripts\r\n\tAs a Warewolf user\r\n\tI want a tool th" +
                    "at allows me to execute commands ", ProgrammingLanguage.CSharp, ((string[])(null)));
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
                        && (testRunner.FeatureContext.FeatureInfo.Title != "Command")))
            {
                Warewolf.ToolsSpecs.Toolbox.Scripting.Command.CommandFeature.FeatureSetup(null);
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
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Execute commands")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Command")]
        public virtual void ExecuteCommands()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Execute commands", ((string[])(null)));
#line 6
this.ScenarioSetup(scenarioInfo);
#line 7
 testRunner.Given("I have a command variable \"[[drive]]\" equal to \"C:\\\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table1576 = new TechTalk.SpecFlow.Table(new string[] {
                        "script"});
            table1576.AddRow(new string[] {
                        "@echo off"});
            table1576.AddRow(new string[] {
                        "REM Testing multiple commands"});
            table1576.AddRow(new string[] {
                        "dir [[drive]]"});
#line 8
 testRunner.Given("I have these command scripts to execute in a single execution run", ((string)(null)), table1576, "Given ");
#line 13
 testRunner.When("the command tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 14
 testRunner.Then("the result of the command tool will be \"Volume in drive C has no label\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 15
 testRunner.And("the execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table1577 = new TechTalk.SpecFlow.Table(new string[] {
                        "Command"});
            table1577.AddRow(new string[] {
                        "String = String"});
#line 16
 testRunner.And("the debug inputs as", ((string)(null)), table1577, "And ");
#line hidden
            TechTalk.SpecFlow.Table table1578 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table1578.AddRow(new string[] {
                        "[[result]] = String"});
#line 19
 testRunner.And("the debug output as", ((string)(null)), table1578, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Execute a command that requires user interaction like pause")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Command")]
        public virtual void ExecuteACommandThatRequiresUserInteractionLikePause()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Execute a command that requires user interaction like pause", ((string[])(null)));
#line 23
this.ScenarioSetup(scenarioInfo);
#line 24
 testRunner.Given("I have this command script to execute \"pause\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 25
 testRunner.When("the command tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 26
 testRunner.Then("the result of the command tool will be \"Press any key to continue . . .\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 27
 testRunner.And("the execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table1579 = new TechTalk.SpecFlow.Table(new string[] {
                        "Command"});
            table1579.AddRow(new string[] {
                        "pause"});
#line 28
 testRunner.And("the debug inputs as", ((string)(null)), table1579, "And ");
#line hidden
            TechTalk.SpecFlow.Table table1580 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table1580.AddRow(new string[] {
                        "[[result]] = Press any key to continue . . ."});
#line 31
 testRunner.And("the debug output as", ((string)(null)), table1580, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Execute a blank cmd")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Command")]
        public virtual void ExecuteABlankCmd()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Execute a blank cmd", ((string[])(null)));
#line 35
this.ScenarioSetup(scenarioInfo);
#line 36
 testRunner.Given("I have this command script to execute \"\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 37
 testRunner.When("the command tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 38
 testRunner.Then("the result of the command tool will be \"\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 39
 testRunner.And("the execution has \"AN\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table1581 = new TechTalk.SpecFlow.Table(new string[] {
                        "Command"});
            table1581.AddRow(new string[] {
                        "\"\""});
#line 40
 testRunner.And("the debug inputs as", ((string)(null)), table1581, "And ");
#line hidden
            TechTalk.SpecFlow.Table table1582 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table1582.AddRow(new string[] {
                        "[[result]] ="});
#line 43
 testRunner.And("the debug output as", ((string)(null)), table1582, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Execute invalid cmd")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Command")]
        public virtual void ExecuteInvalidCmd()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Execute invalid cmd", ((string[])(null)));
#line 47
this.ScenarioSetup(scenarioInfo);
#line 48
 testRunner.Given("I have this command script to execute \"asdf\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 49
 testRunner.When("the command tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 50
 testRunner.Then("the result of the command tool will be \"\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 51
 testRunner.And("the execution has \"AN\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table1583 = new TechTalk.SpecFlow.Table(new string[] {
                        "Command"});
            table1583.AddRow(new string[] {
                        "asdf"});
#line 52
 testRunner.And("the debug inputs as", ((string)(null)), table1583, "And ");
#line hidden
            TechTalk.SpecFlow.Table table1584 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table1584.AddRow(new string[] {
                        "[[result]] ="});
#line 55
 testRunner.And("the debug output as", ((string)(null)), table1584, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Execute cmd with negative recordset index")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Command")]
        public virtual void ExecuteCmdWithNegativeRecordsetIndex()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Execute cmd with negative recordset index", ((string[])(null)));
#line 59
this.ScenarioSetup(scenarioInfo);
#line 60
 testRunner.Given("I have this command script to execute \"dir [[my(-1).dir]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 61
 testRunner.When("the command tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 62
 testRunner.Then("the execution has \"AN\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            TechTalk.SpecFlow.Table table1585 = new TechTalk.SpecFlow.Table(new string[] {
                        "Command"});
            table1585.AddRow(new string[] {
                        "dir [[my(-1).dir]] ="});
#line 63
 testRunner.And("the debug inputs as", ((string)(null)), table1585, "And ");
#line hidden
            TechTalk.SpecFlow.Table table1586 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table1586.AddRow(new string[] {
                        "[[result]] ="});
#line 66
 testRunner.And("the debug output as", ((string)(null)), table1586, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Execute a NULL cmd")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Command")]
        public virtual void ExecuteANULLCmd()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Execute a NULL cmd", ((string[])(null)));
#line 70
this.ScenarioSetup(scenarioInfo);
#line 71
 testRunner.Given("I have a command variable \"[[command]]\" equal to \"NULL\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 72
 testRunner.And("I have this command script to execute \"dir c:\\[[command]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 73
 testRunner.When("the command tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 74
 testRunner.Then("the execution has \"No\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Execute a non existent variable cmd")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Command")]
        public virtual void ExecuteANonExistentVariableCmd()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Execute a non existent variable cmd", ((string[])(null)));
#line 77
this.ScenarioSetup(scenarioInfo);
#line 78
 testRunner.Given("I have this command script to execute \"dir c:\\[[command]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 79
 testRunner.When("the command tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 80
 testRunner.Then("the result of the command tool will be \"\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 81
 testRunner.And("the execution has \"AN\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Execute commands with star notation")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Command")]
        public virtual void ExecuteCommandsWithStarNotation()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Execute commands with star notation", ((string[])(null)));
#line 83
this.ScenarioSetup(scenarioInfo);
#line 84
 testRunner.Given("I have a command variable \"[[coms().command]]\" equal to \"bob\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 85
 testRunner.And("I have a command variable \"[[coms().command]]\" equal to \"dora\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 86
 testRunner.And("I have a command variable \"[[coms().command]]\" equal to \"bill\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 87
 testRunner.And("I have a command variable \"[[results().res]]\" equal to \"res1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 88
 testRunner.And("I have a command result equal to \"[[results(*).res]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table1587 = new TechTalk.SpecFlow.Table(new string[] {
                        "script"});
            table1587.AddRow(new string[] {
                        "echo [[coms(*).command]]"});
#line 89
 testRunner.And("I have these command scripts to execute in a single execution run", ((string)(null)), table1587, "And ");
#line 92
 testRunner.When("the command tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 93
 testRunner.Then("the execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            TechTalk.SpecFlow.Table table1588 = new TechTalk.SpecFlow.Table(new string[] {
                        "Command"});
            table1588.AddRow(new string[] {
                        "String = String"});
            table1588.AddRow(new string[] {
                        "String = String"});
            table1588.AddRow(new string[] {
                        "String = String"});
#line 94
 testRunner.And("the debug inputs as", ((string)(null)), table1588, "And ");
#line hidden
            TechTalk.SpecFlow.Table table1589 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table1589.AddRow(new string[] {
                        "[[results(1).res]] = bob"});
            table1589.AddRow(new string[] {
                        "[[results(2).res]] = dora"});
            table1589.AddRow(new string[] {
                        "[[results(3).res]] = bill"});
#line 99
 testRunner.And("the debug output as", ((string)(null)), table1589, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Execute commands with star notation to append")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Command")]
        public virtual void ExecuteCommandsWithStarNotationToAppend()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Execute commands with star notation to append", ((string[])(null)));
#line 105
this.ScenarioSetup(scenarioInfo);
#line 106
 testRunner.Given("I have a command variable \"[[coms().command]]\" equal to \"bob\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 107
 testRunner.And("I have a command variable \"[[coms().command]]\" equal to \"dora\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 108
 testRunner.And("I have a command variable \"[[coms().command]]\" equal to \"bill\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 109
 testRunner.And("I have a command variable \"[[results().res]]\" equal to \"res1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 110
 testRunner.And("I have a command result equal to \"[[results().res]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table1590 = new TechTalk.SpecFlow.Table(new string[] {
                        "script"});
            table1590.AddRow(new string[] {
                        "echo [[coms(*).command]]"});
#line 111
 testRunner.And("I have these command scripts to execute in a single execution run", ((string)(null)), table1590, "And ");
#line 114
 testRunner.When("the command tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 115
 testRunner.Then("the execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            TechTalk.SpecFlow.Table table1591 = new TechTalk.SpecFlow.Table(new string[] {
                        "Command"});
            table1591.AddRow(new string[] {
                        "String = String"});
            table1591.AddRow(new string[] {
                        "String = String"});
            table1591.AddRow(new string[] {
                        "String = String"});
#line 116
 testRunner.And("the debug inputs as", ((string)(null)), table1591, "And ");
#line hidden
            TechTalk.SpecFlow.Table table1592 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table1592.AddRow(new string[] {
                        "[[results(4).res]] = bill"});
#line 121
 testRunner.And("the debug output as", ((string)(null)), table1592, "And ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
