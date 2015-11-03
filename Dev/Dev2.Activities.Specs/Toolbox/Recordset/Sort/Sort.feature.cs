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
#pragma warning disable
namespace Dev2.Activities.Specs.Toolbox.Recordset.Sort
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.9.0.77")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute()]
    public partial class SortFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "Sort.feature"
#line hidden
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static void FeatureSetup(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Sort", "In order to sort a recordset\r\nAs a Warewolf user\r\nI want a tool I can use to arra" +
                    "nge records in either ascending or descending order", ProgrammingLanguage.CSharp, ((string[])(null)));
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
                        && (TechTalk.SpecFlow.FeatureContext.Current.FeatureInfo.Title != "Sort")))
            {
                Dev2.Activities.Specs.Toolbox.Recordset.Sort.SortFeature.FeatureSetup(null);
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
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Sort a recordset forwards using star notation")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Sort")]
        public virtual void SortARecordsetForwardsUsingStarNotation()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sort a recordset forwards using star notation", ((string[])(null)));
#line 6
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "rs",
                        "value"});
            table1.AddRow(new string[] {
                        "rs().row",
                        "You"});
            table1.AddRow(new string[] {
                        "rs().row",
                        "are"});
            table1.AddRow(new string[] {
                        "rs().row",
                        "the"});
            table1.AddRow(new string[] {
                        "rs().row",
                        "best"});
            table1.AddRow(new string[] {
                        "rs().row",
                        "Warewolf"});
            table1.AddRow(new string[] {
                        "rs().row",
                        "user"});
            table1.AddRow(new string[] {
                        "rs().row",
                        "so far"});
#line 7
 testRunner.Given("I have the following recordset to sort", ((string)(null)), table1, "Given ");
#line 16
 testRunner.And("I sort a record \"[[rs(*).row]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 17
 testRunner.And("my sort order is \"Forward\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 18
 testRunner.When("the sort records tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "rs",
                        "value"});
            table2.AddRow(new string[] {
                        "rs().row",
                        "are"});
            table2.AddRow(new string[] {
                        "rs().row",
                        "best"});
            table2.AddRow(new string[] {
                        "rs().row",
                        "so far"});
            table2.AddRow(new string[] {
                        "rs().row",
                        "the"});
            table2.AddRow(new string[] {
                        "rs().row",
                        "user"});
            table2.AddRow(new string[] {
                        "rs().row",
                        "Warewolf"});
            table2.AddRow(new string[] {
                        "rs().row",
                        "You"});
#line 19
 testRunner.Then("the sorted recordset \"[[rs(*).row]]\"  will be", ((string)(null)), table2, "Then ");
#line 28
 testRunner.And("the execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "Sort Field",
                        "Sort Order"});
            table3.AddRow(new string[] {
                        "[[rs(1).row]] = You",
                        ""});
            table3.AddRow(new string[] {
                        "[[rs(2).row]] = are",
                        ""});
            table3.AddRow(new string[] {
                        "[[rs(3).row]] = the",
                        ""});
            table3.AddRow(new string[] {
                        "[[rs(4).row]] = best",
                        ""});
            table3.AddRow(new string[] {
                        "[[rs(5).row]] = Warewolf",
                        ""});
            table3.AddRow(new string[] {
                        "[[rs(6).row]] = user",
                        ""});
            table3.AddRow(new string[] {
                        "[[rs(7).row]] = so far",
                        "Forward"});
#line 29
 testRunner.And("the debug inputs as", ((string)(null)), table3, "And ");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table4.AddRow(new string[] {
                        "[[rs(1).row]] = are"});
            table4.AddRow(new string[] {
                        "[[rs(2).row]] = best"});
            table4.AddRow(new string[] {
                        "[[rs(3).row]] = so far"});
            table4.AddRow(new string[] {
                        "[[rs(4).row]] = the"});
            table4.AddRow(new string[] {
                        "[[rs(5).row]] = user"});
            table4.AddRow(new string[] {
                        "[[rs(6).row]] = Warewolf"});
            table4.AddRow(new string[] {
                        "[[rs(7).row]] = You"});
#line 38
 testRunner.And("the debug output as", ((string)(null)), table4, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Sort a recordset backwards using star notation")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Sort")]
        public virtual void SortARecordsetBackwardsUsingStarNotation()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sort a recordset backwards using star notation", ((string[])(null)));
#line 48
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "rs",
                        "value"});
            table5.AddRow(new string[] {
                        "rs().row",
                        "You"});
            table5.AddRow(new string[] {
                        "rs().row",
                        "are"});
            table5.AddRow(new string[] {
                        "rs().row",
                        "the"});
            table5.AddRow(new string[] {
                        "rs().row",
                        "best"});
            table5.AddRow(new string[] {
                        "rs().row",
                        "Warewolf"});
            table5.AddRow(new string[] {
                        "rs().row",
                        "user"});
            table5.AddRow(new string[] {
                        "rs().row",
                        "so far"});
#line 49
 testRunner.Given("I have the following recordset to sort", ((string)(null)), table5, "Given ");
#line 58
 testRunner.And("I sort a record \"[[rs(*).row]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 59
 testRunner.And("my sort order is \"Backwards\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 60
 testRunner.When("the sort records tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "rs",
                        "value"});
            table6.AddRow(new string[] {
                        "rs().row",
                        "You"});
            table6.AddRow(new string[] {
                        "rs().row",
                        "Warewolf"});
            table6.AddRow(new string[] {
                        "rs().row",
                        "user"});
            table6.AddRow(new string[] {
                        "rs().row",
                        "the"});
            table6.AddRow(new string[] {
                        "rs().row",
                        "so far"});
            table6.AddRow(new string[] {
                        "rs().row",
                        "best"});
            table6.AddRow(new string[] {
                        "rs().row",
                        "are"});
#line 61
 testRunner.Then("the sorted recordset \"[[rs(*).row]]\"  will be", ((string)(null)), table6, "Then ");
#line 70
 testRunner.And("the execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                        "Sort Field",
                        "Sort Order"});
            table7.AddRow(new string[] {
                        "[[rs(1).row]] = You",
                        ""});
            table7.AddRow(new string[] {
                        "[[rs(2).row]] = are",
                        ""});
            table7.AddRow(new string[] {
                        "[[rs(3).row]] = the",
                        ""});
            table7.AddRow(new string[] {
                        "[[rs(4).row]] = best",
                        ""});
            table7.AddRow(new string[] {
                        "[[rs(5).row]] = Warewolf",
                        ""});
            table7.AddRow(new string[] {
                        "[[rs(6).row]] = user",
                        ""});
            table7.AddRow(new string[] {
                        "[[rs(7).row]] = so far",
                        "Backwards"});
#line 71
 testRunner.And("the debug inputs as", ((string)(null)), table7, "And ");
#line hidden
            TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table8.AddRow(new string[] {
                        "[[rs(1).row]] = You"});
            table8.AddRow(new string[] {
                        "[[rs(2).row]] = Warewolf"});
            table8.AddRow(new string[] {
                        "[[rs(3).row]] = user"});
            table8.AddRow(new string[] {
                        "[[rs(4).row]] = the"});
            table8.AddRow(new string[] {
                        "[[rs(5).row]] = so far"});
            table8.AddRow(new string[] {
                        "[[rs(6).row]] = best"});
            table8.AddRow(new string[] {
                        "[[rs(7).row]] = are"});
#line 80
 testRunner.And("the debug output as", ((string)(null)), table8, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Sort a recordset forwards")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Sort")]
        public virtual void SortARecordsetForwards()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sort a recordset forwards", ((string[])(null)));
#line 90
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table9 = new TechTalk.SpecFlow.Table(new string[] {
                        "rs",
                        "value"});
            table9.AddRow(new string[] {
                        "rs().row",
                        "You"});
            table9.AddRow(new string[] {
                        "rs().row",
                        "are"});
            table9.AddRow(new string[] {
                        "rs().row",
                        "the"});
            table9.AddRow(new string[] {
                        "rs().row",
                        "best"});
            table9.AddRow(new string[] {
                        "rs().row",
                        "Warewolf"});
            table9.AddRow(new string[] {
                        "rs().row",
                        "user"});
            table9.AddRow(new string[] {
                        "rs().row",
                        "so far"});
#line 91
 testRunner.Given("I have the following recordset to sort", ((string)(null)), table9, "Given ");
#line 100
 testRunner.And("I sort a record \"[[rs().row]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 101
 testRunner.And("my sort order is \"Forward\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 102
 testRunner.When("the sort records tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table10 = new TechTalk.SpecFlow.Table(new string[] {
                        "rs",
                        "value"});
            table10.AddRow(new string[] {
                        "rs().row",
                        "are"});
            table10.AddRow(new string[] {
                        "rs().row",
                        "best"});
            table10.AddRow(new string[] {
                        "rs().row",
                        "so far"});
            table10.AddRow(new string[] {
                        "rs().row",
                        "the"});
            table10.AddRow(new string[] {
                        "rs().row",
                        "user"});
            table10.AddRow(new string[] {
                        "rs().row",
                        "Warewolf"});
            table10.AddRow(new string[] {
                        "rs().row",
                        "You"});
#line 103
 testRunner.Then("the sorted recordset \"[[rs(*).row]]\"  will be", ((string)(null)), table10, "Then ");
#line 112
 testRunner.And("the execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table11 = new TechTalk.SpecFlow.Table(new string[] {
                        "Sort Field",
                        "Sort Order"});
            table11.AddRow(new string[] {
                        "[[rs(1).row]] = You",
                        ""});
            table11.AddRow(new string[] {
                        "[[rs(2).row]] = are",
                        ""});
            table11.AddRow(new string[] {
                        "[[rs(3).row]] = the",
                        ""});
            table11.AddRow(new string[] {
                        "[[rs(4).row]] = best",
                        ""});
            table11.AddRow(new string[] {
                        "[[rs(5).row]] = Warewolf",
                        ""});
            table11.AddRow(new string[] {
                        "[[rs(6).row]] = user",
                        ""});
            table11.AddRow(new string[] {
                        "[[rs(7).row]] = so far",
                        "Forward"});
#line 113
 testRunner.And("the debug inputs as", ((string)(null)), table11, "And ");
#line hidden
            TechTalk.SpecFlow.Table table12 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table12.AddRow(new string[] {
                        "[[rs(1).row]] = are"});
            table12.AddRow(new string[] {
                        "[[rs(2).row]] = best"});
            table12.AddRow(new string[] {
                        "[[rs(3).row]] = so far"});
            table12.AddRow(new string[] {
                        "[[rs(4).row]] = the"});
            table12.AddRow(new string[] {
                        "[[rs(5).row]] = user"});
            table12.AddRow(new string[] {
                        "[[rs(6).row]] = Warewolf"});
            table12.AddRow(new string[] {
                        "[[rs(7).row]] = You"});
#line 122
 testRunner.And("the debug output as", ((string)(null)), table12, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Sort a recordset backwards")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Sort")]
        public virtual void SortARecordsetBackwards()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sort a recordset backwards", ((string[])(null)));
#line 132
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table13 = new TechTalk.SpecFlow.Table(new string[] {
                        "rs",
                        "value"});
            table13.AddRow(new string[] {
                        "rs().row",
                        "You"});
            table13.AddRow(new string[] {
                        "rs().row",
                        "are"});
            table13.AddRow(new string[] {
                        "rs().row",
                        "the"});
            table13.AddRow(new string[] {
                        "rs().row",
                        "best"});
            table13.AddRow(new string[] {
                        "rs().row",
                        "Warewolf"});
            table13.AddRow(new string[] {
                        "rs().row",
                        "user"});
            table13.AddRow(new string[] {
                        "rs().row",
                        "so far"});
#line 133
 testRunner.Given("I have the following recordset to sort", ((string)(null)), table13, "Given ");
#line 142
 testRunner.And("I sort a record \"[[rs(*).row]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 143
 testRunner.And("my sort order is \"Backwards\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 144
 testRunner.When("the sort records tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table14 = new TechTalk.SpecFlow.Table(new string[] {
                        "rs",
                        "value"});
            table14.AddRow(new string[] {
                        "rs().row",
                        "You"});
            table14.AddRow(new string[] {
                        "rs().row",
                        "Warewolf"});
            table14.AddRow(new string[] {
                        "rs().row",
                        "user"});
            table14.AddRow(new string[] {
                        "rs().row",
                        "the"});
            table14.AddRow(new string[] {
                        "rs().row",
                        "so far"});
            table14.AddRow(new string[] {
                        "rs().row",
                        "best"});
            table14.AddRow(new string[] {
                        "rs().row",
                        "are"});
#line 145
 testRunner.Then("the sorted recordset \"[[rs(*).row]]\"  will be", ((string)(null)), table14, "Then ");
#line 154
 testRunner.And("the execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table15 = new TechTalk.SpecFlow.Table(new string[] {
                        "Sort Field",
                        "Sort Order"});
            table15.AddRow(new string[] {
                        "[[rs(1).row]] = You",
                        ""});
            table15.AddRow(new string[] {
                        "[[rs(2).row]] = are",
                        ""});
            table15.AddRow(new string[] {
                        "[[rs(3).row]] = the",
                        ""});
            table15.AddRow(new string[] {
                        "[[rs(4).row]] = best",
                        ""});
            table15.AddRow(new string[] {
                        "[[rs(5).row]] = Warewolf",
                        ""});
            table15.AddRow(new string[] {
                        "[[rs(6).row]] = user",
                        ""});
            table15.AddRow(new string[] {
                        "[[rs(7).row]] = so far",
                        "Backwards"});
#line 155
 testRunner.And("the debug inputs as", ((string)(null)), table15, "And ");
#line hidden
            TechTalk.SpecFlow.Table table16 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table16.AddRow(new string[] {
                        "[[rs(1).row]] = You"});
            table16.AddRow(new string[] {
                        "[[rs(2).row]] = Warewolf"});
            table16.AddRow(new string[] {
                        "[[rs(3).row]] = user"});
            table16.AddRow(new string[] {
                        "[[rs(4).row]] = the"});
            table16.AddRow(new string[] {
                        "[[rs(5).row]] = so far"});
            table16.AddRow(new string[] {
                        "[[rs(6).row]] = best"});
            table16.AddRow(new string[] {
                        "[[rs(7).row]] = are"});
#line 164
 testRunner.And("the debug output as", ((string)(null)), table16, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Sort a recordset forwards empty recordset")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Sort")]
        public virtual void SortARecordsetForwardsEmptyRecordset()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sort a recordset forwards empty recordset", ((string[])(null)));
#line 174
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table17 = new TechTalk.SpecFlow.Table(new string[] {
                        "rs",
                        "value"});
#line 175
 testRunner.Given("I have the following recordset to sort", ((string)(null)), table17, "Given ");
#line 177
 testRunner.And("I sort a record \"[[rs().row]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 178
 testRunner.And("my sort order is \"Forward\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 179
 testRunner.When("the sort records tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 180
 testRunner.Then("the execution has \"AN\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Sort a recordset backwards empty recordset")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Sort")]
        public virtual void SortARecordsetBackwardsEmptyRecordset()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sort a recordset backwards empty recordset", ((string[])(null)));
#line 183
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table18 = new TechTalk.SpecFlow.Table(new string[] {
                        "rs",
                        "value"});
#line 184
 testRunner.Given("I have the following recordset to sort", ((string)(null)), table18, "Given ");
#line 186
 testRunner.And("I sort a record \"[[rs().row]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 187
 testRunner.And("my sort order is \"Backwards\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 188
 testRunner.When("the sort records tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 189
 testRunner.Then("the execution has \"AN\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Sort a recordset forwards with one row")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Sort")]
        public virtual void SortARecordsetForwardsWithOneRow()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sort a recordset forwards with one row", ((string[])(null)));
#line 191
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table19 = new TechTalk.SpecFlow.Table(new string[] {
                        "rs",
                        "value"});
            table19.AddRow(new string[] {
                        "rs().row",
                        "Warewolf"});
#line 192
 testRunner.Given("I have the following recordset to sort", ((string)(null)), table19, "Given ");
#line 195
 testRunner.And("I sort a record \"[[rs().row]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 196
 testRunner.And("my sort order is \"Forward\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 197
 testRunner.When("the sort records tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table20 = new TechTalk.SpecFlow.Table(new string[] {
                        "rs",
                        "value"});
            table20.AddRow(new string[] {
                        "rs().row",
                        "Warewolf"});
#line 198
 testRunner.Then("the sorted recordset \"[[rs(*).row]]\"  will be", ((string)(null)), table20, "Then ");
#line 201
 testRunner.And("the execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table21 = new TechTalk.SpecFlow.Table(new string[] {
                        "Sort Field",
                        "Sort Order"});
            table21.AddRow(new string[] {
                        "[[rs(1).row]] = Warewolf",
                        "Forward"});
#line 202
 testRunner.And("the debug inputs as", ((string)(null)), table21, "And ");
#line hidden
            TechTalk.SpecFlow.Table table22 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table22.AddRow(new string[] {
                        "[[rs(1).row]] = Warewolf"});
#line 205
 testRunner.And("the debug output as", ((string)(null)), table22, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Sort a recordset backwards recordset  with one row")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Sort")]
        public virtual void SortARecordsetBackwardsRecordsetWithOneRow()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sort a recordset backwards recordset  with one row", ((string[])(null)));
#line 209
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table23 = new TechTalk.SpecFlow.Table(new string[] {
                        "rs",
                        "value"});
            table23.AddRow(new string[] {
                        "rs().row",
                        "Warewolf"});
#line 210
 testRunner.Given("I have the following recordset to sort", ((string)(null)), table23, "Given ");
#line 213
 testRunner.And("I sort a record \"[[rs().row]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 214
 testRunner.And("my sort order is \"Backwards\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 215
 testRunner.When("the sort records tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table24 = new TechTalk.SpecFlow.Table(new string[] {
                        "rs",
                        "value"});
            table24.AddRow(new string[] {
                        "rs().row",
                        "Warewolf"});
#line 216
 testRunner.Then("the sorted recordset \"[[rs(*).row]]\"  will be", ((string)(null)), table24, "Then ");
#line 219
 testRunner.And("the execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table25 = new TechTalk.SpecFlow.Table(new string[] {
                        "Sort Field",
                        "Sort Order"});
            table25.AddRow(new string[] {
                        "[[rs(1).row]] = Warewolf",
                        "Backwards"});
#line 220
 testRunner.And("the debug inputs as", ((string)(null)), table25, "And ");
#line hidden
            TechTalk.SpecFlow.Table table26 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table26.AddRow(new string[] {
                        "[[rs(1).row]] = Warewolf"});
#line 223
 testRunner.And("the debug output as", ((string)(null)), table26, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Sort Recordset without field Forwards")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Sort")]
        public virtual void SortRecordsetWithoutFieldForwards()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sort Recordset without field Forwards", ((string[])(null)));
#line 266
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table27 = new TechTalk.SpecFlow.Table(new string[] {
                        "rs",
                        "value"});
            table27.AddRow(new string[] {
                        "rs(1).a",
                        "Zambia"});
            table27.AddRow(new string[] {
                        "rec(1).a",
                        "Mangolia"});
            table27.AddRow(new string[] {
                        "rs(2).a",
                        "America"});
            table27.AddRow(new string[] {
                        "rec(2).a",
                        "Australia"});
#line 267
 testRunner.Given("I have the following recordset to sort", ((string)(null)), table27, "Given ");
#line 273
 testRunner.And("I sort a record \"[[rs(*)]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 274
 testRunner.And("my sort order is \"Forward\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 275
 testRunner.When("the sort records tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 276
 testRunner.Then("the execution has \"AN\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Sort Null Recordset")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Sort")]
        public virtual void SortNullRecordset()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sort Null Recordset", ((string[])(null)));
#line 279
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table28 = new TechTalk.SpecFlow.Table(new string[] {
                        "rs",
                        "value"});
            table28.AddRow(new string[] {
                        "[[rs().a]]",
                        "NULL"});
#line 280
 testRunner.Given("I have the following recordset to sort", ((string)(null)), table28, "Given ");
#line 283
 testRunner.And("I sort a record \"[[rs(*).a]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 284
 testRunner.And("my sort order is \"Backwards\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 285
 testRunner.When("the sort records tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 286
 testRunner.Then("the execution has \"No\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Sort non existent Recordset")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Sort")]
        public virtual void SortNonExistentRecordset()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sort non existent Recordset", ((string[])(null)));
#line 289
 this.ScenarioSetup(scenarioInfo);
#line 290
 testRunner.Given("I sort a record \"[[rs(*)]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 291
 testRunner.And("my sort order is \"Backwards\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 292
 testRunner.When("the sort records tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 293
 testRunner.Then("the execution has \"An\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
