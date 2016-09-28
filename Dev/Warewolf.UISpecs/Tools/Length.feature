Feature: Length
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Length onto a new workflow
	When I "Drag_Toolbox_Length_Onto_DesignSurface"
	Then I "Assert_Length_Exists_OnDesignSurface"
