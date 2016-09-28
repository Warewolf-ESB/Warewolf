Feature: Delete
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Delete_Record onto a new workflow
	When I "Drag_Toolbox_Delete_Record_Onto_DesignSurface"
	Then I "Assert_Delete_Record_Exists_OnDesignSurface"
