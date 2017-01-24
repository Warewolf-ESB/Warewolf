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
namespace Warewolf.UISpecs
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.1.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute()]
    public partial class WebConnectorFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "WebConnector.feature"
#line hidden
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static void FeatureSetup(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner(null, 0);
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "WebConnector", "\tIn order to connecto to web services\r\n\tAs a Warewolf Studio user\r\n\tI want to per" +
                    "form a composition of recorded actions", ProgrammingLanguage.CSharp, new string[] {
                        "WebConnector"});
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
                        && (testRunner.FeatureContext.FeatureInfo.Title != "WebConnector")))
            {
                Warewolf.UISpecs.WebConnectorFeature.FeatureSetup(null);
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
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Create and Execute New Web GET Connector")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "WebConnector")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WebConnector")]
        public virtual void CreateAndExecuteNewWebGETConnector()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Create and Execute New Web GET Connector", ((string[])(null)));
#line 7
this.ScenarioSetup(scenarioInfo);
#line 8
 testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 9
 testRunner.When("I Click New Workflow Ribbon Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 10
 testRunner.And("I Click New Web Source Explorer Context Menu Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 11
 testRunner.And("I Type The Testing Site into Web GET Source Wizard Address Textbox", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 12
 testRunner.And("I Click New Web Source Test Connection Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 13
 testRunner.And("I Save Valid Service With Ribbon Button And Dialog As \"WebGETUITestingSource\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 14
 testRunner.And("I Click Close Web Source Wizard Tab Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 15
 testRunner.And("I Drag GETWebTool Onto DesignSurface", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 16
 testRunner.And("I Select Test Source From GET Web Large View Source Combobox", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 17
 testRunner.And("I Click GET Web Large View Generate Outputs", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 18
 testRunner.And("I Click GET Web Large View Test Inputs Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 19
 testRunner.And("I Click GET Web Tool Outputs Done Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 20
 testRunner.And("I Click GET Web Large View Done Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 21
 testRunner.And("I Click Debug Ribbon Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 22
 testRunner.And("I Click DebugInput Debug Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Create and Execute New Web POST Connector")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "WebConnector")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WebConnector")]
        public virtual void CreateAndExecuteNewWebPOSTConnector()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Create and Execute New Web POST Connector", ((string[])(null)));
#line 24
this.ScenarioSetup(scenarioInfo);
#line 25
 testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 26
 testRunner.When("I Click New Workflow Ribbon Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 27
 testRunner.When("I Drag POSTWebTool Onto DesignSurface", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 28
 testRunner.And("I Select Test Source From POST Web Large View Source Combobox", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 29
 testRunner.And("I Click POST Web Large View Generate Outputs", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 30
 testRunner.And("I Click POST Web Large View Test Inputs Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 31
 testRunner.And("I Click POST Web Tool Outputs Done Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 32
 testRunner.And("I Click POST Web Large View Done Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Create and Execute New Web PUT Connector")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "WebConnector")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WebConnector")]
        public virtual void CreateAndExecuteNewWebPUTConnector()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Create and Execute New Web PUT Connector", ((string[])(null)));
#line 34
this.ScenarioSetup(scenarioInfo);
#line 35
 testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 36
 testRunner.When("I Click New Workflow Ribbon Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 37
 testRunner.When("I Drag PUTWebTool Onto DesignSurface", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 38
 testRunner.And("I Select Test Source From PUT Web Large View Source Combobox", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 39
 testRunner.And("I Click PUT Web Large View Generate Outputs", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 40
 testRunner.And("I Click PUT Web Large View Test Inputs Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 41
 testRunner.And("I Click PUT Web Tool Outputs Done Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 42
 testRunner.And("I Click PUT Web Large View Done Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Create and Execute New Web DELETE Connector")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "WebConnector")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WebConnector")]
        public virtual void CreateAndExecuteNewWebDELETEConnector()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Create and Execute New Web DELETE Connector", ((string[])(null)));
#line 44
this.ScenarioSetup(scenarioInfo);
#line 45
 testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 46
 testRunner.When("I Click New Workflow Ribbon Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 47
 testRunner.When("I Drag DELETEWebTool Onto DesignSurface", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 48
 testRunner.And("I Select Test Source From DELETE Web Large View Source Combobox", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 49
 testRunner.And("I Click DELETE Web Large View Generate Outputs", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 50
 testRunner.And("I Click DELETE Web Large View Test Inputs Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 51
 testRunner.And("I Click DELETE Web Tool Outputs Done Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 52
 testRunner.And("I Click DELETE Web Large View Done Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Right click adorner control with error")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "WebConnector")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WebConnector")]
        public virtual void RightClickAdornerControlWithError()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Right click adorner control with error", ((string[])(null)));
#line 54
this.ScenarioSetup(scenarioInfo);
#line 55
 testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 56
 testRunner.When("I Filter the Explorer with \"BrokenDeleteWeb\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 57
 testRunner.And("I DoubleClick Explorer Localhost First Item", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 58
 testRunner.And("I Open DeleteWeb Tool Large View", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 59
 testRunner.And("I Select Test Source From DELETE Web Large View Source Combobox", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 60
 testRunner.And("I Click DELETE Web Large View Generate Outputs", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 61
 testRunner.And("I Click DELETE Web Large View Test Inputs Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 62
 testRunner.Then("There is an error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 63
 testRunner.And("I RightClick Ardoner Hyperlink", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 64
 testRunner.And("There is an error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
