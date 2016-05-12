Feature: OracleServerConnector
	In order to manage my database services
	As a Warewolf User
	I want to be shown the database service setup


Scenario: Creating Oracle Server Connector
	Given I open New Workflow
	And I drag a Oracle Server database connector
	And Source is Enabled
	And Action is Disable
	And Inputs is Disable
	And Outputs is Disable
	And Validate is Disable
	When I Selected "GreenPoint" as Source
	Then Action is Enabled
	When I selected "HR.TESTPROC9" as the action
	Then Inputs is Enabled 
	And Inputs appear as 
	| Input     | Value | Empty is Null |
	| EID		|       | false         |
	And Validate is Enabled
	When I click Validat
	Then the Test Connector and Calculate Outputs window is open
	And Test Inputs appear as
	| EID		 |
	| 100        |
	When I click Tests
	Then Test Connector and Calculate Outputs outputs appear as
	| Column1 |
	| 1       |
	When I click OKay
	Then Outputs appear as
	| Mapped From | Mapped To                     | 
	| Column1     | [[HR_TESTPROC9().Column1]] | 

	And Recordset Name equals "HR_TESTPROC9"	

Scenario: Opening Saved workflow with Oracle Server tool
   Given I open workflow with Oracle connector
	And Source is Enabled
	And Source is "testingDBSrc"
	And Action is Enabled
	And Action is "dbo.Pr_CitiesGetCountries"
	And Inputs is Enabled
	Then Inputs appear as
	| Input | Value | Empty is Null |
	| Prefix | [[Prefix]]    | false         | 
	And Validate is Enabled
	Then Outputs appear as
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Recordset Name equals "dbo_Pr_CitiesGetCountries"

Scenario: Change Oracle Source on Existing tool
	Given I open workflow with Oracle connector
	And Source is Enabled
	And Source is "testingDBSrc"
	And Action is Enabled
	And Action is "dbo.Pr_CitiesGetCountries"
	And Inputs is Enabled
	Then Inputs appear as
	| Input | Value | Empty is Null |
	| Prefix | [[Prefix]]    | false         | 
	And Validate is Enabled
	Then Outputs appear as
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Recordset Name equals "dbo_Pr_CitiesGetCountries"
	When Source is changed from to "GreenPoint"
	Then Action is Enabled
	And Inputs is Disable
	And Outputs is Disable
	And Validate is Enabled


Scenario: Changing Oracle Actions
	Given I open workflow with Oracle connector
	And Source is Enabled
	And Source is "testingDBSrc"
	And Action is Enabled
	And Action is "dbo.Pr_CitiesGetCountries"
	And Inputs is Enabled
	Then Inputs appear as
	| Input | Value | Empty is Null |
	| Prefix | [[Prefix]]    | false         | 
	And Validate is Enabled
	Then Outputs appear as
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Recordset Name equals "dbo_Pr_CitiesGetCountries"
	When Action is changed from to "dbo.ImportOrder"
	And Inputs is Enabled
	And Inputs appear as
	| Input    | Value | Empty is Null |
	| ProductId |               | false         |	
	And Validate is Enabled	


Scenario: Change Oracle Recordset Name
	Given I open workflow with Oracle connector
	And Source is Enabled
	And Source is "testingDBSrc"
	And Action is Enabled
	And Action is "dbo.Pr_CitiesGetCountries"
	And Inputs is Enabled
	Then Inputs appear as
	| Input | Value | Empty is Null |
	| Prefix | [[Prefix]]    | false         | 
	And Validate is Enabled
	Then Outputs appear as
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Recordset Name equals "dbo_Pr_CitiesGetCountries"
	When Recordset Name is changed to "Pr_Cities"
	Then Outputs appear as
	| Mapped From | Mapped To                   |
	| CountryID   | [[Pr_Cities().CountryID]]   |
	| Description | [[Pr_Cities().Description]] |
	