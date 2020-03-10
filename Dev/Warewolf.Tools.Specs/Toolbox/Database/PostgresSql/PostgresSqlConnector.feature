Feature: PostgresSqlConnector
	In order to manage my database services
	As a Warewolf User
	I want to be shown the database service setup

@Database
Scenario: Creating PostgresSql Connector
	Given I drag a PostgresSql Server database connector
	When I select DemoPostgres as the source
	When I select getemployees as the action
	Then Test PostgresSql Inputs appear As
	| Input | Value | Empty is Null |
	| fname |       | false         |
	Then Inputs Are Enabled for PostgresSql
	Given I Enter a value as the input
	| fname |
	| Bill  |
	Then Test button is Enabled 
	Then button is clicked
	| name | salary | age |
	| Bill | 4200   | 45  |

@Database
Scenario: Opening Saved workflow with PostgresSql tool
	Given I Open workflow with PostgreSql connector
	And PostgresSql Source Is Enabled
	And PostgresSql Source Is "postgressql"
	And PostgresSql Action Is Enabled
	And PostgresSql Action Is "getemployees"
	And PostgresSql Inputs Are Enabled
	Then PostgresSql Inputs appear As
	| Input | Value     | Empty is Null |
	| fname | [[fname]] | false         |
	And Validate PostgresSql Is Enabled

@Database
Scenario: Change the source on existing PostgresSql tool
	Given I Open workflow with PostgreSql connector
	And PostgresSql Source Is Enabled
	And PostgresSql Source Is "postgressql"
	And PostgresSql Action Is Enabled
	And PostgresSql Action Is "getemployees"
	And PostgresSql Inputs Are Enabled
	Then PostgresSql Inputs appear As
	| Input | Value     | Empty is Null |
	| fname | [[fname]] | false         |
	And Validate PostgresSql Is Enabled

@Database
Scenario: Change the action on existing PostgresSql tool
	Given I Open workflow with PostgreSql connector
	And PostgresSql Source Is Enabled
	And PostgresSql Source Is "postgressql"
	And PostgresSql Action Is Enabled
	And PostgresSql Action Is "getemployees"
	And PostgresSql Inputs Are Enabled
	Then PostgresSql Inputs appear As
	| Input | Value     | Empty is Null |
	| fname | [[fname]] | false         |  
	And Validate PostgresSql Is Enabled

@Database
Scenario: Change the recordset on existing PostgresSql tool
	Given I Open workflow with PostgreSql connector
	And PostgresSql Source Is Enabled
	And PostgresSql Source Is "postgressql"
	And PostgresSql Action Is Enabled
	And PostgresSql Action Is "getemployees"
	And PostgresSql Inputs Are Enabled
	Then PostgresSql Inputs appear As
	| Input | Value     | Empty is Null |
	| fname | [[fname]] | false         |
	And Validate PostgresSql Is Enabled

@SqlDatabaseBroker
Scenario: Execute Postgres Server With Timeout
	Given this test depends on a remote Postgres database container
    And I have workflow "PostgreWorkflowForTimeout" with "PostgresActivity" Postgres database connector
    And Postgres Server Source is Enabled
    And I Select "NewPostgresSource" as Postgres Source for "PostgresActivity"
    And I Select "get_countries_delayed" as Postgres Server Action for "PostgresActivity"
	And Postgres Command Timeout is "30" milliseconds for "PostgresActivity"
	And Validate Postgres Server is Enabled
    And I click Postgres Generate Outputs
    And I click Postgres Test 
    Then Postgres Server Outputs appear as
	| Mapped From | Mapped To                |
	| id          | [[get_countries_delayed().id]]   |
	| name        | [[get_countries_delayed().name]] |
	| code        | [[get_countries_delayed().code]] |
	And Postgres Server Recordset Name equals "get_countries_delayed"
	And Postgres Command Timeout is "5" milliseconds for "PostgresActivity"
	When Postgres Workflow "PostgreWorkflowForTimeout" containing dbTool is executed
    And the workflow "PostgreWorkflowForTimeout" execution has "AN" error "statement timeout"
	And the workflow "PostgreWorkflowForTimeout" error does not contain "NewLine"
