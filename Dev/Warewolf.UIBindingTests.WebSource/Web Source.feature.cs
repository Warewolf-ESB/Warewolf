// ------------------------------------------------------------------------------
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
namespace Warewolf.UIBindingTests.WebSource
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.3.2.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute()]
    public partial class WebSourceFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private Microsoft.VisualStudio.TestTools.UnitTesting.TestContext _testContext;
        
#line 1 "Web Source.feature"
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
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Web Source", "\tIn order to create a web source for web services\r\n\tAs a Warewolf user\r\n\tI want t" +
                    "o be able to manage web sources easily", ProgrammingLanguage.CSharp, new string[] {
                        "WebSource"});
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
                        && (testRunner.FeatureContext.FeatureInfo.Title != "Web Source")))
            {
                global::Warewolf.UIBindingTests.WebSource.WebSourceFeature.FeatureSetup(null);
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
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Creating New Web Source")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Web Source")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WebSource")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WebSource")]
        public virtual void CreatingNewWebSource()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Creating New Web Source", new string[] {
                        "WebSource",
                        "MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15." +
                            "1.dll",
                        "MSTest:DeploymentItem:Warewolf_Studio.exe",
                        "MSTest:DeploymentItem:Newtonsoft.Json.dll",
                        "MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll",
                        "MSTest:DeploymentItem:System.Windows.Interactivity.dll"});
#line 13
this.ScenarioSetup(scenarioInfo);
#line 14
   testRunner.Given("I open New Web Source", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 15
   testRunner.Then("\"New Web Service Source\" tab is opened", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 16
   testRunner.And("title is \"New Web Service Source\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 17
   testRunner.And("I type Address as \"http://TFSBLD.premier.local/IntegrationTestSite\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 18
   testRunner.And("I type Default Query as \"/GetCountries.ashx?extension=json&prefix=a\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 19
   testRunner.Then("\"New Web Service Source *\" tab is opened", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 20
   testRunner.And("TestQuery is \"http://TFSBLD.premier.local/IntegrationTestSite/GetCountries.ashx?e" +
                    "xtension=json&prefix=a\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 21
   testRunner.And("\"Save\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 22
   testRunner.And("\"Test Connection\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 23
   testRunner.And("\"Cancel Test\" is \"Disabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 24
   testRunner.And("I Select Authentication Type as \"Anonymous\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 25
   testRunner.And("Username field is \"Collapsed\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 26
   testRunner.And("Password field is \"Collapsed\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 27
   testRunner.When("Test Connecton is \"Successful\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 28
   testRunner.And("\"Save\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 29
   testRunner.When("I save as \"Testing Resource Save\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 30
   testRunner.Then("the save dialog is opened", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 31
   testRunner.Then("title is \"Testing Resource Save\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 32
   testRunner.And("\"Testing Resource Save\" tab is opened", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 33
   testRunner.When("I click TestQuery", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 34
   testRunner.Then("the browser window opens with \"http://TFSBLD.premier.local/IntegrationTestSite/Ge" +
                    "tCountries.ashx?extension=json&prefix=a\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Creating New Web Source under auth type as user")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Web Source")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WebSource")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WebSource")]
        public virtual void CreatingNewWebSourceUnderAuthTypeAsUser()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Creating New Web Source under auth type as user", new string[] {
                        "WebSource",
                        "MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15." +
                            "1.dll",
                        "MSTest:DeploymentItem:Warewolf_Studio.exe",
                        "MSTest:DeploymentItem:Newtonsoft.Json.dll",
                        "MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll",
                        "MSTest:DeploymentItem:System.Windows.Interactivity.dll"});
#line 42
this.ScenarioSetup(scenarioInfo);
#line 43
   testRunner.Given("I open New Web Source", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 44
   testRunner.And("I type Address as \"http://TFSBLD.premier.local/IntegrationTestSite\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 45
   testRunner.And("I type Default Query as \"/GetCountries.ashx?extension=json&prefix=a\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 46
   testRunner.And("\"Save\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 47
   testRunner.And("\"Test Connection\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 48
   testRunner.And("I Select Authentication Type as \"User\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 49
   testRunner.And("Username field is \"Visible\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 50
   testRunner.And("Password field is \"Visible\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 51
   testRunner.And("I type Username as \"IntegrationTester\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 52
   testRunner.And("I type Password", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 53
   testRunner.When("Test Connecton is \"Successful\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 54
   testRunner.And("\"Save\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 55
   testRunner.When("I save the source", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 56
   testRunner.Then("the save dialog is opened", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Incorrect address anonymous auth type not allowing save")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Web Source")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WebSource")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WebSource")]
        public virtual void IncorrectAddressAnonymousAuthTypeNotAllowingSave()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Incorrect address anonymous auth type not allowing save", new string[] {
                        "WebSource",
                        "MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15." +
                            "1.dll",
                        "MSTest:DeploymentItem:Warewolf_Studio.exe",
                        "MSTest:DeploymentItem:Newtonsoft.Json.dll",
                        "MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll",
                        "MSTest:DeploymentItem:System.Windows.Interactivity.dll"});
#line 64
this.ScenarioSetup(scenarioInfo);
#line 65
   testRunner.Given("I open New Web Source", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 66
   testRunner.And("I type Address as \"sdfsdfd\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 67
   testRunner.And("I type Default Query as \"/GetCountries.ashx?extension=json&prefix=a\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 68
   testRunner.And("\"Save\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 69
   testRunner.And("\"Test Connection\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 70
   testRunner.And("I Select Authentication Type as \"Anonymous\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 71
   testRunner.When("Test Connecton is \"UnSuccessful\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 72
   testRunner.And("Validation message is thrown", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 73
   testRunner.And("\"Save\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Incorrect address Shows correct error message")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Web Source")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WebSource")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WebSource")]
        public virtual void IncorrectAddressShowsCorrectErrorMessage()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Incorrect address Shows correct error message", new string[] {
                        "WebSource",
                        "MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15." +
                            "1.dll",
                        "MSTest:DeploymentItem:Warewolf_Studio.exe",
                        "MSTest:DeploymentItem:Newtonsoft.Json.dll",
                        "MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll",
                        "MSTest:DeploymentItem:System.Windows.Interactivity.dll"});
#line 81
this.ScenarioSetup(scenarioInfo);
#line 82
   testRunner.Given("I open New Web Source", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 83
   testRunner.And("I type Address as \"sdfsdfd\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 84
   testRunner.And("I type Default Query as \"/GetCountries.ashx?extension=json&prefix=a\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 85
   testRunner.And("\"Save\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 86
   testRunner.And("\"Test Connection\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 87
   testRunner.And("I Select Authentication Type as \"Anonymous\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 88
   testRunner.When("Test Connecton is \"UnSuccessful\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 89
   testRunner.And("Validation message is thrown", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 90
   testRunner.And("\"Save\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 91
   testRunner.And("the error message is \"Illegal characters in path.\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Incorrect address user auth type is not allowing to save")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Web Source")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WebSource")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WebSource")]
        public virtual void IncorrectAddressUserAuthTypeIsNotAllowingToSave()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Incorrect address user auth type is not allowing to save", new string[] {
                        "WebSource",
                        "MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15." +
                            "1.dll",
                        "MSTest:DeploymentItem:Warewolf_Studio.exe",
                        "MSTest:DeploymentItem:Newtonsoft.Json.dll",
                        "MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll",
                        "MSTest:DeploymentItem:System.Windows.Interactivity.dll"});
#line 99
this.ScenarioSetup(scenarioInfo);
#line 100
   testRunner.Given("I open New Web Source", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 101
   testRunner.And("I type Address as \"sdfsdfd\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 102
   testRunner.And("I type Default Query as \"/GetCountries.ashx?extension=json&prefix=a\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 103
   testRunner.And("\"Save\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 104
   testRunner.And("\"Test Connection\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 105
   testRunner.And("I Select Authentication Type as \"User\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 106
   testRunner.And("I type Username as \"test\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 107
   testRunner.And("I type Password", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 108
   testRunner.When("Test Connecton is \"UnSuccessful\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 109
   testRunner.And("Validation message is thrown", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 110
   testRunner.And("\"Save\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Testing Auth type as Anonymous and swaping it resets the test connection")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Web Source")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WebSource")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WebSource")]
        public virtual void TestingAuthTypeAsAnonymousAndSwapingItResetsTheTestConnection()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Testing Auth type as Anonymous and swaping it resets the test connection", new string[] {
                        "WebSource",
                        "MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15." +
                            "1.dll",
                        "MSTest:DeploymentItem:Warewolf_Studio.exe",
                        "MSTest:DeploymentItem:Newtonsoft.Json.dll",
                        "MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll",
                        "MSTest:DeploymentItem:System.Windows.Interactivity.dll"});
#line 118
this.ScenarioSetup(scenarioInfo);
#line 119
   testRunner.Given("I open New Web Source", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 120
   testRunner.And("\"Save\" is \"Disabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 121
   testRunner.And("I type Address as \"http://TFSBLD.premier.local/IntegrationTestSite\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 122
   testRunner.And("I type Default Query as \"/GetCountries.ashx?extension=json&prefix=a\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 123
   testRunner.And("\"Save\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 124
   testRunner.And("\"Test Connection\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 125
   testRunner.And("I Select Authentication Type as \"User\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 126
   testRunner.And("I type Username as \"test\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 127
   testRunner.And("I type Password", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 128
   testRunner.When("Test Connecton is \"Successful\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 129
   testRunner.And("Validation message is Not thrown", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 130
   testRunner.And("\"Save\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 131
   testRunner.And("I Select Authentication Type as \"Anonymous\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 132
   testRunner.And("Username field is \"Collapsed\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 133
   testRunner.And("Password field is \"Collapsed\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 134
   testRunner.And("\"Save\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 135
   testRunner.When("Test Connecton is \"Successful\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 136
   testRunner.And("Validation message is Not thrown", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 137
   testRunner.And("\"Save\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 138
   testRunner.And("I Select Authentication Type as \"User\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 139
   testRunner.And("Username field is \"Visible\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 140
   testRunner.And("Password field is \"Visible\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 141
   testRunner.And("\"Save\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Editing saved Web Source")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Web Source")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WebSource")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WebSource")]
        public virtual void EditingSavedWebSource()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Editing saved Web Source", new string[] {
                        "WebSource",
                        "MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15." +
                            "1.dll",
                        "MSTest:DeploymentItem:Warewolf_Studio.exe",
                        "MSTest:DeploymentItem:Newtonsoft.Json.dll",
                        "MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll",
                        "MSTest:DeploymentItem:System.Windows.Interactivity.dll"});
#line 149
this.ScenarioSetup(scenarioInfo);
#line 150
   testRunner.Given("I open \"Test\" web source", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 151
   testRunner.Then("\"Test\" tab is opened", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 152
   testRunner.And("title is \"Test\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 153
   testRunner.And("Address is \"http://TFSBLD.premier.local/IntegrationTestSite\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 154
   testRunner.And("Default Query is \"/GetCountries.ashx?extension=json&prefix=a\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 155
   testRunner.And("\"Save\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 156
   testRunner.And("\"Test Connection\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 157
   testRunner.And("Select Authentication Type as \"Anonymous\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 158
   testRunner.And("Username field is \"Collapsed\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 159
   testRunner.And("Password field is \"Collapsed\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 160
   testRunner.And("\"Save\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 161
   testRunner.When("I change Address to \"http://TFSBLD.premier.local/IntegrationTestSite\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 162
   testRunner.And("I type Default Query as \"/GetCountries.ashx?extension=json&prefix=b\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 163
   testRunner.Then("\"Test *\" tab is opened", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 164
   testRunner.And("\"Save\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 165
   testRunner.And("\"Test Connection\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 166
   testRunner.And("\"Save\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 167
   testRunner.When("Test Connecton is \"Successfull\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 168
   testRunner.Then("\"Save\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 169
   testRunner.When("I save the source", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Editing saved Web Source auth type")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Web Source")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WebSource")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WebSource")]
        public virtual void EditingSavedWebSourceAuthType()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Editing saved Web Source auth type", new string[] {
                        "WebSource",
                        "MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15." +
                            "1.dll",
                        "MSTest:DeploymentItem:Warewolf_Studio.exe",
                        "MSTest:DeploymentItem:Newtonsoft.Json.dll",
                        "MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll",
                        "MSTest:DeploymentItem:System.Windows.Interactivity.dll"});
#line 177
 this.ScenarioSetup(scenarioInfo);
#line 178
   testRunner.Given("I open \"Test\" web source", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 179
   testRunner.Then("\"Test\" tab is opened", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 180
   testRunner.And("Address is \"http://TFSBLD.premier.local/IntegrationTestSite\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 181
   testRunner.And("Default Query is \"/GetCountries.ashx?extension=json&prefix=a\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 182
   testRunner.And("\"Save\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 183
   testRunner.And("\"Test Connection\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 184
   testRunner.And("Select Authentication Type as \"Anonymous\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 185
   testRunner.And("Username field is \"Collapsed\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 186
   testRunner.And("Password field is \"Collapsed\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 187
   testRunner.When("I edit Authentication Type as \"User\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 188
   testRunner.And("Username field is \"Visible\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 189
   testRunner.And("Password field is \"Visible\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 190
   testRunner.And("Username field as \"IntegrationTester\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 191
   testRunner.And("Password field is \"I73573r0\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 192
   testRunner.And("\"Save\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 193
   testRunner.When("Test Connecton is \"Successfull\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 194
   testRunner.Then("\"Save\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Cancel Seb Source Test")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Web Source")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WebSource")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WebSource")]
        public virtual void CancelSebSourceTest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Cancel Seb Source Test", new string[] {
                        "WebSource",
                        "MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15." +
                            "1.dll",
                        "MSTest:DeploymentItem:Warewolf_Studio.exe",
                        "MSTest:DeploymentItem:Newtonsoft.Json.dll",
                        "MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll",
                        "MSTest:DeploymentItem:System.Windows.Interactivity.dll"});
#line 202
this.ScenarioSetup(scenarioInfo);
#line 203
   testRunner.Given("I open New Web Source", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 204
   testRunner.Then("\"New Web Service Source\" tab is opened", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 205
   testRunner.And("title is \"New Web Service Source\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 206
   testRunner.And("I type Address as \"http://test-warewolf.cloudapp.net:3142/public/Hello%20World?Na" +
                    "me=Me\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 207
   testRunner.When("Test Connecton is \"Long Running\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 208
   testRunner.And("I Cancel the Test", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 209
   testRunner.Then("\"Cancel Test\" is \"Disabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 210
   testRunner.And("Validation message is thrown", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 211
   testRunner.And("Validation message is \"Test Cancelled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Web Source returns text")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Web Source")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("WebSource")]
        public virtual void WebSourceReturnsText()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Web Source returns text", new string[] {
                        "MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15." +
                            "1.dll",
                        "MSTest:DeploymentItem:Warewolf_Studio.exe",
                        "MSTest:DeploymentItem:Newtonsoft.Json.dll",
                        "MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll",
                        "MSTest:DeploymentItem:System.Windows.Interactivity.dll"});
#line 218
this.ScenarioSetup(scenarioInfo);
#line 219
   testRunner.Given("I open New Web Source", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 220
   testRunner.Then("\"New Web Service Source\" tab is opened", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 221
   testRunner.And("title is \"New Web Service Source\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 222
   testRunner.And("I type Address as \"http://warewolf.io/version.txt\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 223
   testRunner.And("\"Test Connection\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 224
   testRunner.When("Test Connecton is \"Successful\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 225
   testRunner.Then("\"Save\" is \"Enabled\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 226
   testRunner.When("I save as \"Testing Return Text\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 227
   testRunner.Then("the save dialog is opened", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 228
   testRunner.Then("title is \"Testing Return Text\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 229
   testRunner.And("\"Testing Return Text\" tab is opened", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
