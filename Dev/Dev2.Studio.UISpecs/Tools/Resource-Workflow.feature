Feature: Resource-Workflow
	In order to be able to use warewolf
	As a warewolf user
	I want to be able to Drag Workflow Tool onto design surface

Scenario: Drag Workflow to design surface and checking services are not showing in workflow resource picker
    Given I have Warewolf running
	Given I click new "Workflow"
	And I send "Workflow" to "TOOLBOX,PART_SearchBox"
	And I drag "TOOLWORKFLOW" onto "ACTIVETAB,UI_WorkflowDesigner_AutoID,UserControl_1,scrollViewer,ActivityTypeDesigner,WorkflowItemPresenter,StartSymbol"
	And I double click "RESOURCEPICKERFOLDERS,UI_BARNEY_AutoID,UI_Decision Testing_AutoID"
	And I click "UI_SelectServiceWindow_AutoID,UI_SelectServiceOKButton_AutoID"
	And I drag "TOOLBOX,PART_Tools,Unlimited.Applications.BusinessDesignStudio.Activities.DsfWorkflowActivity" onto "ACTIVETAB,UI_WorkflowDesigner_AutoID,UserControl_1,scrollViewer,ActivityTypeDesigner,WorkflowItemPresenter,StartSymbol"
    And I send "Control" to "UI_SelectServiceWindow_AutoID,UI_NavigationViewUserControl_AutoID,UI_DatalistFilterTextBox_AutoID,UI_TextBox_AutoID"
	Then "RESOURCEPICKERFOLDERS,UI_EXAMPLES_AutoID,UI_Control Flow - Decision_AutoID" is visible
	And I click "UI_SelectServiceWindow_AutoID,UI_NavigationViewUserControl_AutoID,UI_DatalistFilterTextBox_AutoID,UI_FilterButton_AutoID"
	And I click "RESOURCEPICKERFOLDERS,UI_DBSERVICES_AutoID,Expander"
	And "RESOURCEPICKERFOLDERS,UI_DBSERVICES_AutoID,UI_MapLocations_AutoID" is not visible	
	And close the Studio and Server