Feature: Calculate
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Calculate onto a new workflow
	When I "Drag_Toolbox_Calculate_Onto_DesignSurface"
	Then I "Assert_Calculate_Exists_OnDesignSurface"
