Feature: Utility Web Request
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Web_Request onto a new workflow
	When I "Drag_Toolbox_Web_Request_Onto_DesignSurface"
	Then I "Assert_Web_Request_Exists_OnDesignSurface"

#@NeedsWeb_RequestToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking Web_Request Tool Small View on the Design Surface Opens Large View
	When I "Open_WebRequest_LargeView"
	Then I "Assert_Web_Request_Large_View_Exists_OnDesignSurface"
