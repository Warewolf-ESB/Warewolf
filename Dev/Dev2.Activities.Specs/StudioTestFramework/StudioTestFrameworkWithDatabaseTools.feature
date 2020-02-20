@StudioTestFrameworkWithDatabaseTools
Feature: StudioTestFrameworkWithDatabaseTools
	In order to test workflows that contain database tools in warewolf 
	As a user
	I want to create, edit, delete and update tests in a test window


Background: Setup for workflows for tests
		Given test folder is cleaned	
		And I have "Workflow 1" with inputs as
			| Input Var Name |
			| [[a]]          |
		And "Workflow 1" has outputs as
			| Ouput Var Name  |
			| [[outputValue]] |		
		Given I have "Workflow 2" with inputs as
			| Input Var Name |
			| [[rec().a]]    |
			| [[rec().b]]    |
		And "Workflow 2" has outputs as
			| Ouput Var Name |
			| [[returnVal]]  |
		Given I have "Workflow 3" with inputs as
			| Input Var Name |
			| [[A]]              |
			| [[B]]              |
			| [[C]]              |
		And "Workflow 3" has outputs as
			| Ouput Var Name |
			| [[message]]    |
		Given I have "WorkflowWithTests" with inputs as 
			| Input Var Name |
			| [[input]]      |
		And "WorkflowWithTests" has outputs as
			| Ouput Var Name  |
			| [[outputValue]] |			
		And "WorkflowWithTests" Tests as 
			| TestName | AuthenticationType | Error | TestFailing | TestPending | TestInvalid | TestPassed |
			| Test1    | Windows            | false | false       | false       | false       | true       |
			| Test2    | Windows            | false | true        | false       | false       | false      |
			| Test3    | Windows            | false | false       | false       | true        | false      |
			| Test4    | Windows            | false | false       | true        | false       | false      |


Scenario: Test WF with MySql
		Given I have a workflow "MySqlTestWF"
		 And "MySqlTestWF" contains a mysql database service "MySqlEmail" with mappings for testing as
		  | Input to Service | From Variable | Output from Service | To Variable      |
		  |                  |               | name                | [[rec(*).name]]  |
		  |                  |               | email               | [[rec(*).email]] |	
		And I save workflow "MySqlTestWF"
		Then the test builder is open with "MySqlTestWF"
		And I click New Test
		And a new test is added	
		And test name starts with "Test 1"
		And I Add "MySqlEmail" as TestStep
		And I add StepOutputs as 
		| Variable Name           | Condition | Value              |
		| [[MySqlEmail(1).name]]  | =         | Monk               |
		| [[MySqlEmail(1).email]] | =         | dora@explorers.com |		
		When I save
		And I run the test
		Then test result is Passed
		When I delete "Test 1"
		Then The "DeleteConfirmation" popup is shown I click Ok
		Then workflow "MySqlTestWF" is deleted as cleanup

Scenario: Test WF with Sql Server
		Given I depend on a valid MSSQL server
		And I have a workflow "SqlTestWF"
		And "SqlTestWF" contains a sqlserver database service "dbo.Pr_CitiesGetCountries" with mappings for testing as
		| ParameterName | ParameterValue |
		| Prefix        | D              |
		And I save workflow "SqlTestWF"
		Then the test builder is open with "SqlTestWF"
		And I click New Test
		And a new test is added	
		And test name starts with "Test 1"
		And I Add "dbo.Pr_CitiesGetCountries" as TestStep
		And I add StepOutputs as 
		| Variable Name                                | Condition | Value    |
		| [[dbo_Pr_CitiesGetCountries(2).CountryID]]   | =         | 40       |
		| [[dbo_Pr_CitiesGetCountries(2).Description]] | =         | Djibouti |
		When I save
		And I run the test
		Then test result is Passed
		When I delete "Test 1"
		Then The "DeleteConfirmation" popup is shown I click Ok
		Then workflow "SqlTestWF" is deleted as cleanup

Scenario: Test WF with Oracle
		Given I have a workflow "oracleTestWF"
		 And "oracleTestWF" contains a oracle database service "HR.GET_EMP_RS" with mappings as
		    | ParameterName | ParameterValue |
		    | P_DEPTNO      | 110            |	 
		And I save workflow "oracleTestWF"
		Then the test builder is open with "oracleTestWF"
		And I click New Test
		And a new test is added	
		And test name starts with "Test 1"
		And I Add "HR.GET_EMP_RS" as TestStep
		And I add StepOutputs as 
		| Variable Name                      | Condition | Value        |
		| [[HR_GET_EMP_RS(2).EMPLOYEE_ID]]   | =         | 205          |
		| [[HR_GET_EMP_RS(2).FIRST_NAME]]    | =         | Shelley      |
		| [[HR_GET_EMP_RS(2).LAST_NAME]]     | =         | Higgins      |
		| [[HR_GET_EMP_RS(2).EMAIL]]         | =         | SHIGGINS     |
		When I save
		And I run the test
		Then test result is Passed
		When I delete "Test 1"
		Then The "DeleteConfirmation" popup is shown I click Ok
		Then workflow "oracleTestWF" is deleted as cleanup
		
Scenario: Test WF with PostGre Sql
		Given I depend on a valid PostgreSQL server
		And I have a workflow "PostGreTestWF"
		And "PostGreTestWF" contains a postgre tool using "get_countries" with mappings for testing as
		| ParameterName | ParameterValue |
		| Prefix        | K              |  
		And I save workflow "PostGreTestWF"
		Then the test builder is open with "PostGreTestWF"
		And I click New Test
		And a new test is added	
		And test name starts with "Test 1"
		And I Add "get_countries" as TestStep
		And I add StepOutputs as 
		| Variable Name             | Condition | Value          |
		| [[get_countries(1).id]]   | =         | 2              |
		When I save
		And I run the test
		Then test result is Passed
		When I delete "Test 1"
		Then The "DeleteConfirmation" popup is shown I click Ok
		Then workflow "PostGreTestWF" is deleted as cleanup

Scenario: Test WF with Decision
		Given I have a workflow "DecisionTestWF"
		And "DecisionTestWF" contains an Assign "TestAssign" as
		| variable | value |
		| [[A]]    | 30    |
		And a decision variable "[[A]]" value "30"	
		And decide if "[[A]]" "IsAlphanumeric" 
		And I save workflow "DecisionTestWF"
		Then the test builder is open with "DecisionTestWF"
		And I click New Test
		And a new test is added	
		And test name starts with "Test 1"
		And I Add "TestDecision" as TestStep
		And I add Assert steps as
		| Step Name                  | Output Variable | Output Value | Activity Type |
		| If [[Name]] <> (Not Equal) | Flow Arm        | True         | Decision      |
		When I save
		And I run the test
		Then test result is Passed
		When I delete "Test 1"
		Then The "DeleteConfirmation" popup is shown I click Ok
		Then workflow "DecisionTestWF" is deleted as cleanup

Scenario: Test WF with SqlBulk Insert
		Given I depend on a valid MSSQL server
		And I have a workflow "SqlBulkTestWF"
		And "SqlBulkTestWF" contains an SQL Bulk Insert "BulkInsert" using database "NewSqlBulkInsertSource" and table "dbo.MailingList" and KeepIdentity set "true" and Result set "[[result]]" for testing as
		| Column | Mapping             | IsNullable | DataTypeName | MaxLength | IsAutoIncrement |
		| Name   | Warewolf            | false      | varchar      | 50        | false           |
		| Email  | Warewolf@dev2.co.za | false      | varchar      | 50        | false           |
		And I save workflow "SqlBulkTestWF"
		Then the test builder is open with "SqlBulkTestWF"
		And I click New Test
		And a new test is added	
		And test name starts with "Test 1"
		And I Add "BulkInsert" as TestStep
		And I add StepOutputs as 
		| Variable Name | Condition | Value   |
		| [[result]]    | =         | Success |
		When I save
		And I run the test
		Then test result is Passed
		When I delete "Test 1"
		Then The "DeleteConfirmation" popup is shown I click Ok
		Then workflow "SqlBulkTestWF" is deleted as cleanup		
