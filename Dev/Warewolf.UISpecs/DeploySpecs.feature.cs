// ------------------------------------------------------------------------------
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
    public partial class DeploySpecsFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "DeploySpecs.feature"
#line hidden
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static void FeatureSetup(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner(null, 0);
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "DeploySpecs", null, ProgrammingLanguage.CSharp, new string[] {
                        "Deploy"});
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
                        && (testRunner.FeatureContext.FeatureInfo.Title != "DeploySpecs")))
            {
                Warewolf.UISpecs.DeploySpecsFeature.FeatureSetup(null);
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
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Deploying From Explorer Opens The Deploy With Resource Already Checked")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "DeploySpecs")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Deploy")]
        public virtual void DeployingFromExplorerOpensTheDeployWithResourceAlreadyChecked()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Deploying From Explorer Opens The Deploy With Resource Already Checked", ((string[])(null)));
#line 4
this.ScenarioSetup(scenarioInfo);
#line 5
    testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 6
 testRunner.And("I Filter the Explorer with \"Hello World\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 7
 testRunner.And("I RightClick Explorer Localhost First Item", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 8
 testRunner.And("I Select Deploy From Explorer Context Menu", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 9
 testRunner.And("I Select \"Hello World\" from the source tab", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 10
 testRunner.Then("Filtered Resourse Is Checked For Deploy", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Deploy ViewOnlyWorkflow to remoteConnection")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "DeploySpecs")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Deploy")]
        public virtual void DeployViewOnlyWorkflowToRemoteConnection()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Deploy ViewOnlyWorkflow to remoteConnection", ((string[])(null)));
#line 12
this.ScenarioSetup(scenarioInfo);
#line 13
 testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 14
 testRunner.When("I Set Resource Permissions For \"DeployViewOnly\" to Group \"Public\" and Permissions" +
                    " for View to \"true\" and Contribute to \"false\" and Execute to \"false\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 15
 testRunner.And("I Click Deploy Ribbon Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 16
 testRunner.And("I Select RemoteConnectionIntegration From Deploy Tab Destination Server Combobox", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 17
 testRunner.And("I Select \"DeployViewOnly\" from the source tab", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 18
 testRunner.Then("Filtered Resourse Is Checked For Deploy", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 19
 testRunner.And("I Click Deploy button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 20
 testRunner.And("I Click Deploy version conflicts MessageBox OK", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 21
 testRunner.And("I Click Deploy conflicts MessageBox OK", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 22
 testRunner.And("I Click Deploy Successful MessageBox OK", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Deploy button is enabling when selecting resource in source side")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "DeploySpecs")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Deploy")]
        public virtual void DeployButtonIsEnablingWhenSelectingResourceInSourceSide()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Deploy button is enabling when selecting resource in source side", ((string[])(null)));
#line 24
this.ScenarioSetup(scenarioInfo);
#line 25
  testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 26
  testRunner.When("I Click Deploy Ribbon Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 27
  testRunner.And("I Select RemoteConnectionIntegration From Deploy Tab Destination Server Combobox", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 28
  testRunner.Then("Deploy Button is enabled  \"false\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 29
  testRunner.And("I Select \"Hello world\" from the source tab", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 30
  testRunner.Then("Deploy Button is enabled  \"true\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Filtering and clearing filter on source side")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "DeploySpecs")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Deploy")]
        public virtual void FilteringAndClearingFilterOnSourceSide()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Filtering and clearing filter on source side", ((string[])(null)));
#line 32
this.ScenarioSetup(scenarioInfo);
#line 33
 testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 34
 testRunner.When("I Click Deploy Ribbon Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 35
 testRunner.And("I Select RemoteConnectionIntegration From Deploy Tab Destination Server Combobox", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 36
 testRunner.And("I filter for \"Hello World\" on the source filter", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 37
 testRunner.And("I filter for \"\" on the source filter", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 38
 testRunner.And("Deploy Button is enabled  \"false\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Deploying with filter enabled")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "DeploySpecs")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Deploy")]
        public virtual void DeployingWithFilterEnabled()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Deploying with filter enabled", ((string[])(null)));
#line 40
this.ScenarioSetup(scenarioInfo);
#line 41
     testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 42
  testRunner.When("I Click Deploy Ribbon Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 43
  testRunner.And("I Select RemoteConnectionIntegration From Deploy Tab Destination Server Combobox", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 44
  testRunner.And("I filter for \"Hello World\" on the source filter", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 45
  testRunner.And("Resources is visible on the tree", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 46
  testRunner.And("I Select \"Hello World\" from the source tab", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 47
  testRunner.And("I Click Deploy button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 48
  testRunner.And("I Click Deploy version conflicts MessageBox OK", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 49
  testRunner.And("I Click Deploy conflicts MessageBox OK", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 50
  testRunner.And("I Click Deploy Successful MessageBox OK", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Deploy is enabled when I change server after validation thrown")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "DeploySpecs")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Deploy")]
        public virtual void DeployIsEnabledWhenIChangeServerAfterValidationThrown()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Deploy is enabled when I change server after validation thrown", ((string[])(null)));
#line 52
this.ScenarioSetup(scenarioInfo);
#line 53
  testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 54
  testRunner.When("I Click Deploy Ribbon Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 55
  testRunner.And("I Select LocalhostConnected From Deploy Tab Destination Server Combobox", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 56
  testRunner.And("I filter for \"Hello world\" on the source filter", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 57
  testRunner.And("Deploy Button is enabled  \"false\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 58
  testRunner.And("The deploy validation message is \"Source and Destination cannot be the same.\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 59
  testRunner.And("I Select RemoteConnectionIntegration From Deploy Tab Destination Server Combobox", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 60
  testRunner.And("I Select \"Hello world\" from the source tab", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 61
  testRunner.And("Deploy Button is enabled  \"true\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Refresh does not uncheck all resources")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "DeploySpecs")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Deploy")]
        public virtual void RefreshDoesNotUncheckAllResources()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Refresh does not uncheck all resources", ((string[])(null)));
#line 63
this.ScenarioSetup(scenarioInfo);
#line 64
  testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 65
  testRunner.When("I Click Deploy Ribbon Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 66
  testRunner.And("I Select LocalhostConnected From Deploy Tab Destination Server Combobox", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 67
  testRunner.And("I Select RemoteConnectionIntegration From Deploy Tab Destination Server Combobox", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 68
  testRunner.And("I Select localhost checkbox from the source tab", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 69
  testRunner.Then("Source explorer first item is checked", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 70
  testRunner.When("I Click Deploy Tab Source Refresh Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 71
  testRunner.Then("Source explorer first item is checked", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Deploying From Explorer Opens The Deploy With All Resources in Folder Already Che" +
            "cked")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "DeploySpecs")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Deploy")]
        public virtual void DeployingFromExplorerOpensTheDeployWithAllResourcesInFolderAlreadyChecked()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Deploying From Explorer Opens The Deploy With All Resources in Folder Already Che" +
                    "cked", ((string[])(null)));
#line 73
this.ScenarioSetup(scenarioInfo);
#line 74
 testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 75
 testRunner.When("I Filter the Explorer with \"Unit Tests\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 76
 testRunner.And("I RightClick Explorer Localhost First Item", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 77
 testRunner.And("I Select Deploy From Explorer Context Menu", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 78
 testRunner.And("I Enter \"Unit Tests\" Into Deploy Source Filter", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 79
 testRunner.Then("Filtered Resourse Is Checked For Deploy", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 80
 testRunner.And("I Click Close Deploy Tab Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Cancel Deploy Returns to Deploy Tab")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "DeploySpecs")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Deploy")]
        public virtual void CancelDeployReturnsToDeployTab()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Cancel Deploy Returns to Deploy Tab", ((string[])(null)));
#line 82
this.ScenarioSetup(scenarioInfo);
#line 83
 testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 84
 testRunner.When("I Filter the Explorer with \"Unit Tests\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 85
 testRunner.And("I RightClick Explorer Localhost First Item", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 86
 testRunner.And("I Select Deploy From Explorer Context Menu", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 87
 testRunner.And("I Click Deploy Tab Destination Server Combobox", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 88
 testRunner.And("I Click Deploy Tab Destination Server Remote Connection Intergration Item", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 89
 testRunner.Then("Deploy Button Is Enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 90
 testRunner.When("I Click Deploy Tab Deploy Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 91
 testRunner.Then("Deploy Version Conflict Window Shows", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 92
 testRunner.And("I Click MessageBox Cancel", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 93
 testRunner.And("Deploy Window Is Still Open", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Deploy Conflicting Resource With Resource In A Different Path")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "DeploySpecs")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Deploy")]
        public virtual void DeployConflictingResourceWithResourceInADifferentPath()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Deploy Conflicting Resource With Resource In A Different Path", ((string[])(null)));
#line 95
this.ScenarioSetup(scenarioInfo);
#line 96
 testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 97
 testRunner.When("I Click Deploy Ribbon Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 98
 testRunner.And("I Select RemoteConnectionIntegration From Deploy Tab Destination Server Combobox", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 99
 testRunner.And("I Enter \"ResourceToDeployInADifferentPath\" Into Deploy Source Filter", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 100
 testRunner.And("I Select Deploy First Source Item", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 101
 testRunner.And("I Click Deploy Tab Deploy Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 102
 testRunner.And("I Click Deploy version conflicts MessageBox OK", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 103
 testRunner.And("I Click Deploy conflicts MessageBox OK", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 104
 testRunner.And("I Click Deploy Successful MessageBox OK", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 105
 testRunner.And("I Select Remote Connection Integration (Connected) From Explorer", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 106
 testRunner.And("I Filter the Explorer with \"ResourceToDeployInADifferentPath\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 107
 testRunner.Then("First remote Item should be \"ResourceToDeployInADifferentPath\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Changing Authentication Type of Resource And Save Keeps the Changes")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "DeploySpecs")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Deploy")]
        public virtual void ChangingAuthenticationTypeOfResourceAndSaveKeepsTheChanges()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Changing Authentication Type of Resource And Save Keeps the Changes", ((string[])(null)));
#line 109
this.ScenarioSetup(scenarioInfo);
#line 110
 testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 111
 testRunner.When("I Click Deploy Ribbon Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 112
 testRunner.And("I Select RemoteConnectionIntegration From Deploy Tab Source Server Combobox", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 113
 testRunner.And("I Click Deploy Tab Source Server Edit Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 114
 testRunner.And("I change Server Authentication type and validate", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Changing Server AuthenticationType from Deploy And Save Edit Server From Explorer" +
            " Has Changes")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "DeploySpecs")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Deploy")]
        public virtual void ChangingServerAuthenticationTypeFromDeployAndSaveEditServerFromExplorerHasChanges()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Changing Server AuthenticationType from Deploy And Save Edit Server From Explorer" +
                    " Has Changes", ((string[])(null)));
#line 116
this.ScenarioSetup(scenarioInfo);
#line 117
 testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 118
 testRunner.When("I Click Deploy Ribbon Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 119
 testRunner.And("I Select RemoteConnectionIntegration From Deploy Tab Source Server Combobox", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 120
 testRunner.And("I Click Deploy Tab Source Server Edit Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 121
 testRunner.And("I change Server Authentication From Deploy And Validate Changes From Explorer", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Changing Seleced Server On desploy Source While Connected To Remote Server On the" +
            " Explorer")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "DeploySpecs")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Deploy")]
        public virtual void ChangingSelecedServerOnDesploySourceWhileConnectedToRemoteServerOnTheExplorer()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Changing Seleced Server On desploy Source While Connected To Remote Server On the" +
                    " Explorer", ((string[])(null)));
#line 123
this.ScenarioSetup(scenarioInfo);
#line 124
 testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 125
 testRunner.When("I Click Deploy Ribbon Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 126
 testRunner.And("I Select RemoteConnectionIntegration From Deploy Tab Source Server Combobox", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 127
 testRunner.And("I Select Remote Connection Integration From Explorer", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 128
 testRunner.And("I Select localhost From Deploy Tab Source Server Combobox", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 129
 testRunner.And("I validate the Resource tree is loaded", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Deploy From RemoteConnection")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "DeploySpecs")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Deploy")]
        public virtual void DeployFromRemoteConnection()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Deploy From RemoteConnection", ((string[])(null)));
#line 131
this.ScenarioSetup(scenarioInfo);
#line 132
 testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 133
 testRunner.When("I Click Deploy Ribbon Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 134
    testRunner.And("I Select RemoteConnectionIntegration From Deploy Tab Source Server Combobox", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 135
    testRunner.And("Resources is visible on the tree", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 136
 testRunner.And("I Select \"Hello World\" from the source tab", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 137
 testRunner.And("I Click Deploy button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 138
    testRunner.And("I Click Deploy version conflicts MessageBox OK", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 139
 testRunner.And("I Click Deploy conflicts MessageBox OK", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 140
 testRunner.And("I Click Deploy Successful MessageBox OK", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
