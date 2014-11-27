Feature: Recordset-FindRecordIndex
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@FindRecordIndex
Scenario: Find Record index Large view Invalid Recordset Expected Validation on Done Button
	Given I have Warewolf running
	And all tabs are closed	
	Given I click "EXPLORERCONNECTCONTROL"
	Given I click "U_UI_ExplorerServerCbx_AutoID_localhost"
	And I click "RIBBONNEWENDPOINT"
	#Dragging DataSplit Tool From Tool Box
	Given I send "Find" to "TOOLBOX,PART_SearchBox"
    Given I drag "TOOLFIND" onto "WORKSURFACE,StartSymbol"
	#Opening Data Merge Large View
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Find Record Index (1)(FindRecordsMultipleCriteriaDesigner)"
	# Passing Invalid Recordset Tool into InFields Field And Checking Validation on Done
	Given I type "[[rec@(1).a]]" in "WORKSURFACE,Find Record Index (1)(FindRecordsMultipleCriteriaDesigner),LargeViewContent,UI__FieldsToSearchtxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Find Record Index (1)(FindRecordsMultipleCriteriaDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible
	Given "WORKSURFACE,UI_Error1_AutoID" is visible
	Given "WORKSURFACE,UI_Error2_AutoID" is visible
	Given I type "[[rec().a]]" in "WORKSURFACE,Find Record Index (1)(FindRecordsMultipleCriteriaDesigner),LargeViewContent,UI__FieldsToSearchtxt_AutoID"
	And I send "{TAB}" to ""
	#Checking Recordset Variable Appeared In Variable List
	Given "VARIABLERECORDSET,UI_RecordSet_rec_AutoID,UI_NameTextBox_AutoID" is visible within "2" seconds
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Find Record Index (1)(FindRecordsMultipleCriteriaDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error3_AutoID" is visible
	Given I click "WORKSURFACE,Find Record Index (1)(FindRecordsMultipleCriteriaDesigner),LargeViewContent,LargeDataGrid,UI__Row1_SearchType_AutoID,UI_ComboBoxItem_=_AutoID"
	Given I type "[[rec().a]]" in "WORKSURFACE,Find Record Index (1)(FindRecordsMultipleCriteriaDesigner),LargeViewContent,LargeDataGrid,UI__Row1_SearchCriteria_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Find Record Index (1)(FindRecordsMultipleCriteriaDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error4_AutoID" is visible
	#Passing Invalid Variable In Result Throws Validation Message on Done Button
	Given I type "[[result*]]" in "WORKSURFACE,Find Record Index (1)(FindRecordsMultipleCriteriaDesigner),LargeViewContent,UI__Result_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Find Record Index (1)(FindRecordsMultipleCriteriaDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error5_AutoID" is visible
	Given I type "[[result]]" in "WORKSURFACE,Find Record Index (1)(FindRecordsMultipleCriteriaDesigner),LargeViewContent,UI__Result_AutoID"
	And I send "{TAB}" to ""
	#Checking Scalar Variable Appeared In Variable List
	And "VARIABLESCALAR,UI_Variable_result_AutoID" is visible within "2" seconds
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Find Record Index (1)(FindRecordsMultipleCriteriaDesigner),DoneButton"
	Given "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Find Record Index (1)(FindRecordsMultipleCriteriaDesigner),SmallViewContent,SmallDataGrid" is visible




