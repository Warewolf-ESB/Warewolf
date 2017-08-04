﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.2.0.0
//      SpecFlow Generator Version:2.2.0.0
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
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.2.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UITesting.CodedUITestAttribute()]
    public partial class ExplorerFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "Explorer.feature"
#line hidden
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static void FeatureSetup(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            SetDefaultPlaybackSettings.TestContext = testContext;
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner(null, 0);
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Explorer", "\tIn order to manage services\r\n\tAs a Warewolf Studio user\r\n\tI want to perform a co" +
                    "mposition of recorded actions", ProgrammingLanguage.CSharp, new string[] {
                        "Explorer",
                        "MSTest:DeploymentItem:avformat-57.dll",
                        "MSTest:DeploymentItem:avutil-55.dll",
                        "MSTest:DeploymentItem:swresample-2.dll",
                        "MSTest:DeploymentItem:swscale-4.dll",
                        "MSTest:DeploymentItem:avcodec-57.dll"});
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
                        && (testRunner.FeatureContext.FeatureInfo.Title != "Explorer")))
            {
                global::Warewolf.UISpecs.ExplorerFeature.FeatureSetup(null);
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
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Drag on Remote Subworkflow from Explorer and Execute it")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Explorer")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Explorer")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avformat-57.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avutil-55.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swresample-2.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swscale-4.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avcodec-57.dll")]
        public virtual void DragOnRemoteSubworkflowFromExplorerAndExecuteIt()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Drag on Remote Subworkflow from Explorer and Execute it", ((string[])(null)));
#line 12
this.ScenarioSetup(scenarioInfo);
#line 13
 testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 14
 testRunner.When("I Create New Workflow using shortcut", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 15
 testRunner.And("I Connect To Remote Server", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 16
 testRunner.And("I Wait For Explorer First Remote Server Spinner", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 17
 testRunner.And("I Filter the Explorer with \"GenericResource\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 18
 testRunner.And("I Drag Explorer Remote GenericResource Onto Workflow Design Surface", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 19
 testRunner.And("I Save With Ribbon Button And Dialog As \"LocalGenericResourceWithRemoteSubworkflo" +
                    "w\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 20
 testRunner.And("I Click Debug Ribbon Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 21
 testRunner.And("I Click DebugInput Debug Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 22
 testRunner.And("I Click Debug Output GenericResource Name", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Opening and Editing Workflow from Explorer Remote")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Explorer")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Explorer")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avformat-57.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avutil-55.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swresample-2.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swscale-4.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avcodec-57.dll")]
        public virtual void OpeningAndEditingWorkflowFromExplorerRemote()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Opening and Editing Workflow from Explorer Remote", ((string[])(null)));
#line 24
this.ScenarioSetup(scenarioInfo);
#line 25
 testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 26
 testRunner.When("I Connect To Remote Server", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 27
 testRunner.And("I Wait For Explorer First Remote Server Spinner", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 28
 testRunner.And("I Filter the Explorer with \"Hello World\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 29
 testRunner.When("I open \"Hello World\" in Remote Connection Integration", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Opening Workflow local and remote using right click")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Explorer")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Explorer")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avformat-57.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avutil-55.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swresample-2.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swscale-4.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avcodec-57.dll")]
        public virtual void OpeningWorkflowLocalAndRemoteUsingRightClick()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Opening Workflow local and remote using right click", ((string[])(null)));
#line 31
this.ScenarioSetup(scenarioInfo);
#line 32
   testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 33
   testRunner.When("I Connect To Remote Server", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 34
   testRunner.And("I Wait For Explorer First Remote Server Spinner", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 35
   testRunner.And("I Filter the Explorer with \"Hello World\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 36
   testRunner.And("I RightClick Explorer First Remote Server First Item", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 37
   testRunner.And("I Select Open From Explorer Context Menu", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 38
   testRunner.Then("Remote \"Hello World - Remote Connection Integration\" is open", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 39
   testRunner.Then("I RightClick Explorer Localhost First Item", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 40
   testRunner.And("I Select Open From Explorer Context Menu", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 41
   testRunner.Then("Local \"Hello World\" is open", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Deleting a Resource localhost")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Explorer")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Explorer")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avformat-57.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avutil-55.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swresample-2.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swscale-4.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avcodec-57.dll")]
        public virtual void DeletingAResourceLocalhost()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Deleting a Resource localhost", ((string[])(null)));
#line 43
 this.ScenarioSetup(scenarioInfo);
#line 44
   testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 45
   testRunner.And("I Filter the Explorer with \"LocalWorkflowWithRemoteSubworkflowToDelete\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 46
   testRunner.And("I RightClick Explorer Localhost First Item", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 47
   testRunner.And("I Select Delete From Explorer Context Menu", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 48
   testRunner.And("I Click MessageBox Yes", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Deleting a Folder in localhost")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Explorer")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Explorer")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avformat-57.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avutil-55.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swresample-2.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swscale-4.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avcodec-57.dll")]
        public virtual void DeletingAFolderInLocalhost()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Deleting a Folder in localhost", ((string[])(null)));
#line 50
 this.ScenarioSetup(scenarioInfo);
#line 51
   testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 52
   testRunner.When("I Filter the Explorer with \"FolderToDelete\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 53
   testRunner.And("I RightClick Explorer Localhost First Item", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 54
   testRunner.And("I Select Delete From Explorer Context Menu", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 55
   testRunner.And("I Click MessageBox Yes", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Filter Should Clear On Connection Of Remote Server")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Explorer")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Explorer")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avformat-57.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avutil-55.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swresample-2.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swscale-4.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avcodec-57.dll")]
        public virtual void FilterShouldClearOnConnectionOfRemoteServer()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Filter Should Clear On Connection Of Remote Server", ((string[])(null)));
#line 57
 this.ScenarioSetup(scenarioInfo);
#line 58
   testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 59
   testRunner.When("I Filter the Explorer with \"Hello World\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 60
   testRunner.When("I Connect To Remote Server", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 61
   testRunner.Then("Filter Textbox is cleared", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Deleting a Resource Remote")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Explorer")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Explorer")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avformat-57.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avutil-55.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swresample-2.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swscale-4.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avcodec-57.dll")]
        public virtual void DeletingAResourceRemote()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Deleting a Resource Remote", ((string[])(null)));
#line 63
 this.ScenarioSetup(scenarioInfo);
#line 64
   testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 65
   testRunner.When("I Connect To Remote Server", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 66
   testRunner.And("I Wait For Explorer First Remote Server Spinner", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 67
   testRunner.And("I Click New Workflow Ribbon Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 68
   testRunner.And("I validate and delete the existing resource with \"LocalWorkflowWithRemoteSubworkf" +
                    "lowToDelete\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 69
   testRunner.And("I Filter the Explorer with \"GenericResource\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 70
   testRunner.And("I Drag Explorer Remote GenericResource Onto Workflow Design Surface", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 71
   testRunner.And("I Save With Ribbon Button And Dialog As \"LocalWorkflowWithRemoteSubworkflowToDele" +
                    "te\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 72
   testRunner.And("I Filter the Explorer with \"LocalWorkflowWithRemoteSubworkflowToDelete\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 73
   testRunner.And("I RightClick Explorer First Remote Server First Item", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 74
   testRunner.And("I Select Delete From Explorer Context Menu", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 75
   testRunner.And("I Click MessageBox Yes", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Clear filter")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Explorer")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Explorer")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avformat-57.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avutil-55.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swresample-2.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swscale-4.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avcodec-57.dll")]
        public virtual void ClearFilter()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Clear filter", ((string[])(null)));
#line 77
this.ScenarioSetup(scenarioInfo);
#line 78
   testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 79
   testRunner.When("I Filter the Explorer with \"Hello World\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 80
   testRunner.Then("Filter Textbox has \"Hello World\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 81
   testRunner.And("I Click Explorer Filter Clear Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 82
   testRunner.Then("Filter Textbox is cleared", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Drag on service from Explorer and change input and output")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Explorer")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Explorer")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avformat-57.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avutil-55.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swresample-2.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swscale-4.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avcodec-57.dll")]
        public virtual void DragOnServiceFromExplorerAndChangeInputAndOutput()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Drag on service from Explorer and change input and output", ((string[])(null)));
#line 84
this.ScenarioSetup(scenarioInfo);
#line 85
 testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 86
 testRunner.When("I Create New Workflow using shortcut", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 87
 testRunner.And("I Filter the Explorer with \"Hello World\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 88
 testRunner.And("I Drag Explorer workflow Onto Workflow Design Surface", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 89
 testRunner.And("I change Hello World input variable", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 90
 testRunner.And("I change Hello World output variable", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
