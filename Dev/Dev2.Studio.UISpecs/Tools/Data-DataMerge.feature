Feature: DataMerge
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@mytag
Scenario: DataMerge Large View Invalid Variables Expected Error On done Button
    Given I have Warewolf running
	And all tabs are closed	
	And I click "RIBBONNEWENDPOINT"
	#Dragging DataMerge Tool From Tool Box
	Given I send "Data" to "TOOLBOX,PART_SearchBox"
    Given I drag "TOOLDATAMERGE" onto "WORKSURFACE,StartSymbol"
	#Passing Variables In Small View
	Given I type "[[rec@(1).a]]" in "TOOLDATAMERGESMALLVIEWGRID,UI__Row1_InputVariable_AutoID"
	And I send "{TAB}{TAB}" to ""
	And I send "8" to ""
	And I send "{TAB}" to ""
	And I send "[[rec(2).a]]" to ""
	And I send "{TAB}{TAB}" to ""
	And I send "10" to ""
	Given I type "[[result]]" in "WORKSURFACE,Data Merge (2)(DataMergeDesigner),SmallViewContent,UI__Resulttxt_AutoID"
	#Opening Larger View
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Data Merge (2)(DataMergeDesigner)"
	##Bug12561 #Checking Validation Errors For Invalid Input Variable
	##And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Data Merge (2)(DataMergeDesigner),DoneButton"
	##Given "WORKSURFACE,UI_Error0_AutoID" is visible
	##Given I type "[[rec(1).a]]" in "WORKSURFACE,Data Merge (2)(DataMergeDesigner),LargeViewContent,LargeDataGrid,UI_DataGridCell_AutoID,UI__Row1_InputVariable_AutoID"
	##And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Data Merge (2)(DataMergeDesigner),DoneButton"
	#Given "WORKSURFACE,Data Merge (2)(DataMergeDesigner),SmallViewContent" is visible
	#Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Data Merge (2)(DataMergeDesigner)"
	#Checking Validation Error For Invalid Using Number
	Given I type "%" in "WORKSURFACE,Data Merge (2)(DataMergeDesigner),LargeViewContent,LargeDataGrid,UI__At_Row1_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Data Merge (2)(DataMergeDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible
	Given I type "[[$]]" in "WORKSURFACE,Data Merge (2)(DataMergeDesigner),LargeViewContent,LargeDataGrid,UI__At_Row1_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Data Merge (2)(DataMergeDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error1_AutoID" is visible
	Given I type "4" in "WORKSURFACE,Data Merge (2)(DataMergeDesigner),LargeViewContent,LargeDataGrid,UI__At_Row1_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Data Merge (2)(DataMergeDesigner),DoneButton"
	Given "WORKSURFACE,Data Merge (2)(DataMergeDesigner),SmallViewContent" is visible


