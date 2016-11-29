Feature: WorkflowTestingTab
	In order to test workflows
	As a Warewolf Studio user
	I want to perform a composition of recorded actions

Scenario: Unsaved Tests Contain a Star in their Name
	Given The Warewolf Studio is running
	When I Click View Tests In Explorer Context Menu for "Hello World"
	And I Click The Create a New Test Button
	Then The Added Test Exists
	And The Added Test "Has" Unsaved Star
	When I Click Save Ribbon Button Without Expecting a Dialog
	Then The Added Test "Has No" Unsaved Star
	When I Click The Create a New Test Button
	Then The Second Added Test Exists
	And The Second Added Test "Has" Unsaved Star
	When I Click Save Ribbon Button Without Expecting a Dialog
	Then The Second Added Test "Has No" Unsaved Star
	When I Toggle First Added Test Enabled
	Then The Added Test "Has" Unsaved Star
	When I Click Save Ribbon Button Without Expecting a Dialog
	Then The Added Test "Has No" Unsaved Star

Scenario: Run Passing Tests
	When I Click New Workflow Ribbon Button
	And I Drag Toolbox Random Onto DesignSurface
	And I Enter Dice Roll Values
	And I Press F6
	And I wait for output spinner
	And I Click Save Ribbon Button to Open Save Dialog
	And I Enter Service Name Into Save Dialog As "Testing123"
	And I Click SaveDialog Save Button
	And I Click Close Workflow Tab Button
	And I Click View Tests In Explorer Context Menu for "Testing123"
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