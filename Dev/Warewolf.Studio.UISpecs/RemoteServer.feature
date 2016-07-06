Feature: RemoteServer
	One big test covering:
	Deploy
	Dependancy Graph
	Security Settings
	Remote Servers in the Explorer
	
Scenario: Big Remote Server UI Test
	Given I "Assert_Connect_Control_Exists_InExplorer"
	When I "Click_Connect_Control_InExplorer"
	Then "New Remote Server..." exists in the dropdown list

#Scenario: Selecting "New Remote Server..." From the Remote Server Dropdownbox Opens New Server Source Wizard
#	Given "New Remote Server..." exists in the dropdown list
	When I select "New Remote Server..." from the dropdown list
	Then I "Assert_ServerSourceWizard_Exists"

#Scenario: Selecting "Create New..." From the Remote Server Dropdownbox Opens New Server Source Wizard
#	Given "Create New..." exists in the dropdown list
	When I select "Create New..." from the dropdown list
	Then I "Assert_ServerSourceWizrd_Exists"

	Given I "Assert_NewWorkFlow_RibbonButton_Exists"
	When I "Click_New_Workflow_Ribbon_Button"
	Then I "Assert_StartNode_Exists"
