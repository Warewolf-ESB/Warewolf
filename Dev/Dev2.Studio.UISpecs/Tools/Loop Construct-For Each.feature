Feature: Loop Construct-For Each
	As a Warewolf User order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@ForEach
Scenario: For Each containing calculate tool Exectuing 1k times within expected time
	Given I have Warewolf running
    And all tabs are closed
	Given I click "EXPLORERCONNECTCONTROL"
	Given I click "U_UI_ExplorerServerCbx_AutoID_localhost"
	#Opening New Design Surface
	And I click "RIBBONNEWENDPOINT"
	And I double click "TOOLBOX,PART_SearchBox"
	#Dragging Assign Tool
	And I send "Assign" to ""
	And I drag "TOOLASSIGN" onto "WORKSURFACE,StartSymbol"
	Then "WORKSURFACE,Assign (1)(MultiAssignDesigner),SmallViewContent" is visible
	Given "WORKSURFACE,Assign (1)(MultiAssignDesigner),SmallViewContent,SmallDataGrid,UI_ActivityGridRow_0_AutoID,UI_TextBox_AutoID" is visible within "2" seconds
	Given I type "[[rec().a]]" in "WORKSURFACE,Assign (1)(MultiAssignDesigner),SmallViewContent,SmallDataGrid,UI_ActivityGridRow_0_AutoID,UI_TextBox_AutoID"
	And I send "{TAB}" to ""
	Given "VARIABLERECORDSET,UI_Field_rec().a_AutoID,UI_NameTextBox_AutoID" is visible within "2" seconds
	And I send "1" to ""
	When I double click "TOOLBOX,PART_SearchBox"
	And I send "Each" to ""
	#Dragging For Each Tool
	 Given I drag "TOOLFOREACH" onto "WORKSURFACE,Assign (1)(MultiAssignDesigner)"
	When I double click "TOOLBOX,PART_SearchBox"
	And I send "Calculate" to ""
	#Dragging Calculate Tool into For Each
	And I drag "TOOLCALCULATE" to point "130,80" on "WORKSURFACE,For Each(ForeachDesigner)"
	And "WORKSURFACE,Calculate(CalculateDesigner)" is visible within "2" seconds
	And I type "[[rec().a]]+1" in "WORKSURFACE,For Each(ForeachDesigner),SmallViewContent,UI__DropPoint_AutoID,Calculate(CalculateDesigner),SmallViewContent,UI__fxtxt_AutoID"
	And I type "[[rec().a]]" in "WORKSURFACE,For Each(ForeachDesigner),SmallViewContent,UI__DropPoint_AutoID,Calculate(CalculateDesigner),SmallViewContent,UI__Resulttxt_AutoID"
	And I click "WORKSURFACE,For Each(ForeachDesigner),SmallViewContent,UI__ForEachType_AutoID"
	#Setup For Each to "No. Of Executes" and 1000 times
	And I send "UI_ComboBoxItem_No. of Executes_AutoID{ENTER}" to ""
	And I type "1000" in "WORKSURFACE,For Each(ForeachDesigner),SmallViewContent,UI__ForEachNumberTextbox_AutoID"
	And I send "{F6}" to ""
	##Checking Debug Output is generating as expected
	##And "DEBUGOUTPUT,For Each,Calculate[1]" contains text "[[rec(1001).a]]"


Scenario: For Each Exectuing Workflow in it and debug output is generating within expected time
    Given I have Warewolf running
    And all tabs are closed
	Given I click "EXPLORERCONNECTCONTROL"
	Given I click "U_UI_ExplorerServerCbx_AutoID_localhost"
	#Opening New Design Surface
	And I click "RIBBONNEWENDPOINT"
	And I double click "TOOLBOX,PART_SearchBox"
	And I send "Each" to ""
	#Dragging For Each Tool
	Given I drag "TOOLFOREACH" onto "WORKSURFACE,StartSymbol"
	#Dragging Workflow into For Each
	And I drag "EXPLORER,UI_localhost_AutoID,UI_Examples_AutoID,UI_Utility - Assign_AutoID" to point "130,80" on "WORKSURFACE,For Each(ForeachDesigner)"
	Given I click "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,closeBtn"
	#Setup For Each to "No. Of Executes" and 1000 times
	And I click "WORKSURFACE,For Each(ForeachDesigner),SmallViewContent,UI__ForEachType_AutoID"
	And I send "UI_ComboBoxItem_No. of Executes_AutoID{ENTER}" to ""
	And I type "100" in "WORKSURFACE,For Each(ForeachDesigner),SmallViewContent,UI__ForEachNumberTextbox_AutoID"
	#Dragging Assign Tool
	When I double click "TOOLBOX,PART_SearchBox"
	And I send "{DELETE}" to ""
	And I send "Assign" to "TOOLBOX,PART_SearchBox"
	And I drag "TOOLASSIGN" onto "WORKSURFACE,For Each(ForeachDesigner)"
	Then "WORKSURFACE,Assign (1)(MultiAssignDesigner),SmallViewContent" is visible
	Given "WORKSURFACE,Assign (1)(MultiAssignDesigner),SmallViewContent,SmallDataGrid,UI_ActivityGridRow_0_AutoID,UI_TextBox_AutoID" is visible within "2" seconds
	Given I type "[[pushups]]" in "WORKSURFACE,Assign (1)(MultiAssignDesigner),SmallViewContent,SmallDataGrid,UI_ActivityGridRow_0_AutoID,UI_TextBox_AutoID"
	And I send "{TAB}" to ""
	And I send "[[hero(100).pushups]]" to ""
	And I send "{F6}" to ""
	#Then "DEBUGOUTPUT,Assign" is visible within "25" seconds


	
Scenario:Testing Decision and switch is alowing in foreach
    ###DragADecisionSwitchIntoForEachExpectNotAddedToForEach
	###DragSequenceIntoForEachAndExpectedAddedToForEach
	Given I have Warewolf running
	And all tabs are closed
	Given I click "EXPLORERCONNECTCONTROL"
	Given I click "U_UI_ExplorerServerCbx_AutoID_localhost"
	And I click "RIBBONNEWENDPOINT"
	And I double click "TOOLBOX,PART_SearchBox"
    And I send "For" to ""
    And I drag "TOOLFOREACH" onto "WORKSURFACE,StartSymbol"
	And I double click "TOOLBOX,PART_SearchBox"
    And I send "switch" to ""
	And I drag "TOOLSWITCH" to point "130,80" on "WORKSURFACE,For Each(ForeachDesigner)"
	Then "WORKSURFACE,For Each(ForeachDesigner),SmallViewContent,UI__DropPoint_AutoID,FlowSwitchDesigner" is invisible within "1" seconds
	And I double click "TOOLBOX,PART_SearchBox"
    And I send "Decision" to ""
	And I drag "TOOLDECISION" to point "130,80" on "WORKSURFACE,For Each(ForeachDesigner)"
	Then "WORKSURFACE,For Each(ForeachDesigner),SmallViewContent,UI__DropPoint_AutoID,FlowDecisionDesigner" is invisible within "1" seconds
	And I double click "TOOLBOX,PART_SearchBox"
    And I send "Sequence" to ""
	And I drag "TOOLSEQUENCE" to point "130,80" on "WORKSURFACE,For Each(ForeachDesigner)"
	Then "WORKSURFACE,For Each(ForeachDesigner),SmallViewContent,UI__DropPoint_AutoID,Sequence(SequenceDesigner)" is visible within "1" seconds


Scenario:Testing Decision and switch is alowing in foreach
    ###DragADecisionSwitchIntoForEachExpectNotAddedToForEach
	###DragSequenceIntoForEachAndExpectedAddedToForEach
	Given I have Warewolf running
	And all tabs are closed
	Given I click "EXPLORERCONNECTCONTROL"
	Given I click "U_UI_ExplorerServerCbx_AutoID_localhost"
	And I click "RIBBONNEWENDPOINT"
	And I double click "TOOLBOX,PART_SearchBox"
    And I send "For" to ""
    And I drag "TOOLFOREACH" onto "WORKSURFACE,StartSymbol"
	And I double click "TOOLBOX,PART_SearchBox"
    And I send "switch" to ""
	And I drag "TOOLSWITCH" to point "130,80" on "WORKSURFACE,For Each(ForeachDesigner)"
	Then "WORKSURFACE,For Each(ForeachDesigner),SmallViewContent,UI__DropPoint_AutoID,FlowSwitchDesigner" is invisible within "1" seconds
	And I double click "TOOLBOX,PART_SearchBox"
    And I send "Decision" to ""
	And I drag "TOOLDECISION" to point "130,80" on "WORKSURFACE,For Each(ForeachDesigner)"
	Then "WORKSURFACE,For Each(ForeachDesigner),SmallViewContent,UI__DropPoint_AutoID,FlowDecisionDesigner" is invisible within "1" seconds
	And I double click "TOOLBOX,PART_SearchBox"
    And I send "Sequence" to ""
	And I drag "TOOLSEQUENCE" to point "130,80" on "WORKSURFACE,For Each(ForeachDesigner)"
	Then "WORKSURFACE,For Each(ForeachDesigner),SmallViewContent,UI__DropPoint_AutoID,Sequence(SequenceDesigner)" is visible within "1" seconds













