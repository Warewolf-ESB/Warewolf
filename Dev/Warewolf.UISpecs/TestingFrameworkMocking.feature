Feature: TestingFrameworkMocking

Scenario: Creating A Test From Debug While Theres An Unsaved Test In The Tests Tab
	Given I have Hello World workflow to test
	When I Filter the Explorer with "Hello World"
	And I Open Explorer First Item Tests using Context Menu
	And I Click The Create "4"th test Button
	Then I Double Click Explorer Localhost First Item
	And I click fsix
	And I Click Create Test From Debug
	And Message box window appears
	And I click Message box OK 
	And Test tab is open
	And I Click Close Clean Workflow Tab
	And I click Click_Close_Tests_Tab
	When I Click MessageBox No

Scenario: Run Test Then Edit The Workflow Sets The Test To Invalid
	Given I have Hello World workflow to test
	When I Filter the Explorer with "Hello World"
	Then I Double Click Explorer Localhost First Item
	And I click fsix
	And I Click Create Test From Debug
	Then I click Run "4"th test expecting "Pass"
	And I Enter "Hello There World" in the Output test step
	When I Click Save Ribbon Button Without Expecting a Dialog
	When I Click Test Tab
	Then The Test step in now "Invalid"