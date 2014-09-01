Feature: Data-Base Conversion
	In order to use Base Conversion Tool
	As a Warewolf User
	I want to be able to use Base Convert Tool

Scenario: DragOnBaseCovert1
    Given I have Warewolf running
	Given all tabs are closed
	And I click new "Workflow"
	And I send "Base" to "TOOLBOX,PART_SearchBox"
	When I drag "TOOLBASECONVERT" onto "ACTIVETAB,UI_WorkflowDesigner_AutoID,UserControl_1,scrollViewer,ActivityTypeDesigner,WorkflowItemPresenter,StartSymbol"
	Given I send "[[rec().a]]" to "ACTIVETAB,ActivityTypeDesigner,WorkflowItemPresenter,Unsaved 1(FlowchartDesigner),Base Conversion (1)(BaseConvertDesigner),SmallViewContent,SmallDataGrid,UI__Row1_FromExpressiontxt_AutoID"
	
