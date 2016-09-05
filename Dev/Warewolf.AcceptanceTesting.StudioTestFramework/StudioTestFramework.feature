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

Scenario: Create new test from Explorer opens new window with default properties
	Given I right click on the WorkflowService on the explorer		
	Then The WorkflowTestBuilder window is opened
	And The WorkflowTestBuilder has "test1" as a TestName
	And The WorkflowTestBuilder has workflow Name plus Tests
	And The WorkflowTestBuilder has a tests URL populated
	And WorkflowTestBuilder has an empty UserName
	And WorkflowTestBuilder has an empty Password
	And WorkflowTestBuilder has empty Inputs
	And WorkflowTestBuilder has empty Outputs
	And WorkflowTestBuilder has no Error selected
	And WorkflowTestBuilder has an empty debug output

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







	



