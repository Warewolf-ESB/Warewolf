Feature: Create JSON
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@NeedsBlankWorkflow
Scenario: Drag toolbox create JSON onto a new workflow
	When I "Drag_Toolbox_JSON_Onto_DesignSurface"
	Then I "Assert_Create_JSON_Exists_OnDesignSurface"

#@NeedsCreateJSONToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking Create JSON Tool Small View on the Design Surface Opens Large View
	When I "Open_Json_Tool_Large_View"
	Then I "Assert_Json_Large_View_Exists_OnDesignSurface"

#@NeedsCreateJSONLargeViewOnTheDesignSurface
#Scenario: Click Create JSON Tool QVI Button Opens Qvi
	When I "Open_Json_Tool_Qvi_Large_View"
	Then I "Assert_Json_Qvi_Large_View_Exists_OnDesignSurface"
