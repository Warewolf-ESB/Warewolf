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
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute()]
    public partial class SaveDialogFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "SaveDialog.feature"
#line hidden
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static void FeatureSetup(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner(null, 0);
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "SaveDialog", "\tIn order to save services\r\n\tAs a warewolf studio user\r\n\tI want to give the workf" +
                    "low a name and location", ProgrammingLanguage.CSharp, new string[] {
                        "SaveDialog",
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
                        && (testRunner.FeatureContext.FeatureInfo.Title != "SaveDialog")))
            {
                global::Warewolf.UISpecs.SaveDialogFeature.FeatureSetup(null);
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
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Create WorkFlow In Folder Opens Save Dialog With Folder Already Selected")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "SaveDialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("SaveDialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avformat-57.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avutil-55.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swresample-2.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swscale-4.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avcodec-57.dll")]
        public virtual void CreateWorkFlowInFolderOpensSaveDialogWithFolderAlreadySelected()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Create WorkFlow In Folder Opens Save Dialog With Folder Already Selected", ((string[])(null)));
#line 12
this.ScenarioSetup(scenarioInfo);
#line 13
 testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 14
 testRunner.And("I Create New Workflow using shortcut", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 15
 testRunner.And("I RightClick Explorer Localhost First Item", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 16
 testRunner.Then("I Select NewWorkflow From Explorer Context Menu", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 17
 testRunner.And("I Make Workflow Savable", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 18
 testRunner.And("I Click Save Ribbon Button to Open Save Dialog", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 19
 testRunner.Then("I Enter Service Name Into Save Dialog As \"TestService\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 20
 testRunner.And("I Click SaveDialog Save Button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 21
 testRunner.And("\"TestService\" Resource Exists In Windows Directory \"C:\\ProgramData\\Warewolf\\Resou" +
                    "rces\\Unit Tests\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Filter Save Dialog Close And ReOpen Clears The Filter")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "SaveDialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("SaveDialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avformat-57.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avutil-55.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swresample-2.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swscale-4.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avcodec-57.dll")]
        public virtual void FilterSaveDialogCloseAndReOpenClearsTheFilter()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Filter Save Dialog Close And ReOpen Clears The Filter", ((string[])(null)));
#line 23
this.ScenarioSetup(scenarioInfo);
#line 24
 testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 25
 testRunner.And("I Create New Workflow using shortcut", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 26
 testRunner.And("I Make Workflow Savable And Then Save", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 27
 testRunner.And("I Filter Save Dialog Explorer with \"Hello World\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 28
 testRunner.And("Filtered Item Exists", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 29
 testRunner.And("I Click SaveDialog CancelButton", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 30
 testRunner.Then("Explorer Items appear on the Explorer Tree", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 31
 testRunner.And("I Click Save Ribbon Button to Open Save Dialog", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 32
 testRunner.Then("Explorer Items appear on the Save Dialog Explorer Tree", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Create New Folder In Localhost Then Open Context Menu Server From Save Dialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "SaveDialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("SaveDialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avformat-57.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avutil-55.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swresample-2.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swscale-4.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avcodec-57.dll")]
        public virtual void CreateNewFolderInLocalhostThenOpenContextMenuServerFromSaveDialog()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Create New Folder In Localhost Then Open Context Menu Server From Save Dialog", ((string[])(null)));
#line 34
this.ScenarioSetup(scenarioInfo);
#line 35
 testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 36
 testRunner.When("I Create New Workflow using shortcut", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 37
 testRunner.And("I Make Workflow Savable And Then Save", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 38
 testRunner.And("I Filter Save Dialog Explorer with \"Created Another Folder\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 39
 testRunner.And("I RightClick Save Dialog Localhost", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 40
 testRunner.And("I Select New Folder From SaveDialog Context Menu", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 41
 testRunner.And("I Enter New Folder Name as \"Created Another Folder\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 42
 testRunner.And("I RightClick Save Dialog Localhost First Item", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 43
 testRunner.Then("Context Menu Has Two Items", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Create New Folder In Localhost From Save Dialog Then Delete In Main Explorer")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "SaveDialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("SaveDialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avformat-57.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avutil-55.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swresample-2.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swscale-4.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avcodec-57.dll")]
        public virtual void CreateNewFolderInLocalhostFromSaveDialogThenDeleteInMainExplorer()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Create New Folder In Localhost From Save Dialog Then Delete In Main Explorer", ((string[])(null)));
#line 45
this.ScenarioSetup(scenarioInfo);
#line 46
 testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 47
 testRunner.And("I Create New Workflow using shortcut", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 48
 testRunner.And("I Make Workflow Savable And Then Save", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 49
 testRunner.And("I Filter Save Dialog Explorer with \"I_Will_Delete_This_Folder\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 50
 testRunner.And("I RightClick Save Dialog Localhost", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 51
 testRunner.And("I Select New Folder From SaveDialog Context Menu", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 52
 testRunner.And("I Name New Folder as \"I_Will_Delete_This_Folder\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 53
 testRunner.And("I Click SaveDialog CancelButton", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 54
 testRunner.And("I Filter the Explorer with \"I_Will_Delete_This_Folder\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 55
 testRunner.And("I RightClick Explorer Localhost First Item", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 56
 testRunner.And("I Select Delete From Explorer Context Menu", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 57
 testRunner.And("I Click MessageBox Yes", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 58
 testRunner.And("Folder Is Removed From Explorer", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Create New Folder In Localhost Server From Save Dialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "SaveDialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("SaveDialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avformat-57.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avutil-55.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swresample-2.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swscale-4.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avcodec-57.dll")]
        public virtual void CreateNewFolderInLocalhostServerFromSaveDialog()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Create New Folder In Localhost Server From Save Dialog", ((string[])(null)));
#line 60
this.ScenarioSetup(scenarioInfo);
#line 61
 testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 62
 testRunner.And("I Create New Workflow using shortcut", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 63
 testRunner.And("I Make Workflow Savable And Then Save", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 64
 testRunner.And("I Filter Save Dialog Explorer with \"New Created Folder\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 65
 testRunner.And("I RightClick Save Dialog Localhost", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 66
 testRunner.And("I Select New Folder From SaveDialog Context Menu", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 67
 testRunner.And("I Name New Folder as \"New Created Folder\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 68
 testRunner.And("I Click SaveDialog CancelButton", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 69
 testRunner.When("I Refresh Explorer", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 70
 testRunner.And("I Filter the Explorer with \"New Created Folder\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 71
 testRunner.Then("Explorer Contain Item \"New Created Folder\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Create New Folder In Existing Folder As A Sub Folder From Save Dialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "SaveDialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("SaveDialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avformat-57.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avutil-55.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swresample-2.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swscale-4.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avcodec-57.dll")]
        public virtual void CreateNewFolderInExistingFolderAsASubFolderFromSaveDialog()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Create New Folder In Existing Folder As A Sub Folder From Save Dialog", ((string[])(null)));
#line 73
this.ScenarioSetup(scenarioInfo);
#line 74
 testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 75
 testRunner.And("I Create New Workflow using shortcut", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 76
 testRunner.And("I Make Workflow Savable And Then Save", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 77
 testRunner.And("I Filter Save Dialog Explorer with \"Acceptance Tests\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 78
 testRunner.And("I Create New Folder Item using Shortcut", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 79
 testRunner.And("I Name New Sub Folder as \"New Created Sub Folder\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 80
 testRunner.And("I Click SaveDialog CancelButton", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 81
 testRunner.And("I Filter the Explorer with \"New Created Sub Folder\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 82
 testRunner.Then("Explorer Contain Sub Item \"New Created Sub Folder\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Rename Resource From Save Dialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "SaveDialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("SaveDialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avformat-57.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avutil-55.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swresample-2.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swscale-4.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avcodec-57.dll")]
        public virtual void RenameResourceFromSaveDialog()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Rename Resource From Save Dialog", ((string[])(null)));
#line 84
this.ScenarioSetup(scenarioInfo);
#line 85
 testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 86
 testRunner.And("I Create New Workflow using shortcut", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 87
 testRunner.And("I Make Workflow Savable And Then Save", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 88
 testRunner.And("I Filter Save Dialog Explorer with \"ResourceToRename\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 89
 testRunner.And("I RightClick Save Dialog Localhost First Item", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 90
 testRunner.And("I Select Rename From SaveDialog Context Menu", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 91
 testRunner.And("I Rename Save Dialog Explorer First Item To \"ResourceRenamed\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 92
 testRunner.And("I Click SaveDialog CancelButton", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 93
 testRunner.And("I Filter the Explorer with \"ResourceRenamed\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 94
 testRunner.Then("Explorer Contain Item \"ResourceRenamed\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Delete Resource From Save Dialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "SaveDialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("SaveDialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avformat-57.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avutil-55.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swresample-2.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swscale-4.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avcodec-57.dll")]
        public virtual void DeleteResourceFromSaveDialog()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Delete Resource From Save Dialog", ((string[])(null)));
#line 96
this.ScenarioSetup(scenarioInfo);
#line 97
 testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 98
 testRunner.And("I Create New Workflow using shortcut", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 99
 testRunner.And("I Make Workflow Savable And Then Save", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 100
 testRunner.And("I Filter Save Dialog Explorer with \"ResourceToDelete\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 101
 testRunner.And("I RightClick Save Dialog Localhost First Item", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 102
 testRunner.And("I Select Delete From SaveDialog Context Menu", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 103
 testRunner.And("I Click MessageBox Yes", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 104
 testRunner.And("I Click SaveDialog CancelButton", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 105
 testRunner.And("I Filter the Explorer with \"ResourceToDelete\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 106
 testRunner.Then("Explorer Does Not Contain First Item First Sub Item", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Create New Folder In Localhost Server From Save Dialog Then Escape Creates The Fo" +
            "lder")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "SaveDialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("SaveDialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avformat-57.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avutil-55.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swresample-2.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swscale-4.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avcodec-57.dll")]
        public virtual void CreateNewFolderInLocalhostServerFromSaveDialogThenEscapeCreatesTheFolder()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Create New Folder In Localhost Server From Save Dialog Then Escape Creates The Fo" +
                    "lder", ((string[])(null)));
#line 108
this.ScenarioSetup(scenarioInfo);
#line 109
 testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 110
 testRunner.And("I Create New Workflow using shortcut", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 111
 testRunner.And("I Make Workflow Savable And Then Save", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 112
 testRunner.And("I Filter Save Dialog Explorer with \"New Folder\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 113
 testRunner.And("I RightClick Save Dialog Localhost", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 114
 testRunner.And("I Select New Folder From SaveDialog Context Menu", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 115
 testRunner.And("I Hit Escape Key On The Keyboard", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 116
 testRunner.And("Save Dialog Explorer Contain Item \"New Folder\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 117
 testRunner.And("I Hit Escape Key On The Keyboard", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Create New Folder In Localhost Server From Save Dialog Then Click Away Creates Th" +
            "e Folder")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "SaveDialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("SaveDialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avformat-57.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avutil-55.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swresample-2.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("swscale-4.dll")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("avcodec-57.dll")]
        public virtual void CreateNewFolderInLocalhostServerFromSaveDialogThenClickAwayCreatesTheFolder()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Create New Folder In Localhost Server From Save Dialog Then Click Away Creates Th" +
                    "e Folder", ((string[])(null)));
#line 119
this.ScenarioSetup(scenarioInfo);
#line 120
 testRunner.Given("The Warewolf Studio is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 121
 testRunner.And("I Create New Workflow using shortcut", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 122
 testRunner.And("I Make Workflow Savable And Then Save", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 123
 testRunner.Then("I Enter Service Name Into Save Dialog As \"RandomText\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 124
 testRunner.And("I Hit Escape Key On The Keyboard", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
