Feature: WorkflowTestingTab
	In order to test workflows
	As a Warewolf Studio user
	I want to perform a composition of recorded actions

Scenario: Unsaved Tests Contain a Star in their Name
	Given The Warewolf Studio is running
	When I Click View Tests In Explorer Context Menu for "Hello World"
	And I Click The Create a New Test Button
	Then The First Test Exists
	And The First Test "Has" Unsaved Star
	When I Click Save Ribbon Button Without Expecting a Dialog
	Then The First Test "Has No" Unsaved Star
	When I Click The Create a New Test Button
	Then The Second Test Exists
	And The Second Test "Has" Unsaved Star
	When I Click Save Ribbon Button Without Expecting a Dialog
	Then The Second Test "Has No" Unsaved Star
	When I Toggle First Test Enabled
	Then The First Test "Has" Unsaved Star
	When I Click Save Ribbon Button Without Expecting a Dialog
	Then The First Test "Has No" Unsaved Star

Scenario: Run Passing Tests
	When I Click New Workflow Ribbon Button
	And I Drag Toolbox MultiAssign Onto DesignSurface
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