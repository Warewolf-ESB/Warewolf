@Database
Feature: OracleServerConnector
	In order to manage my database services
	As a Warewolf User
	I want to be shown the database service setup

@CreatingOracleToolInstance
Scenario: Creating a new Oracle Tool Instance
	Given I open New Workflow containing an Oracle Connector
	And I drag a Oracle Server database connector
	And Oracle Source is Enabled
	And Oracle Action is Disabled
	And Oracle Inputs are Disabled
	And Oracle Outputs are Disabled
	When I Selected GreenPoint as Source
	Then Action is Enable
	When I select HR.TESTPROC9 as the Oracle action
	Then Inputs is Enable
	And Oracle Inputs appear as 
	| Input   | Value   | Empty is Null |
	| EID     | [[EID]] | false         |
	And Validate is Enable
	When I click Oracle Validate
	And Test Oracle Inputs appear as
	| EID	|
	| 100   |
	When I click Oracle Tests
	When I click Oracle OK
	Then Oracle Outputs appear as
	| Mapped From | Mapped To                  | 
	| Column1     | [[HR_TESTPROC9().Column1]] | 
	Then Oracle Recordset Name equals "HR_TESTPROC9"	

Scenario: Opening Saved workflow with Existing Oracle tool
   Given I open workflow with Oracle connector
	And Oracle Source is Enabled
	And Oracle Source is "testingDBSrc"
	And Oracle Action is "dbo.Pr_CitiesGetCountries"
	And Inputs is Enable
	Then Oracle Inputs appear as
	| Input  | Value      | Empty is Null |
	| Prefix | [[Prefix]] | false         |
	And Validate is Enable
	Then Oracle Outputs appear as
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Oracle Recordset Name equals "dbo_Pr_CitiesGetCountries"

Scenario: Change Source on Existing Oracle Tool
	Given I open workflow with Oracle connector
	And Oracle Source is Enabled
	And Oracle Source is "testingDBSrc"
	And Oracle Action is "dbo.Pr_CitiesGetCountries"
	And Inputs is Enable
	Then Oracle Inputs appear as
	| Input  | Value      | Empty is Null |
	| Prefix | [[Prefix]] | false         |
	And Validate is Enable
	Then Oracle Outputs appear as
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Oracle Recordset Name equals "dbo_Pr_CitiesGetCountries"
	When Oracle Source is changed from to GreenPoint
	Then Action is Enable
	And Inputs is Enable
	And Validate is Enable

Scenario: Changing Actions on Existing Oracle Tool
	Given I open workflow with Oracle connector
	And Oracle Source is Enabled
	And Oracle Source is "testingDBSrc"	
	And Oracle Action is "dbo.Pr_CitiesGetCountries"
	And Inputs is Enable
	Then Oracle Inputs appear as
	| Input  | Value      | Empty is Null |
	| Prefix | [[Prefix]] | false         |
	And Validate is Enable
	Then Oracle Outputs appear as
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Oracle Recordset Name equals "dbo_Pr_CitiesGetCountries"
	When Oracle Action is changed from to dbo.ImportOrder
	And Inputs is Enable
	And Oracle Inputs appear as
	| Input     | Value         | Empty is Null |
	| ProductId | [[ProductId]] | false         |
	And Validate is Enable

Scenario: Change Recordset Name on Existing Oracle Tool
	Given I open workflow with Oracle connector
	And Oracle Source is Enabled
	And Oracle Source is "testingDBSrc"	
	And Oracle Action is "dbo.Pr_CitiesGetCountries"
	And Inputs is Enable
	Then Oracle Inputs appear as
	| Input  | Value      | Empty is Null |
	| Prefix | [[Prefix]] | false         |
	And Validate is Enable
	Then Oracle Outputs appear as
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Oracle Recordset Name equals "dbo_Pr_CitiesGetCountries"
	When Oracle Recordset Name is changed to "Pr_Cities"
	Then Oracle Outputs appear as
	| Mapped From | Mapped To                   |
	| CountryID   | [[Pr_Cities().CountryID]]   |
	| Description | [[Pr_Cities().Description]] |
	