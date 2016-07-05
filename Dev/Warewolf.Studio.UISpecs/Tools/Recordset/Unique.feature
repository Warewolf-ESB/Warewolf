Feature: Unique
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Unique_Records onto a new workflow
	When I "Drag_Toolbox_Unique_Records_Onto_DesignSurface"
	Then I "Assert_Unique_Records_Exists_OnDesignSurface"
