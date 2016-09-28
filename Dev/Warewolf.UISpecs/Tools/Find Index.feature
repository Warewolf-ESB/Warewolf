Feature: Find Index
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Find_Index onto a new workflow
	When I "Drag_Toolbox_Find_Index_Onto_DesignSurface"
	Then I "Assert_Find_Index_Exists_OnDesignSurface"
