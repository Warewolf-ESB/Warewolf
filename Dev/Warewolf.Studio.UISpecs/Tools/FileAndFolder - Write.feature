Feature: FileAndFolder - Write
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Write_File onto a new workflow
	When I "Drag_Toolbox_Write_File_Onto_DesignSurface"
	Then I "Assert_Write_File_Exists_OnDesignSurface"

#@NeedsWrite_FileToolSmallViewOnTheDesignSurface
#Scenario: Double Clicking Write_File Tool Small View on the Design Surface Opens Large View
	When I "Open_Write_File_Tool_Large_View"
	Then I "Assert_Write_File_Large_View_Exists_OnDesignSurface"
