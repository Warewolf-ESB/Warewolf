Feature: Count
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Count_Records onto a new workflow
	When I "Drag_Toolbox_Count_Records_Onto_DesignSurface"
	Then I "Assert_Count_Records_Exists_OnDesignSurface"
