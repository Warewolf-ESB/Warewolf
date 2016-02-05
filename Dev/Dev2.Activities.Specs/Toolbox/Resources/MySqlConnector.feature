Feature: MySqlConnector
	In order to manage my database services
	As a Warewolf User
	I want to be shown the database service setup

# Creating MySQL Connector
# Opening Saved workflow with SQL Server tool
# Change Source on Existing tool
# Editing MySql Connector and Test Execution is unsuccesful
# Changing Actions

Scenario: Creating MySQL Connector
	Given I open New Workflow
	Then "New Workflow" tab is opened
	And "Source" is focused
	And "Source" is "Enabled"
	And "Action" is "Disabled"
	And "Inputs/Outputs" is "Disabled" 
	And "Mapping" is "Disabled" 
	When I Select "mysqlSource" as Source
	Then "Action" is "Enabled"
	When I select "new_procedure" as the action
	Then "Inputs/Outputs" is "Enabled" 
	And "Validate" is "Enabled"
	When I click "Validate"
	Then the "Test Connector and Calculate Outputs" window is opened
	And I click "Test"
	Then Outputs appear as
	| id        | value         |
	| 123123123 | aasdasdasdasd |
	| 1212      | ffff          |
	| 123123123 | aasdasdasdasd |
	| 1212      | ffff          |
	| 123123123 | aasdasdasdasd |
	| 1212      | ffff          |
	| 123123123 | aasdasdasdasd |
	| 1212      | ffff          |
	When I click "OK"
	And mappings are
	| Mapped From | Mapped To                 |
	| ID          | [[new_procedure().id]]    |
	| value       | [[new_procedure().value]] |  
	And "Recordset Name" equals "new_procedure"
	When the MySql Connector tool is executed
	And the execution has "No" error
	Then the debug output as 
	|                                  |
	| [[new_procedure().id]] = 1212    |
	| [[new_procedure().value]] = ffff |

Scenario: Opening Saved workflow with MySQL Connector tool
   Given I open MySql_workflow
	Then "MySql_workflow" tab is opened
	And "Source" is "Enabled"
	And "Source" equals "mysqlSource"
	And "Action" is "Enabled"
	And "Action" equals "new_procedure"
	And "Input/Output" is "Enabled"
	And input mappings are
	| Inputs | Value      | Empty is Null |
	And "Validate" is "Enabled"
	And "Mapping" is "Enabled"
	And mappings are
	| Mapped From | Mapped To                 |
	| id          | [[new_procedure().id]]    |
	| value       | [[new_procedure().value]] |
	And "Recordset Name" equals "new_procedure"

Scenario: Change Source on Existing tool
	Given I open MySql_workflow
	Then "MySql_workflow" tab is opened
	And "Source" is "Enabled"
	And "Source" equals "mysqlSource"
	And "Action" is "Enabled"
	And "Action" equals "new_procedure"
	And "Input/Output" is "Enabled"
	And input mappings are
	| Inputs | Value      | Empty is Null |
	And "Validate" is "Enabled"
	And "Mapping" is "Enabled"
	And mappings are
	| Mapped From | Mapped To                 |
	| id          | [[new_procedure().id]]    |
	| value       | [[new_procedure().value]] |
	And "Recordset Name" equals "new_procedure"
	When "Source" is changed from "mysqlSource" to "TestMysql"
	Then "Action" is "Enabled"
	And "Inputs/Outputs" is "Disabled" 
	And "Mapping" is "Disabled" 
	And "Validate" is "Disabled"

#Spec to be modified once test results section is included in tool window
@ignore
 Scenario: Editing MySql Connector and Test Execution is unsuccesful
   Given I open "MySql_workflow" service
   And "MySql_workflow" tab is opened
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
   And "Test" is "Enabled"   
   And "Save" is "Disabled"  
   When testing the action fails
   Then "4 Defaults and Mapping" is "Disabled" 
   And input mappings are
	| Inputs         | Default Value | Required Field | Empty is Null |
	And output mappings are
	| Output | Output Alias | Recordset Name      |
	And "Save" is "Disabled"


Scenario: Changing Actions
	Given I open MySql_workflow
	Then "MySql_workflow" tab is opened
	And "Source" is "Enabled"
	And "Source" equals "mysqlSource"
	And "Action" is "Enabled"
	And "Action" equals "new_procedure"
	And "Input/Output" is "Enabled"
	And "Input/Output" mappings are
	| Inputs | Value      | Empty is Null |
	And "Validate" is "Enabled"
	And "Mapping" is "Enabled"
	And mappings are
	| Mapped From | Mapped To                 |
	| id          | [[new_procedure().id]]    |
	| value       | [[new_procedure().value]] |
	When "Action" is changed from "new_procedure" to "getCountries"
	Then "Inputs/Outputs" is "Enabled" 
	And "Mapping" is "Disabled" 
	And "Validate" is "Enabled"
	And "Inputs/Outputs" appear as
	| Input | Value | Empty is Null |
	When I click "Validate"
	Then the "Test Connector and Calculate Outputs" window is opened
	And I click "Test"
	Then Outputs appear as
	| id        | value         |
	| 123123123 | aasdasdasdasd |
	| 1212      | ffff          |
	| 123123123 | aasdasdasdasd |
	| 1212      | ffff          |
	| 123123123 | aasdasdasdasd |
	| 1212      | ffff          |
	| 123123123 | aasdasdasdasd |
	| 1212      | ffff          |
	When I click "OK"
	And mappings are
	| Mapped From | Mapped To                 |
	| ID          | [[new_procedure().id]]    |
	| value       | [[new_procedure().value]] | 
	And "Recordset Name" equals "new_procedure"
	


Scenario: Change Recordset Name
	Given I open MySql_workflow
	Then "MySql_workflow" tab is opened
	And "Source" is "Enabled"
	And "Source" equals "mysqlSource"
	And "Action" is "Enabled"
	And "Action" equals "new_procedure"
	And "Input/Output" is "Enabled"
	And "Inputs/Outputs" appear as
	| Input | Value | Empty is Null |
	When I click "Validate"
	Then the "Test Connector and Calculate Outputs" window is opened
	And I click "Test"
	And "Mapping" is "Disabled" 
	When I test the action
	And Outputs appear as
	| id        | value         |
	| 123123123 | aasdasdasdasd |
	| 1212      | ffff          |
	| 123123123 | aasdasdasdasd |
	| 1212      | ffff          |
	| 123123123 | aasdasdasdasd |
	| 1212      | ffff          |
	| 123123123 | aasdasdasdasd |
	| 1212      | ffff          |
	When I click "OK"
	And mappings are
	| Mapped From | Mapped To                 |
	| ID          | [[new_procedure().id]]    |
	| value       | [[new_procedure().value]] | 
	And "Recordset Name" equals "new_procedure"
	When "Recordset Name" is changed from "new_procedure" to "New_Category"
	Then mappings are
	| Mapped From | Mapped To                |
	| id          | [[New_Category().id]]    |
	| value       | [[New_Category().value]] |


	
Scenario: Invalid Recordset name
	Given I open New Workflow
	Then "New Workflow" tab is opened
	And "Source" is focused
	And "Source" is "Enabled"
	And "Action" is "Disabled"
	And "Inputs/Outputs" is "Disabled" 
	And "Mapping" is "Disabled" 
	When I Select "mysqlSource" as Source
	Then "Action" is "Enabled"
	When I select "new_procedure" as the action
	Then "Inputs/Outputs" is "Enabled" 
	And "Validate" is "Enabled"
	And "Inputs/Outputs" appear as
	| Input | Value | Empty is Null |
	When I click "Validate"
	Then the "Test Connector and Calculate Outputs" window is opened
	And I click "Test"
	And "Mapping" is "Disabled" 
	When I test the action
	And Outputs appear as
	| id        | value         |
	| 123123123 | aasdasdasdasd |
	| 1212      | ffff          |
	| 123123123 | aasdasdasdasd |
	| 1212      | ffff          |
	| 123123123 | aasdasdasdasd |
	| 1212      | ffff          |
	| 123123123 | aasdasdasdasd |
	| 1212      | ffff          |
	When I click "OK"
	And mappings are
	| Mapped From | Mapped To                 |
	| ID          | [[new_procedure().id]]    |
	| value       | [[new_procedure().value]] | 
	And "Recordset Name" equals "new_procedure"
	When "Recordset Name" is changed from "new_procedure" to "#$New_Category"
	When the MySql Connector tool is executed
	And the execution has "AN" error
	Then the debug output as 
	|                                          |
	| Error : input must be recordset or value |



@ignore
Scenario: No Action to be loaded Error
	Given I have a workflow "NoStoredProceedure"
	And "NoStoredProceedure" contains "Testing/MySql/MySQLEmpty" from server "localhost" with mapping as
	     | Input Data or [[Variable]] | Parameter | Empty is Null |
	When "NoStoredProceedure" is executed
	Then the workflow execution has "An" error
	And the 'Testing/MySql/MySQLEmpty' in Workflow 'NoStoredProceedure' debug outputs as
	  |                                                                  |
	  | Error: The selected database does not contain actions to perform |

@ignore
Scenario: Passing Null Input value
	Given I have a workflow "PassingNullValue"
	And "PassingNullValue" contains "Acceptance Testing Resources/mysqlSource" from server "localhost" with mapping as
	     | Input Data or [[Variable]] | Parameter | Empty is Null |
	     | [[value]]                  | a         | True          |
	When "PassingNullValue" is executed
	Then the workflow execution has "An" error
	And the 'Acceptance Testing Resources/mysqlSource' in Workflow 'PassingNullValue' debug outputs as
	  |                                       |
	  | Error: Scalar value { value } is NULL |

@ignore
Scenario: Mapped To Recordsets incorrect
	Given I have a workflow "WillAlwaysError"
	And "WillAlwaysError" contains "Acceptance Testing Resources/mysqlSource" from server "localhost" with mapping as
	     | Input Data or [[Variable]] | Parameter | Empty is Null |
	And And "WillAlwaysError" contains "Acceptance Testing Resources/mysqlSource" from server "localhost" with Mapping To as
	| Mapped From | Mapped To               |
	| 1           | [[willalwayserror().1]] |
	When "WillAlwaysError" is executed
	Then the workflow execution has "An" error
	And the 'Acceptance Testing Resources/mysqlSource' in Workflow 'WillAlwaysError' debug outputs as
	  |                                                                    |
	  | [[willalwayserror()]]: Recordset must contain one or more field(s) |


@ignore
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
	Given I have a workflow "RenameRecordsetIncorrectly"
	And "RenameRecordsetIncorrectly" contains "Acceptance Testing Resources/mysqlSource" from server "localhost" with mapping as
	     | Input Data or [[Variable]] | Parameter | Empty is Null |
	When "RenameRecordsetIncorrectly" is executed
	Then the workflow execution has "An" error
	And the 'Acceptance Testing Resources/mysqlSource' in Workflow 'RenameRecordsetIncorrectly' debug outputs as
	  |                                                              |
	  | [[getCountrie.s().id]] : Recordset name has invalid format   |
	  | [[getCountrie.s().value]]: Recordset name has invalid format |


#Wolf-1262
@ignore
Scenario: backward Compatiblity
	Given I have a workflow "MySQLMigration"
	And "MySQLMigration" contains "MySQLDATA" from server "localhost" with mapping as
      | Input to Service | From Variable | Output from Service                | To Variable                    |
      |                  |               | [[dbo_GetCountries().CountryID]]   | dbo_GetCountries().CountryID   |
      |                  |               | [[dbo_GetCountries().Description]] | dbo_GetCountries().Description |
	When "MySQLMigration" is executed
	Then the workflow execution has "NO" error