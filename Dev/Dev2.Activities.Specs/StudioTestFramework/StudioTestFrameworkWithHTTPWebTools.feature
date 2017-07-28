@StudioTestFrameworkWithHTTPWebTools
Feature: StudioTestFrameworkWithHTTPWebTools
	In order to test workflows that contain tools from the HTTP web category of tools in warewolf studio
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

			
Scenario: Run Selected Test in Web
	Given the test builder is open with "Hello World"
	And Tab Header is "Hello World - Tests"
	And there are no tests
	And I click New Test
	Then a new test is added
	And Tab Header is "Hello World - Tests *"
	And test name starts with "Test 1"
	And inputs are
	| Variable Name | Value |
	| Name          |       |
	And outputs as
	| Variable Name | Value |
	| Message       |       |
	And save is enabled
	When I save	
	When I run selected test in Web
	Then The WebResponse as
	| Test Name | Result | Message                                                                                                                     |
	| Test 1    | Failed | Failed Output For Variable: Message Message: Failed: Assert Equal. Expected Equal To '' for 'Message' but got 'Hello World.' |
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	
	Scenario: Run Selected Test in Web with wrong credentials
	Given the test builder is open with "Hello World"
	And Tab Header is "Hello World - Tests"
	And there are no tests
	And I click New Test
	And test AuthenticationType as "User"
	And username is "WrongUser"
	And password is "badPassword"
	Then a new test is added
	And Tab Header is "Hello World - Tests *"
	And test name starts with "Test 1"
	And inputs are
	| Variable Name | Value |
	| Name          |       |
	And outputs as
	| Variable Name | Value |
	| Message       |       |
	And save is enabled
	When I save	
	When I run selected test in Web
	Then The WebResponse as
	| Test Name | Result | Message                                                                                                                     |
	| Test 1    | Failed | Failed: The user running the test is not authorized to execute resource Hello World. |
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok

Scenario: Run Selected Test in Web with input variable value
	Given the test builder is open with "Hello World"
	And Tab Header is "Hello World - Tests"
	And there are no tests
	And I click New Test
	Then a new test is added
	And Tab Header is "Hello World - Tests *"
	And test name starts with "Test 1"
	And I update inputs as
	| Variable Name | Value |
	| Name             |  Bob     |
	And I update outputs as
	| Variable Name | Value |
	| Message   |   Hello Bob.    |
	And save is enabled
	When I save	
	When I run selected test in Web
	Then The WebResponse as
	| Test Name | Result | Message |
	| Test 1    | Passed |         |
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok

Scenario: Run All Tests in Web 
	Given the test builder is open with "Hello World"
	And Tab Header is "Hello World - Tests"
	And there are no tests
	And I click New Test
	Then a new test is added
	And Tab Header is "Hello World - Tests *"
	And test name starts with "Test 1"	
	And I update outputs as
	| Variable Name | Value       |
	| Message       | Hello World. |
	And save is enabled
	When I save
	Then Tab Header is "Hello World - Tests"
	And I close the test builder
	When the test builder is open with "Hello World"
	Then there are 1 tests
	When I run all tests in Web
	Then The WebResponse as
	| Test Name | Result | Message |
	| Test 1    | Passed |         |

Scenario: Run All Tests in Web with failing test
	Given the test builder is open with "Hello World"
	And Tab Header is "Hello World - Tests"
	And there are no tests
	And I click New Test
	Then a new test is added
	And Tab Header is "Hello World - Tests *"
	And test name starts with "Test 1"	
	And I update outputs as
	| Variable Name | Value       |
	| Message       | Hello World. |
	And save is enabled
	When I save
	And I click New Test
	Then a new test is added
	And Tab Header is "Hello World - Tests *"
	And test name starts with "Test 2"
	And inputs are
	| Variable Name | Value |
	| Name          |       |
	And outputs as
	| Variable Name | Value |
	| Message       |       |
	And save is enabled
	When I save	
	Then Tab Header is "Hello World - Tests"
	And I close the test builder
	When the test builder is open with "Hello World"
	Then there are 2 tests
	When I run all tests in Web
	Then The WebResponse as
	| Test Name | Result | Message                                                                                                                     |
	| Test 1    | Passed |                                                                                                                             |
	| Test 2    | Failed | Failed Output For Variable: Message Message: Failed: Assert Equal. Expected Equal To '' for 'Message' but got 'Hello World.' |

Scenario: Run Selected Test passed with all teststeps fails
	Given the test builder is open with "Hello World"
	And Tab Header is "Hello World - Tests"
	And there are no tests
	And I click New Test
	Then a new test is added
	And Tab Header is "Hello World - Tests *"
	And test name starts with "Test 1"
	And inputs are
	| Variable Name | Value |
	| Name             |       |
	And outputs as
	| Variable Name | Value |
	| Message   |       |
	And I Add all TestSteps
	And save is enabled
	When I save
	And I run the test
	Then test result is Failed
	
Scenario: Run Selected Test passed with assign teststep Passes
	Given the test builder is open with "Hello World"
	And Tab Header is "Hello World - Tests"
	And there are no tests
	And I click New Test
	Then a new test is added
	And Tab Header is "Hello World - Tests *"
	And test name starts with "Test 1"
	And inputs are
	| Variable Name | Value |
	| Name             |       |
	And outputs as
	| Variable Name | Value |
	| Message   |       |
	And I Add "Assign a value to Name if blank (1)" as TestStep
	And save is enabled
	When I save
	And I run the test
	Then test result is Failed
