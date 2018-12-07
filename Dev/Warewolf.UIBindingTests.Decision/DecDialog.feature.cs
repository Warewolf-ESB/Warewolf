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
namespace Warewolf.UIBindingTests.Decision
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.3.2.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute()]
    public partial class DecDialogFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private Microsoft.VisualStudio.TestTools.UnitTesting.TestContext _testContext;
        
#line 1 "DecDialog.feature"
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
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "DecDialog", "\tIn order to create a decision\r\n\tAs a Warewolf User\r\n\tI want to be shown the deci" +
                    "sion window setup", ProgrammingLanguage.CSharp, new string[] {
                        "DecDialog"});
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
                        && (testRunner.FeatureContext.FeatureInfo.Title != "DecDialog")))
            {
                global::Warewolf.UIBindingTests.Decision.DecDialogFeature.FeatureSetup(null);
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
        
        public virtual void EnsureInputsAreEnabledOnDecisionWindowLoad(string no, string inputs, string variable, string variable2, string value2, string variable3, string value3, string matchType, string[] exampleTags)
        {
            string[] @__tags = new string[] {
                    "DecDialog",
                    "MSTest:DeploymentItem:InfragisticsWPF4.DataPresenter.v15.1.dll",
                    "MSTest:DeploymentItem:InfragisticsWPF4.Controls.Editors.XamComboEditor.v15.1.dll",
                    "MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15." +
                        "1.dll",
                    "MSTest:DeploymentItem:InfragisticsWPF4.Controls.Editors.XamRichTextEditor.v15.1.d" +
                        "ll",
                    "MSTest:DeploymentItem:InfragisticsWPF4.DockManager.v15.1.dll",
                    "MSTest:DeploymentItem:Warewolf.Studio.CustomControls.dll",
                    "MSTest:DeploymentItem:Dev2.CustomControls.dll"};
            if ((exampleTags != null))
            {
                @__tags = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Concat(@__tags, exampleTags));
            }
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Ensure Inputs are enabled on decision window load", @__tags);
#line 15
this.ScenarioSetup(scenarioInfo);
#line 16
 testRunner.Given("I have a workflow New Workflow", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 17
 testRunner.And("drop a Decision tool onto the design surface", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 18
 testRunner.Then("the Decision window is opened", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 19
 testRunner.And(string.Format("\"{0}\" fields are \"Enabled\"", inputs), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 20
 testRunner.And(string.Format("the decision match variables \"{0}\"and match \"{1}\" and to match\"{2}\"", variable, variable2, variable3), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 21
 testRunner.And(string.Format("MatchType  is \"{0}\"", matchType), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 22
 testRunner.Then(string.Format("the inputs are \"{0}\"", inputs), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Ensure Inputs are enabled on decision window load: 1")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "DecDialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("DecDialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("DecDialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "1")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:No", "1")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Inputs", "Visible, Visible, Visible")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Variable", "[[a]]")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Variable2", "[[b]]")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Value2", "12")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Variable3", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Value3", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:MatchType", "<")]
        public virtual void EnsureInputsAreEnabledOnDecisionWindowLoad_1()
        {
#line 15
this.EnsureInputsAreEnabledOnDecisionWindowLoad("1", "Visible, Visible, Visible", "[[a]]", "[[b]]", "12", "", "", "<", ((string[])(null)));
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Ensure Inputs are enabled on decision window load: 2")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "DecDialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("DecDialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("DecDialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "2")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:No", "2")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Inputs", "Visible, Visible")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Variable", "[[a]]")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Variable2", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Value2", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Variable3", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Value3", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:MatchType", "is Alphanumeric")]
        public virtual void EnsureInputsAreEnabledOnDecisionWindowLoad_2()
        {
#line 15
this.EnsureInputsAreEnabledOnDecisionWindowLoad("2", "Visible, Visible", "[[a]]", "", "", "", "", "is Alphanumeric", ((string[])(null)));
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Ensure Inputs are enabled on decision window load: 3")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "DecDialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("DecDialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("DecDialog")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "3")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:No", "3")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Inputs", "Visible, Visible, Visible, Visible")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Variable", "[[a]]")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Variable2", "[[b]]")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Value2", "5")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Variable3", "[[c]]")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Value3", "15")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:MatchType", "Is Between")]
        public virtual void EnsureInputsAreEnabledOnDecisionWindowLoad_3()
        {
#line 15
this.EnsureInputsAreEnabledOnDecisionWindowLoad("3", "Visible, Visible, Visible, Visible", "[[a]]", "[[b]]", "5", "[[c]]", "15", "Is Between", ((string[])(null)));
#line hidden
        }
    }
}
#pragma warning restore
#endregion
