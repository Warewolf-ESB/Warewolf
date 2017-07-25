@Database
Feature: MySqlConnector
	In order to manage my database services
	As a Warewolf User
	I want to be shown the database service setup

Scenario: Creating mysql server connector
	Given I drag in mysql connector tool
	And Source is enabled for mysql connector tool
	And Action is Not enabled for mysql connector tool
	And Input is Not eabled for mysql connector tool
	Then I select "DemoSqlsource" Source on mysql connector tool
	And Action is Enabled on mysql connector tool
	And Input is Not enabled for mysql connector tool
	And I select "someAction" Action for mysql connector tool
	And Input is enabled for mysql connector tool
	And Inputs are "SomeInput" for mysql connector tool
	Then I click validate on mysql connector tool
	When I click Test on mysql connector tool
	And The Connector and Calculate Outputs appear for mysql connector tool
	Then I click Okay on mysql connector tool
	And The recordset name appear as "SomeRecordSet" on mysql connector tool
	
Scenario: Opening exisitng mysql server connector tool
	Given I open an existing mysql connector tool
	And Source is enabled and set to "DemoSqlsource" on mysql connector tool
	And Action is Enabled and set to "someAction" on mysql connector tool	
	And Input is enabled for existing mysql connector tool
	And Inputs are "SomeInput" for mysql connector tool
	Then I click validate on mysql connector tool
	And The outputs appear as "DemoSqlsource" on mysql connector tool

Scenario: Change the source on existing mysql server connector tool
	Given I open an existing mysql connector tool
	And Source is enabled and set to "DemoSqlsource" on mysql connector tool
	And Action is Enabled and set to "someAction" on mysql connector tool
	And Input is enabled for existing mysql connector tool
	And Inputs are "SomeInput" for mysql connector tool
	Then I select "AnotherSqlSource" Source on mysql connector tool
	And Action on mysql connector tool is null
	And Inputs on mysql connector tool is null
	Then I click validate on mysql connector tool

Scenario: Change the action on existing mysql server connector tool
	Given I open an existing mysql connector tool
	And Source is enabled and set to "DemoSqlsource" on mysql connector tool
	And Action is Enabled and set to "someAction" on mysql connector tool
	And Input is enabled for existing mysql connector tool
	And Inputs are "SomeInput" for mysql connector tool
	Then I select "AnotherAction" Action for mysql connector tool
	And Inputs on mysql connector tool is null
	Then I click validate on mysql connector tool

Scenario: Change the recordset on existing mysql server connector tool
	Given I open an existing mysql connector tool
	And Source is enabled and set to "DemoSqlsource" on mysql connector tool
	And Action is Enabled and set to "someAction" on mysql connector tool
	And Input is enabled for existing mysql connector tool
	And Inputs are "SomeInput" for mysql connector tool
	When I select "AnotherAction" Action for mysql connector tool
	Then The recordset name changes to "SomeRecordSet" for mysql connector tool