Feature: MySqlConnector
	In order to manage my database services
	As a Warewolf User
	I want to be shown the database service setup
	
@Database
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
	Then I click OK on mysql connector tool
	And The recordset name appear as "SomeRecordSet" on mysql connector tool

@Database
Scenario: Opening exisitng mysql server connector tool
	Given I open an existing mysql connector tool
	And Source is enabled and set to "DemoSqlsource" on mysql connector tool
	And Action is Enabled and set to "someAction" on mysql connector tool	
	And Input is enabled for existing mysql connector tool
	And Inputs are "SomeInput" for mysql connector tool
	Then I click validate on mysql connector tool
	And The outputs appear as "DemoSqlsource" on mysql connector tool

@Database
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

@Database
Scenario: Change the action on existing mysql server connector tool
	Given I open an existing mysql connector tool
	And Source is enabled and set to "DemoSqlsource" on mysql connector tool
	And Action is Enabled and set to "someAction" on mysql connector tool
	And Input is enabled for existing mysql connector tool
	And Inputs are "SomeInput" for mysql connector tool
	Then I select "AnotherAction" Action for mysql connector tool
	And Inputs on mysql connector tool is null
	Then I click validate on mysql connector tool

@Database
Scenario: Change the recordset on existing mysql server connector tool
	Given I open an existing mysql connector tool
	And Source is enabled and set to "DemoSqlsource" on mysql connector tool
	And Action is Enabled and set to "someAction" on mysql connector tool
	And Input is enabled for existing mysql connector tool
	And Inputs are "SomeInput" for mysql connector tool
	When I select "AnotherAction" Action for mysql connector tool
	Then The recordset name changes to "SomeRecordSet" for mysql connector tool
	
@SqlDatabaseBroker
Scenario: Execute MySql Server With Timeout
    Given I have workflow "MySqlWorkflowForTimeout" with "MySqlActivity" MySql database connector
    And Mysql server is Enabled
    And I Select "NewMySqlSource" as MySql Server Source for "MySqlActivity"
    And I Select "Pr_CitiesGetCountries_Delayed" as MySql Server Action for "MySqlActivity"
	And MySql Command Timeout is "30" millisenconds for "MySqlActivity"
	And Validate MySql Server is Enabled
    And I click MySql Generate Outputs
    And I click Test on Mysql
	And Mysql Server Recordset Name equals "Pr_CitiesGetCountries_Delayed"
	And MySql Command Timeout is "5" millisenconds for "MySqlActivity"
	When MySql Workflow "MySqlWorkflowForTimeout" containing dbTool is executed
    And the workflow "MySqlWorkflowForTimeout" execution has "AN" error "Timeout expired."
	And the workflow "MySqlWorkflowForTimeout" error does not contain "NewLine"