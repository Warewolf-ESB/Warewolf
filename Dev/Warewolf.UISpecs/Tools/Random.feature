Feature: Random
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Random onto a new workflow
	When I "Drag_Toolbox_Random_Onto_DesignSurface"
	Then I "Assert_Random_Exists_OnDesignSurface"
