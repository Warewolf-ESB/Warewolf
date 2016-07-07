Feature: RemoteServer
	One big test covering:
	Deploy
	Dependancy Graph
	Security Settings
	Remote Servers in the Explorer
	
Scenario: Big Remote Server UI Test
	Given I "Assert_Connect_Control_Exists_InExplorer"
	When I "Click_Connect_Control_InExplorer"
	Then I "Assert_Explorer_Remote_Server_DropdownList_Contains_NewRemoteServer"

#Scenario: Selecting "New Remote Server..." From the Remote Server Dropdownbox Opens New Server Source Wizard
#	Given "New Remote Server..." exists in the dropdown list
	When I "Select_NewRemoteServer_From_Explorer_Server_Dropdownlist"
	Then I "Assert_Server_Source_Wizard_Address_Textbox_Exists"

#Scenario: Entering a value into the address textbox of the server source wizard brings up the intellisense
#	Given I "Assert_Server_Source_Wizard_New_Exists"
	When I "Type_tstci_into_Server_Source_Wizard_Address_Textbox"
	Then I "Assert_Server_Source_Wizard_DropdownList_Contains_TSTCIREMOTE"

#Scenario: Selecting an item out of the address textbox intellisense in the server source wizard sets the text
#	Given "TST-CI-REMOTE" exists in the dropdown list
	When I "Select_TSTCIREMOTE_From_Server_Source_Wizard_Dropdownlist"
	Then I "Assert_Server_Source_Wizard_Address_Textbox_Text_Equals_TSTCIREMOTE"
