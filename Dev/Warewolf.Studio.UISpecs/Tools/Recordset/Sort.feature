Feature: Sort
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Sort_Record onto a new workflow
	When I "Drag_Toolbox_Sort_Record_Onto_DesignSurface"
	Then I "Assert_Sort_Records_Exists_OnDesignSurface"
