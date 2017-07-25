@Database
Feature: PostgresSqlConnector
	In order to manage my database services
	As a Warewolf User
	I want to be shown the database service setup

Scenario: Creating PostgresSql Server Connector
	Given I drag a PostgresSql Server database connector
	When I select "DemoPostgres" as the source
	When I select "getemployees" as the action
	Then Test Inputs appear
	| Input | Value | Empty is Null |
	| fname |       | false         |
	Then Inputs is Enabled for postgresSql
	Given I Enter a value as the input
	| fname |
	| Bill  |
	Then Test button is Enabled 
	Then button is clicked
	| name | salary | age |
	| Bill | 4200   | 45  |
@OpeningSavedWorkflowWithPostgresServerTool
Scenario: Opening Saved workflow with Postgres Server tool
	Given I Open workflow with PostgreSql connector
	And Source Is Enable
	And Source Is "postgressql"
	And Action Is Enable
	And Action Is "getemployees"
	And Inputs Is Enable
	Then Inputs appear As
	| Input | Value     | Empty is Null |
	| fname | [[fname]] | false         |
	And Validate Is Enable

@ChangeTheSourceOnExistingPostgresql	
Scenario: Change the source on existing postgresql tool
	Given I Open workflow with PostgreSql connector
	And Source Is Enable
	And Source Is "postgressql"
	And Action Is Enable
	And Action Is "getemployees"
	And Inputs Is Enable
	Then Inputs appear As
	| Input | Value     | Empty is Null |
	| fname | [[fname]] | false         |
	And Validate Is Enable

@ChangeTheActionOnExistingPostgresql
Scenario: Change the action on existing postgresql tool
	Given I Open workflow with PostgreSql connector
	And Source Is Enable
	And Source Is "postgressql"
	And Action Is Enable
	And Action Is "getemployees"
	And Inputs Is Enable
	Then Inputs appear As
	| Input | Value     | Empty is Null |
	| fname | [[fname]] | false         |  
	And Validate Is Enable

Scenario: Change the recordset on existing postgresql tool
	Given I Open workflow with PostgreSql connector
	And Source Is Enable
	And Source Is "postgressql"
	And Action Is Enable
	And Action Is "getemployees"
	And Inputs Is Enable
	Then Inputs appear As
	| Input | Value     | Empty is Null |
	| fname | [[fname]] | false         |
	And Validate Is Enable