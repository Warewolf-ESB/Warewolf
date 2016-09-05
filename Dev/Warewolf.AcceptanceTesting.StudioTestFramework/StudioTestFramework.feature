Feature: StudioTestFramework
	In order to test workflows in warewolf 
	As a user
	I want to create, edit, delete and update tests in a test window


Scenario: Create new test from Explorer opens new window
	Given I right click on the ExplorerItem on the explorer
	And The ExplorerItem is not a source
	And The ExplorerItem is not a serverSource	
	When The ExplorerItem is a workflow service 
	Then The WorkflowTestBuilder window is opened

Scenario: Create New Test
	Given the test builder is open with "Workflow 1"
	When I click New Test
	Then new test case is visible
	And Test name starts with "Workflow 1_"
	And username is blank
	And password is blank
	And save is disabled
	And test status is pending
	And test is enabled
	And run test is disabled
	And delete is enabled	
	

Scenario: Edit existing test
	Given The WorkflowTestBuilder window is opened
	And I select "test1"
	Then Test name is "test1"
	And test URL is "http://localhost:3142/secure/Examples/Workflow 1.tests/test1"
	And UserName is empty
	And Password is empty
	And Inputs are empty
	And Outputs are empty
	And No Error selected
	And debug output is empty
	And Tab name is "Workflow 1 - Tests"
	And save is disabled
	When I change the test name to "test2"
	Then save is enabled
	And Tab name is "Worklow 1 - Tests *"
	When I save the test
	Then test URL is "http://localhost:3142/secure/Examples/Workflow 1.tests/test2"
	And Tab name is "Workflow 1 - Tests"
	And save is disabled

Scenario: Create new test from Explorer opens a new window with correct button state
	Given I right click on the ExplorerItem on the explorer
	Then The WorkflowTestBuilder window is opened
	And The WorkflowTestBuilder has RunAll buttons disabled
	And The WorkflowTestBuilder has Delete button disabled
	And The WorkflowTestBuilder has Run button enabled


Scenario: Create new test from Explorer opens a new window with correct test icon and state
	Given I right click on the ExplorerItem on the explorer
	Then The WorkflowTestBuilder window is opened
	And The WorkflowTestBuilder has a pending status
	And The WorkflowTestBuilder has a pending icon
	And The test in the WorkflowTestBuilder is enabled


Scenario: Create and Edit tests values sets the values for the test
	Given I Change Test values on the WorkflowTestBuilder 
	When I change the TestName to UnitTest1
	Then the TestName is changed to UnitTest1
	And The TestUrl is updated correctly
	When The Workflow  UnitTest1 has "2" inputs
	Then The Inputs variables are populated with "2" workflow inputs
	When the Workflow has "1" OutPut 
	Then the Outputs are populated with "1" inputs







	



