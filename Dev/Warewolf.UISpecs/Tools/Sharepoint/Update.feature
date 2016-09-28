Feature: Update
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Sharepoint_Update onto a new workflow
	When I "Drag_Toolbox_Sharepoint_Update_Onto_DesignSurface"
	Then I "Assert_Sharepoint_Update_Exists_OnDesignSurface"

#@NeedsSharepoint_UpdateToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking Sharepoint_Update Tool Small View on the Design Surface Opens Large View
	When I "Open_Sharepoint_Update_Tool_Large_View"
	Then I "Assert_Sharepoint_Update_Large_View_Exists_OnDesignSurface"
