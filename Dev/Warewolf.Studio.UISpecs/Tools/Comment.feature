Feature: Comment
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Comment onto a new workflow
	When I "Drag_Toolbox_Comment_Onto_DesignSurface"
	Then I "Assert_Comment_Exists_OnDesignSurface"
