Feature: Upload
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	

@NeedsBlankWorkflow
Scenario: Drag toolbox Dropbox Upload onto a new workflow
	When I "Drag_Toolbox_Dropbox_Upload_Onto_DesignSurface"
	Then I "Assert_Dropbox_Upload_Exists_OnDesignSurface"
