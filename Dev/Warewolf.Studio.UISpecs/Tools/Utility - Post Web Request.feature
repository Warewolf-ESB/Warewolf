Feature: Utility - Post Web Request
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Post Web Request Tool onto a new workflow creates Post Web Request tool with large view on the design surface
	When I "Drag_PostWeb_RequestTool_Onto_DesignSurface"
	Then I "Assert_PostWeb_RequestTool_Small_View_Exists_OnDesignSurface"

#@NeedsPostWebRequestToolLargeViewOnTheDesignSurface
#Scenario: Double Clicking Post Web Request Tool Large View on the Design Surface Collapses it to Small View
	When I "Open_PostWeb_RequestTool_Large_View"
	Then I "Assert_PostWeb_RequestTool_Large_View_Exists_OnDesignSurface"
