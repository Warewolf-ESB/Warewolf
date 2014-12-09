Feature: Resource-Service
	In order to be able to use Service
	As a warewolf user
	I want to be able to Drag Service Tool onto design surface
@Workflow
Scenario: Drag Workflow to design surface and checking services are not showing in workflow resource picker
    Given I have Warewolf running
	Given all tabs are closed
	Given I click "EXPLORERFILTERCLEARBUTTON"  
	Given I click "EXPLORERCONNECTCONTROL"
	Given I click "U_UI_ExplorerServerCbx_AutoID_localhost"
	##Opening New Workflow
	Given I click new "Workflow"
	##Dragging Tool Workflow to design surface
	When I send "workflow" to "TOOLBOX,PART_SearchBox"
	Given I drag "TOOLWORKFLOW" onto "ACTIVETAB,UI_WorkflowDesigner_AutoID,UserControl_1,scrollViewer,ActivityTypeDesigner,WorkflowItemPresenter,StartSymbol"
    ##Testing resource picker is filtering as expected
    And I send "Control Flow - Decision" to "UI_SelectServiceWindow_AutoID,UI_NavigationViewUserControl_AutoID,UI_DatalistFilterTextBox_AutoID"
	Then "RESOURCEPICKERFOLDERS,UI_Examples_AutoID,UI_Control Flow - Decision_AutoID" is visible
	Given I click "TOOLWORKFLOWRISOURCEPICKFILTER"
	##Searching for DB service by using filter, expected DB services are not visible
    And I send "DBSERVICES" to "UI_SelectServiceWindow_AutoID,UI_NavigationViewUserControl_AutoID,UI_DatalistFilterTextBox_AutoID"
	Given "RESOURCEPICKERFOLDERS" is invisible within "2" seconds
	And I send "{ESC}" to ""
	And I drag "TOOLWORKFLOW" onto "ACTIVETAB,UI_WorkflowDesigner_AutoID,UserControl_1,scrollViewer,ActivityTypeDesigner,WorkflowItemPresenter,StartSymbol"
	##Searching a workflow in resource picker and selecting
	And I send "Decision Testing" to "UI_SelectServiceWindow_AutoID,UI_NavigationViewUserControl_AutoID,UI_DatalistFilterTextBox_AutoID"
	And I double click "RESOURCEPICKERFOLDERS,UI_Acceptance Testing Resources_AutoID,UI_Decision Testing_AutoID"
	And I click "UI_SelectServiceWindow_AutoID,UI_SelectServiceOKButton_AutoID"
	And "WORKSURFACE,Acceptance Testing Resources\Decision Testing(ServiceDesigner)" is visible within "2" seconds

	