Feature: SystemInformation
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@NeedsBlankWorkflow
Scenario: Drag toolbox System_Information onto a new workflow
	When I "Drag_Toolbox_System_Information_Onto_DesignSurface"
	Then I "Assert_System_information_Exists_OnDesignSurface"

#@NeedsSystem_InformationToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking System_Information Tool Small View on the Design Surface Opens Large View
	When I "Open_System_Information_Tool_Qvi_Large_View"
	Then I "Assert_System_Info_Qvi_Large_View_Exists_OnDesignSurface"
