Feature: SqlServerConnector
	In order to manage my database services
	As a Warewolf User
	I want to be shown the database service setup


Scenario: Creating SQL Server Connector
	Given I open New Workflow
	Then "New Workflow" tab is opened
	And "Source" is focused
	And "Source" is "Enabled"
	And "Action" is "Disabled"
	And "Inputs/Outputs" is "Disabled" 
	And "Mapping" is "Disabled" 
	When I Select "GreenPoint" as Source
	Then "Action" is "Enabled"
	When I select "dbo.ImportOrder" as the action
	Then "Inputs/Outputs" is "Enabled" 
	And "Validate" is "Enabled"
	And "Inputs/Outputs" appear as 
	| Input     | Default Value | Empty is Null |
	| ProductId |               | false         |
	And "Mapping" is "Disabled" 
	When I click "Validate"
	Then the "Test Connector and Calculate Outputs" window is opened
	And inputs appear as
	| ProductId |
	| 1         |
	When I click "Test"
	Then "Test Connector and Calculate Outputs" outputs appear as
	| Column1 |
	| 1       |
	When I click "OK"
	Then "Inputs/Outputs" appear as
	| Inputs    | Default Value | Empty is Null |
	| ProductId | 1             | false         |  
	Then "Mapping" is "Enabled"
	Then Mapping outputs appear as
	| Mapped From | Mapped To                     | 
	| Column1     | [[dbo_ImportOrder().Column1]] | 
	And "Recordset Name" equals "dbo_ImportOrder"
	When the SQL Server Connector tool is executed
	And the execution has "No" error
	Then the debug output as 
	|                                   |
	| [[dbo_ImportOrder().Column1]] = 1 |

Scenario: Opening Saved workflow with SQL Server tool
   Given I open Wolf-860
	Then "Wolf-860" tab is opened
	And "Source" is "Enabled"
	And "Source" equals "testingDBSrc"
	And "Action" is "Enabled"
	And "Action" equals "dbo.Pr_CitiesGetCountries"
	And "Input/Output" is "Enabled"
	And "Inputs/Outputs" mappings are
	| Inputs | Default Value | Empty is Null |
	| Prefix | [[Prefix]]    | false         | 
	And "Validate" is "Enabled"
	And "Mapping" is "Enabled"
	And mappings are
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And "Recordset Name" equals "dbo_Pr_CitiesGetCountries"

Scenario: Change Source on Existing tool
	Given I open Wolf-860
	Then "Wolf-860" tab is opened
	And "Source" is "Enabled"
	And "Source" equals "testingDBSrc"
	And "Action" is "Enabled"
	And "Action" equals "dbo.Pr_CitiesGetCountries"
	And "Input/Output" is "Enabled"
	And input mappings are
	| Inputs | Default Value | Empty is Null |
	| Prefix | [[Prefix]]    | false         | 
	And "Validate" is "Enabled"
	And "Mapping" is "Enabled"
	And mappings are
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And "Recordset Name" equals "dbo_Pr_CitiesGetCountries"
	When "Source" is changed from "testingDBSrc" to "GreenPoint"
	Then "Action" is "Enabled"
	And "Inputs/Outputs" is "Disabled" 
	And "Mapping" is "Disabled" 
	And "Validate" is "Disabled"

#Spec to be modified once test results section is included in tool window
@ignore
 Scenario: Editing DB Service and Test Execution is unsuccesful
   Given I open "InsertDummyUser" service
   And "InsertDummyUser" tab is opened
   Then "1 Data Source" is "Enabled"
   And Data Source is focused
   When "DemoDB" is selected as the data source
   Then "2 Select Action" is "Enabled"
   And "dbo.InsertDummyUser" is selected as the action
   Then "3 Test Connector and Calculate Outputs" is "Enabled" 
   And Inspect Data Connector hyper link is "Visible"
   And inputs are
   | fname  | lname | username | password | lastAccessDate |
   | Change | Test  | wolf     | Dev      | 10/1/1990      |
   And "Validate" is "Enabled"   
   And "Save" is "Disabled"  
   When testing the action fails
   Then "4 Defaults and Mapping" is "Disabled" 
   And input mappings are
	| Inputs         | Default Value | Required Field | Empty is Null |
	And output mappings are
	| Output | Output Alias | Recordset Name      |
	And "Save" is "Disabled"


Scenario: Changing Actions
	Given I open Wolf-860
	Then "Wolf-860" tab is opened
	And "Source" is "Enabled"
	And "Source" equals "testingDBSrc"
	And "Action" is "Enabled"
	And "Action" equals "dbo.Pr_CitiesGetCountries"
	And "Input/Output" is "Enabled"
	And "Input/Output" mappings are
	| Inputs | Default Value | Empty is Null |
	| Prefix | [[Prefix]]    | false         | 
	And "Validate" is "Enabled"
	And "Mapping" is "Enabled"
	And mappings are
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	When "Action" is changed from "dbo.Pr_CitiesGetCountries" to "dbo.ImportOrder"
	Then "Inputs/Outputs" is "Enabled" 
	And "Inputs/Outputs" mappings are
	| Inputs    | Default Value | Empty is Null |
	| ProductID |               | false         |
	And "Mapping" is "Disabled" 
	And "Validate" is "Enabled"
	When I click "Validate"
	Then the "Test Connector and Calculate Outputs" window is opened
	And inputs appear as
	| ProductId |
	| 1         |
	When I click "Test"
	Then "Test Connector and Calculate Outputs" outputs appear as
	| Column1 |
	| 1       |
	When I click "OK"
	Then mappings are
	| Mapped From | Mapped To                     |
	| Column1     | [[dbo_ImportOrder().Column1]] |
	And "Recordset Name" equals "dbo_ImportOrder"


Scenario: Change Recordset Name
	Given I open Wolf-860
	Then "Wolf-860" tab is opened
	And "Source" is "Enabled"
	And "Source" equals "testingDBSrc"
	And "Action" is "Enabled"
	And "Action" equals "dbo.Pr_CitiesGetCountries"
	And "Input/Output" is "Enabled"
	And input mappings are
	| Inputs | Default Value | Empty is Null |
	| Prefix | [[Prefix]]    | false         | 
	And "Validate" is "Enabled"
	And "Mapping" is "Enabled"
	And mappings are
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And "Recordset Name" equals "dbo_Pr_CitiesGetCountries"
	When "Recordset Name" is changed from "dbo_Pr_CitiesGetCountries" to "Pr_Cities"
	Then mappings are
	| Mapped From | Mapped To                   |
	| CountryID   | [[Pr_Cities().CountryID]]   |
	| Description | [[Pr_Cities().Description]] |
	
Scenario: Invalid Recordset Name
	Given I open New Workflow
	Then "New Workflow" tab is opened
	And "Source" is focused
	And "Source" is "Enabled"
	And "Action" is "Disabled"
	And "Inputs/Outputs" is "Disabled" 
	And "Mapping" is "Disabled" 
	When I Select "GreenPoint" as Source
	Then "Action" is "Enabled"
	When I select "dbo.ImportOrder" as the action
	Then "Inputs/Outputs" is "Enabled" 
	And "Validate" is "Enabled"
	And "Mapping" is "Disabled" 
	When I click "Validate"
	Then the "Test Connector and Calculate Outputs" window is opened
	And inputs are
	| ProductId |
	| 1         |	
	When I click "Test"
	Then "Test Connector and Calculate Outputs" outputs appear as
	| Column1 |
	| 1       |
	When I click "OK"
	Then "Inputs/Outputs" mappings are
	| Inputs    | Default Value | Empty is Null |
	| ProductId | 1             | false         |  
	And "Mapping" is "Enabled" 
	And mapping are
	| Mapped From | Mapped To                     | 
	| Column1     | [[dbo_ImportOrder().Column1]] | 
	And "Recordset Name" equals "dbo_ImportOrder"
	When "Recordset Name" is changed from "dbo_ImportOrder" to "1"
	When the SQL Server Connector tool is executed
	And the execution has "AN" error
	Then the debug output as 
	|                                          |
	| Error : input must be recordset or value |