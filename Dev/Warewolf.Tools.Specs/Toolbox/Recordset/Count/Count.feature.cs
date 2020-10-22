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
namespace Warewolf.Tools.Specs.Toolbox.Recordset.Count
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.3.2.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute()]
    public partial class CountFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private Microsoft.VisualStudio.TestTools.UnitTesting.TestContext _testContext;
        
#line 1 "Count.feature"
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
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Count", "\tIn order to count records\r\n\tAs a Warewolf user\r\n\tI want a tool that takes a reco" +
                    "rd set counts it", ProgrammingLanguage.CSharp, new string[] {
                        "Recordset"});
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
                        && (testRunner.FeatureContext.FeatureInfo.Title != "Count")))
            {
                global::Warewolf.Tools.Specs.Toolbox.Recordset.Count.CountFeature.FeatureSetup(null);
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
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Count a number of records in a recordset with 3 rows")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Count")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Recordset")]
        public virtual void CountANumberOfRecordsInARecordsetWith3Rows()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Count a number of records in a recordset with 3 rows", ((string[])(null)));
#line 8
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table1825 = new TechTalk.SpecFlow.Table(new string[] {
                        "[[rs]]",
                        ""});
            table1825.AddRow(new string[] {
                        "rs().row",
                        "1"});
            table1825.AddRow(new string[] {
                        "rs().row",
                        "2"});
            table1825.AddRow(new string[] {
                        "rs().row",
                        "3"});
#line 9
 testRunner.Given("I have a recordset with this shape", ((string)(null)), table1825, "Given ");
#line 14
 testRunner.And("count on record \"[[rs()]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 15
 testRunner.When("the count tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 16
 testRunner.Then("the result count should be 3", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 17
 testRunner.And("the execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table1826 = new TechTalk.SpecFlow.Table(new string[] {
                        "Recordset"});
            table1826.AddRow(new string[] {
                        "[[rs(1).row]] = 1"});
            table1826.AddRow(new string[] {
                        "[[rs(2).row]] = 2"});
            table1826.AddRow(new string[] {
                        "[[rs(3).row]] = 3"});
#line 18
 testRunner.And("the debug inputs as", ((string)(null)), table1826, "And ");
#line hidden
            TechTalk.SpecFlow.Table table1827 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table1827.AddRow(new string[] {
                        "[[result]] = 3"});
#line 23
 testRunner.And("the debug output as", ((string)(null)), table1827, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Count a number of records in a recordset with 8 rows")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Count")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Recordset")]
        public virtual void CountANumberOfRecordsInARecordsetWith8Rows()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Count a number of records in a recordset with 8 rows", ((string[])(null)));
#line 27
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table1828 = new TechTalk.SpecFlow.Table(new string[] {
                        "rs",
                        ""});
            table1828.AddRow(new string[] {
                        "rs().row",
                        "1"});
            table1828.AddRow(new string[] {
                        "rs().row",
                        "2"});
            table1828.AddRow(new string[] {
                        "rs().row",
                        "3"});
            table1828.AddRow(new string[] {
                        "rs().row",
                        "4"});
            table1828.AddRow(new string[] {
                        "rs().row",
                        "5"});
            table1828.AddRow(new string[] {
                        "rs().row",
                        "6"});
            table1828.AddRow(new string[] {
                        "rs().row",
                        "7"});
            table1828.AddRow(new string[] {
                        "rs().row",
                        "8"});
#line 28
 testRunner.Given("I have a recordset with this shape", ((string)(null)), table1828, "Given ");
#line 38
 testRunner.And("count on record \"[[rs()]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 39
 testRunner.When("the count tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 40
 testRunner.Then("the result count should be 8", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 41
 testRunner.And("the execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table1829 = new TechTalk.SpecFlow.Table(new string[] {
                        "Recordset"});
            table1829.AddRow(new string[] {
                        "[[rs(1).row]] =  1"});
            table1829.AddRow(new string[] {
                        "[[rs(2).row]] =  2"});
            table1829.AddRow(new string[] {
                        "[[rs(3).row]] =  3"});
            table1829.AddRow(new string[] {
                        "[[rs(4).row]] =  4"});
            table1829.AddRow(new string[] {
                        "[[rs(5).row]] =  5"});
            table1829.AddRow(new string[] {
                        "[[rs(6).row]] =  6"});
            table1829.AddRow(new string[] {
                        "[[rs(7).row]] =  7"});
            table1829.AddRow(new string[] {
                        "[[rs(8).row]] =  8"});
#line 42
 testRunner.And("the debug inputs as", ((string)(null)), table1829, "And ");
#line hidden
            TechTalk.SpecFlow.Table table1830 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table1830.AddRow(new string[] {
                        "[[result]] = 8"});
#line 52
 testRunner.And("the debug output as", ((string)(null)), table1830, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Count a number of records in a empty recordset")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Count")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Recordset")]
        public virtual void CountANumberOfRecordsInAEmptyRecordset()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Count a number of records in a empty recordset", ((string[])(null)));
#line 58
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table1831 = new TechTalk.SpecFlow.Table(new string[] {
                        "rs",
                        ""});
            table1831.AddRow(new string[] {
                        "[[rs().row]]",
                        "NULL"});
#line 59
 testRunner.Given("I have a recordset with this shape", ((string)(null)), table1831, "Given ");
#line 62
 testRunner.And("count on record \"[[rs()]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 63
 testRunner.When("the count tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 64
 testRunner.Then("the result count should be 0", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 65
 testRunner.And("the execution has \"No\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Count a number of records in a unassigned recordset")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Count")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Recordset")]
        public virtual void CountANumberOfRecordsInAUnassignedRecordset()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Count a number of records in a unassigned recordset", ((string[])(null)));
#line 67
this.ScenarioSetup(scenarioInfo);
#line 68
 testRunner.Given("count on record \"[[rs()]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 69
 testRunner.When("the count tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 70
 testRunner.Then("the execution has \"An\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Count record with invalid variables")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Count")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Recordset")]
        public virtual void CountRecordWithInvalidVariables()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Count record with invalid variables", ((string[])(null)));
#line 73
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table1832 = new TechTalk.SpecFlow.Table(new string[] {
                        "rs",
                        ""});
            table1832.AddRow(new string[] {
                        "rs().row",
                        "1"});
            table1832.AddRow(new string[] {
                        "rs().row",
                        "2"});
            table1832.AddRow(new string[] {
                        "rs().row",
                        "3"});
            table1832.AddRow(new string[] {
                        "rs().row",
                        "4"});
            table1832.AddRow(new string[] {
                        "rs().row",
                        "5"});
            table1832.AddRow(new string[] {
                        "rs().row",
                        "6"});
            table1832.AddRow(new string[] {
                        "rs().row",
                        "7"});
            table1832.AddRow(new string[] {
                        "rs().row",
                        "8"});
#line 74
 testRunner.Given("I have a recordset with this shape", ((string)(null)), table1832, "Given ");
#line 84
 testRunner.And("count on record \"[[rs().#$]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 85
 testRunner.When("the count tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 86
 testRunner.Then("the result count should be 0", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 87
 testRunner.And("the execution has \"AN\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table1833 = new TechTalk.SpecFlow.Table(new string[] {
                        "Recordset"});
#line 88
 testRunner.And("the debug inputs as", ((string)(null)), table1833, "And ");
#line hidden
            TechTalk.SpecFlow.Table table1834 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
#line 90
 testRunner.And("the debug output as", ((string)(null)), table1834, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Count only one column record")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Count")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Recordset")]
        public virtual void CountOnlyOneColumnRecord()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Count only one column record", ((string[])(null)));
#line 93
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table1835 = new TechTalk.SpecFlow.Table(new string[] {
                        "rs",
                        ""});
            table1835.AddRow(new string[] {
                        "rs().row",
                        "1"});
            table1835.AddRow(new string[] {
                        "rs().row",
                        "2"});
            table1835.AddRow(new string[] {
                        "rs().row",
                        "3"});
            table1835.AddRow(new string[] {
                        "rs().row",
                        "4"});
            table1835.AddRow(new string[] {
                        "rs().row",
                        "5"});
            table1835.AddRow(new string[] {
                        "rs().row",
                        "6"});
            table1835.AddRow(new string[] {
                        "rs().row",
                        "7"});
            table1835.AddRow(new string[] {
                        "rs().row",
                        "8"});
#line 94
 testRunner.Given("I have a recordset with this shape", ((string)(null)), table1835, "Given ");
#line 104
 testRunner.And("count on record \"[[rs(*).row]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 105
 testRunner.When("the count tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 106
 testRunner.Then("the result count should be 0", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 107
 testRunner.And("the execution has \"AN\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table1836 = new TechTalk.SpecFlow.Table(new string[] {
                        "Recordset"});
#line 108
 testRunner.And("the debug inputs as", ((string)(null)), table1836, "And ");
#line hidden
            TechTalk.SpecFlow.Table table1837 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
#line 110
 testRunner.And("the debug output as", ((string)(null)), table1837, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Count only one coloumn record")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Count")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Recordset")]
        public virtual void CountOnlyOneColoumnRecord()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Count only one coloumn record", ((string[])(null)));
#line 113
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table1838 = new TechTalk.SpecFlow.Table(new string[] {
                        "rs",
                        ""});
            table1838.AddRow(new string[] {
                        "rs().row",
                        "1"});
            table1838.AddRow(new string[] {
                        "rs().row",
                        "2"});
            table1838.AddRow(new string[] {
                        "rs().row",
                        "3"});
            table1838.AddRow(new string[] {
                        "rs().row",
                        "4"});
            table1838.AddRow(new string[] {
                        "fs().row",
                        "5"});
            table1838.AddRow(new string[] {
                        "fs().row",
                        "6"});
            table1838.AddRow(new string[] {
                        "fs().row",
                        "7"});
            table1838.AddRow(new string[] {
                        "fs().row",
                        "8"});
#line 114
 testRunner.Given("I have a recordset with this shape", ((string)(null)), table1838, "Given ");
#line 124
 testRunner.And("count on record \"[[rs().row]],[[fs().row]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 125
 testRunner.When("the count tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 126
 testRunner.Then("the result count should be 0", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 127
 testRunner.And("the execution has \"AN\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table1839 = new TechTalk.SpecFlow.Table(new string[] {
                        "Recordset"});
#line 128
 testRunner.And("the debug inputs as", ((string)(null)), table1839, "And ");
#line hidden
            TechTalk.SpecFlow.Table table1840 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
#line 130
 testRunner.And("the debug output as", ((string)(null)), table1840, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Count a number of records when two recordsets are defined.")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Count")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Recordset")]
        public virtual void CountANumberOfRecordsWhenTwoRecordsetsAreDefined_()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Count a number of records when two recordsets are defined.", ((string[])(null)));
#line 133
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table1841 = new TechTalk.SpecFlow.Table(new string[] {
                        "rs",
                        ""});
            table1841.AddRow(new string[] {
                        "rs().row",
                        "a"});
            table1841.AddRow(new string[] {
                        "fs().row",
                        "a"});
            table1841.AddRow(new string[] {
                        "rs().row",
                        "b"});
            table1841.AddRow(new string[] {
                        "rs().row",
                        "c"});
            table1841.AddRow(new string[] {
                        "fs().row",
                        "b"});
            table1841.AddRow(new string[] {
                        "rs().row",
                        "d"});
            table1841.AddRow(new string[] {
                        "fs().row",
                        "c"});
            table1841.AddRow(new string[] {
                        "rs().row",
                        "e"});
#line 134
 testRunner.Given("I have a recordset with this shape", ((string)(null)), table1841, "Given ");
#line 144
 testRunner.And("count on record \"[[rs()]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 145
 testRunner.When("the count tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 146
 testRunner.Then("the result count should be 5", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 147
 testRunner.And("the execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table1842 = new TechTalk.SpecFlow.Table(new string[] {
                        "Recordset"});
            table1842.AddRow(new string[] {
                        "[[rs(1).row]] = a"});
            table1842.AddRow(new string[] {
                        "[[rs(2).row]] = b"});
            table1842.AddRow(new string[] {
                        "[[rs(3).row]] = c"});
            table1842.AddRow(new string[] {
                        "[[rs(4).row]] =  d"});
            table1842.AddRow(new string[] {
                        "[[rs(5).row]] =  e"});
#line 148
 testRunner.And("the debug inputs as", ((string)(null)), table1842, "And ");
#line hidden
            TechTalk.SpecFlow.Table table1843 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table1843.AddRow(new string[] {
                        "[[result]] = 5"});
#line 155
 testRunner.And("the debug output as", ((string)(null)), table1843, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Executing Count with two variables in result field")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Count")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Recordset")]
        public virtual void ExecutingCountWithTwoVariablesInResultField()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Executing Count with two variables in result field", ((string[])(null)));
#line 159
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table1844 = new TechTalk.SpecFlow.Table(new string[] {
                        "rs",
                        ""});
            table1844.AddRow(new string[] {
                        "rs(1).row",
                        "1"});
            table1844.AddRow(new string[] {
                        "rs(2).row",
                        "2"});
            table1844.AddRow(new string[] {
                        "rs(3).row",
                        "3"});
            table1844.AddRow(new string[] {
                        "rs(4).row",
                        "4"});
#line 160
 testRunner.Given("I have a recordset with this shape", ((string)(null)), table1844, "Given ");
#line 166
 testRunner.And("count on record \"[[rs()]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 167
    testRunner.And("result variable as \"[[b]][[a]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 168
 testRunner.When("the count tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 169
 testRunner.Then("the result count should be 0", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 170
 testRunner.And("the execution has \"AN\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table1845 = new TechTalk.SpecFlow.Table(new string[] {
                        "Recordset"});
            table1845.AddRow(new string[] {
                        "[[rs(1).row]] = 1"});
            table1845.AddRow(new string[] {
                        "[[rs(2).row]] = 2"});
            table1845.AddRow(new string[] {
                        "[[rs(3).row]] = 3"});
            table1845.AddRow(new string[] {
                        "[[rs(4).row]] = 4"});
#line 171
 testRunner.And("the debug inputs as", ((string)(null)), table1845, "And ");
#line hidden
            TechTalk.SpecFlow.Table table1846 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
#line 177
 testRunner.And("the debug output as", ((string)(null)), table1846, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        public virtual void EnsureVariablesOfDifferentTypesProduceDesiredResults(string count, string val, string error, string message, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Ensure variables of different types produce desired results", exampleTags);
#line 180
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table1847 = new TechTalk.SpecFlow.Table(new string[] {
                        "[[rs]]",
                        ""});
            table1847.AddRow(new string[] {
                        "rs().row",
                        "1"});
            table1847.AddRow(new string[] {
                        "rs().row",
                        "2"});
            table1847.AddRow(new string[] {
                        "rs().row",
                        "3"});
#line 181
 testRunner.Given("I have a recordset with this shape", ((string)(null)), table1847, "Given ");
#line 186
 testRunner.And("count on record \"<count>\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 187
 testRunner.When("the count tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 188
 testRunner.Then(string.Format("the result count should be \"{0}\"", val), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 189
 testRunner.And(string.Format("the execution has \"{0}\" error", error), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Ensure variables of different types produce desired results: ")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Count")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Recordset")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Count", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:val", "0")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:error", "AN")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Message", "Recordset not given")]
        public virtual void EnsureVariablesOfDifferentTypesProduceDesiredResults_()
        {
#line 180
this.EnsureVariablesOfDifferentTypesProduceDesiredResults("", "0", "AN", "Recordset not given", ((string[])(null)));
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Ensure variables of different types produce desired results: [[var]]")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Count")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Recordset")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "[[var]]")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Count", "[[var]]")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:val", "0")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:error", "AN")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Message", "Scalar not allowed")]
        public virtual void EnsureVariablesOfDifferentTypesProduceDesiredResults_Var()
        {
#line 180
this.EnsureVariablesOfDifferentTypesProduceDesiredResults("[[var]]", "0", "AN", "Scalar not allowed", ((string[])(null)));
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Ensure variables of different types produce desired results: [[q]]")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Count")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Recordset")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "[[q]]")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Count", "[[q]]")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:val", "0")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:error", "AN")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Message", "Value must be a recordset")]
        public virtual void EnsureVariablesOfDifferentTypesProduceDesiredResults_Q()
        {
#line 180
this.EnsureVariablesOfDifferentTypesProduceDesiredResults("[[q]]", "0", "AN", "Value must be a recordset", ((string[])(null)));
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Ensure variables of different types produce desired results: Test")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Count")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Recordset")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "Test")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Count", "Test")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:val", "0")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:error", "AN")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Message", "Value must be a recordset")]
        public virtual void EnsureVariablesOfDifferentTypesProduceDesiredResults_Test()
        {
#line 180
this.EnsureVariablesOfDifferentTypesProduceDesiredResults("Test", "0", "AN", "Value must be a recordset", ((string[])(null)));
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Ensure variables of different types produce desired results: 99")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Count")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Recordset")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "99")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Count", "99")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:val", "0")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:error", "AN")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Message", "Value must be a recordset")]
        public virtual void EnsureVariablesOfDifferentTypesProduceDesiredResults_99()
        {
#line 180
this.EnsureVariablesOfDifferentTypesProduceDesiredResults("99", "0", "AN", "Value must be a recordset", ((string[])(null)));
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Ensure variables of different types produce desired results: [[rs([[var]])]]")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Count")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Recordset")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "[[rs([[var]])]]")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Count", "[[rs([[var]])]]")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:val", "0")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:error", "AN")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Message", "[[result]] = Error")]
        public virtual void EnsureVariablesOfDifferentTypesProduceDesiredResults_RsVar()
        {
#line 180
this.EnsureVariablesOfDifferentTypesProduceDesiredResults("[[rs([[var]])]]", "0", "AN", "[[result]] = Error", ((string[])(null)));
#line hidden
        }
    }
}
#pragma warning restore
#endregion
