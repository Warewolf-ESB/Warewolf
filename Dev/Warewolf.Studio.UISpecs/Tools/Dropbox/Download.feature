Feature: Download
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	

@NeedsBlankWorkflow
Scenario: Drag toolbox Dropbox Download onto a new workflow
	When I "Drag_Toolbox_Dropbox_Download_Onto_DesignSurface"
	Then I "Assert_Dropbox_Download_Exists_OnDesignSurface"
