Feature: DotNet Dll
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@NeedsBlankWorkflow
Scenario: Drag toolbox DotNet Dll Tool onto a new workflow creates base conversion tool with small view on the design surface
	When I "Drag_DotNet_DLL_Connector_Onto_DesignSurface"
	Then I "Assert_DotNet_DLL_Connector_Exists_OnDesignSurface"

#@NeedsDotNetDllToolLargeViewOnTheDesignSurface
#Scenario: Double Clicking DotNet Dll Tool Large View on the Design Surface Collapses it to Small View
	When I "Open_DotNet_DLL_Connector_Tool_Small_View"
	Then I "Assert_DotNet_DLL_Connector_Exists_OnDesignSurface"
