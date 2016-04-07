Feature: SqlServerConnector
	In order to manage my database services
	As a Warewolf User
	I want to be shown the database service setup

Scenario: Opening Saved workflow with SQL Server tool
   Given I open workflow with database connector
	And Source is Enabled
	And Source is "testingDBSrc"
	And Action is Enabled
	And Action is "dbo.Pr_CitiesGetCountries"
	And Inputs is Enabled
	And Inputs appear as
	| Input | Value      | Empty is Null |
	| Prefix | [[Prefix]] | false         |
	And Validate is Enabled
	Then Outputs appear as
	| Mapped From | Mapped To                                   | 
	| CountryID   | [[dbo_Pr_CitiesGetCountries().CountryID]]   |
	| Description | [[dbo_Pr_CitiesGetCountries().Description]] |
	And Recordset Name equals "dbo_Pr_CitiesGetCountries"

Scenario: Change Source on Existing tool
	Given I open workflow with database connector
	And Source is Enabled
	And Source is "testingDBSrc"
	And Action is Enabled
	And Action is "dbo.Pr_CitiesGetCountries"
	And Inputs is Enabled
	And Inputs appear as
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
	And Inputs is Disabled 
	And Outputs is Disabled
	And Validate is Enabled 

#Spec to be modified once test results section is included in tool window
@ignore
 Scenario: Editing DB Service and Test Execution is unsuccesful
   Given I open workflow with database connector
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
	Given I open workflow with database connector
	And Source is Enabled
	And Source is "testingDBSrc"
	And Action is Enabled
	And Action is "dbo.Pr_CitiesGetCountries"
	And Inputs is Enabled
	And Inputs appear as
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

Scenario: Change Recordset Name
	Given I open workflow with database connector
	And Source is Enabled
	And Source is "testingDBSrc"
	And Action is Enabled
	And Action is "dbo.Pr_CitiesGetCountries"
	And Inputs is Enabled
	And Inputs appear as
	| Input | Value      | Empty is Null |
	| Prefix | [[Prefix]] | false         |
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


