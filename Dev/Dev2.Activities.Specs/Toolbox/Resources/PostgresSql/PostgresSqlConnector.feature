Feature: PostgresSqlConnector
	In order to manage my database services
	As a Warewolf User
	I want to be shown the database service setup

Scenario: Creating PostgresSql Server Connector
	Given I drag a PostgresSql Server database connector
	When I select "DemoPostgres" as the source
	When I select "getemployees" as the action
	Then Test Inputs appear
	| Input     | Value | Empty is Null |
	| fname		|       | false         |
	Then Inputs is Enabled 
	Given I Enter a value as the input
	| fname |
	| Bill  |
	Then Test button is Enabled 
	Then Test button is Clicked 
	Then Test Connector and Calculate Outputs outputs appear as
	| name | salary | age |
	| Bill | 4200   | 45  |
	
