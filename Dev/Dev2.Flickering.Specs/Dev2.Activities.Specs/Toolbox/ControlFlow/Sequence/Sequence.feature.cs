﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.9.3.0
//      SpecFlow Generator Version:1.9.0.0
//      Runtime Version:4.0.30319.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace Dev2.Flickering.Specs.Dev2_Activities_Specs.Toolbox.ControlFlow.Sequence
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.9.3.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Sequence")]
    public partial class SequenceFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "Sequence.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Sequence", "In order to execute sequence \r\nAs a Warewolf user\r\nI want to a tool that will all" +
                    "ow me to construct and execute tools and services in sequence", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.TestFixtureTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
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
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Execute a Sequence with For each with 3 executions")]
        public virtual void ExecuteASequenceWithForEachWith3Executions()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Execute a Sequence with For each with 3 executions", ((string[])(null)));
#line 6
this.ScenarioSetup(scenarioInfo);
#line 7
      testRunner.Given("I have a ForEach \"ForEachTest\" as \"NumOfExecution\" executions \"3\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 8
   testRunner.And("I have a Sequence \"Test\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Variable",
                        "Selected"});
            table1.AddRow(new string[] {
                        "[[test().date]]",
                        "Date & Time"});
#line 9
   testRunner.And("\"Test\" contains Gather System Info \"Sys info\" as", ((string)(null)), table1, "And ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Input1",
                        "Input2",
                        "Input Format",
                        "Output In",
                        "Result"});
            table2.AddRow(new string[] {
                        "2013-11-29",
                        "2050-11-29",
                        "yyyy-mm-dd",
                        "Years",
                        "[[test().result1]]"});
#line 12
   testRunner.And("\"Test\" contains Date and Time Difference \"Date&Time\" as", ((string)(null)), table2, "And ");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "Input",
                        "Input Format",
                        "Add Time",
                        "Output Format",
                        "Result"});
            table3.AddRow(new string[] {
                        "2013-11-29",
                        "yyyy-mm-dd",
                        "1",
                        "yyyy-mm-dd",
                        "[[test().result2]]"});
#line 15
   testRunner.And("\"Test\" contains Date and Time \"Date\" as", ((string)(null)), table3, "And ");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "Type",
                        "From",
                        "To",
                        "Result"});
            table4.AddRow(new string[] {
                        "Numbers",
                        "1",
                        "10",
                        "[[test().result3]]"});
#line 18
   testRunner.And("\"Test\" contains Random \"Random\" as", ((string)(null)), table4, "And ");
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "Number",
                        "Rounding Selected",
                        "Rounding To",
                        "Decimal to show",
                        "Result"});
            table5.AddRow(new string[] {
                        "788.894564545645",
                        "Up",
                        "3",
                        "3",
                        "[[test().result4]]"});
#line 21
   testRunner.And("\"Test\" contains Format Number \"Fnumber\" as", ((string)(null)), table5, "And ");
#line 24
   testRunner.When("the ForEach \"ForEachTest\" tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 25
   testRunner.Then("the execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "",
                        "Number"});
            table6.AddRow(new string[] {
                        "No. of Executes",
                        "3"});
#line 26
   testRunner.And("the \"ForEachTest\" debug inputs as", ((string)(null)), table6, "And ");
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                        "#",
                        "",
                        ""});
            table7.AddRow(new string[] {
                        "1",
                        "[[test().date]] =",
                        "Date & Time"});
#line 29
    testRunner.And("the \"Sys info\" debug inputs as", ((string)(null)), table7, "And ");
#line hidden
            TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                        "#",
                        ""});
            table8.AddRow(new string[] {
                        "1",
                        "[[test(6).date]] = String"});
#line 32
     testRunner.And("the \"Sys info\" debug outputs as", ((string)(null)), table8, "And ");
#line hidden
            TechTalk.SpecFlow.Table table9 = new TechTalk.SpecFlow.Table(new string[] {
                        "Input 1",
                        "Input 2",
                        "Input Format",
                        "Output In"});
            table9.AddRow(new string[] {
                        "2013-11-29",
                        "2050-11-29",
                        "yyyy-mm-dd",
                        "Years"});
#line 35
    testRunner.And("the \"Date&Time\" debug inputs as", ((string)(null)), table9, "And ");
#line hidden
            TechTalk.SpecFlow.Table table10 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table10.AddRow(new string[] {
                        "[[test(6).result1]] = 37"});
#line 38
   testRunner.And("the \"Date&Time\" debug outputs as", ((string)(null)), table10, "And ");
#line hidden
            TechTalk.SpecFlow.Table table11 = new TechTalk.SpecFlow.Table(new string[] {
                        "Input",
                        "Input Format",
                        "Add Time",
                        "",
                        "Output Format"});
            table11.AddRow(new string[] {
                        "2013-11-29",
                        "yyyy-mm-dd",
                        "Years",
                        "1",
                        "yyyy-mm-dd"});
#line 41
   testRunner.And("the \"Date\" debug inputs as", ((string)(null)), table11, "And ");
#line hidden
            TechTalk.SpecFlow.Table table12 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table12.AddRow(new string[] {
                        "[[test(6).result2]] = 2014-11-29"});
#line 44
   testRunner.And("the \"Date\" debug outputs as", ((string)(null)), table12, "And ");
#line hidden
            TechTalk.SpecFlow.Table table13 = new TechTalk.SpecFlow.Table(new string[] {
                        "Random",
                        "From",
                        "To"});
            table13.AddRow(new string[] {
                        "Numbers",
                        "1",
                        "10"});
#line 47
   testRunner.And("the \"Random\" debug inputs as", ((string)(null)), table13, "And ");
#line hidden
            TechTalk.SpecFlow.Table table14 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table14.AddRow(new string[] {
                        "[[test(6).result3]] = Int32"});
#line 50
   testRunner.And("the \"Random\" debug outputs as", ((string)(null)), table14, "And ");
#line hidden
            TechTalk.SpecFlow.Table table15 = new TechTalk.SpecFlow.Table(new string[] {
                        "Number",
                        "Rounding",
                        "Rounding Value",
                        "Decimals to show"});
            table15.AddRow(new string[] {
                        "788.894564545645",
                        "Up",
                        "3",
                        "3"});
#line 53
   testRunner.And("the \"Fnumber\" debug inputs as", ((string)(null)), table15, "And ");
#line hidden
            TechTalk.SpecFlow.Table table16 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table16.AddRow(new string[] {
                        "[[test().result4]] = 788.895"});
#line 56
   testRunner.And("the \"Fnumber\" debug outputs as", ((string)(null)), table16, "And ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
