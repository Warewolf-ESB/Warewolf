Feature: ODBCConnector
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

Scenario: Creating ODBC Server Connector
	Given I open New Workflow
	And I drag a ODBC Server database connector
	And Source iz Enable
	And Action iz Disable
	And Inputs iz Disable
	And Outputs iz Disable
	When I Selected "GreenPoint" az Source
	Then Action iz Enable
	When I selected "Command" as thee action
	Then Inputs iz Enable 
	And Inputs appears az 
	| Input     | Value | Empty is Null |
	| EID		|       | false         |
	And Validate iz Enable
	When I click Validatt
	Then the Test Connector and Calculate Outputs window is open
	When I click Tezt
	Then Test Connector and Calculate Outputz outputs appear az
	| Column1 |
	| 1       |
	When I clicked OKay
	Then Outputs appears az
	| Mapped From | Mapped To                     | 
	| Column1     | [[Command().Column1]] | 

	And Recordset Name equal "Command"	

Scenario: Opening Saved workflow with ODBC Server tool
   Given I open "Wolf-860"
	And Source iz Enable
	And Source is "localODBCTest"
	And Action iz Enable
	And Action is dbo.Pr_CitiesGetCountries
	And Inputs iz Enable
	Then Inputs appears az
	| Inputs | Default Value | Empty is Null |
	| Prefix | [[Prefix]]    | false         | 
	And Validate iz Enable
	And Mapping iz Enable
	Then Outputs appears az
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Recordset Name equal "dbo_Pr_CitiesGetCountries"

Scenario: Change Source on Existing tool
	Given I open Wolf-860
	Then "Wolf-860" tab is opened
	And "Source" is "Enabled"
	And "Source" equals "localODBCTest"
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
	When "Source" is changed from "localODBCTest" to "GreenPoint"
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
   When "DemoDB" is selected az the data source
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
	And "Source" equals "localODBCTest"
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
	And inputs appears az
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
	And "Source" equals "localODBCTest"
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
	
