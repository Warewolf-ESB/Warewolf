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
   Given I open "Wolf-860"
	And Source is Enabled
	And Source is "localOracleTest"
	And Action is Enabled
	And Action is dbo.Pr_CitiesGetCountries
	And Inputs is Enabled
	And Inputs appear az
	| Inputs | Default Value | Empty is Null |
	| Prefix | [[Prefix]]    | false         | 
	And Validate is Enabled
	And Mapping is Enabled
	Then Outputs appear as
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Recordset Name equals "dbo_Pr_CitiesGetCountries"

Scenario: Change Source on Existing tool
	Given I open Wolf-860
	Then "Wolf-860" tab is opened
	And "Source" is "Enabled"
	And "Source" equals "localOracleTest"
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
	When "Source" is changed from "localOracleTest" to "GreenPoint"
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
	And "Source" equals "localOracleTest"
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
	And inputs appear az
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
	And "Source" equals "localOracleTest"
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
	