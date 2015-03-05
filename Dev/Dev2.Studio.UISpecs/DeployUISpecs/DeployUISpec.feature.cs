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
namespace Dev2.Studio.UI.Specs.DeployUISpecs
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.9.0.77")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UITesting.CodedUITestAttribute()]
    public partial class DeployFeatureFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "DeployUISpec.feature"
#line hidden
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static void FeatureSetup(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "DeployFeature", "In order to avoid silly mistakes\r\nAs a math idiot\r\nI want to be told the sum of t" +
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
                        && (TechTalk.SpecFlow.FeatureContext.Current.FeatureInfo.Title != "DeployFeature")))
            {
                Dev2.Studio.UI.Specs.DeployUISpecs.DeployFeatureFeature.FeatureSetup(null);
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
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("IsDeployButtonEnabledWithNothingToDeploy_Expected_DeployButtonIsDisabled12")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "DeployFeature")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("DeployTab")]
        public virtual void IsDeployButtonEnabledWithNothingToDeploy_Expected_DeployButtonIsDisabled12()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IsDeployButtonEnabledWithNothingToDeploy_Expected_DeployButtonIsDisabled12", new string[] {
                        "DeployTab"});
#line 7
this.ScenarioSetup(scenarioInfo);
#line 8
    testRunner.Given("I have Warewolf running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 9
    testRunner.And("all tabs are closed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 10
    testRunner.And("I click \"EXPLORERFILTERCLEARBUTTON\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 11
    testRunner.And("I click \"RIBBONDEPLOY\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 13
    testRunner.And("\"DEPLOYERROR\" is visible", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 15
    testRunner.And("\"DEPLOYBUTTON\" is disabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 17
    testRunner.And("I type \"Decision Testing\" in \"DEPLOYSOURCEFILTER\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 18
    testRunner.And("I click \"DEPLOYSOURCE,UI_SourceServer_UI_Acceptance Testing Resources_AutoID_Auto" +
                    "ID,UI_SourceServer_UI_Decision Testing_AutoID_AutoID,UI_CheckBoxDecision Testing" +
                    "_AutoID\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 19
    testRunner.And("\"DEPLOYBUTTON\" is disabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 21
    testRunner.And("I click \"DEPLOYSOURCE,UI_SourceServer_UI_Acceptance Testing Resources_AutoID_Auto" +
                    "ID,Expander\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 23
    testRunner.And("I click \"DEPLOYSOURCE,UI_SourceServer_UI_Acceptance Testing Resources_AutoID_Auto" +
                    "ID,UI_SourceServer_UI_Decision Testing_AutoID_AutoID,UI_CheckBoxDecision Testing" +
                    "_AutoID\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Address",
                        "AuthType",
                        "UserName",
                        "Password"});
            table1.AddRow(new string[] {
                        "http://SANDBOX-DEV2:3142/dsf",
                        "User",
                        "IntegrationTester",
                        "I73573r0"});
#line 24
    testRunner.Given("I create a new remote connection as \"Deployrem\" in Deploy Destination", ((string)(null)), table1, "Given ");
#line 28
    testRunner.And("I click \"DEPLOYSOURCE,UI_SourceServer_UI_Acceptance Testing Resources_AutoID_Auto" +
                    "ID,UI_SourceServer_UI_Decision Testing_AutoID_AutoID,UI_CheckBoxDecision Testing" +
                    "_AutoID\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 29
       testRunner.And("\"DEPLOYBUTTON\" is enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 32
    testRunner.Given("I click \"EXPLORER,UI_localhost_AutoID\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 33
    testRunner.Given("I click \"EXPLORER,UI_Deployrem (http://sandbox-dev2:3142/)_AutoID\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 34
    testRunner.Given("I click \"EXPLORERCONNECTBUTTON\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 35
    testRunner.Given("I click \"EXPLORER,UI_SourceServerRefreshbtn_AutoID\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 36
       testRunner.Given("I send \"Deployrem\" to \"EXPLORERFILTER\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 37
    testRunner.And("I right click \"EXPLORERFOLDERS,UI_Deployrem_AutoID\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 38
    testRunner.And("I send \"{TAB}{TAB}{TAB}{TAB}{ENTER}\" to \"\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 39
    testRunner.And("I click \"UI_MessageBox_AutoID,UI_YesButton_AutoID\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
