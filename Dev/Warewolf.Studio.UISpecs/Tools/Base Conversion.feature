Feature: Base Conversion
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@NeedsBlankWorkflow
Scenario: Drag toolbox base conversion onto a new workflow creates base conversion tool with small view on the design surface
	When I "Drag_Toolbox_Base_Conversion_Onto_DesignSurface"
	Then I "Assert_Base_Conversion_Exists_OnDesignSurface"

#@NeedsBaseConversionSmallViewOnTheDesignSurface
#Scenario: Double Clicking Base Conversion Tool Small View on the Design Surface Opens Large View
	When I "Open_Base_Conversion_Tool_Qvi_Large_View"
	Then I "Assert_Base_Conversion_Qvi_Large_View_Exists_OnDesignSurface"
