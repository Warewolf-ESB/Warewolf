Feature: ODBCConnector
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

Scenario: Creating ODBC Server Connector
	Given I drag a ODBC Server database connector
	And Source is Enable
	And Action is Disable
	And Inputs is Disable
	And Outputs is Disable
	When I Selected "GreenPoint" as Source
	Then Action is Enable
	When I selected "Command" as thee action
	Then Inputs is Enable 
	And Inputs appears as 
	| Input     | Value | Empty is Null |
	| EID		|       | false         |
	And Validate is Enable
	When I click Validate
	Then the Test Connector and Calculate Outputs window is open
	When I click Test
	Then Test Connector and Calculate Outputs outputs appear as
	| Column1 |
	| 1       |
	When I clicked OKay
	Then Outputs appears as
	| Mapped From | Mapped To             | 
	| Column1     | [[Command().Column1]] |
	And Recordset Name equal "Command"	

Scenario: Opening Saved workflow with ODBC Server tool
	Given I open workflow with ODBC connector
	And Source is Enable
	And Source is "localODBCTest"
	And Action is Enable
	And Action is "dbo.Pr_CitiesGetCountries"
	And Inputs is Enable
	Then Inputs appears as
	| Input | Value | Empty is Null |
	| Prefix | [[Prefix]]    | false         | 
	And Validate is Enable
	Then Outputs appears as
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Recordset Name equal "dbo_Pr_CitiesGetCountries"

Scenario: Change ODBC Source on Existing tool
	Given I open workflow with ODBC connector
	And Source is Enable
	And Source is "localODBCTest"
	And Action is Enable
	And Action is "dbo.Pr_CitiesGetCountries"
	And Inputs is Enable
	Then Inputs appears as
	| Input | Value | Empty is Null |
	| Prefix | [[Prefix]]    | false         | 
	And Validate is Enable
	Then Outputs appears as
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Recordset Name equal "dbo_Pr_CitiesGetCountries"
	When Source is changed to "GreenPoint"
	And Action is Enable
	And Inputs is Disable
	And Outputs is Disable  
	And Validate is Enable

#Spec to be modified once test results section is included in tool window
 Scenario: Editing ODBC Service and Test Execution is unsuccesful
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


Scenario: Changing ODBC Actions
	Given I open workflow with ODBC connector
	And Source is Enable
	And Source is "localODBCTest"
	And Action is Enable
	And Action is "dbo.Pr_CitiesGetCountries"
	And Inputs is Enable
	Then Inputs appears as
	| Input | Value | Empty is Null |
	| Prefix | [[Prefix]]    | false         | 
	And Validate is Enable
	Then Outputs appears as
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Recordset Name equal "dbo_Pr_CitiesGetCountries"
	When Action is changed to "dbo.ImportOrder"
	And Inputs is Enable
	And Inputs appears as
	| Input    | Value | Empty is Null |
	| ProductId |               | false         |	
	And Validate is Enable	


Scenario: Change ODBC Recordset Name
	Given I open workflow with ODBC connector
	And Source is Enable
	And Source is "localODBCTest"
	And Action is Enable
	And Action is "dbo.Pr_CitiesGetCountries"
	And Inputs is Enable
	Then Inputs appears as
	| Input | Value | Empty is Null |
	| Prefix | [[Prefix]]    | false         | 
	And Validate is Enable
	Then Outputs appears as
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Recordset Name equal "dbo_Pr_CitiesGetCountries"
	When Recordset Name is changed from to "Pr_Cities"
	Then Outputs appears as
	| Mapped From | Mapped To                   |
	| CountryID   | [[Pr_Cities().CountryID]]   |
	| Description | [[Pr_Cities().Description]] |
	
