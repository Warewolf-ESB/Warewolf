Feature: Replace
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Replace onto a new workflow
	When I "Drag_Toolbox_Replace_Onto_DesignSurface"
	Then I "Assert_Replace_Exists_OnDesignSurface"
