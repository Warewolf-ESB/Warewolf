Feature: WorkflowTestingTab
	In order to test workflows
	As a Warewolf Studio user
	I want to perform a composition of recorded actions

Scenario: DirtyTest Should Set Star Next To The Tab Name And Test Name
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
	