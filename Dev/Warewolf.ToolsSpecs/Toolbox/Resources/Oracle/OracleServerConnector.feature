Feature: OracleServerConnector
	In order to manage my database services
	As a Warewolf User
	I want to be shown the database service setup


Scenario: Creating Oracle Server Connector
	Given I open New oracleDb Workflow
	And I drag a Oracle Server database connector
	And Source is Enable
	And Action is Disable
	And Inputs is Disable
	And Outputs is Disable
	When I Selected "GreenPoint" as Source
	Then Action is Enable
	When I selected "HR.TESTPROC9" az the action
	Then Inputs is Enable 
	And Inputs appear az 
	| Input     | Value | Empty is Null |
	| EID		|  [[EID]]     | false         |
	And Validate is Enable
	When I click Validat
	And Test Inputs appear az
	| EID		 |
	| 100        |
	When I click Testz
	Then Test Connector and Calculate Outputs outputs appear az
	| Column1 |
	| 1       |
	When I click OKay
	Then Outputs appear az
	| Mapped From | Mapped To                     | 
	| Column1     | [[HR_TESTPROC9().Column1]] | 

	Then Recordset Name equalz "HR_TESTPROC9"	

Scenario: Opening Saved workflow with Oracle Server tool
   Given I open workflow with Oracle connector
	And Source is Enable
	And Source iss "testingDBSrc"
	And Action iss "dbo.Pr_CitiesGetCountries"
	And Inputs is Enable
	Then Inputs appear az
	| Input  | Value      | Empty is Null |
	| Prefix | [[Prefix]] | false         |
	And Validate is Enable
	Then Outputs appear az
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Recordset Name equalz "dbo_Pr_CitiesGetCountries"

Scenario: Change Source on Existing tool
	Given I open workflow with Oracle connector
	And Source is Enable
	And Source iss "testingDBSrc"
	#And Action is Enable
	And Action iss "dbo.Pr_CitiesGetCountries"
	And Inputs is Enable
	Then Inputs appear az
	| Input  | Value      | Empty is Null |
	| Prefix | [[Prefix]] | false         |
	And Validate is Enable
	Then Outputs appear az
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Recordset Name equalz "dbo_Pr_CitiesGetCountries"
	When Source iz changed from to "GreenPoint"
	Then Action is Enable
	And Inputs is Enable
	And Validate is Enable

Scenario: Changing Actions
	Given I open workflow with Oracle connector
	And Source is Enable
	And Source iss "testingDBSrc"	
	And Action iss "dbo.Pr_CitiesGetCountries"
	And Inputs is Enable
	Then Inputs appear az
	| Input  | Value      | Empty is Null |
	| Prefix | [[Prefix]] | false         |
	And Validate is Enable
	Then Outputs appear az
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Recordset Name equalz "dbo_Pr_CitiesGetCountries"
	When Action iz changed from to "dbo.ImportOrder"
	And Inputs is Enable
	And Inputs appear az
	| Input     | Value         | Empty is Null |
	| ProductId | [[ProductId]] | false         |
	And Validate is Enable

Scenario: Change Recordset Name
	Given I open workflow with Oracle connector
	And Source is Enable
	And Source iss "testingDBSrc"	
	And Action iss "dbo.Pr_CitiesGetCountries"
	And Inputs is Enable
	Then Inputs appear az
	| Input  | Value      | Empty is Null |
	| Prefix | [[Prefix]] | false         |
	And Validate is Enable
	Then Outputs appear az
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Recordset Name equalz "dbo_Pr_CitiesGetCountries"
	When Recordset Name iz changed to "Pr_Cities"
	Then Outputs appear az
	| Mapped From | Mapped To                   |
	| CountryID   | [[Pr_Cities().CountryID]]   |
	| Description | [[Pr_Cities().Description]] |
	