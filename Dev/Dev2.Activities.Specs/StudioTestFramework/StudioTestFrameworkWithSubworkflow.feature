@StudioTestFrameworkWithSubworkflow
Feature: StudioTestFrameworkWithSubworkflow
	In order to test workflows that contain other workflows in warewolf 
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

Scenario: Create New Test with Service that as recordset inputs
	Given the test builder is open with "Workflow 2"	
	And Tab Header is "Workflow 2 - Tests"
	And there are no tests
	When I click New Test
	Then a new test is added
	And Tab Header is "Workflow 2 - Tests *"
	And test name starts with "Test 1"
	And username is blank
	And password is blank
	And inputs are
	| Variable Name | Value |
	| rec(1).a      |       |
	| rec(1).b      |       |
	And I update inputs as
	| Variable Name | Value |
	| rec(1).a      | val1  |
	| rec(1).b      |       |
	Then inputs are
	| Variable Name | Value |
	| rec(1).a      | val1  |
	| rec(1).b      |       |
	| rec(2).a      |       |
	| rec(2).b      |       |

Scenario: Test with ForEach containing a Service
	Given the test builder is open with "ForEachWithHelloWorldTest"
	And Tab Header is "ForEachWithHelloWorldTest - Tests"
	And there are no tests
	And I click New Test
	Then a new test is added
	And I update outputs as
	| Variable Name    | Value    |
	| messages(1).name | Hello 1. |
	| messages(2).name | Hello 2. |
	| messages(3).name | Hello 3. |	
	When I run the test
	Then the service debug outputs as
	  | Variable             | Value    |
	  | [[Message]] | Hello 1. |
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	And test folder is cleaned
	
Scenario: Run a test expecting error 
	Given the test builder is open with existing service "HelloWorldWithError"	
	And Tab Header is "HelloWorldWithError - Tests"
	When I click New Test
	Then a new test is added
	And Tab Header is "HelloWorldWithError - Tests *"
	And test name starts with "Test 1"
	And username is blank
	And password is blank
	And I update inputs as
	| Variable Name | Value | EmptyIsNull |
	| Name          |       | true        |
	And I expect Error "p"
	And save is enabled
	And test status is pending
	And test is enabled	
	And I remove all Test Steps
	And I save
	When I run the test
	Then the service debug assert message contains "Failed: Expected Error containing 'p' but got 'variable not found'"	
	Then test result is Failed	
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	And test folder is cleaned

Scenario: Run a test expecting no error as an unknown user
	Given the test builder is open with existing service "Hello World"	
	And Tab Header is "Hello World - Tests"
	When I click New Test
	Then a new test is added
	And Tab Header is "Hello World - Tests *"
	And test name starts with "Test 1"
	And test AuthenticationType as "User"
	And username is "WrongUser"
	And password is "badPassword"
	And I update inputs as
	| Variable Name | Value | EmptyIsNull |
	| Name          |       | true        |
	And save is enabled
	And test status is pending
	And test is enabled	
	And I remove all Test Steps
	And I save
	When I run the test
	Then the service debug assert Aggregate message contains "Failed: The user running the test is not authorized to execute resource Hello World"	
	Then test result is Failed	
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	And test folder is cleaned

Scenario: Run a test expecting an error as an unknown user
	Given the test builder is open with existing service "Hello World"	
	And Tab Header is "Hello World - Tests"
	When I click New Test
	Then a new test is added
	And Tab Header is "Hello World - Tests *"
	And test name starts with "Test 1"	
	And test AuthenticationType as "User"
	And username is "WrongUser"
	And password is "badPassword"
	And I update inputs as
	| Variable Name | Value | EmptyIsNull |
	| Name          | Bob   | true        |
	And save is enabled
	And test status is pending
	And test is enabled	
	And I remove all Test Steps
	And I save
	And I expect Error "The user"
	When I run the test
	Then the service debug assert Aggregate message contains "Passed"	
	Then test result is Passed	
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	And test folder is cleaned
	
Scenario: Run a test expecting a wrong error as an unknown user
	Given the test builder is open with existing service "Hello World"	
	And Tab Header is "Hello World - Tests"
	When I click New Test
	Then a new test is added
	And Tab Header is "Hello World - Tests *"
	And test name starts with "Test 1"	
	And test AuthenticationType as "User"
	And username is "WrongUser"
	And password is "badPassword"
	And I update inputs as
	| Variable Name | Value | EmptyIsNull |
	| Name          | Bob   | true        |
	And save is enabled
	And test status is pending
	And test is enabled	
	And I remove all Test Steps
	And I save
	And I expect Error "Stackoverflow"
	When I run the test
	Then the service debug assert Aggregate message contains "Failed: expected error containing 'stackoverflow' but got 'Failed: the user running the test is not authorized to execute resource hello world.'"	
	Then test result is Failed	
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	And test folder is cleaned

Scenario: Run a test with invalid inputs and pending results
    Given the test builder is open with existing service "Hello World"	
	And Tab Header is "Hello World - Tests"
	When I click New Test
	Then a new test is added
	And Tab Header is "Hello World - Tests *"
	And test name starts with "Test 1"
	And username is blank
	And password is blank
	And I update inputs as
	| Variable Name | Value    | 
	| Name          | [[Home]] |
	And I save
	When I run the test
	Then test result is Failed	
	When I delete "Test 1" 
	Then The "DeleteConfirmation" popup is shown I click Ok
	
Scenario: Run a test with invalid and pending results
    Given the test builder is open with existing service "Hello World"	
	And Tab Header is "Hello World - Tests"
	When I click New Test
	Then a new test is added
	And Tab Header is "Hello World - Tests *"
	And test name starts with "Test 1"
	And username is blank
	And password is blank
	And I update inputs as
	| Variable Name | Value | 
	| Name          | Bob   | 
	And I Add Decision "If [[Name]] <> (Not Equal)" as TestStep
	And I change Decision "If [[Name]] <> (Not Equal)" arm to "Name Input"
	And I Add "Assign a value to Name if blank (1)" as TestStep
	And I Add "Set the output variable (1)" as TestStep
	And I update outputs as
         | Variable Name | Value      |
         | Message       | Hello Bob. |
	When I run the test
	When I delete "Test 1" 
	Then The "DeleteConfirmation" popup is shown I click Ok


Scenario: Run a test with single scalar inputs and outputs invalid no variable
	Given the test builder is open with existing service "Hello World"	
	And Tab Header is "Hello World - Tests"
	When I click New Test
	Then a new test is added
	And Tab Header is "Hello World - Tests *"
	And test name starts with "Test 1"
	And username is blank
	And password is blank
	And inputs are
	| Variable Name | Value |
	| Name          |       |
	And outputs as
	| Variable Name | Value |
	| Message       |       |
	And save is enabled
	And test status is pending
	And test is enabled
	And I update inputs as
	| Variable Name | Value |
	| Name          | Bob   |
	And I update outputs as
	| Variable Name | Value       |
	|               | Hello Mary. |
	And I save
	When I run the test
	Then test result is Failed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	And test folder is cleaned

Scenario: Run a test with single scalar inputs and outputs invalid no input variable
	Given the test builder is open with existing service "Hello World"	
	And Tab Header is "Hello World - Tests"
	When I click New Test
	Then a new test is added
	And Tab Header is "Hello World - Tests *"
	And test name starts with "Test 1"
	And username is blank
	And password is blank
	And inputs are
	| Variable Name | Value |
	| Name          |       |
	And outputs as
	| Variable Name | Value |
	| Message       |       |
	And save is enabled
	And test status is pending
	And test is enabled
	And I update inputs as
	| Variable Name | Value |
	|               |       |
	And I update outputs as
	| Variable Name | Value       |
	| Message       | Hello Mary. |
	And I save
	When I run the test
	Then test result is Failed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	And test folder is cleaned
