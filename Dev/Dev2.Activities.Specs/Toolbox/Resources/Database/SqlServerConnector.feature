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


#----------
@ignore
Scenario: No Action to be loaded Error
	Given I have a workflow "NoStoredProceedureToLoad"
	And "NoStoredProceedureToLoad" contains "Testing/SQL/NoSqlStoredProceedure" from server "localhost" with mapping as
	     | Input Data or [[Variable]] | Parameter | Empty is Null |
	When "NoStoredProceedureToLoad" is executed
	Then the workflow execution has "An" error
	And the 'Testing/SQL/NoSqlStoredProceedure' in Workflow 'NoStoredProceedureToLoad' debug outputs as
	  |                                                                  |
	  | Error: The selected database does not contain actions to perform |

@ignore
Scenario: Passing Null Input values
	Given I have a workflow "PassingNullInputValue"
	And "PassingNullInputValue" contains "Acceptance Testing Resources/GreenPoint" from server "localhost" with mapping as
	     | Input Data or [[Variable]] | Parameter | Empty is Null |
	     | [[value]]                  | a         | True          |
	When "PassingNullInputValue" is executed
	Then the workflow execution has "An" error
	And the 'Acceptance Testing Resources/GreenPoint' in Workflow 'PassingNullInputValue' debug outputs as
	  |                                       |
	  | Error: Scalar value { value } is NULL |

@ignore
Scenario: Mapped To Recordsets incorrect
	Given I have a workflow "BadSqlParameterName"
	And "BadSqlParameterName" contains "Acceptance Testing Resources/GreenPoint" from server "localhost" with mapping as
	     | Input Data or [[Variable]] | Parameter | Empty is Null |
	     |                            | a         | True          |
	And And "BadSqlParameterName" contains "Acceptance Testing Resources/GreenPoint" from server "localhost" with Mapping To as
	| Mapped From      | Mapped To                                |
	| id               | [[dbo_leon bob proc().id]]               |
	| some column Name | [[dbo_leon bob proc().some column Name]] |
	When "BadSqlParameterName" is executed
	Then the workflow execution has "An" error
	And the 'Acceptance Testing Resources/GreenPoint' in Workflow 'BadSqlParameterName' debug outputs as
	  |                               |
	  | Error: Sql Error: parse error |


@ignore
#Needs Work
Scenario: Parameter not found in the collection
	Given I have a workflow "BadMySqlParameterName"
	And "BadMySqlParameterName" contains "Testing/MySql/MySqlParameters" from server "localhost" with mapping as
	     | Input Data or [[Variable]] | Parameter      | Empty is Null |
	     |                            | `p_startswith` | false         |
	When "BadMySqlParameterName" is executed
	Then the workflow execution has "An" error
	And the 'Testing/MySql/MySqlParameters' in Workflow 'BadMySqlParameterName' debug outputs as
	  |                                                      |
	  | Parameter 'p_startswith' not found in the collection |


@ignore
Scenario: Recordset has invalid character
	Given I have a workflow "MappingHasIncorrectCharacter"
	And "MappingHasIncorrectCharacter" contains "Acceptance Testing Resources/GreenPoint" from server "localhost" with mapping as
	     | Input Data or [[Variable]] | Parameter | Empty is Null |
	     | 1                          | charValue | True          |
	When "MappingHasIncorrectCharacter" is executed
	Then the workflow execution has "An" error
	And the 'Acceptance Testing Resources/GreenPoint' in Workflow 'MappingHasIncorrectCharacter' debug outputs as
	  |                                                                    |
	  | [[dbo_ConvertTo,Int().result]] : Recordset name has invalid format |
	  


#Wolf-1262
@ignore
Scenario: backward Compatiblity
	Given I have a workflow "DataMigration"
	And "DataMigration" contains "DataCon" from server "localhost" with mapping as
      | Input to Service | From Variable | Output from Service                | To Variable                    |
      | [[ProductId]]    | productId     | [[dbo_GetCountries().CountryID]]   | dbo_GetCountries().CountryID   |
      |                  |               | [[dbo_GetCountries().Description]] | dbo_GetCountries().Description |
	When "DataMigration" is executed
	Then the workflow execution has "NO" error
