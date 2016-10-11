Feature: SaveDialog
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@mytag
Scenario: SaveDialogServiceNameValidation
	Given The Warewolf Studio is running
	When I Click New Workflow Ribbon Button
	And I Drag Toolbox MultiAssign Onto DesignSurface
	And I Assign Value To Variable With Assign Tool Small View Row 1
	And I Click Save Ribbon Button to Open Save Dialog
	And I Remove WorkflowName From Save Dialog
	And I Enter Service Name Into Save Dialog As "TestingWF"
	And I Click SaveDialog Save Button
	And I Click Close Workflow Tab Button
	And I Click Duplicate From Explorer Context Menu for Service "TestingWF"
	And I Enter Service Name Into Duplicate Dialog As "TestingWF"
	And I Enter Invalid Service Name Into Duplicate Dialog As "Inv@lid N&m#"
	And I Enter Invalid Service Name With Whitespace Into Duplicate Dialog As "Test "
	When I Enter Service Name Into Duplicate Dialog As "ValidWFName"
	And I Click Duplicate From Duplicate Dialog
