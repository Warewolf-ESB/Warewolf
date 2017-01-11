@TestingFrameworkMocking
Feature: TestingFrameworkMocking


Scenario: Executing Release Tests for Hello World should all be passing
	Given The Warewolf Studio is running
	And I have Hello World workflow on the Explorer
	And I Open Explorer First Item Tests With Context Menu
	And I Click Run all tests button
	Then The First Test "Is" Passing	
	And The Second Test "Is" Passing	
	And The Third Test "Is" Passing	

Scenario: Creating A Test From Debug While Theres An Unsaved Test In The Tests Tab
	Given The Warewolf Studio is running
	And I have Hello World workflow on the Explorer
	And I Open Explorer First Item Tests With Context Menu
	And I Click The Create "4"th test Button
	Then I Open Explorer First Item Context Menu
	And I Press F6
	And I Click Create Test From Debug
	And Message box window appears
	And I Click MessageBox OK 
	And Test tab is open
	And I Click Close Clean Workflow Tab
	And I Click EnableDisable Test 4, dirty "true"
	And I Delete Test "4"
	And I Click MessageBox Yes
	And I Click Close Tests Tab
	And I Try Click MessageBox No

Scenario: Run Test Then Edit The Workflow Sets The Test To Invalid
	Given The Warewolf Studio is running
	And I have Hello World workflow on the Explorer
	Then I Open Explorer First Item Context Menu
	And I Press F6
	And I Click Create Test From Debug
	And I Click Save Ribbon Button Without Expecting a Dialog
	And I Click Run all tests button
	Then I Click workflow tab
	And I Enter "Hello There World" in the Assign message tool
	When I Click Save Ribbon Button Without Expecting a Dialog
	When I Click Test Tab
	Then The Test step in now "Invalid"
	And I delete Test4 as a Cleanup step
	And I Click Close Workflow Tab Without Confirmation