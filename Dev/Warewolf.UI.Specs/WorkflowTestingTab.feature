@WorkflowTestingTabFeature
Feature: WorkflowTestingTab
	In order to test workflows
	As a Warewolf Studio user
	I want to perform a composition of recorded actions

Scenario: Unsaved Tests Contain a Star in their Name
	Given The Warewolf Studio is running
	When I Click View Tests In Explorer Context Menu for Sub Item "Control Flow - Decision"
	And I Click The Create a New Test Button
	Then The "1st" Added Test Exists
	And The "1st" Added Test "Has" Unsaved Star
	When I Click Save Ribbon Button Without Expecting a Dialog
	Then The "1st" Added Test "Has No" Unsaved Star
	When I Click The Create a New Test Button
	Then The "2nd" Added Test Exists
	And The "2nd" Added Test "Has" Unsaved Star
	When I Click Save Ribbon Button Without Expecting a Dialog
	Then The "2nd" Added Test "Has No" Unsaved Star
	When I Toggle "1st" Added Test Enabled
	Then The "1st" Added Test "Has" Unsaved Star
	When I Click Save Ribbon Button Without Expecting a Dialog
	Then The "1st" Added Test "Has No" Unsaved Star

Scenario: Run Passing Tests
	Given The Warewolf Studio is running
	When I Click View Tests In Explorer Context Menu for "Testing123"
	And I Click The Create a New Test Button
	And I Update Test Name To "Testing123_Test"
	And I Click Save Ribbon Button Without Expecting a Dialog
	And I Click First Test Run Button
	Then The First Test "Is" Passing
	When I Toggle First Test Enabled
	And I Click First Test Run Button
	Then The First Test "Is" Invalid
	When I Click First Test Delete Button
	And I Click MessageBox Yes

Scenario: Run Test Then Edit The Workflow Sets The Test To Invalid
	Given The Warewolf Studio is running
	When I Run All Hello World Tests
	And I Open Explorer First Item With Double Click
	And I Click VariableList Scalar Row2 IsInputCheckbox
	And I Click Save Ribbon Button Without Expecting a Dialog
	And I Open Explorer First Item Tests With Context Menu
	Then The First Test "Is" Invalid