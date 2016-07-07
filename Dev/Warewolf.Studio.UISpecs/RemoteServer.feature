Feature: RemoteServer
	One big test covering:
	Connect Control
	Server Source Wizard
	Dependancy Graph
	Deploy
	Security Settings
	Remote Servers in the Explorer
	
Scenario: Big Remote Server UI Test
#Scenario: Explorer dropdownlist contains "New Remote Server..." option
	Given I "Assert_Connect_Control_Exists_InExplorer"
	When I "Click_Connect_Control_InExplorer"
	Then I "Assert_Explorer_Remote_Server_DropdownList_Contains_NewRemoteServer"

#Scenario: Selecting "New Remote Server..." From the Remote Server Dropdownbox Opens New Server Source Wizard
#	Given "New Remote Server..." exists in the dropdown list
	When I "Select_NewRemoteServer_From_Explorer_Server_Dropdownlist"
	Then I "Assert_Server_Source_Wizard_Address_Protocol_Dropdown_Exists"

#Scenario: Http is in the address protocol dropdown list
#	Given I "Assert_Server_Source_Wizard_Address_Protocol_Dropdown_Exists"
	When I "Click_Server_Source_Wizard_Address_Protocol_Dropdown"
	Then I "Assert_Server_Source_Wizard_Address_Protocol_Dropdown_Contains_Http"

#Scenario: Selecting http from the address protocol dropdown list sets the selected item to http
#	Given I "Assert_Server_Source_Wizard_Address_Protocol_Dropdown_Contains_http"
	When I "Select_http_From_Server_Source_Wizard_Address_Protocol_Dropdown"
	Then I "Assert_Server_Source_Wizard_Address_Protocol_Equals_Http"
	And I "Assert_Server_Source_Wizard_Address_Textbox_Exists"

#Scenario: Domain server is in the server source wizard address intellisense
#	Given I "Assert_Server_Source_Wizard_Address_Textbox_Exists"
	When I "Type_tstci_into_Server_Source_Wizard_Address_Textbox"
	Then I "Assert_Server_Source_Wizard_DropdownList_Contains_TSTCIREMOTE"

#Scenario: Selecting an item out of the address textbox intellisense in the server source wizard sets the text
#	Given "TST-CI-REMOTE" exists in the dropdown list
	When I "Select_TSTCIREMOTE_From_Server_Source_Wizard_Dropdownlist"
	Then I "Assert_Server_Source_Wizard_Address_Textbox_Text_Equals_TSTCIREMOTE"
	And I "Assert_Server_Source_Wizard_Test_Connection_Button_Exists"

#Scenario: Clicking test connection button enables save button
#	Given I "Assert_Server_Source_Wizard_Address_Textbox_Text_Equals_TSTCIREMOTE"
#	And I "Assert_Server_Source_Wizard_Test_Connection_Button_Exists"
	When I "Click_Server_Source_Wizard_Test_Connection_Button"
	Then I "Assert_Save_Ribbon_Button_Enabled"

#Scenario: Saving a new server source opens save dialog
#	Given I "Assert_Save_Ribbon_Button_Enabled"
	When I "Click_Save_Ribbon_Button"
	Then I "Assert_SaveDialog_Exists"
	And I "Assert_SaveDialog_ServiceName_Textbox_Exists"

#Scenario: Entering a valid server source name into the save dialog does not set the error state of the textbox to true
#	Given I "Assert_New_Server_Source_Save_Dialog_Exists"
#	And I "Assert_SaveDialog_ServiceName_Textbox_Exists"
	When I "Enter_Servicename_As_TSTCIREMOTE"
	Then I "Assert_SaveDialog_SaveButton_Enabled"

#Scenario: Clicking the save button in the save dialog adds it to the explorer remote servers dropdown list
#	Given I "Assert_SaveDialog_SaveButton_Enabled"
	When I "Click_SaveDialog_YesButton"
#TODO: Remove this workaround
	And I scroll up in the explorer tree
#TODO: Remove this workaround
	And I scroll to the bottom of the explorer tree
	Then "localhost\TSTCIREMOTE" exists in the explorer tree

#Scenario: Server source named TSTCIREMOTE appears in the explorer remote server dropdown list
#	Given "localhost\TSTCIREMOTE" exists in the explorer tree
	When I "Click_Connect_Control_InExplorer"
	Then I "Assert_Explorer_Remote_Server_DropdownList_Contains_TSTCIREMOTE"
	And I "Click_Connect_Control_InExplorer"
	






#These units must go last:
#Scenario: Explorer context menu delete exists
#	Given "localhost\TSTCIREMOTE" exists in the explorer tree
#TODO: Remove this workaround
	When I scroll up in the explorer tree
#TODO: Remove this workaround
	And I scroll to the bottom of the explorer tree
	And I right click "localhost\TSTCIREMOTE" in the explorer tree
	Then I "Assert_ExplorerContextMenu_Delete_Exists"

#Scenario: Clicking delete in the explorer context menu on TSTCIREMOTE server source shows message box
#	Given I "Assert_ExplorerConextMenu_Delete_Exists"
	When I "Select_Delete_FromExplorerContextMenu"
	Then I "Assert_MessageBox_Yes_Button_Exists"

#Scenario: Clicking Yes on the delete prompt dialog removes TSTCIREMOTE from the explorer tree
#	Given I "Assert_MessageBox_Yes_Button_Exists"
	When I "Click_MessageBox_Yes"
	Then "localhost\TSTCIREMOTE" does not exist in the explorer tree

#Scenario: When a server source is deleted from the explorer tree it must be removed from the explorer remote server dropdown list
#	Given "localhost\TSTCIREMOTE" does not exist in the explorer tree
	When I "Click_Connect_Control_InExplorer"
	Then I "Assert_Explorer_Remote_Server_DropdownList_Does_Not_Contain_TSTCIREMOTE"
	And I "Click_Connect_Control_InExplorer"
