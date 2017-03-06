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
	And I Execute Workflow Using DebugRun Button
	And I Click Create Test From Debug
	And Message box window appears
	And I Click Save Before Continuing MessageBox OK 
	And Test tab is open
	And I Click Close Clean Workflow Tab
	And I Click EnableDisable Test 4, dirty "true"
	And I Delete Test "4"
	And I Click MessageBox Yes