Feature: Date and Time Difference
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@NeedsBlankWorkflow
Scenario: Drag toolbox DateTime_Difference onto a new workflow
	When I "Drag_Toolbox_DateTime_Difference_Onto_DesignSurface"
	Then I "Assert_DateTime_Difference_Conversion_Exists_OnDesignSurface"
