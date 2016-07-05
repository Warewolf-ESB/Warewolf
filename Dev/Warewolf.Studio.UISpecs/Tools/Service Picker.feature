Feature: Service Picker
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Service Picker onto a new workflow
	When I "Drag_Toolbox_Service_Picker_Onto_DesignSurface"
	Then I "Assert_Service_Picker_Cancel_Button_Exists"

#@NeedsServicePickerDialog
#Scenario: Click Cancel Service Picker Dialog Creates Service Picker on Design Surface
	#Given I "Assert_Service_Picker_Cancel_Button_Exists"
	When I "Click_Cancel_Service_Picker_Dialog"
	Then I "Assert_Service_Picker_Exists_OnDesignSurface"
