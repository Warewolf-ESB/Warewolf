Feature: Read
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Sharepoint_Read onto a new workflow
	When I "Drag_Toolbox_Sharepoint_Read_Onto_DesignSurface"
	Then I "Assert_Sharepoint_Read_Exists_OnDesignSurface"

#@NeedsSharepoint_ReadToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking Sharepoint_Read Tool Small View on the Design Surface Opens Large View
	When I "Open_Sharepoint_Read_Tool_Large_View"
	Then I "Assert_Sharepoint_Read_Large_View_Exists_OnDesignSurface"