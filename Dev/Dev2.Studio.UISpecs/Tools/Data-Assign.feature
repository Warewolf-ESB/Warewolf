Feature: Data-Assign
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Assign
Scenario: CheckLargeViewDataEntryDragAndDropSmallView
	#Given I have Warewolf running
	#And all tabs are closed
	#And I click "RIBBONNEWENDPOINT"
	#And I double click "TOOLBOX,PART_SearchBox"
	#And I send "Assign" to ""
	#Given I drag "TOOLMULTIASSIGN" onto "WORKSURFACE"
	#And "WORKSURFACE,Assign (1)(MultiAssignDesigner),SmallViewContent,SmallDataGrid,UI_ActivityGrid_Row_0_AutoID,UI__Row1_FieldName_AutoID" is visible within "1" seconds
	#Given I type "myvar" in "WORKSURFACE,Assign (1)(MultiAssignDesigner),SmallViewContent,SmallDataGrid,UI_ActivityGrid_Row_0_AutoID,UI__Row1_FieldName_AutoID"
	#And I send "{TAB}" to ""
	#And "VARIABLESCALAR,UI_Variable_myvar_AutoID" is visible within "1" seconds
	#And I send "=[[rec(1).set]]+1" to ""
	#And I send "{TAB}" to ""
	#And I send "5{TAB}" to ""
	#And "VARIABLESCALAR,UI_Variable_5_AutoID" is invisible within "1" seconds
	#And "VARIABLERECORDSET,UI_RecordSet_rec_AutoID,UI_Field_rec().set_AutoID" is visible within "1" seconds
	#And I click "WORKSURFACE,Assign (2)(MultiAssignDesigner)"
	Given I right click "WORKSURFACE,Assign (2)(MultiAssignDesigner)"
	And I click "UI_ShowLargeViewMenuItem_AutoID"
	And I click point "5,5" on "WORKSURFACE,Assign (2)(MultiAssignDesigner),LargeViewContent,LargeDataGrid,UI_ActivityGrid_Row_1_AutoID,UI__Row2_FieldName_AutoID"