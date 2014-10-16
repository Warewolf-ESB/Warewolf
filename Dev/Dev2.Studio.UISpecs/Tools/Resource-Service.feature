Feature: Resource-Workflow
	In order to be able to use warewolf
	As a warewolf user
	I want to be able to Drag Workflow Tool onto design surface

@Services
Scenario: Drag Service to design surface and checking Workflows are not showing in Services resource picker
    Given I have Warewolf running
	Given all tabs are closed
    And I click "EXPLORER,UI_localhost_AutoID"
	Given I click new "Workflow"
	When I send "service" to "TOOLBOX,PART_SearchBox"
	Given I drag "TOOLSERVICES" onto "ACTIVETAB,UI_WorkflowDesigner_AutoID,UserControl_1,scrollViewer,ActivityTypeDesigner,WorkflowItemPresenter,StartSymbol"
	# Below Step needs plumbing 
	#And "RESOURCEPICKERFOLDERS,UI_BARNEY_AutoID" is invisible within "2" seconds
    And I send "Control" to "UI_SelectServiceWindow_AutoID,UI_NavigationViewUserControl_AutoID,UI_DatalistFilterTextBox_AutoID"
	#Then "RESOURCEPICKERFOLDERS,UI_Examples_AutoID,UI_Control Flow - Decision_AutoID" is invisible within "2" seconds
	Given I click "TOOLWORKFLOWRISOURCEPICKFILTER"
    And I send "DBServices" to "UI_SelectServiceWindow_AutoID,UI_NavigationViewUserControl_AutoID,UI_DatalistFilterTextBox_AutoID"
	And "RESOURCEPICKERFOLDERS,UI_DATABASE_AutoID,UI_bug9394DBServicesCall_AutoID" is visible within "2" seconds
	And I double click "RESOURCEPICKERFOLDERS,UI_DATABASE_AutoID,UI_bug9394DBServicesCall_AutoID"
	And I click "UI_SelectServiceWindow_AutoID,UI_SelectServiceOKButton_AutoID"
	And "WORKSURFACE,DATABASE\bug9394DBServicesCall(ServiceDesigner)" is visible within "2" seconds

		