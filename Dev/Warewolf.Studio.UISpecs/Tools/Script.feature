Feature: Script
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Script onto a new workflow
	When I "Drag_Toolbox_Script_Onto_DesignSurface"
	Then I "Assert_Script_Exists_OnDesignSurface"
