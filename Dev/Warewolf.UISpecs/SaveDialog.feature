Feature: SaveDialog
	In order to save services
	As a warewolf studio user
	I want to give the workflow a name and location

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
	And I Enter Service Name Into Duplicate Dialog As "TestingWF1"
	And I Enter Invalid Service Name Into Duplicate Dialog As "Inv@lid N&m#"
	And I Enter Invalid Service Name With Whitespace Into Duplicate Dialog As "Test "
	And I Enter Service Name Into Duplicate Dialog As "ValidWFName"
	And I Click Duplicate From Duplicate Dialog
