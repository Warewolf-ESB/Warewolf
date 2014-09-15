Feature: Data-Assign
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Assign
Scenario: AssignCheckVariableAddBadVariableLargeViewValidationErrorSmallViewNoDrillDownInForEach
	Given I have Warewolf running
	And all tabs are closed
	And I click "EXPLORER,UI_localhost_AutoID"
	And I click "RIBBONNEWENDPOINT"
	And I double click "TOOLBOX,PART_SearchBox"
	And I send "Assign" to ""
	And I drag "TOOLASSIGN" onto "WORKSURFACE,StartSymbol"
	Given "WORKSURFACE,Assign (1)(MultiAssignDesigner),SmallViewContent,SmallDataGrid,UI_ActivityGridRow_0_AutoID,UI_TextBox_AutoID" is visible within "2" seconds
	#CheckVariableAdd
	Given I type "myvar" in "WORKSURFACE,Assign (1)(MultiAssignDesigner),SmallViewContent,SmallDataGrid,UI_ActivityGridRow_0_AutoID,UI_TextBox_AutoID"
	And I send "{TAB}" to ""
	Given "VARIABLESCALAR,UI_Variable_myvar_AutoID,UI_NameTextBox_AutoID" is visible within "2" seconds
	And I send "=[[rec(1).set]]+1" to ""
	And I send "{TAB}" to ""
	And I send "5{TAB}" to ""
	And I send "[[5]]{TAB}" to ""
	#BadVariable
	And "VARIABLESCALAR,UI_Variable_5_AutoID" is invisible within "1" seconds
	Given "VARIABLERECORDSET,UI_RecordSet_rec_AutoID,UI_NameTextBox_AutoID" is visible within "1" seconds
	#LargeView
	When I double click point "5,5" on "WORKSURFACE,Assign (2)(MultiAssignDesigner)"
	#ValidationError
	When I click "WORKSURFACE,Assign (2)(MultiAssignDesigner),DoneButton"
	Then "WORKSURFACE,UI_Error0_AutoID" is visible
	When I click point "-40,35" on "WORKSURFACE,UI_CloseHelp_AutoID"
	And I send "{DELETE}{DELETE}{DELETE}{DELETE}{DELETE}" to ""
	And I send "newvar{TAB}" to ""
	And I send "{DELETE}{DELETE}{DELETE}{DELETE}{DELETE}" to ""
	And I send "newvar{TAB}" to ""
	#SmallView
	Then "VARIABLESCALAR,UI_Variable_newvar_AutoID" is visible within "1" seconds
	When I click "WORKSURFACE,Assign (2)(MultiAssignDesigner),DoneButton"
	Then "WORKSURFACE,Assign (2)(MultiAssignDesigner),SmallViewContent" is visible
	#NoDrillDownInForEach [Check that after drop Start is still visible ]
	When I double click "TOOLBOX,PART_SearchBox"
	And I send "Each" to ""
	And I click point "40,10" on "TOOLFOREACH"
	And I drag "TOOLFOREACH" to point "40,400" on "WORKSURFACE"
	And I drag "WORKSURFACE,Assign (2)(MultiAssignDesigner)" to point "130,80" on "WORKSURFACE,For Each(ForeachDesigner)"
	Then "WORKSURFACE,StartSymbol" is visible within "1" seconds
	And "WORKSURFACE,For Each(ForeachDesigner),SmallViewContent,UI__DropPoint_AutoID,Assign (2)(MultiAssignDesigner)" is visible
		