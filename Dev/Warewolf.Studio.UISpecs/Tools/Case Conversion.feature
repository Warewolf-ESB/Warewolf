Feature: Case Conversion
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Case Conversion onto a new workflow creates Case Conversion tool with small view on the design surface
	When I "Drag_Toolbox_Case_Conversion_Onto_DesignSurface"
	Then I "Assert_Case_Conversion_Exists_OnDesignSurface"

#@NeedsPostWebRequestToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking Post Web Request Tool Small View on the Design Surface Opens Large View
	When I "Open_Case_Conversion_Tool_Qvi_Large_View"
	Then I "Assert_Case_Conversion_Qvi_Large_View_Exists_OnDesignSurface"
