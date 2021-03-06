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
namespace Warewolf.Tools.Specs.Toolbox.Database.SqlServer
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.3.2.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute()]
    public partial class SqlServerConnectorFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private Microsoft.VisualStudio.TestTools.UnitTesting.TestContext _testContext;
        
#line 1 "SqlServerConnector.feature"
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
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "SqlServerConnector", "    In order to manage my database services\r\n\r\n    As a Warewolf User\r\n\r\n    I wa" +
                    "nt to be shown the database service setup", ProgrammingLanguage.CSharp, ((string[])(null)));
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
                        && (testRunner.FeatureContext.FeatureInfo.Title != "SqlServerConnector")))
            {
                global::Warewolf.Tools.Specs.Toolbox.Database.SqlServer.SqlServerConnectorFeature.FeatureSetup(null);
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
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Passing Timeouts to SQL Server")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "SqlServerConnector")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Database")]
        public virtual void PassingTimeoutsToSQLServer()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Passing Timeouts to SQL Server", new string[] {
                        "Database"});
#line 9
this.ScenarioSetup(scenarioInfo);
#line 10
    testRunner.Given("I have a workflow \"Test Sql With Timeouts\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table2369 = new TechTalk.SpecFlow.Table(new string[] {
                        "Input Data or[[Variable]]",
                        "Parameter",
                        "Empty is Null"});
#line 11
 testRunner.And("\"Test Sql With Timeouts\" contains \"TestSqlExecutesWithTimeouts\" from server \"loca" +
                    "lhost\" with mapping as", ((string)(null)), table2369, "And ");
#line 13
 testRunner.When("\"Test Sql With Timeouts\" is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 14
    testRunner.Then("the workflow containing the Sql Server connector has \"An\" execution error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            TechTalk.SpecFlow.Table table2370 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table2370.AddRow(new string[] {
                        ""});
#line 16
    testRunner.And("The Sql Server step \"TestSqlExecutesWithTimeouts\" in Workflow \"Test Sql With Time" +
                    "outs\" debug outputs appear as", ((string)(null)), table2370, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Opening Saved workflow with SQL Server tool")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "SqlServerConnector")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Database")]
        public virtual void OpeningSavedWorkflowWithSQLServerTool()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Opening Saved workflow with SQL Server tool", new string[] {
                        "Database"});
#line 21
this.ScenarioSetup(scenarioInfo);
#line 22
   testRunner.Given("I open workflow with database connector", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 24
    testRunner.And("Sql Server Source is Enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 25
    testRunner.And("Sql Server Source is \"testingDBSrc\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 26
 testRunner.And("Sql Server Action is \"dbo.Pr_CitiesGetCountries\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 27
 testRunner.And("Sql Server Inputs Are Enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table2371 = new TechTalk.SpecFlow.Table(new string[] {
                        "Input",
                        "Value",
                        "Empty is Null"});
            table2371.AddRow(new string[] {
                        "Prefix",
                        "[[Prefix]]",
                        "false"});
#line 29
    testRunner.And("Sql Server Inputs appear as", ((string)(null)), table2371, "And ");
#line 32
 testRunner.And("Validate Sql Server is Enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table2372 = new TechTalk.SpecFlow.Table(new string[] {
                        "Mapped From",
                        "Mapped To"});
            table2372.AddRow(new string[] {
                        "CountryID",
                        "[[dbo_Pr_CitiesGetCountries().CountryID]]"});
            table2372.AddRow(new string[] {
                        "Description",
                        "[[dbo_Pr_CitiesGetCountries().Description]]"});
#line 33
    testRunner.Then("Sql Server Outputs appear as", ((string)(null)), table2372, "Then ");
#line 37
 testRunner.And("Sql Server Recordset Name equals \"dbo_Pr_CitiesGetCountries\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Change SQL Server Source on Existing tool")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "SqlServerConnector")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Database")]
        public virtual void ChangeSQLServerSourceOnExistingTool()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Change SQL Server Source on Existing tool", new string[] {
                        "Database"});
#line 40
this.ScenarioSetup(scenarioInfo);
#line 41
    testRunner.Given("I open workflow with database connector", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 43
    testRunner.And("Sql Server Source is Enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 44
    testRunner.And("Sql Server Source is \"testingDBSrc\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 45
 testRunner.And("Sql Server Action is \"dbo.Pr_CitiesGetCountries\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 46
 testRunner.And("Sql Server Inputs Are Enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table2373 = new TechTalk.SpecFlow.Table(new string[] {
                        "Input",
                        "Value",
                        "Empty is Null"});
            table2373.AddRow(new string[] {
                        "Prefix",
                        "[[Prefix]]",
                        "false"});
#line 48
    testRunner.And("Sql Server Inputs appear as", ((string)(null)), table2373, "And ");
#line 51
 testRunner.And("Validate Sql Server is Enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table2374 = new TechTalk.SpecFlow.Table(new string[] {
                        "Mapped From",
                        "Mapped To"});
            table2374.AddRow(new string[] {
                        "CountryID",
                        "[[dbo_Pr_CitiesGetCountries().CountryID]]"});
            table2374.AddRow(new string[] {
                        "Description",
                        "[[dbo_Pr_CitiesGetCountries().Description]]"});
#line 52
    testRunner.Then("Sql Server Outputs appear as", ((string)(null)), table2374, "Then ");
#line 56
 testRunner.And("Sql Server Recordset Name equals \"dbo_Pr_CitiesGetCountries\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 57
 testRunner.When("Source is changed from to \"GreenPoint\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 58
 testRunner.Then("Sql Server Action is Enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 59
    testRunner.And("Sql Server Inputs Are Enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 60
    testRunner.And("Validate Sql Server is Enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Changing SQL Server Actions")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "SqlServerConnector")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Database")]
        public virtual void ChangingSQLServerActions()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Changing SQL Server Actions", new string[] {
                        "Database"});
#line 63
this.ScenarioSetup(scenarioInfo);
#line 64
    testRunner.Given("I open workflow with database connector", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 65
    testRunner.And("Sql Server Source is Enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 66
    testRunner.And("Sql Server Source is \"testingDBSrc\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 67
 testRunner.And("Sql Server Action is \"dbo.Pr_CitiesGetCountries\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 68
 testRunner.And("Sql Server Inputs Are Enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table2375 = new TechTalk.SpecFlow.Table(new string[] {
                        "Input",
                        "Value",
                        "Empty is Null"});
            table2375.AddRow(new string[] {
                        "Prefix",
                        "[[Prefix]]",
                        "false"});
#line 70
    testRunner.And("Sql Server Inputs appear as", ((string)(null)), table2375, "And ");
#line 73
 testRunner.And("Validate Sql Server is Enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table2376 = new TechTalk.SpecFlow.Table(new string[] {
                        "Mapped From",
                        "Mapped To"});
            table2376.AddRow(new string[] {
                        "CountryID",
                        "[[dbo_Pr_CitiesGetCountries().CountryID]]"});
            table2376.AddRow(new string[] {
                        "Description",
                        "[[dbo_Pr_CitiesGetCountries().Description]]"});
#line 74
    testRunner.Then("Sql Server Outputs appear as", ((string)(null)), table2376, "Then ");
#line 78
 testRunner.And("Sql Server Recordset Name equals \"dbo_Pr_CitiesGetCountries\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 79
 testRunner.When("Action is changed from to \"dbo.ImportOrder\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 80
 testRunner.And("Sql Server Inputs Are Enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table2377 = new TechTalk.SpecFlow.Table(new string[] {
                        "Input",
                        "Value",
                        "Empty is Null"});
            table2377.AddRow(new string[] {
                        "ProductId",
                        "[[ProductId]]",
                        "false"});
#line 81
    testRunner.And("Sql Server Inputs appear as", ((string)(null)), table2377, "And ");
#line 84
 testRunner.And("Validate Sql Server is Enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Change SQL Server Recordset Name")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "SqlServerConnector")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Database")]
        public virtual void ChangeSQLServerRecordsetName()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Change SQL Server Recordset Name", new string[] {
                        "Database"});
#line 87
this.ScenarioSetup(scenarioInfo);
#line 88
    testRunner.Given("I open workflow with database connector", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 89
    testRunner.And("Sql Server Source is Enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 90
    testRunner.And("Sql Server Source is \"testingDBSrc\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 91
 testRunner.And("Sql Server Action is \"dbo.Pr_CitiesGetCountries\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 92
 testRunner.And("Sql Server Inputs Are Enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table2378 = new TechTalk.SpecFlow.Table(new string[] {
                        "Input",
                        "Value",
                        "Empty is Null"});
            table2378.AddRow(new string[] {
                        "Prefix",
                        "[[Prefix]]",
                        "false"});
#line 93
    testRunner.And("Sql Server Inputs appear as", ((string)(null)), table2378, "And ");
#line 96
 testRunner.And("Validate Sql Server is Enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table2379 = new TechTalk.SpecFlow.Table(new string[] {
                        "Mapped From",
                        "Mapped To"});
            table2379.AddRow(new string[] {
                        "CountryID",
                        "[[dbo_Pr_CitiesGetCountries().CountryID]]"});
            table2379.AddRow(new string[] {
                        "Description",
                        "[[dbo_Pr_CitiesGetCountries().Description]]"});
#line 97
    testRunner.Then("Sql Server Outputs appear as", ((string)(null)), table2379, "Then ");
#line 101
 testRunner.And("Sql Server Recordset Name equals \"dbo_Pr_CitiesGetCountries\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 102
 testRunner.When("Recordset Name is changed to \"Pr_Cities\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table2380 = new TechTalk.SpecFlow.Table(new string[] {
                        "Mapped From",
                        "Mapped To"});
            table2380.AddRow(new string[] {
                        "CountryID",
                        "[[Pr_Cities().CountryID]]"});
            table2380.AddRow(new string[] {
                        "Description",
                        "[[Pr_Cities().Description]]"});
#line 103
 testRunner.Then("Sql Server Outputs appear as", ((string)(null)), table2380, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("No SQL Server Action to be loaded Error")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "SqlServerConnector")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Database")]
        public virtual void NoSQLServerActionToBeLoadedError()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("No SQL Server Action to be loaded Error", new string[] {
                        "Database"});
#line 109
this.ScenarioSetup(scenarioInfo);
#line 110
    testRunner.Given("I have a workflow \"NoStoredProceedureToLoad\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table2381 = new TechTalk.SpecFlow.Table(new string[] {
                        "Input Data or[[Variable]]",
                        "Parameter",
                        "Empty is Null"});
#line 111
 testRunner.And("\"NoStoredProceedureToLoad\" contains \"Testing/SQL/NoSqlStoredProceedure\" from serv" +
                    "er \"localhost\" with mapping as", ((string)(null)), table2381, "And ");
#line 113
 testRunner.When("\"NoStoredProceedureToLoad\" is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 114
    testRunner.Then("the workflow containing the Sql Server connector has \"An\" execution error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            TechTalk.SpecFlow.Table table2382 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table2382.AddRow(new string[] {
                        "Error: The selected database does not contain actions to perform"});
#line 115
    testRunner.And("The Sql Server step \"Testing/SQL/NoSqlStoredProceedure\" in Workflow \"NoStoredProc" +
                    "eedureToLoad\" debug outputs appear as", ((string)(null)), table2382, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Passing Null Input values to SQL Server")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "SqlServerConnector")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Database")]
        public virtual void PassingNullInputValuesToSQLServer()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Passing Null Input values to SQL Server", new string[] {
                        "Database"});
#line 120
this.ScenarioSetup(scenarioInfo);
#line 121
    testRunner.Given("I have a workflow \"PassingNullInputValue\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table2383 = new TechTalk.SpecFlow.Table(new string[] {
                        "Input Data or[[Variable]]",
                        "Parameter",
                        "Empty is Null"});
            table2383.AddRow(new string[] {
                        "[[value]]",
                        "a",
                        "True"});
#line 122
 testRunner.And("\"PassingNullInputValue\" contains \"Acceptance Testing Resources/GreenPoint\" from s" +
                    "erver \"localhost\" with mapping as", ((string)(null)), table2383, "And ");
#line 125
 testRunner.When("\"PassingNullInputValue\" is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 126
    testRunner.Then("the workflow containing the Sql Server connector has \"An\" execution error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            TechTalk.SpecFlow.Table table2384 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table2384.AddRow(new string[] {
                        "Error: Scalar value { value } is NULL"});
#line 127
    testRunner.And("The Sql Server step \"Acceptance Testing Resources/GreenPoint\" in Workflow \"Passin" +
                    "gNullInputValue\" debug outputs appear as", ((string)(null)), table2384, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Mapped To Recordsets incorrect")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "SqlServerConnector")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Database")]
        public virtual void MappedToRecordsetsIncorrect()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Mapped To Recordsets incorrect", new string[] {
                        "Database"});
#line 132
this.ScenarioSetup(scenarioInfo);
#line 133
    testRunner.Given("I have a workflow \"BadSqlParameterName\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table2385 = new TechTalk.SpecFlow.Table(new string[] {
                        "Input Data or[[Variable]]",
                        "Parameter",
                        "Empty is Null"});
            table2385.AddRow(new string[] {
                        "",
                        "a",
                        "True"});
#line 134
 testRunner.And("\"BadSqlParameterName\" contains \"Acceptance Testing Resources/GreenPoint\" from ser" +
                    "ver \"localhost\" with mapping as", ((string)(null)), table2385, "And ");
#line hidden
            TechTalk.SpecFlow.Table table2386 = new TechTalk.SpecFlow.Table(new string[] {
                        "Mapped From",
                        "Mapped To"});
            table2386.AddRow(new string[] {
                        "id",
                        "[[dbo_leon bob proc().id]]"});
            table2386.AddRow(new string[] {
                        "some column Name",
                        "[[dbo_leon bob proc().some column Name]]"});
#line 137
 testRunner.And("And \"BadSqlParameterName\" contains \"Acceptance Testing Resources/GreenPoint\" from" +
                    " server \"localhost\" with Mapping To as", ((string)(null)), table2386, "And ");
#line 141
 testRunner.When("\"BadSqlParameterName\" is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 142
    testRunner.Then("the workflow containing the Sql Server connector has \"An\" execution error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            TechTalk.SpecFlow.Table table2387 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table2387.AddRow(new string[] {
                        "Error: Sql Error: parse error"});
#line 143
    testRunner.And("The Sql Server step \"Acceptance Testing Resources/GreenPoint\" in Workflow \"BadSql" +
                    "ParameterName\" debug outputs appear as", ((string)(null)), table2387, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Parameter not found in the collection")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "SqlServerConnector")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Database")]
        public virtual void ParameterNotFoundInTheCollection()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Parameter not found in the collection", new string[] {
                        "Database"});
#line 148
this.ScenarioSetup(scenarioInfo);
#line 149
    testRunner.Given("I have a workflow \"BadMySqlParameterName\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table2388 = new TechTalk.SpecFlow.Table(new string[] {
                        "Input Data or[[Variable]]",
                        "Parameter",
                        "Empty is Null"});
            table2388.AddRow(new string[] {
                        "",
                        "`p_startswith`",
                        "false"});
#line 150
 testRunner.And("\"BadMySqlParameterName\" contains \"Testing/MySql/MySqlParameters\" from server \"loc" +
                    "alhost\" with mapping as", ((string)(null)), table2388, "And ");
#line 153
 testRunner.When("\"BadMySqlParameterName\" is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 154
    testRunner.Then("the workflow containing the Sql Server connector has \"An\" execution error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            TechTalk.SpecFlow.Table table2389 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table2389.AddRow(new string[] {
                        "Parameter \"p_startswith\" not found in the collection"});
#line 156
    testRunner.And("The Sql Server step \"Testing/MySql/MySqlParameters\" in Workflow \"BadMySqlParamete" +
                    "rName\" debug outputs appear as", ((string)(null)), table2389, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Recordset has invalid character")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "SqlServerConnector")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Database")]
        public virtual void RecordsetHasInvalidCharacter()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Recordset has invalid character", new string[] {
                        "Database"});
#line 161
this.ScenarioSetup(scenarioInfo);
#line 162
    testRunner.Given("I have a workflow \"MappingHasIncorrectCharacter\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table2390 = new TechTalk.SpecFlow.Table(new string[] {
                        "Input Data or[[Variable]]",
                        "Parameter",
                        "Empty is Null"});
            table2390.AddRow(new string[] {
                        "1",
                        "charValue",
                        "True"});
#line 163
 testRunner.And("\"MappingHasIncorrectCharacter\" contains \"Acceptance Testing Resources/GreenPoint\"" +
                    " from server \"localhost\" with mapping as", ((string)(null)), table2390, "And ");
#line 166
 testRunner.When("\"MappingHasIncorrectCharacter\" is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 167
    testRunner.Then("the workflow containing the Sql Server connector has \"An\" execution error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            TechTalk.SpecFlow.Table table2391 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table2391.AddRow(new string[] {
                        "[[dbo_ConvertTo, Int().result]] : Recordset name has invalid format"});
#line 168
    testRunner.And("The Sql Server step \"Acceptance Testing Resources/GreenPoint\" in Workflow \"Mappin" +
                    "gHasIncorrectCharacter\" debug outputs appear as", ((string)(null)), table2391, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("SqlServer backward Compatiblity")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "SqlServerConnector")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Database")]
        public virtual void SqlServerBackwardCompatiblity()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("SqlServer backward Compatiblity", new string[] {
                        "Database"});
#line 173
this.ScenarioSetup(scenarioInfo);
#line 174
    testRunner.Given("I have a workflow \"DataMigration\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table2392 = new TechTalk.SpecFlow.Table(new string[] {
                        "Input to Service",
                        "From Variable",
                        "Output from Service",
                        "To Variable"});
            table2392.AddRow(new string[] {
                        "[[ProductId]]",
                        "productId",
                        "[[dbo_GetCountries().CountryID]]",
                        "dbo_GetCountries().CountryID"});
            table2392.AddRow(new string[] {
                        "",
                        "",
                        "[[dbo_GetCountries().Description]]",
                        "dbo_GetCountries().Description"});
#line 175
 testRunner.And("\"DataMigration\" contains \"DataCon\" from server \"localhost\" with mapping as", ((string)(null)), table2392, "And ");
#line 179
 testRunner.When("\"DataMigration\" is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 180
    testRunner.Then("the workflow \"DataMigration\" execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Execute Sql Server With Timeout")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "SqlServerConnector")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("DatabaseTimeout")]
        public virtual void ExecuteSqlServerWithTimeout()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Execute Sql Server With Timeout", new string[] {
                        "DatabaseTimeout"});
#line 183
this.ScenarioSetup(scenarioInfo);
#line 184
 testRunner.Given("I have workflow \"SqlServerWorkflowForTimeout\" with \"SqlServerActivity\" SqlServer " +
                    "database connector", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 185
 testRunner.And("Sql Server Source is Enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 186
 testRunner.And("I Select \"NewSqlServerSource\" as SqlServer Source for \"SqlServerActivity\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 187
 testRunner.And("I Select \"dbo.Pr_CitiesGetCountries_Delayed\" as Server Action for \"SqlServerActiv" +
                    "ity\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 188
 testRunner.And("Sql Command Timeout is \"60\" milliseconds for \"SqlServerActivity\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 189
 testRunner.And("Validate Sql Server is Enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 190
 testRunner.And("I click Sql Generate Outputs", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 191
 testRunner.And("I click Test", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 192
 testRunner.And("Sql Command Timeout is \"1\" milliseconds for \"SqlServerActivity\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 193
 testRunner.When("Sql Workflow \"SqlServerWorkflowForTimeout\" containing dbTool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 194
 testRunner.And("the workflow \"SqlServerWorkflowForTimeout\" execution has \"AN\" error \"SQL Error: E" +
                    "xecution Timeout Expired\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 195
 testRunner.And("the workflow \"SqlServerWorkflowForTimeout\" error does not contain \"NewLine\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
