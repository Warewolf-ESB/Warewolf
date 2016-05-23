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
   Given I open workflow with ODBC connector
	And Source iz Enable
	And Source iz "localODBCTest"
	And Action iz Enable
	And Action iz "dbo.Pr_CitiesGetCountries"
	And Inputs iz Enable
	Then Inputs appears az
	| Input | Value | Empty is Null |
	| Prefix | [[Prefix]]    | false         | 
	And Validate iz Enable
	Then Outputs appears az
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Recordset Name equal "dbo_Pr_CitiesGetCountries"

Scenario: Change Source on Existing tool
	Given I open workflow with ODBC connector
	And Source iz Enable
	And Source iz "localODBCTest"
	And Action iz Enable
	And Action iz "dbo.Pr_CitiesGetCountries"
	And Inputs iz Enable
	Then Inputs appears az
	| Input | Value | Empty is Null |
	| Prefix | [[Prefix]]    | false         | 
	And Validate iz Enable
	Then Outputs appears az
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Recordset Name equal "dbo_Pr_CitiesGetCountries"
	When Source iz changed to "GreenPoint"
	And Action iz Enable
	And Inputs iz Disable
	And Outputs iz Disable  
	And Validate iz Enable

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
	Given I open workflow with ODBC connector
	And Source iz Enable
	And Source iz "localODBCTest"
	And Action iz Enable
	And Action iz "dbo.Pr_CitiesGetCountries"
	And Inputs iz Enable
	Then Inputs appears az
	| Input | Value | Empty is Null |
	| Prefix | [[Prefix]]    | false         | 
	And Validate iz Enable
	Then Outputs appears az
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Recordset Name equal "dbo_Pr_CitiesGetCountries"
	When Action iz changed to "dbo.ImportOrder"
	And Inputs iz Enable
	And Inputs appears az
	| Input    | Value | Empty is Null |
	| ProductId |               | false         |	
	And Validate iz Enable	


Scenario: Change Recordset Name
	Given I open workflow with ODBC connector
	And Source iz Enable
	And Source iz "localODBCTest"
	And Action iz Enable
	And Action iz "dbo.Pr_CitiesGetCountries"
	And Inputs iz Enable
	Then Inputs appears az
	| Input | Value | Empty is Null |
	| Prefix | [[Prefix]]    | false         | 
	And Validate iz Enable
	Then Outputs appears az
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Recordset Name equal "dbo_Pr_CitiesGetCountries"
	When Recordset Name iz changed from to "Pr_Cities"
	Then Outputs appears az
	| Mapped From | Mapped To                   |
	| CountryID   | [[Pr_Cities().CountryID]]   |
	| Description | [[Pr_Cities().Description]] |
	
