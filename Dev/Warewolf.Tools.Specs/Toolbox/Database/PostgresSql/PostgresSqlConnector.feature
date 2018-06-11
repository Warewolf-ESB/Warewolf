@Database
Feature: PostgresSqlConnector
	In order to manage my database services
	As a Warewolf User
	I want to be shown the database service setup

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

@OpeningSavedWorkflowWithPostgresServerTool
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

@ChangeTheSourceOnExistingPostgresql	
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

@ChangeTheActionOnExistingPostgresql
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

@ExecutePostgresServerWithTimeout		
Scenario: Execute Postgres Server With Timeout
    Given I have workflow "PostgreWorkflowForTimeout" with "PostgresActivity" Postgres database connector
    And Postgres Server Source is Enabled
    And I Select "NewPostgresSource" as Postgres Source for "PostgresActivity"
    And I Select "get_countries" as Postgres Server Action for "PostgresActivity"
    And Postgres Server Inputs Are Enabled	
	And Validate Postgres Server is Enabled
    And I click Postgres Generate Outputs
    And I click Postgres Test 
    Then Postgres Server Outputs appear as
	| Mapped From | Mapped To                |
	| id          | [[get_countries().id]]   |
	| name        | [[get_countries().name]] |
	| code        | [[get_countries().code]] |
	And Postgres Server Recordset Name equals "get_countries"
	And Postgres input variable "[[countrynamecontains]]" is ""
	When Postgres Workflow "PostgreWorkflowForTimeout" containing dbTool is executed
    And the workflow "PostgreWorkflowForTimeout" execution has "NO" error
