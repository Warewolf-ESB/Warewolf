Feature: Date and Time
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Date_And_Time onto a new workflow
	When I "Drag_Toolbox_Date_And_Time_Onto_DesignSurface"
	Then I "Assert_Date_And_Time_Exists_OnDesignSurface"
