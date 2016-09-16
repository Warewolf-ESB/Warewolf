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
		Given I have "Hello World" with inputs as
			| Input Var Name |
			| [[Name]]          |
		And "Hello World" has outputs as
			| Ouput Var Name  |
			| [[Message]] |
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

				

Scenario: Create New Test
	Given the test builder is open with "Workflow 1"	
	And Tab Header is "Workflow 1 - Tests"
	And there are no tests
	When I click New Test
	Then a new test is added
	And Tab Header is "Workflow 1 - Tests *"
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
	And Duplicate Test is "Disabled"

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
	And Tab Header is "Workflow 1 - Tests *"
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
	Then there are 1 tests
	And "Dummy Test" is selected
	And I select "Test 1"
	And "Test 1" is selected
	And test name starts with "Test 1"
	And inputs are
	| Variable Name | Value |
	| a             |       |
	And outputs as
	| Variable Name | Value |
	| outputValue   |       |
	And save is disabled

Scenario: Save multiple New Tests Enabled Save after Edit
	Given the test builder is open with "Workflow 1"
	And Tab Header is "Workflow 1 - Tests"
	And there are no tests
	And I click New Test
	Then a new test is added
	And Tab Header is "Workflow 1 - Tests *"
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
	Then there are 1 tests
	And I select "Test 1"
	And "Test 1" is selected
	And test name starts with "Test 1"
	And inputs are
	| Variable Name | Value |
	| a             |       |
	And outputs as
	| Variable Name | Value |
	| outputValue   |       |
	And save is disabled
	And I click New Test
	Then a new test is added
	And Tab Header is "Workflow 1 - Tests *"
	And test name starts with "Test 2"
	And inputs are
	| Variable Name | Value |
	| a             |       |
	And outputs as
	| Variable Name | Value |
	| outputValue   |       |
	And save is enabled
	When I save
	Then Tab Header is "Workflow 1 - Tests"
	When I select "Test 1"
	When I change the test name to "testing2"
	Then "Save" test is visible

Scenario: Save a New Test fails when workflow deleted
	Given the test builder is open with "Workflow 1"
	And Tab Header is "Workflow 1 - Tests"
	And there are no tests
	And I click New Test
	Then a new test is added
	And Tab Header is "Workflow 1 - Tests *"
	And test name starts with "Test 1"
	And inputs are
	| Variable Name | Value |
	| a             |       |
	And outputs as
	| Variable Name | Value |
	| outputValue   |       |
	And save is enabled
	When "Workflow 1" is deleted
	When I save
	Then The "Workflow Deleted" popup is shown I click Ok
	And the tab is closed

Scenario: Run Selected Test fails when workflow deleted
	Given the test builder is open with "Workflow 1"
	And Tab Header is "Workflow 1 - Tests"
	And there are no tests
	And I click New Test
	Then a new test is added
	And Tab Header is "Workflow 1 - Tests *"
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
	Then there are 1 tests
	And "Dummy Test" is selected
	And I select "Test 1"
	And "Test 1" is selected
	And test name starts with "Test 1"
	And inputs are
	| Variable Name | Value |
	| a             |       |
	And outputs as
	| Variable Name | Value |
	| outputValue   |       |
	And save is disabled
	When "Workflow 1" is deleted
	And I run selected test
	Then The "Workflow Deleted" popup is shown I click Ok
	And the tab is closed

Scenario: Run Selected Test in browser fails when workflow deleted
	Given the test builder is open with "Workflow 1"
	And Tab Header is "Workflow 1 - Tests"
	And there are no tests
	And I click New Test
	Then a new test is added
	And Tab Header is "Workflow 1 - Tests *"
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
	Then there are 1 tests
	And "Dummy Test" is selected
	And I select "Test 1"
	And "Test 1" is selected
	And test name starts with "Test 1"
	And inputs are
	| Variable Name | Value |
	| a             |       |
	And outputs as
	| Variable Name | Value |
	| outputValue   |       |
	And save is disabled
	When "Workflow 1" is deleted
	And I run selected test in browser
	Then The "Workflow Deleted" popup is shown I click Ok
	And the tab is closed

Scenario: Run All Tests fails when workflow deleted
	Given the test builder is open with "Workflow 1"
	And Tab Header is "Workflow 1 - Tests"
	And there are no tests
	And I click New Test
	Then a new test is added
	And Tab Header is "Workflow 1 - Tests *"
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
	Then there are 1 tests
	When "Workflow 1" is deleted
	And I run all tests
	Then The "Workflow Deleted" popup is shown I click Ok
	And the tab is closed

Scenario: Run All Tests in browser fails when workflow deleted
	Given the test builder is open with "Workflow 1"
	And Tab Header is "Workflow 1 - Tests"
	And there are no tests
	And I click New Test
	Then a new test is added
	And Tab Header is "Workflow 1 - Tests *"
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
	Then there are 1 tests
	When "Workflow 1" is deleted
	And I run all tests in browser
	Then The "Workflow Deleted" popup is shown I click Ok
	And the tab is closed

Scenario: Edit existing test validate star	
	Given the test builder is open with "Workflow 3"
	And Tab Header is "Workflow 3 - Tests"
	And there are no tests
	And I click New Test
	And I set Test Values as
	| TestName | AuthenticationType | Error |
	| Test1    | Windows            | true  |
	Then NoErrorExpected is "true"	
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
	And I select "Test2"
	And I set Test Values as
	| TestName | AuthenticationType | Error |
	| Test2    | Public             | true  |
	Then Name for display is "Test2 *" and test is edited
	And I set Test Values as
	| TestName | AuthenticationType | Error |
	| Test2    | Windows            | true  |
	Then Name for display is "Test2" and test is not edited

Scenario: Edit existing test  	
	Given the test builder is open with "Workflow 3"
	And Tab Header is "Workflow 3 - Tests"
	And there are no tests
	And I click New Test
	And I set Test Values as
	| TestName | AuthenticationType | Error |
	| Test1    | Windows            | true  |
	Then NoErrorExpected is "true"	
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
	And I select "Test2"
	Then Duplicate Test is "Enabled"
	And I set Test Values as
	| TestName | AuthenticationType | Error |
	| Test2    | Public            | true  |
	When I save
	Then Tab Header is "Workflow 3 - Tests"
	And I close the test builder
	When the test builder is open with "Workflow 3"
	Then there are 2 tests
	And I select "Test2"
	And Test name is "Test2"
	And Authentication is Public

Scenario: Rename existing test
	Given the test builder is open with "Workflow 3"
	And Tab Header is "Workflow 3 - Tests"
	And there are no tests
	And I click New Test
	And I set Test Values as
	| TestName | AuthenticationType | Error |
	| Test1    | Windows            | true  |
	Then NoErrorExpected is "true"	
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
	And I select "Test2"
	When I change the test name to "testing2"
	Then save is enabled
	When I save
	Then test URL is "http://localhost:3142/secure/Examples/Workflow 1.tests/testing2"
	Then Tab Header is "Workflow 3 - Tests"
	And I close the test builder
	When the test builder is open with "Workflow 3"
	Then there are 2 tests
	And I select "testing2"
	And Test name is "testing2"

Scenario: Loading existing Tests has correct Name for display
	Given the test builder is open with "Workflow 3"
	And Tab Header is "Workflow 3 - Tests"
	And there are no tests
	And I click New Test
	And I set Test Values as
	| TestName | AuthenticationType | Error |
	| Test1    | Windows            | true  |
	Then NoErrorExpected is "true"	
	And save is enabled
	When I save
	Then Tab Header is "Workflow 3 - Tests"
	And I close the test builder
	When the test builder is open with "Workflow 3"
	Then there are 1 tests
	And I select "Test1"
	And Name for display is "Test1" and test is not edited

Scenario: Loading existing Tests has correct Test Status
	Given the test builder is open with "Workflow 3"
	And Tab Header is "Workflow 3 - Tests"
	And there are no tests
	And I click New Test
	And I set Test Values as
	| TestName | AuthenticationType | Error |
	| Test1    | Windows            | true  |
	Then NoErrorExpected is "true"	
	Then Test Status saved is "TestPending"	
	And save is enabled
	When I save
	Then Tab Header is "Workflow 3 - Tests"
	And I close the test builder
	When the test builder is open with "Workflow 3"
	Then there are 1 tests
	And Test Status is "TestPending"

Scenario: Loading existing Tests  	
	Given the test builder is open with "Workflow 3"
	And Tab Header is "Workflow 3 - Tests"
	And there are no tests
	And I click New Test
	And I set Test Values as
	| TestName | AuthenticationType | Error |
	| Test1    | Windows            | true  |
	Then NoErrorExpected is "true"	
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

Scenario: Close test window
	Given the test builder is open with "Workflow 3"
	And Tab Header is "Workflow 3 - Tests"
	And there are no tests
	And I click New Test
	And I set Test Values as
	| TestName | AuthenticationType | Error |
	| Test1    | Windows            | true  |
	Then NoErrorExpected is "true"	
	And save is enabled
	When I save
	Then Tab Header is "Workflow 3 - Tests"
	When I click "Test1"
	And I set Test Values as
	| TestName | AuthenticationType | Error |
	| NewName  | Windows            | false |
	Then I close the test builder
	#And The Pending Changes Confirmation popup is shown I click Ok
	#And Test name is "NewName"
	#And Error is "false"


Scenario: Delete an Disabled Test
	Given the test builder is open with "Workflow 3"
	And Tab Header is "Workflow 3 - Tests"
	And there are no tests
	And I click New Test
	And I set Test Values as
	| TestName | AuthenticationType | Error |
	| Test1    | Windows            | true  |
	Then NoErrorExpected is "true"	
	And save is enabled
	When I save
	Then Tab Header is "Workflow 3 - Tests"
	And there are 1 tests
	When I enable "Test1"
	Then Delete is disabled for "Test1"
	When I disable "Test1"
	Then Delete is enabled for "Test1"
	When I delete "Test1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then there are 0 tests


Scenario: Delete Resource with tests
	Given I have a resouce "PluginSource"
	And I add "test1,test2,test3" as tests
	Then "PluginSource" has 3 tests
	When I delete resource "PluginSource"
	Then "PluginSource" has 0 tests

Scenario: Delete folder with resources deletes all tests
	Given I have a folder "Home" 
	And I have a resouce workflow "PluginSource1" inside Home
	And I add "test 1,test 2,test 3" to "PluginSource1"
	And I have a resouce workflow "Hello world" inside Home
	And I add "test 4,test 5,test 6" to "Hello world"
	Then "PluginSource1" has 3 tests
	And "Hello world" has 3 tests
	When I delete folder "Home"
	Then "PluginSource1" has 0 tests
	And "Hello world" has 0 tests

#Scenario: Saved workflow with tests changes the inputs
#		Given the test builder is open with "WorkflowWithTests"
#		Then there are 4 tests
#		And I close the test builder
#		When I remove input "input" from workflow "WorkflowWithTests" 
#		And I save Workflow	"WorkflowWithTests"
#		When the test builder is open with "WorkflowWithTests"
#		And Tab Header is "WorkflowWithTests - Tests"				
#		And all test are invalid	

Scenario: Run all unselects all tests on selection shows corect debug
	Given the test builder is open with "WorkflowWithTests"
	Then there are 4 tests
	And "Test1" is passing
	And "Test2" is failing
	And "Test3" is invalid
	And "Test4" is pending
	When I run all tests
	And selected test is empty
	And I select "Test1"
	Then debug window is visible	


Scenario: Duplicate a test
	Given the test builder is open with "Workflow 1"
	And Tab Header is "Workflow 1 - Tests"
	And there are no tests
	And I click New Test
	Then a new test is added
	And Tab Header is "Workflow 1 - Tests *"
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
	And there are 1 tests
	When I click "Test 1"
	Then Duplicate Test is visible
	When I click duplicate 
	Then there are 2 tests
	And the duplicated tests is "Test 1 1"
	And save is enabled
	When I save

Scenario: Duplicate a test with same name fails
	Given the test builder is open with "Workflow 1"
	And Tab Header is "Workflow 1 - Tests"
	And there are no tests
	And I click New Test
	Then a new test is added
	And Tab Header is "Workflow 1 - Tests *"
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
	And there are 1 tests
	When I click "Test 1"
	Then Duplicate Test is visible
	When I click duplicate 
	Then there are 2 tests
	And the duplicated tests is "Test 1 1"
	When I change the test name to "Test 1"
	And save is enabled
	When I save
	Then The duplicate Name popup is shown
	And Duplicate Test is "Disabled"

Scenario: Run a test with single scalar inputs and outputs
	Given the test builder is open with existing service "Hello World"	
	And Tab Header is "Hello World - Tests"
	And there are no tests
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
	| Variable Name | Value      |
	| Message       | Hello Bob. |
	And I save
	When I run the test
	Then test result is Passed
	And service debug inputs as
		| Variable | Value |
		| [[Name]] | Bob   |
	And the service debug outputs as
	  | Variable    | Value      |
	  | [[Message]] | Hello Bob. |
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then there are no tests

Scenario: Run a test with single scalar inputs and outputs failure
	Given the test builder is open with existing service "Hello World"	
	And Tab Header is "Hello World - Tests"
	And there are no tests
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
	| Variable Name | Value      |
	| Message       | Hello Mary. |
	And I save
	When I run the test
	Then test result is Failed
	Then service debug inputs as
		| Variable | Value |
		| [[Name]] | Bob   |	
	And the service debug outputs as
	  | Variable    | Value      |
	  | [[Message]] | Hello Bob. |
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then there are no tests

#Feedback specs
Scenario: Duplicate test new test has name
	Given the test builder is open with "Workflow 1"
	And Tab Header is "Workflow 1 - Tests"
	And there are no tests
	And I click New Test
	Then a new test is added
	And Tab Header is "Workflow 1 - Tests *"
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
	And there are 1 tests
	When I click "Test 1"
	Then Duplicate Test is visible
	When I click duplicate 
	Then there are 2 tests
	And Test name is "Test 1 1"

Scenario: Run Selected Test Shows Stop Option
	Given the test builder is open with "Workflow 1"
	And Tab Header is "Workflow 1 - Tests"
	And there are no tests
	And I click New Test
	Then a new test is added
	And Tab Header is "Workflow 1 - Tests *"
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
	Then there are 1 tests
	And "Dummy Test" is selected
	And I select "Test 1"
	And "Test 1" is selected
	And test name starts with "Test 1"
	And inputs are
	| Variable Name | Value |
	| a             |       |
	And outputs as
	| Variable Name | Value |
	| outputValue   |       |
	And save is disabled
	When I run selected test
	#Then "Stop" test is visible





	

	



	






	



