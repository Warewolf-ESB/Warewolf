﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.9.0.77
//      SpecFlow Generator Version:1.9.0.0
//      Runtime Version:4.0.30319.18444
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code

using TechTalk.SpecFlow;

#pragma warning disable
namespace Dev2.Activities.Specs.Toolbox.Recordset.Length
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.9.0.77")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute()]
    public partial class LengthFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "Length.feature"
#line hidden
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static void FeatureSetup(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Length", "In order to get the length of a records\r\nAs a Warewolf user\r\nI want a tool that t" +
                    "akes a record set gives me its length", ProgrammingLanguage.CSharp, ((string[])(null)));
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
                        && (TechTalk.SpecFlow.FeatureContext.Current.FeatureInfo.Title != "Length")))
            {
                FeatureSetup(null);
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
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Length of a recordset with 3 rows")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Length")]
        public virtual void LengthOfARecordsetWith3Rows()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Length of a recordset with 3 rows", ((string[])(null)));
#line 6
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "[[rs]]",
                        ""});
            table1.AddRow(new string[] {
                        "rs(1).row",
                        "1"});
            table1.AddRow(new string[] {
                        "rs(3).row",
                        "2"});
            table1.AddRow(new string[] {
                        "rs(5).row",
                        "3"});
#line 7
 testRunner.Given("I get  the length from a recordset that looks like with this shape", ((string)(null)), table1, "Given ");
#line 12
 testRunner.And("get length on record \"[[rs()]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 13
 testRunner.When("the length tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 14
 testRunner.Then("the length result should be 5", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 15
 testRunner.And("the execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Recordset"});
            table2.AddRow(new string[] {
                        "[[rs(1).row]] = 1"});
            table2.AddRow(new string[] {
                        "[[rs(3).row]] = 2"});
            table2.AddRow(new string[] {
                        "[[rs(5).row]] = 3"});
#line 16
 testRunner.And("the debug inputs as", ((string)(null)), table2, "And ");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table3.AddRow(new string[] {
                        "[[result]] = 5"});
#line 21
 testRunner.And("the debug output as", ((string)(null)), table3, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Length of a recordset with 8 rows")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Length")]
        public virtual void LengthOfARecordsetWith8Rows()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Length of a recordset with 8 rows", ((string[])(null)));
#line 26
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "rs",
                        ""});
            table4.AddRow(new string[] {
                        "rs(1).row",
                        "1"});
            table4.AddRow(new string[] {
                        "rs(2).row",
                        "2"});
            table4.AddRow(new string[] {
                        "rs(3).row",
                        "3"});
            table4.AddRow(new string[] {
                        "rs(4).row",
                        "4"});
            table4.AddRow(new string[] {
                        "rs(5).row",
                        "5"});
            table4.AddRow(new string[] {
                        "rs(6).row",
                        "6"});
            table4.AddRow(new string[] {
                        "rs(7).row",
                        "7"});
            table4.AddRow(new string[] {
                        "rs(8).row",
                        "8"});
#line 27
 testRunner.Given("I get  the length from a recordset that looks like with this shape", ((string)(null)), table4, "Given ");
#line 37
 testRunner.And("get length on record \"[[rs()]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 38
 testRunner.When("the length tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 39
 testRunner.Then("the length result should be 8", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 40
 testRunner.And("the execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "Recordset"});
            table5.AddRow(new string[] {
                        "[[rs(1).row]] =  1"});
            table5.AddRow(new string[] {
                        "[[rs(2).row]] =  2"});
            table5.AddRow(new string[] {
                        "[[rs(3).row]] =  3"});
            table5.AddRow(new string[] {
                        "[[rs(4).row]] =  4"});
            table5.AddRow(new string[] {
                        "[[rs(5).row]] =  5"});
            table5.AddRow(new string[] {
                        "[[rs(6).row]] =  6"});
            table5.AddRow(new string[] {
                        "[[rs(7).row]] =  7"});
            table5.AddRow(new string[] {
                        "[[rs(8).row]] =  8"});
#line 41
 testRunner.And("the debug inputs as", ((string)(null)), table5, "And ");
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table6.AddRow(new string[] {
                        "[[result]] = 8"});
#line 51
 testRunner.And("the debug output as", ((string)(null)), table6, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Length of a recordset with 0 rows")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Length")]
        public virtual void LengthOfARecordsetWith0Rows()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Length of a recordset with 0 rows", ((string[])(null)));
#line 55
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                        "rs"});
#line 56
 testRunner.Given("I get  the length from a recordset that looks like with this shape", ((string)(null)), table7, "Given ");
#line 58
 testRunner.And("get length on record \"[[rs()]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 59
 testRunner.When("the length tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 60
 testRunner.Then("the length result should be 0", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 61
 testRunner.And("the execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table8.AddRow(new string[] {
                        "[[result]] = 0"});
#line 62
 testRunner.And("the debug output as", ((string)(null)), table8, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Recordset length for coloumn")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Length")]
        public virtual void RecordsetLengthForColoumn()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Recordset length for coloumn", ((string[])(null)));
#line 67
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table9 = new TechTalk.SpecFlow.Table(new string[] {
                        "rs",
                        ""});
            table9.AddRow(new string[] {
                        "rs(1).row",
                        "1"});
            table9.AddRow(new string[] {
                        "rs(2).row",
                        "2"});
            table9.AddRow(new string[] {
                        "rs(3).row",
                        "3"});
            table9.AddRow(new string[] {
                        "rs(4).row",
                        "4"});
            table9.AddRow(new string[] {
                        "rs(5).row",
                        "5"});
            table9.AddRow(new string[] {
                        "rs(6).row",
                        "6"});
            table9.AddRow(new string[] {
                        "rs(7).row",
                        "7"});
            table9.AddRow(new string[] {
                        "rs(8).row",
                        "8"});
#line 68
 testRunner.Given("I get  the length from a recordset that looks like with this shape", ((string)(null)), table9, "Given ");
#line 78
 testRunner.And("get length on record \"[[rs().row]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 79
 testRunner.When("the length tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 80
 testRunner.Then("the execution has \"AN\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            TechTalk.SpecFlow.Table table10 = new TechTalk.SpecFlow.Table(new string[] {
                        "Recordset"});
#line 81
 testRunner.And("the debug inputs as", ((string)(null)), table10, "And ");
#line hidden
            TechTalk.SpecFlow.Table table11 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
#line 83
 testRunner.And("the debug output as", ((string)(null)), table11, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Recordset length for coloumns invalid")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Length")]
        public virtual void RecordsetLengthForColoumnsInvalid()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Recordset length for coloumns invalid", ((string[])(null)));
#line 86
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table12 = new TechTalk.SpecFlow.Table(new string[] {
                        "rs",
                        ""});
            table12.AddRow(new string[] {
                        "rs().row",
                        "1"});
            table12.AddRow(new string[] {
                        "rs().row",
                        "2"});
            table12.AddRow(new string[] {
                        "rs().row",
                        "3"});
            table12.AddRow(new string[] {
                        "rs().row",
                        "4"});
            table12.AddRow(new string[] {
                        "rs().row2",
                        "5"});
            table12.AddRow(new string[] {
                        "rs().row2",
                        "6"});
            table12.AddRow(new string[] {
                        "rs().row2",
                        "7"});
#line 87
 testRunner.Given("I get  the length from a recordset that looks like with this shape", ((string)(null)), table12, "Given ");
#line 96
 testRunner.And("get length on record \"[[rs().row]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 97
 testRunner.When("the length tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 98
 testRunner.Then("the execution has \"AN\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            TechTalk.SpecFlow.Table table13 = new TechTalk.SpecFlow.Table(new string[] {
                        "Recordset"});
#line 99
 testRunner.And("the debug inputs as", ((string)(null)), table13, "And ");
#line hidden
            TechTalk.SpecFlow.Table table14 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
#line 101
 testRunner.And("the debug output as", ((string)(null)), table14, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Recordset length")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Length")]
        public virtual void RecordsetLength()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Recordset length", ((string[])(null)));
#line 104
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table15 = new TechTalk.SpecFlow.Table(new string[] {
                        "rs",
                        ""});
            table15.AddRow(new string[] {
                        "rs().row",
                        "1"});
            table15.AddRow(new string[] {
                        "rs().row",
                        "2"});
            table15.AddRow(new string[] {
                        "rs().row",
                        "3"});
            table15.AddRow(new string[] {
                        "rs().row",
                        "4"});
            table15.AddRow(new string[] {
                        "rs().row2",
                        "5"});
            table15.AddRow(new string[] {
                        "rs().row2",
                        "6"});
            table15.AddRow(new string[] {
                        "rs().row2",
                        "7"});
#line 105
 testRunner.Given("I get  the length from a recordset that looks like with this shape", ((string)(null)), table15, "Given ");
#line 114
 testRunner.And("get length on record \"[[rs()]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 115
 testRunner.When("the length tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 116
 testRunner.Then("the length result should be 6", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 117
 testRunner.And("the execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table16 = new TechTalk.SpecFlow.Table(new string[] {
                        "Recordset"});
            table16.AddRow(new string[] {
                        "[[rs(1).row]] =  1"});
            table16.AddRow(new string[] {
                        "[[rs(2).row]] =  2"});
            table16.AddRow(new string[] {
                        "[[rs(3).row]] =  3"});
            table16.AddRow(new string[] {
                        "[[rs(4).row]] =  4"});
            table16.AddRow(new string[] {
                        "[[rs(4).row2]] =  5"});
            table16.AddRow(new string[] {
                        "[[rs(5).row2]] =  6"});
            table16.AddRow(new string[] {
                        "[[rs(6).row2]] =  7"});
#line 118
 testRunner.And("the debug inputs as", ((string)(null)), table16, "And ");
#line hidden
            TechTalk.SpecFlow.Table table17 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table17.AddRow(new string[] {
                        "[[result]] = 6"});
#line 127
 testRunner.And("the debug output as", ((string)(null)), table17, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Recordset length for invalid recordset")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Length")]
        public virtual void RecordsetLengthForInvalidRecordset()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Recordset length for invalid recordset", ((string[])(null)));
#line 131
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table18 = new TechTalk.SpecFlow.Table(new string[] {
                        "rs",
                        ""});
            table18.AddRow(new string[] {
                        "rs(1).row",
                        "1"});
            table18.AddRow(new string[] {
                        "rs(2).row",
                        "2"});
            table18.AddRow(new string[] {
                        "rs(3).row",
                        "3"});
            table18.AddRow(new string[] {
                        "rs(4).row",
                        "4"});
            table18.AddRow(new string[] {
                        "rs(5).row",
                        "5"});
            table18.AddRow(new string[] {
                        "rs(6).row",
                        "6"});
            table18.AddRow(new string[] {
                        "rs(7).row",
                        "7"});
            table18.AddRow(new string[] {
                        "rs(8).row",
                        "8"});
#line 132
 testRunner.Given("I get  the length from a recordset that looks like with this shape", ((string)(null)), table18, "Given ");
#line 142
 testRunner.And("get length on record \"[[rs().&^]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 143
 testRunner.When("the length tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 144
 testRunner.Then("the execution has \"AN\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            TechTalk.SpecFlow.Table table19 = new TechTalk.SpecFlow.Table(new string[] {
                        "Recordset"});
#line 145
 testRunner.And("the debug inputs as", ((string)(null)), table19, "And ");
#line hidden
            TechTalk.SpecFlow.Table table20 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
#line 147
 testRunner.And("the debug output as", ((string)(null)), table20, "And ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
