Feature: Control Flow - ForEach
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@NeedsBlankWorkflow
Scenario: Drag toolbox For_Each onto a new workflow
	When I "Drag_Toolbox_For_Each_Onto_DesignSurface"
	Then I "Assert_For_Each_Exists_OnDesignSurface"
