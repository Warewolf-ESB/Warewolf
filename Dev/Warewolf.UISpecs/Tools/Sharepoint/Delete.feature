Feature: Delete
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Sharepoint_Delete onto a new workflow
	When I "Drag_Toolbox_Sharepoint_Delete_Onto_DesignSurface"
	Then I "Assert_Sharepoint_Delete_Exists_OnDesignSurface"

#@NeedsSharepoint_DeleteToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking Sharepoint_Delete Tool Small View on the Design Surface Opens Large View
	When I "Open_Sharepoint_Delete_Tool_Large_View"
	Then I "Assert_Sharepoint_Delete_Large_View_Exists_OnDesignSurface"
