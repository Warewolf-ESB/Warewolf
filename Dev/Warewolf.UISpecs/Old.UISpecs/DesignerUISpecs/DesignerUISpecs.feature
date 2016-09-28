﻿Feature: Designer
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@DesignerUISpecs
Scenario: ChangeWorkflowMappingsAlertsAffectedOnSave
	Given I have Warewolf running
	And all tabs are closed
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I click "EXPLORER,UI_localhost_AutoID"
	##Searching Workflow with the name "InnerWF" in explorer search
	And I send "InnerWF" to "EXPLORERFILTER"
	##Opening WF from explorer
	And I double click "EXPLORERFOLDERS,UI_Acceptance Testing Resources_AutoID,UI_InnerWF_AutoID"
	#Changing "InnerWF" mappings and save, expected popup message as dependency workflows affected
	And I click "VARIABLESCALAR,UI_Variable_result_AutoID,UI_IsInputCheckbox_AutoID"
	When I click "RIBBONSAVE"
	Then "UI_DeleteResourceNoBtn_AutoID" is visible within "2" seconds
	When I click "UI_ResourceChangedWarningDialog_AutoID,UI_ShowAffectedWorkflowsButton_AutoID"
	Given I double click point "500,96" on "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,myScrollViewer"
	Given I double click point "482,121" on "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,myScrollViewer"
	Given I double click point "649,200" on "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,myScrollViewer"
	#Then "WORKFLOWDESIGNER,ServiceExecutionTest(FlowchartDesigner)" is visible within "4" seconds
	#Then InnerWf1 should have error icon
	#Then "WORKFLOWDESIGNER,ServiceExecutionTest(FlowchartDesigner),InnerWF(ServiceDesigner)[0],SmallViewContent,UI_FixErrors_AutoID,UI_ErrorsAdorner_AutoID" is visible within "2" seconds
	#And "WORKFLOWDESIGNER,ServiceExecutionTest(FlowchartDesigner),InnerWF(ServiceDesigner)[1],SmallViewContent,UI_FixErrors_AutoID,UI_ErrorsAdorner_AutoID" is visible within "2" seconds
	#And "WORKFLOWDESIGNER,ServiceExecutionTest(FlowchartDesigner),InnerWF(ServiceDesigner)[2],SmallViewContent,UI_FixErrors_AutoID,UI_ErrorsAdorner_AutoID" is visible within "2" seconds
	#And "WORKFLOWDESIGNER,ServiceExecutionTest(FlowchartDesigner),InnerWF(ServiceDesigner)[3],SmallViewContent,UI_FixErrors_AutoID,UI_ErrorsAdorner_AutoID" is visible within "2" seconds
	



##Test will be Open once Ashley Setup an Automation ID"s for Grid Rows
Scenario: DeleteFirstDatagridRow_Expected_RowIsNotDeleted12
	Given I have Warewolf running
	And all tabs are closed
	And I click "RIBBONNEWENDPOINT"
	And I double click "TOOLBOX,PART_SearchBox"
	And I send "Assign" to ""
	And I drag "TOOLASSIGN" onto "WORKSURFACE,StartSymbol"
	And "WORKSURFACE,Assign (1)(MultiAssignDesigner),SmallViewContent,SmallDataGrid,UI_ActivityGridRow_0_AutoID,UI_TextBox_AutoID" is visible within "1" seconds
	#Adding ROW 1
	Given I type "Delete1" in "WORKSURFACE,Assign (1)(MultiAssignDesigner),SmallViewContent,SmallDataGrid,UI_ActivityGridRow_0_AutoID,UI_TextBox_AutoID"
	And I send "{TAB}" to ""
	And "VARIABLESCALAR,UI_Variable_Delete1_AutoID,UI_NameTextBox_AutoID" is visible within "1" seconds
	And I send "ROW1" to ""
	#Adding ROW 2
	And I send "{TAB}" to ""
	And I send "Delete2{TAB}" to ""
	And I send "Row2{TAB}" to ""
	#Deleting Row 1
	Given I right click "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,UI_WorkflowDesigner_AutoID,UserControl_1,Unsaved 1(FlowchartDesigner),Assign (2)(MultiAssignDesigner),SmallViewContent,SmallDataGrid,UI_ActivityGridRow_0_AutoID,UI_DataGridCell_AutoID[2]"
	And I send "{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{ENTER}" to ""
	#Checking Expected Row Deleted
	Given "WORKSURFACE,Assign (1)(MultiAssignDesigner),SmallViewContent,SmallDataGrid,UI_ActivityGridRow_0_AutoID,UI_TextBox_AutoID" contains text "Delete2"
	#Checking Delete and Insert Row Options are disabled when Grid contains one row only
	Given I right click "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,UI_WorkflowDesigner_AutoID,UserControl_1,Unsaved 1(FlowchartDesigner),Assign (1)(MultiAssignDesigner),SmallViewContent,SmallDataGrid,UI_ActivityGridRow_0_AutoID,UI_DataGridCell_AutoID[2]" 
	And I send "{TAB}{TAB}" to ""
	Given "UI_DeleteRowMenuItem_AutoID" is disabled
	Then "UI_InsertRowMenuItem_AutoID" is disabled within "2" seconds	



Scenario: Drag resource multiple times from explorer and expected mappings are not changing
	Given I have Warewolf running
	And all tabs are closed
	And I click "EXPLORERFILTERCLEARBUTTON"
	And "EXPLORER,UI_localhost_AutoID" is visible within "5" seconds
	And I click "EXPLORER,UI_localhost_AutoID"
	And I click new "Workflow"
	And I send "Utility - Assign" to "EXPLORERFILTER"
	Given I drag "EXPLORER,UI_localhost_AutoID,UI_Examples_AutoID,UI_Utility - Assign_AutoID" onto "WORKSURFACE,StartSymbol"
	Given "WORKSURFACE,Examples\Utility - Assign(ServiceDesigner),LargeViewContent" is visible
	##Testing Row1 Vriable
	Given "WORKSURFACE,Examples\Utility - Assign(ServiceDesigner),LargeViewContent,OutputsDataGrid,UI_ActivityGridRow_0_AutoID,UI_DataGridCell_AutoID[1]" contains text "[[rec().set]]"
	#Testing Row1 Vriable
	Given "WORKSURFACE,Examples\Utility - Assign(ServiceDesigner),LargeViewContent,OutputsDataGrid,UI_ActivityGridRow_1_AutoID,UI_DataGridCell_AutoID[1]" contains text "[[hero().pushups]]"
	#Testing Row1 Vriable
	Given "WORKSURFACE,Examples\Utility - Assign(ServiceDesigner),LargeViewContent,OutputsDataGrid,UI_ActivityGridRow_2_AutoID,UI_DataGridCell_AutoID[1]" contains text "[[hero().name]]"
	Given I drag "EXPLORER,UI_localhost_AutoID,UI_Examples_AutoID,UI_Utility - Assign_AutoID" onto "WORKSURFACE,Examples\Utility - Assign(ServiceDesigner)"
	Given "WORKSURFACE,Examples\Utility - Assign(ServiceDesigner)[1],LargeViewContent" is visible
	##Testing Row1 Vriable
	Given "WORKSURFACE,Examples\Utility - Assign(ServiceDesigner)[1],LargeViewContent,OutputsDataGrid,UI_ActivityGridRow_0_AutoID,UI_DataGridCell_AutoID[1]" contains text "[[rec().set]]"
	#Testing Row1 Vriable
	Given "WORKSURFACE,Examples\Utility - Assign(ServiceDesigner)[1],LargeViewContent,OutputsDataGrid,UI_ActivityGridRow_1_AutoID,UI_DataGridCell_AutoID[1]" contains text "[[hero().pushups]]"
	#Testing Row1 Vriable
	Given "WORKSURFACE,Examples\Utility - Assign(ServiceDesigner)[1],LargeViewContent,OutputsDataGrid,UI_ActivityGridRow_2_AutoID,UI_DataGridCell_AutoID[1]" contains text "[[hero().name]]"

#Bug 18272
Scenario: Draging out the TAB is expected not to shutdown the studio 
	Given I have Warewolf running
	And all tabs are closed	
	And "EXPLORER,UI_localhost_AutoID" is visible within "20" seconds
	And I click "EXPLORER,UI_localhost_AutoID"
	And I click new "Workflow"
	#Opening StartPage
	And I double click point "968,51" on "MouseOverBorder"
	#Opening Hello World workflow
	Given I send "Hello World" to "EXPLORERFILTER"
	And I double click "EXPLORERFOLDERS,UI_Hello World_AutoID" 
	#Dragging hello World tab 
	#Given I drag click point "60,2" on "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID" to "WORKSURFACE"
    #And I drag "ACTIVETAB" onto "WORKSURFACE"
	Then "RIBBONNEWENDPOINT" is visible


Scenario: Testing NewWorkflow ShortcutKey Works as Expected
	###NewWorkflowShortcutKeyExpectedWorkflowOpens
	Given I have Warewolf running
	And all tabs are closed	
	And I click new "Workflow"
    Given I send "{CTRL}W" to "WORKSURFACE"
	Then "WORKFLOWDESIGNER,Unsaved 2(FlowchartDesigner)" is visible within "2" seconds
    Given I send "{CTRL}{SHIFT}W" to "WORKFLOWDESIGNER"
	Then "WebBrowserWindow" is visible within "2" seconds
	Given I send "{ESC}" to "WebBrowserWindow"
    Given I send "{CTRL}{SHIFT}D" to "WORKFLOWDESIGNER"
	Then "WebBrowserWindow" is visible within "2" seconds
	Given I send "{ESC}" to "WebBrowserWindow"
	Given I send "{CTRL}{SHIFT}P" to "WORKFLOWDESIGNER"
	Then "WebBrowserWindow" is visible within "2" seconds
	Given I send "{ESC}" to "WebBrowserWindow"
	Given I send "{CTRL}S" to ""
	Then "WebBrowserWindow" is visible within "2" seconds
	Given I send "{ESC}" to "WebBrowserWindow"
	Given I send "{CTRL}D" to ""
	Then "DEPLOYSOURCE" is visible within "2" seconds












	

	





	















