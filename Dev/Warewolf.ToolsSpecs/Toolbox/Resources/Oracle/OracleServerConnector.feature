Feature: OracleServerConnector
	In order to manage my database services
	As a Warewolf User
	I want to be shown the database service setup


Scenario: Creating Oracle Server Connector
	Given I open New Workflow
	And I drag a Oracle Server database connector
	And Source is Enable
	And Action is Disable
	And Inputs is Disable
	And Outputs is Disable
	And Validate is Disable
	When I Selected "GreenPoint" as Source
	Then Action is Enable
	When I selected "HR.TESTPROC9" az the action
	Then Inputs is Enable 
	And Inputs appear az 
	| Input     | Value | Empty is Null |
	| EID		|       | false         |
	And Validate is Enable
	When I click Validat
	Then the Test Connector and Calculate Outputs window is open
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

	And Recordset Name equalz "HR_TESTPROC9"	

Scenario: Opening Saved workflow with Oracle Server tool
   Given I open workflow with Oracle connector
	And Source is Enable
	And Source iss "testingDBSrc"
	And Action is Enable
	And Action iss "dbo.Pr_CitiesGetCountries"
	And Inputs is Enable
	Then Inputs appear az
	| Input | Value | Empty is Null |
	| Prefix | [[Prefix]]    | false         | 
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
	And Action is Enable
	And Action iss "dbo.Pr_CitiesGetCountries"
	And Inputs is Enable
	Then Inputs appear az
	| Input | Value | Empty is Null |
	| Prefix | [[Prefix]]    | false         | 
	And Validate is Enable
	Then Outputs appear az
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Recordset Name equalz "dbo_Pr_CitiesGetCountries"
	When Source iz changed from to "GreenPoint"
	Then Action is Enable
	And Inputs is Disable
	And Outputs is Disable
	And Validate is Enable

#Spec to be modified once test results section is included in tool window
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
	Given I open workflow with Oracle connector
	And Source is Enable
	And Source iss "testingDBSrc"
	And Action is Enable
	And Action iss "dbo.Pr_CitiesGetCountries"
	And Inputs is Enable
	Then Inputs appear az
	| Input | Value | Empty is Null |
	| Prefix | [[Prefix]]    | false         | 
	And Validate is Enable
	Then Outputs appear az
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Recordset Name equalz "dbo_Pr_CitiesGetCountries"
	When Action iz changed from to "dbo.ImportOrder"
	And Inputs is Enable
	And Inputs appear az
	| Input    | Value | Empty is Null |
	| ProductId |               | false         |	
	And Validate is Enable	



Scenario: Change Recordset Name
	Given I open workflow with Oracle connector
	And Source is Enable
	And Source iss "testingDBSrc"
	And Action is Enable
	And Action iss "dbo.Pr_CitiesGetCountries"
	And Inputs is Enable
	Then Inputs appear az
	| Input | Value | Empty is Null |
	| Prefix | [[Prefix]]    | false         | 
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
	