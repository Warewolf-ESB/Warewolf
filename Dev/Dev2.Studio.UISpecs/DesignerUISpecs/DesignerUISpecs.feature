Feature: Designer
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@DesignerUISpecs
#Scenario: ChangeWorkflowMappingsAlertsAffectedOnSave
#	Given I have Warewolf running
#	And all tabs are closed
#	And I click "EXPLORER,UI_localhost_AutoID"
#	And I send "InnerWF" to "EXPLORERFILTER"
#	And I double click "EXPLORERFOLDERS,UI_TestCategory_AutoID,UI_InnerWF_AutoID"
#	And I click "VARIABLESCALAR,UI_Variable_result_AutoID,UI_IsInputCheckbox_AutoID"
#	When I click "RIBBONSAVE"
#	Then "UI_DeleteResourceNoBtn_AutoID" is visible within "2" seconds
#	When I click "Show Affected Workflows"
#	Given I click point "95,130" on ""
#	Given I double click point "0,270" on ""
#	Then InnerWf1 should have error icon
#	And InnerWf2 should have error icon
#	And InnerWf2 should have error icon
#	And InnerWf3 should have error icon
#	And InnerWf4 should have error icon
#

#Test will be Opne once Ashley Setup an Automation ID's for Grid Rows
#Scenario: DeleteFirstDatagridRow_Expected_RowIsNotDeleted12
#	Given I have Warewolf running
#	And all tabs are closed
#	And I click "RIBBONNEWENDPOINT"
#	And I double click "TOOLBOX,PART_SearchBox"
#	And I send "Assign" to ""
#	And I drag "TOOLASSIGN" onto "WORKSURFACE,StartSymbol"
#	And "WORKSURFACE,Assign (1)(MultiAssignDesigner),SmallViewContent,SmallDataGrid,UI_ActivityGrid_Row_0_AutoID,UI__Row1_FieldName_AutoID" is visible within "1" seconds
#	#Adding ROW 1
#	Given I type "Delete1" in "WORKSURFACE,Assign (1)(MultiAssignDesigner),SmallViewContent,SmallDataGrid,UI_ActivityGrid_Row_0_AutoID,UI__Row1_FieldName_AutoID"
#	And I send "{TAB}" to ""
#	And "VARIABLESCALAR,UI_Variable_Delete1_AutoID,UI_NameTextBox_AutoID" is visible within "1" seconds
#	And I send "ROW1" to ""
#	#Adding ROW 2
#	And I send "{TAB}" to ""
#	And I send "Delete2{TAB}" to ""
#	And I send "Row2{TAB}" to ""
#	#Deleting Row 1
#	Given I right click point "705,423" on ""
#	And I send "{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{ENTER}" to ""
#	#Checking Expected Row Deleted
#	Given "WORKSURFACE,Assign (1)(MultiAssignDesigner),SmallViewContent,SmallDataGrid,UI_ActivityGrid_Row_0_AutoID,UI__Row1_FieldName_AutoID" contains text "Delete2"
#	#Checking Delete and Inser Row Options are disabled when Grid contains one row only
#	Given I right click "WORKSURFACE,Assign (1)(MultiAssignDesigner),SmallViewContent,SmallDataGrid,UI_ActivityGrid_Row_1_AutoID" 
#	And I send "{TAB}" to ""
#	Then "UI_DeleteRowMenuItem_AutoID" is disabled
#	Then "UI_InsertRowMenuItem_AutoID" is disabled
#	