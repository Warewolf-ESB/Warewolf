Feature: Resource-Service
	In order to be able to use Service
	As a warewolf user
	I want to be able to Drag Service Tool onto design surface
@Workflow
Scenario: Drag Workflow to design surface and checking services are not showing in workflow resource picker
    Given I have Warewolf running
	Given all tabs are closed
    And I click "EXPLORER,UI_localhost_AutoID"
	Given I click new "Workflow"
	When I send "workflow" to "TOOLBOX,PART_SearchBox"
	Given I drag "TOOLWORKFLOW" onto "ACTIVETAB,UI_WorkflowDesigner_AutoID,UserControl_1,scrollViewer,ActivityTypeDesigner,WorkflowItemPresenter,StartSymbol"
    And I send "Control" to "UI_SelectServiceWindow_AutoID,UI_NavigationViewUserControl_AutoID,UI_DatalistFilterTextBox_AutoID"
	Then "RESOURCEPICKERFOLDERS,UI_Examples_AutoID,UI_Control Flow - Decision_AutoID" is visible
	Given I click "TOOLWORKFLOWRISOURCEPICKFILTER"
    And I send "DBSERVICES" to "UI_SelectServiceWindow_AutoID,UI_NavigationViewUserControl_AutoID,UI_DatalistFilterTextBox_AutoID"
	Given "RESOURCEPICKERFOLDERS" is invisible within "2" seconds
	Given I click point "324,11" on "UI_SelectServiceWindow_AutoID"
	And I drag "TOOLWORKFLOW" onto "ACTIVETAB,UI_WorkflowDesigner_AutoID,UserControl_1,scrollViewer,ActivityTypeDesigner,WorkflowItemPresenter,StartSymbol"
	And I double click "RESOURCEPICKERFOLDERS,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	And I click "UI_SelectServiceWindow_AutoID,UI_SelectServiceOKButton_AutoID"
	And "WORKSURFACE,BARNEY\Decision Testing(ServiceDesigner)" is visible within "2" seconds

	