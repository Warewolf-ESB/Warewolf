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
namespace Warewolf.Tools.Specs.Toolbox.Utility.Redis.Cache
{
    using TechTalk.SpecFlow;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute()]
    public partial class RedisCacheFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private Microsoft.VisualStudio.TestTools.UnitTesting.TestContext _testContext;
        
        private static string[] featureTags = ((string[])(null));
        
#line 1 "CacheRedis.feature"
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
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Toolbox/Utility/Redis/Cache", "RedisCache", "\tIn order to avoid rerunning the work-flow every time we need generated data\r\n\tAs" +
                    " a user\r\n\tI want to be to cached data while the Time To Live has not elapsed ", ProgrammingLanguage.CSharp, featureTags);
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
                        && (testRunner.FeatureContext.FeatureInfo.Title != "RedisCache")))
            {
                global::Warewolf.Tools.Specs.Toolbox.Utility.Redis.Cache.RedisCacheFeature.FeatureSetup(null);
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
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("No data in cache")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "RedisCache")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("RedisCache")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("AnonymousRedis")]
        public void NoDataInCache()
        {
            string[] tagsOfScenario = new string[] {
                    "RedisCache",
                    "AnonymousRedis"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("No data in cache", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 8
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 9
 testRunner.Given("valid Redis source", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 10
 testRunner.And("I have a key \"MyData\" with GUID and ttl of \"3000\" milliseconds", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 11
 testRunner.And("No data in the cache", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table2841 = new TechTalk.SpecFlow.Table(new string[] {
                            "var",
                            "value"});
                table2841.AddRow(new string[] {
                            "[[Var1]]",
                            "\"Test1\""});
#line 12
 testRunner.And("an assign \"dataToStore\" as", ((string)(null)), table2841, "And ");
#line hidden
#line 15
 testRunner.When("I execute the Redis Cache tool", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table2842 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Data"});
                table2842.AddRow(new string[] {
                            "MyData",
                            "\"[[Var1]],Test1\""});
#line 16
 testRunner.Then("the cache will contain", ((string)(null)), table2842, "Then ");
#line hidden
                TechTalk.SpecFlow.Table table2843 = new TechTalk.SpecFlow.Table(new string[] {
                            "var",
                            "value"});
                table2843.AddRow(new string[] {
                            "[[Var1]]",
                            "\"Test1\""});
#line 19
 testRunner.And("output variables have the following values", ((string)(null)), table2843, "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Data exists for given TTL not hit")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "RedisCache")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("RedisCache")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("AnonymousRedis")]
        public void DataExistsForGivenTTLNotHit()
        {
            string[] tagsOfScenario = new string[] {
                    "RedisCache",
                    "AnonymousRedis"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Data exists for given TTL not hit", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 25
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 26
 testRunner.Given("valid Redis source", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 27
 testRunner.And("I have a key \"MyData\" with GUID and ttl of \"20000\" milliseconds", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table2844 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Data"});
                table2844.AddRow(new string[] {
                            "MyData",
                            "\"[[Var1]],Data in cache\""});
#line 28
 testRunner.And("data exists (TTL not hit) for key \"MyData\" with GUID as", ((string)(null)), table2844, "And ");
#line hidden
                TechTalk.SpecFlow.Table table2845 = new TechTalk.SpecFlow.Table(new string[] {
                            "var",
                            "value"});
                table2845.AddRow(new string[] {
                            "[[Var1]]",
                            "\"Test1\""});
#line 31
 testRunner.And("an assign \"dataToStore\" as", ((string)(null)), table2845, "And ");
#line hidden
#line 34
 testRunner.When("I execute the Redis Cache tool", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 35
 testRunner.Then("the assign \"dataToStore\" is not executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
                TechTalk.SpecFlow.Table table2846 = new TechTalk.SpecFlow.Table(new string[] {
                            "var",
                            "value"});
                table2846.AddRow(new string[] {
                            "[[Var1]]",
                            "\"[[Var1]],Data in cache\""});
#line 36
 testRunner.And("output variables have the following values", ((string)(null)), table2846, "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Data Not Exist For Given Key (TTL exceeded) Spec")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "RedisCache")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("RedisCache")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("AnonymousRedis")]
        public void DataNotExistForGivenKeyTTLExceededSpec()
        {
            string[] tagsOfScenario = new string[] {
                    "RedisCache",
                    "AnonymousRedis"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Data Not Exist For Given Key (TTL exceeded) Spec", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 42
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 43
 testRunner.Given("valid Redis source", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 44
 testRunner.And("I have a key \"MyData\" with GUID and ttl of \"3000\" milliseconds", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table2847 = new TechTalk.SpecFlow.Table(new string[] {
                            "",
                            ""});
#line 45
 testRunner.And("data does not exist (TTL exceeded) for key \"MyData\" as", ((string)(null)), table2847, "And ");
#line hidden
                TechTalk.SpecFlow.Table table2848 = new TechTalk.SpecFlow.Table(new string[] {
                            "var",
                            "value"});
                table2848.AddRow(new string[] {
                            "[[Var1]]",
                            "\"Test1\""});
#line 47
 testRunner.And("an assign \"dataToStore\" as", ((string)(null)), table2848, "And ");
#line hidden
#line 50
 testRunner.When("I execute the Redis Cache tool", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 51
 testRunner.Then("the assign \"dataToStore\" is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
                TechTalk.SpecFlow.Table table2849 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Data"});
                table2849.AddRow(new string[] {
                            "MyData",
                            "\"[[Var1]],Test1\""});
#line 52
 testRunner.Then("the cache will contain", ((string)(null)), table2849, "Then ");
#line hidden
                TechTalk.SpecFlow.Table table2850 = new TechTalk.SpecFlow.Table(new string[] {
                            "var",
                            "value"});
                table2850.AddRow(new string[] {
                            "[[Var1]]",
                            "\"Test1\""});
#line 55
 testRunner.And("output variables have the following values", ((string)(null)), table2850, "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Input Variable Keys Are Less Then Cached Data Variable Keys")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "RedisCache")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("RedisCache")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("AnonymousRedis")]
        public void InputVariableKeysAreLessThenCachedDataVariableKeys()
        {
            string[] tagsOfScenario = new string[] {
                    "RedisCache",
                    "AnonymousRedis"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Input Variable Keys Are Less Then Cached Data Variable Keys", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 61
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 62
 testRunner.Given("valid Redis source", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 63
 testRunner.And("I have \"key1\" of \"MyData\" with GUID and \"ttl1\" of \"15\" seconds", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 64
 testRunner.And("I have \"key2\" of \"MyData\" with GUID and \"ttl2\" of \"3\" seconds", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table2851 = new TechTalk.SpecFlow.Table(new string[] {
                            "name",
                            "value"});
                table2851.AddRow(new string[] {
                            "[[Var1]]",
                            "\"Test1\""});
                table2851.AddRow(new string[] {
                            "[[Var2]]",
                            "\"Test2\""});
#line 65
 testRunner.And("an assign \"dataToStore1\" into \"DsfMultiAssignActivity1\" with", ((string)(null)), table2851, "And ");
#line hidden
                TechTalk.SpecFlow.Table table2852 = new TechTalk.SpecFlow.Table(new string[] {
                            "name",
                            "value"});
                table2852.AddRow(new string[] {
                            "[[Var1]]",
                            "\"Test21\""});
#line 69
 testRunner.And("an assign \"dataToStore2\" into \"DsfMultiAssignActivity2\" with", ((string)(null)), table2852, "And ");
#line hidden
#line 72
 testRunner.Then("the assigned \"key1\", \"ttl1\" and innerActivity \"DsfMultiAssignActivity1\" is execut" +
                        "ed by \"RedisActivity1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
                TechTalk.SpecFlow.Table table2853 = new TechTalk.SpecFlow.Table(new string[] {
                            "name",
                            "value"});
                table2853.AddRow(new string[] {
                            "[[Var1]]",
                            "\"Test1\""});
                table2853.AddRow(new string[] {
                            "[[Var2]]",
                            "\"Test2\""});
#line 73
 testRunner.And("the Redis Cache under \"key1\" with GUID will contain", ((string)(null)), table2853, "And ");
#line hidden
#line 77
 testRunner.Then("the assigned \"key2\", \"ttl2\" and innerActivity \"DsfMultiAssignActivity2\" is execut" +
                        "ed by \"RedisActivity2\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
                TechTalk.SpecFlow.Table table2854 = new TechTalk.SpecFlow.Table(new string[] {
                            "label",
                            "variable",
                            "operator",
                            "value"});
                table2854.AddRow(new string[] {
                            "Redis key { MyData } found",
                            "null",
                            "",
                            ""});
                table2854.AddRow(new string[] {
                            "null",
                            "[[Var1]]",
                            "=",
                            "\"Test21\""});
                table2854.AddRow(new string[] {
                            "null",
                            "[[Var2]]",
                            "=",
                            "\"Test22\""});
                table2854.AddRow(new string[] {
                            "null",
                            "[[Var3]]",
                            "=",
                            "\"Test23\""});
                table2854.AddRow(new string[] {
                            "null",
                            "[[Var4]]",
                            "=",
                            "\"Test24\""});
#line 78
 testRunner.Then("\"RedisActivity2\" output variables have the following values", ((string)(null)), table2854, "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Input Variable Keys Are Greater Then Cached Data Variable Keys")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "RedisCache")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("RedisCache")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("AnonymousRedis")]
        public void InputVariableKeysAreGreaterThenCachedDataVariableKeys()
        {
            string[] tagsOfScenario = new string[] {
                    "RedisCache",
                    "AnonymousRedis"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Input Variable Keys Are Greater Then Cached Data Variable Keys", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 88
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 89
 testRunner.Given("valid Redis source", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 90
 testRunner.And("I have \"key1\" of \"MyData\" with GUID and \"ttl1\" of \"15\" seconds", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 91
 testRunner.And("I have \"key2\" of \"MyData\" with GUID and \"ttl2\" of \"3\" seconds", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table2855 = new TechTalk.SpecFlow.Table(new string[] {
                            "name",
                            "value"});
                table2855.AddRow(new string[] {
                            "[[Var1]]",
                            "\"Test1\""});
                table2855.AddRow(new string[] {
                            "[[Var2]]",
                            "\"Test2\""});
#line 92
 testRunner.And("an assign \"dataToStore1\" into \"DsfMultiAssignActivity1\" with", ((string)(null)), table2855, "And ");
#line hidden
                TechTalk.SpecFlow.Table table2856 = new TechTalk.SpecFlow.Table(new string[] {
                            "name",
                            "value"});
                table2856.AddRow(new string[] {
                            "[[Var1]]",
                            "\"Test21\""});
                table2856.AddRow(new string[] {
                            "[[Var2]]",
                            "\"Test22\""});
                table2856.AddRow(new string[] {
                            "[[Var3]]",
                            "\"Test23\""});
                table2856.AddRow(new string[] {
                            "[[Var4]]",
                            "\"Test24\""});
                table2856.AddRow(new string[] {
                            "[[bank(1).name]]",
                            "\"FNB\""});
                table2856.AddRow(new string[] {
                            "[[bank(1).id]]",
                            "\"100\""});
                table2856.AddRow(new string[] {
                            "[[bank(2).name]]",
                            "\"discovery\""});
                table2856.AddRow(new string[] {
                            "[[bank(2).id]]",
                            "\"200\""});
#line 96
 testRunner.And("an assign \"dataToStore2\" into \"DsfMultiAssignActivity2\" with", ((string)(null)), table2856, "And ");
#line hidden
#line 106
 testRunner.Then("the assigned \"key1\", \"ttl1\" and innerActivity \"DsfMultiAssignActivity1\" is execut" +
                        "ed by \"RedisActivity1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
                TechTalk.SpecFlow.Table table2857 = new TechTalk.SpecFlow.Table(new string[] {
                            "name",
                            "value"});
                table2857.AddRow(new string[] {
                            "[[Var1]]",
                            "\"Test1\""});
                table2857.AddRow(new string[] {
                            "[[Var2]]",
                            "\"Test2\""});
#line 107
 testRunner.And("the Redis Cache under \"key1\" with GUID will contain", ((string)(null)), table2857, "And ");
#line hidden
#line 111
 testRunner.Then("the assigned \"key2\", \"ttl2\" and innerActivity \"DsfMultiAssignActivity2\" is execut" +
                        "ed by \"RedisActivity2\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
                TechTalk.SpecFlow.Table table2858 = new TechTalk.SpecFlow.Table(new string[] {
                            "label",
                            "variable",
                            "operator",
                            "value"});
                table2858.AddRow(new string[] {
                            "Redis key { MyData } found",
                            "null",
                            "",
                            ""});
                table2858.AddRow(new string[] {
                            "null",
                            "[[Var1]]",
                            "=",
                            "\"Test21\""});
                table2858.AddRow(new string[] {
                            "null",
                            "[[Var2]]",
                            "=",
                            "\"Test22\""});
                table2858.AddRow(new string[] {
                            "null",
                            "[[Var3]]",
                            "=",
                            "\"Test23\""});
                table2858.AddRow(new string[] {
                            "null",
                            "[[Var4]]",
                            "=",
                            "\"Test24\""});
                table2858.AddRow(new string[] {
                            "null",
                            "[[bank(1).name]]",
                            "=",
                            "\"FNB\""});
                table2858.AddRow(new string[] {
                            "null",
                            "[[bank(1).id]]",
                            "=",
                            "\"100\""});
                table2858.AddRow(new string[] {
                            "null",
                            "[[bank(2).name]]",
                            "=",
                            "\"discovery\""});
                table2858.AddRow(new string[] {
                            "null",
                            "[[bank(2).id]]",
                            "=",
                            "\"200\""});
#line 112
 testRunner.Then("\"RedisActivity2\" output variables have the following values", ((string)(null)), table2858, "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
