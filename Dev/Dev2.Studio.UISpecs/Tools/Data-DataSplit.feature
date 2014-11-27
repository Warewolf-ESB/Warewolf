Feature: Data-DataSplit
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Data-DataSplit
Scenario: Data Split Large view Invalid Variables Expected Validation on Done Button
	Given I have Warewolf running
	And all tabs are closed	
	Given I click "EXPLORERCONNECTCONTROL"
	Given I click "U_UI_ExplorerServerCbx_AutoID_localhost"
	And I click "RIBBONNEWENDPOINT"
	#Dragging DataSplit Tool From Tool Box
	Given I send "Data" to "TOOLBOX,PART_SearchBox"
    Given I drag "TOOLDATASPLIT" onto "WORKSURFACE,StartSymbol"
	#Opening Data Merge Large View
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Data Split (1)(DataSplitDesigner)"
	#BuG12561 Passing Invalid Recordset Variable in String To Split Field And Checking Validation on Done
	Given I type "[[rec@(1).a]]" in "WORKSURFACE,Data Split (1)(DataSplitDesigner),LargeViewContent,UI__SourceStringtxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Data Split (1)(DataSplitDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible
	Given I type "[[rec(1).a]]" in "WORKSURFACE,Data Split (1)(DataSplitDesigner),LargeViewContent,UI__SourceStringtxt_AutoID"
	And I send "{TAB}" to ""
	#Checking Recordset Variable Appeared In Variable List
	Given "VARIABLERECORDSET,UI_RecordSet_rec_AutoID,UI_NameTextBox_AutoID" is visible within "2" seconds
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Data Split (1)(DataSplitDesigner),DoneButton"
	Given "TOOLDATASPLITSMALLVIEW" is visible
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Data Split (1)(DataSplitDesigner)"
	#Passing Invalid Recordset Variable in Row1 And Checking Validation on Done
	Given I type "[[rec(%1).a]]" in "WORKSURFACE,Data Split (1)(DataSplitDesigner),LargeViewContent,LargeDataGrid,UI__Row1_OutputVariable_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Data Split (1)(DataSplitDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible
	Given "WORKSURFACE,UI_Error1_AutoID" is visible
	Given I type "[[Split(1).a]]" in "WORKSURFACE,Data Split (1)(DataSplitDesigner),LargeViewContent,LargeDataGrid,UI__Row1_OutputVariable_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Data Split (1)(DataSplitDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error2_AutoID" is visible
	#Passing Invalid Variable in Using Field And Checking Validation on Done
	Given I type "[[a!]]" in "WORKSURFACE,Data Split (1)(DataSplitDesigner),LargeViewContent,LargeDataGrid,UI__At_Row1_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Data Split (1)(DataSplitDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error3_AutoID" is visible
	Given I type "[[a]]" in "WORKSURFACE,Data Split (1)(DataSplitDesigner),LargeViewContent,LargeDataGrid,UI__At_Row1_AutoID"
	And I send "{TAB}" to ""
	#Checking Scalar Variable Appeared In Variable List
	And "VARIABLESCALAR,UI_Variable_a_AutoID" is visible within "2" seconds
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Data Split (1)(DataSplitDesigner),DoneButton"
	Given "TOOLDATASPLITSMALLVIEW" is visible
