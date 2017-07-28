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