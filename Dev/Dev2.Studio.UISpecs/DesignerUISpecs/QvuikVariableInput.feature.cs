﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.9.0.77
//      SpecFlow Generator Version:1.9.0.0
//      Runtime Version:4.0.30319.18063
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace Dev2.Studio.UI.Specs.DesignerUISpecs
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.9.0.77")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UITesting.CodedUITestAttribute()]
    public partial class QvuikVariableInputFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "QvuikVariableInput.feature"
#line hidden
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static void FeatureSetup(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "QvuikVariableInput", "In order to avoid silly mistakes\r\nAs a math idiot\r\nI want to be told the sum of t" +
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
                        && (TechTalk.SpecFlow.FeatureContext.Current.FeatureInfo.Title != "QvuikVariableInput")))
            {
                Dev2.Studio.UI.Specs.DesignerUISpecs.QvuikVariableInputFeature.FeatureSetup(null);
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
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("QVIAddToMoreThanOneAndReplace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "QvuikVariableInput")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("QVI")]
        public virtual void QVIAddToMoreThanOneAndReplace()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("QVIAddToMoreThanOneAndReplace", new string[] {
                        "QVI"});
#line 7
this.ScenarioSetup(scenarioInfo);
#line 8
 testRunner.Given("I have Warewolf running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 9
 testRunner.And("all tabs are closed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 10
 testRunner.And("I click \"RIBBONNEWENDPOINT\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 11
 testRunner.And("I double click \"TOOLBOX,PART_SearchBox\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 12
 testRunner.And("I send \"{DELETE}Merge\" to \"\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 13
 testRunner.And("I drag \"TOOLDATAMERGE\" onto \"WORKSURFACE,StartSymbol\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 15
 testRunner.And("I click point \"248,10\" on \"WORKSURFACE,Data Merge (1)(DataMergeDesigner)\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 16
 testRunner.And("I send \"aa,bb,cc,dd,ee,ff,gg,hh,ii,jj\" to \"WORKSURFACE,Data Merge (1)(DataMergeDe" +
                    "signer),QuickVariableInputContent,QviVariableListBox\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 17
 testRunner.And("I send \",\" to \"WORKSURFACE,Data Merge (1)(DataMergeDesigner),QuickVariableInputCo" +
                    "ntent,QviSplitOnCharacter\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 18
 testRunner.When("I send \"{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{ENTER}\" to \"\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 19
 testRunner.Then("\"WORKSURFACE,Data Merge (10)(DataMergeDesigner)\" is visible within \"1\" seconds", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 21
 testRunner.And("I drag \"TOOLDATAMERGE\" onto \"WORKSURFACE,Data Merge (10)(DataMergeDesigner)\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 22
 testRunner.And("I click point \"248,10\" on \"WORKSURFACE,Data Merge (1)(DataMergeDesigner)\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 23
 testRunner.And("I send \"aa,bb,cc,dd,ee,ff,gg,hh,ii\" to \"WORKSURFACE,Data Merge (1)(DataMergeDesig" +
                    "ner),QuickVariableInputContent,QviVariableListBox\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 24
 testRunner.And("I send \",\" to \"WORKSURFACE,Data Merge (1)(DataMergeDesigner),QuickVariableInputCo" +
                    "ntent,QviSplitOnCharacter\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 25
 testRunner.When("I send \"{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{ENTER}\" to \"\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 26
 testRunner.Then("\"WORKSURFACE,Data Merge (9)(DataMergeDesigner)\" is visible within \"1\" seconds", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 28
 testRunner.And("I click point \"248,10\" on \"WORKSURFACE,Data Merge (10)(DataMergeDesigner)\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 29
 testRunner.And("I send \"XX,YY,ZZ\" to \"WORKSURFACE,Data Merge (10)(DataMergeDesigner),QuickVariabl" +
                    "eInputContent,QviVariableListBox\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 30
 testRunner.And("I send \",\" to \"WORKSURFACE,Data Merge (10)(DataMergeDesigner),QuickVariableInputC" +
                    "ontent,QviSplitOnCharacter\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 31
 testRunner.And("I click \"WORKSURFACE,Data Merge (10)(DataMergeDesigner),QuickVariableInputContent" +
                    ",ReplaceOption\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 32
 testRunner.When("I send \"{TAB}{TAB}{ENTER}\" to \"\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 33
 testRunner.Then("\"WORKSURFACE,Data Merge (3)(DataMergeDesigner)\" is visible within \"1\" seconds", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
