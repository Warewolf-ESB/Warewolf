Feature: Create
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Sharepoint_Create onto a new workflow
	When I "Drag_Toolbox_Sharepoint_Create_Onto_DesignSurface"
	Then I "Assert_Sharepoint_Create_Exists_OnDesignSurface"

#@NeedsSharepoint_CreateToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking Sharepoint_Create Tool Small View on the Design Surface Opens Large View
	When I "Open_Sharepoint_Create_Tool_Large_View"
	Then I "Assert_Sharepoint_Create_Large_View_Exists_OnDesignSurface"
