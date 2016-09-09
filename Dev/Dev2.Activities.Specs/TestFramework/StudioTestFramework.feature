Feature: StudioTestFramework
	In order to test workflows in warewolf 
	As a user
	I want to create, edit, delete and update tests in a test window


Background: Setup for workflows for tests
			Given I have "Workflow 1" with inputs as
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

Scenario: Create New Test
	Given the test builder is open with "Workflow 1"	
	And Tab Header is "Workflow 1 - Tests"
	And there are no tests
	When I click New Test
	Then a new test is added
	#And Tab Header is "Workflow 1 - Tests *"
	And test name starts with "Test 1"
	And username is blank
	And password is blank
	And inputs are
	| Variable Name | Value |
	| a             |       |
	And outputs as
	| Variable Name | Value |
	| outputValue   |       |
	And save is enabled
	And test status is pending
	And test is enabled

Scenario: Create New Test with Service that as recordset inputs
	Given the test builder is open with "Workflow 2"	
	And Tab Header is "Workflow 2 - Tests"
	And there are no tests
	When I click New Test
	Then a new test is added
	#And Tab Header is "Workflow 2 - Tests *"
	And test name starts with "Test 1"
	And username is blank
	And password is blank
	And inputs are
	| Variable Name | Value |
	| rec(1).a      |       |
	| rec(1).b      |       |
	When I updated the inputs as
	| Variable Name | Value |
	| rec(1).a      | val1  |
	| rec(1).b      |       |
	Then inputs are
	| Variable Name | Value |
	| rec(1).a      | val1  |
	| rec(1).b      |       |
	| rec(2).a      |       |
	| rec(2).b      |       |

Scenario: Save a New Test
	Given the test builder is open with "Workflow 1"
	And Tab Header is "Workflow 1 - Tests"
	And there are no tests
	And I click New Test
	Then a new test is added
	#And Tab Header is "Workflow 1 - Tests *"
	And test name starts with "Test 1"
	And inputs are
	| Variable Name | Value |
	| a             |       |
	And outputs as
	| Variable Name | Value |
	| outputValue   |       |
	And save is enabled
	When I save
	Then Tab Header is "Workflow 1 - Tests"
	And I close the test builder
	When the test builder is open with "Workflow 1"
	Then Tab Header is "Workflow 1 - Tests"
	And there are 2 tests
	And "Test 1" is selected
	And test name starts with "Test 1"
	And inputs are
	| Variable Name | Value |
	| a             |       |
	And outputs as
	| Variable Name | Value |
	| outputValue   |       |
	And save is disabled   



Scenario: Edit existing test
	Given the test builder is open with "Workflow 1"
	And I select "test1"
	Then Test name is "test1"
	And test URL is "http://localhost:3142/secure/Examples/Workflow 1.tests/test1"
	And username is blank
	And password is blank
	And Inputs are empty
	And Outputs are empty
	And No Error selected
	And debug output is empty
	And Tab Header is "Workflow 1 - Tests"
	And save is disabled
	When I change the test name to "test2"
	Then save is enabled
	And Tab Header is "Worklow 1 - Tests *"
	When I save
	Then test URL is "http://localhost:3142/secure/Examples/Workflow 1.tests/test2"
	And Tab Header is "Workflow 1 - Tests"
	And save is disabled


Scenario: Loading exisiting Tests  |	
	And the test builder is open with "Workflow 3"
	And Tab Header is "Workflow 3 - Tests"
	And there are no tests
	And I click New Test
	And I set Test Values as
	| TestName | AuthenticationType | Error |
	| Test1    | Windows            | true  |
	Then NoErrorExpected is "false"	
	And save is enabled
	When I save
	Then Tab Header is "Workflow 3 - Tests"
	When I click New Test
	And I set Test Values as
	| TestName | AuthenticationType | Error |
	| Test2    | Windows            | true  |
	And save is enabled
	When I save
	Then Tab Header is "Workflow 3 - Tests"
	And I close the test builder
	When the test builder is open with "Workflow 3"
	Then there are 2 tests


Scenario: Delete an existing test with correct permissions
	Given the test builder is open with "Workflow 3"
	And Tab Header is "Workflow 3 - Tests"
	And there are no tests
	And I click New Test
		And I set Test Values as
	| TestName | AuthenticationType | Error |
	| Test1    | Windows            | true  |
	Then NoErrorExpected is "false"	
	And save is enabled
	When I save
	Then Tab Header is "Workflow 3 - Tests"
	And there are 1 tests
	And Delete is enabled
	And Run is enabled
	When I delete selected Test
	Then The Confirmation popup is shown
	When I click cancel
	Then there are 1 tests
	And Delete is enabled
	And Run is enabled
	When I delete selected test
	Then The Confirmation popup is shown
	When I click Ok
	Then there are no tests
	And Delete is disabled
	And run is disabled
	And add new test is enabled


Scenario: Delete test not enabled when test disabled
	Given the test builder is open with "Workflow 3"
	And Tab Header is "Workflow 3 - Tests"
	And there are no tests
	And I click New Test
		And I set Test Values as
	| TestName | AuthenticationType | Error |
	| Test1    | Windows            | true  |
	Then NoErrorExpected is "false"	
	And save is enabled
	When I save
	Then Tab Header is "Workflow 3 - Tests"
	And there are 1 tests
	When test is disabled 
	Then Delete is disabled
	When test is enabled
	Then Delete is enabled
	When I delete selected Test
	Then The Confirmation popup is shown
	When I click cancel
	Then there are 1 tests
	And Delete is enabled
	And Run is enabled
	When I delete selected test
	Then The Confirmation popup is shown
	When I click Ok
	Then there are no tests
	And Delete is disabled
	And run is disabled
	And add new test is enabled

#Scenario: Duplicate a test
#	Given the test builder is open with "Workflow 1"
#	And Tab Header is "Workflow 1 - Tests"
#	And there are no tests
#	And I click New Test
#	Then a new test is added
#	#And Tab Header is "Workflow 1 - Tests *"
#	And test name starts with "Test 1"
#	And inputs are
#	| Variable Name | Value |
#	| a             |       |
#	And outputs as
#	| Variable Name | Value |
#	| outputValue   |       |
#	And save is enabled
#	When I save
#	Then Tab Header is "Workflow 1 - Tests"
#	When I right click "Test 1"
#	Then Duplicate Test
#
#
#
#
#	
#
#	



	






	



