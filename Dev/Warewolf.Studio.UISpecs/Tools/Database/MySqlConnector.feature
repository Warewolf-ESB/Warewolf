Feature: MySqlConnector
	In order to manage my database services
	As a Warewolf User
	I want to be shown the database service setup
	
@NeedsBlankWorkflow
Scenario: Drag toolbox MySql Tool onto a new workflow creates MySql tool with large view on the design surface
	When I "Drag_Toolbox_MySql_Database_Onto_DesignSurface"
	Then I "Assert_Mysql_Database_Large_View_Exists_OnDesignSurface"

#@NeedsMySqlToolLargeViewOnTheDesignSurface
#Scenario: Double Clicking MySql Tool Large View on the Design Surface Collapses it to Small View
	When I "Open_MySql_Database_Tool_Small_View"
	Then I "Assert_Mysql_Database_Large_View_Exists_OnDesignSurface"

# Creating MySQL Connector
# Opening Saved workflow with SQL Server tool
# Change Source on Existing tool
# Editing MySql Connector and Test Execution is unsuccesful
# Changing Actions

@ignore
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
	When I select new_procedure as the action
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

@ignore
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

@ignore
Scenario: Change MySQL Source on Existing tool
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

@ignore
#Spec to be modified once test results section is included in tool window
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

@ignore
Scenario: Changing MySQL Actions
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

@ignore
Scenario: Change MySQL Recordset Name
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

@ignore
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
	When I select new_procedure as the action
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


