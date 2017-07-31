@Database
Feature: ODBCConnector
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

Scenario: Running ODBC Tool Test
	Given I open workflow with ODBC connector
	And ODBC Source is Enabled
	When I Select GreenPoint as ODBC Source
	Then ODBC Action is Enabled
	When I select "dbo_Pr_CitiesGetCountries" as the ODBC action
	Then ODBC Inputs are Enabled
	And ODBC Inputs appear as
	| Input     | Value | Empty is Null |
	| EID		|       | false         |
	And Validate ODBC is Enabled
	When I click Validate ODBC
	When I click Test ODBC
	Then Test ODBC Connector Calculate Outputs outputs appear as
	| CountryID |
	| 1         |
	When I click OK on ODBC Test
	Then ODBC Outputs appear as
	| Mapped From | Mapped To                                 | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]] | 
	And ODBC Recordset Name equals "dbo_Pr_CitiesGetCountries"	

Scenario: Opening Saved workflow with ODBC tool
	Given I open workflow with ODBC connector
	And ODBC Source is Enabled
	And ODBC Source is localODBCTest
	And ODBC Action is Enabled
	And ODBC Action is "dbo.Pr_CitiesGetCountries"
	And ODBC Inputs are Enabled
	Then ODBC Inputs appear as
	| Input  | Value      | Empty is Null |
	| Prefix | [[Prefix]] | false         | 
	And Validate ODBC is Enabled
	Then ODBC Outputs appear as
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And ODBC Recordset Name equals "dbo_Pr_CitiesGetCountries"

Scenario: Change ODBC tool Source
	Given I open workflow with ODBC connector
	And ODBC Source is Enabled
	And ODBC Source is localODBCTest
	And ODBC Action is Enabled
	And ODBC Action is "dbo.Pr_CitiesGetCountries"
	And ODBC Inputs are Enabled
	Then ODBC Inputs appear as
	| Input | Value        | Empty is Null |
	| Prefix | [[Prefix]]  | false         | 
	And Validate ODBC is Enabled
	Then ODBC Outputs appear as
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And ODBC Recordset Name equals "dbo_Pr_CitiesGetCountries"
	When ODBC Source is changed to GreenPoint
	And ODBC Action is Enabled
	And ODBC Inputs are Disabled
	And ODBC Outputs are Disabled
	And Validate ODBC is Enabled

Scenario: Changing ODBC Tool Actions
	Given I open workflow with ODBC connector
	And ODBC Source is Enabled
	And ODBC Source is localODBCTest
	And ODBC Action is Enabled
	And ODBC Action is "dbo.Pr_CitiesGetCountries"
	And ODBC Inputs are Enabled
	Then ODBC Inputs appear as
	| Input | Value       | Empty is Null |
	| Prefix | [[Prefix]] | false         | 
	And Validate ODBC is Enabled
	Then ODBC Outputs appear as
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And ODBC Recordset Name equals "dbo_Pr_CitiesGetCountries"
	When ODBC Action is changed to "dbo.ImportOrder"
	And ODBC Inputs are Enabled
	And ODBC Inputs appear as
	| Input     | Value | Empty is Null |
	| ProductId |       | false         |	
	And Validate ODBC is Enabled	


Scenario: Change ODBC Tool Recordset Name
	Given I open workflow with ODBC connector
	And ODBC Source is Enabled
	And ODBC Source is localODBCTest
	And ODBC Action is Enabled
	And ODBC Action is "dbo.Pr_CitiesGetCountries"
	And ODBC Inputs are Enabled
	Then ODBC Inputs appear as
	| Input  | Value      | Empty is Null |
	| Prefix | [[Prefix]] | false         | 
	And Validate ODBC is Enabled
	Then ODBC Outputs appear as
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And ODBC Recordset Name equals "dbo_Pr_CitiesGetCountries"
	When ODBC Recordset Name is changed to "Pr_Cities"
	Then ODBC Outputs appear as
	| Mapped From | Mapped To                   |
	| CountryID   | [[Pr_Cities().CountryID]]   |
	| Description | [[Pr_Cities().Description]] |
	
