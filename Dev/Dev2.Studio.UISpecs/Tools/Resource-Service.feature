Feature: Resource-Workflow
	In order to be able to use warewolf
	As a warewolf user
	I want to be able to Drag Workflow Tool onto design surface

@Services
Scenario: Drag Service to design surface and checking Workflows are not showing in Services resource picker
    Given I have Warewolf running
	Given all tabs are closed
	Given I click "EXPLORERCONNECTCONTROL"
	Given I click "U_UI_ExplorerServerCbx_AutoID_localhost"
	#Opening New Workflow
	Given I click new "Workflow"
	#Searching Tool Service by using Toolbox search
	When I send "service" to "TOOLBOX,PART_SearchBox"
	#Dragging Service Tool to Design Surface
	Given I drag "TOOLSERVICES" onto "ACTIVETAB,UI_WorkflowDesigner_AutoID,UserControl_1,scrollViewer,ActivityTypeDesigner,WorkflowItemPresenter,StartSymbol"
	#Checking WORKFLOW resources is invisible in Service resource picker
	And I send "Decision Testing" to "UI_SelectServiceWindow_AutoID,UI_NavigationViewUserControl_AutoID,UI_DatalistFilterTextBox_AutoID,UI_TextBox_AutoID"
	And "RESOURCEPICKERFOLDERS,UI_Acceptance Testing Resources_AutoID" is invisible within "2" seconds
    And I click "UI_SelectServiceWindow_AutoID,UI_NavigationViewUserControl_AutoID,UI_DatalistFilterTextBox_AutoID,UI_FilterButton_AutoID"
	And I send "Control" to "UI_SelectServiceWindow_AutoID,UI_NavigationViewUserControl_AutoID,UI_DatalistFilterTextBox_AutoID"
	Then "RESOURCEPICKERFOLDERS,UI_Examples_AutoID,UI_Control Flow - Decision_AutoID" is invisible within "2" seconds
	Given I click "TOOLWORKFLOWRISOURCEPICKFILTER"
	#Selecting Service in Resource Picker
    And I send "DBServices" to "UI_SelectServiceWindow_AutoID,UI_NavigationViewUserControl_AutoID,UI_DatalistFilterTextBox_AutoID"
	And "RESOURCEPICKERFOLDERS,UI_Acceptance Testing Resources_AutoID,UI_bug9394DBServicesCall_AutoID" is visible within "10" seconds
	And I double click "RESOURCEPICKERFOLDERS,UI_Acceptance Testing Resources_AutoID,UI_bug9394DBServicesCall_AutoID"
	And I click "UI_SelectServiceWindow_AutoID,UI_SelectServiceOKButton_AutoID"
	#Checking Selected Service by using resource picker is come through to design surface or not
	And "WORKSURFACE,Acceptance Testing Resources\bug9394DBServicesCall(ServiceDesigner)" is visible within "2" seconds

		