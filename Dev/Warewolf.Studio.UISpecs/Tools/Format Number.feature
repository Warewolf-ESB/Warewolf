Feature: Format Number
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Format_Number onto a new workflow
	When I "Drag_Toolbox_Format_Number_Onto_DesignSurface"
	Then I "Assert_Format_Number_Exists_OnDesignSurface"
