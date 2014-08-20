Feature: Workflow
	In order to be able to use warewolf
	As a warewolf user
	I want to be able to create and execute a workflow

Scenario: Correcting errors on sql bulk insert clicking Done shows small view (using ids)
	Given I have Warewolf running	
	And I click "UI_RibbonHomeTabWorkflowBtn_AutoID"
	When I send "Sql Bulk Insert" to "TOOLBOX,PART_SearchBox"
	And I drag "TOOLBOX,PART_Tools,Recordset,Dev2.Activities.DsfSqlBulkInsertActivity" onto "WORKSURFACE,StartSymbol"
	And I double click "UNSAVED1,SQL Bulk Insert(SqlBulkInsertDesigner)"
	And I click "UNSAVED1,SQL Bulk Insert(SqlBulkInsertDesigner),DoneButton"
	Then "UNSAVED1,A database must be selected." is visible
	Then I click "UNSAVED1,SQL Bulk Insert(SqlBulkInsertDesigner),LargeViewContent,UI__Database_AutoID,GetCities"
	And I click "UNSAVED1,SQL Bulk Insert(SqlBulkInsertDesigner),LargeViewContent,UI__TableName_AutoID,dbo.City"
	And I click "UNSAVED1,SQL Bulk Insert(SqlBulkInsertDesigner),DoneButton"
	Then "UNSAVED1,A database must be selected." is not visible

Scenario: Debug GatherSystemInformation same variables in two activites
	Given I have Warewolf running
	And I send "11330_Integration tests" to "EXPLORER,FilterTextBox,UI_DataListSearchtxt_AutoID"
	And I double click "EXPLORER,Navigation,UI_localhost,UI_SPINT 7_AutoID,UI_11330_Integration tests_AutoID"
	And I wait till "WORKSURFACE" is visible
	And I send "{F6}" to ""
	And I wait till "DEBUGOUTPUT,Dev2StatusBarAutomationID,StatusBar" is not visible
	Then "DEBUGOUTPUT,DebugOutputTree,Gather System Info 1 (2),Date & Time" is visible
	Then "DEBUGOUTPUT,DebugOutputTree,Gather System Info 1 (2),CPU Available" is visible
	Then "DEBUGOUTPUT,DebugOutputTree,Gather System Info 2 (2),Date & Time" is visible
	Then "DEBUGOUTPUT,DebugOutputTree,Gather System Info 2 (2),CPU Available" is visible
	
Scenario: Drag on Multiassign
	Given I have Warewolf running
	And I click "UI_RibbonHomeTabWorkflowBtn_AutoID"
	When I send "Assign" to "TOOLBOX,PART_SearchBox"
	And I drag "TOOLMULTIASSIGN" onto "ACTIVETAB,Dev2.Studio.ViewModels.WorkSurface.WorkSurfaceContextViewModel,WorkflowDesignerView,UserControl_1,scrollViewer,ActivityTypeDesigner,WorkflowItemPresenter,StartSymbol"
	#And I double click "WORKSURFACE,Assign (1)(MultiAssignDesigner)"
	And I send "[[rec().a]]" to "ACTIVETAB,Assign (1)(MultiAssignDesigner),SmallViewContent,SmallDataGrid,Unlimited.Applications.BusinessDesignStudio.Activities.ActivityDTO,Column Display Index: 1,UI__Row1_FieldName_AutoID"
	And I send "test" to "UNSAVED1,Assign (1)(MultiAssignDesigner),SmallViewContent,SmallDataGrid,Unlimited.Applications.BusinessDesignStudio.Activities.ActivityDTO,Column Display Index: 1,UI__Row1_FieldValue_AutoID"

Scenario: Drag on BaseCovert
	Given I have Warewolf running
	And I click new "Workflow"
	When I send "Base" to "TOOLBOX,PART_SearchBox"
	And I drag "TOOLBASECONVERT" onto "WORKSURFACE,StartSymbol"
	And I double click "WORKSURFACE,BaseConvert (1)(DsfBaseConvertActivity)"
	And I send "[[rec().a]]" to "WORKSURFACE,BaseConvert (1)(BaseConvertDesigner),SmallViewContent,SmallDataGrid,Unlimited.Applications.BusinessDesignStudio.Activities.ActivityDTO,Column Display Index: 1,UI__Row1_FieldName_AutoID"
	And close the Studio and Server


Scenario: Drag
    Given I have Warewolf running
    Given I click new "Workflow"
	And I send "Data Merge" to "TOOLBOX,PART_SearchBox"
	When I drag "UI_DocManager_AutoID,Zc30a7af8e0c54bb5bccfbea116f8ab0d,Zf1166e575b5d43bb89f15f346eccb7b1,UI_ToolboxPane_AutoID,UI_ToolboxControl_AutoID,PART_Tools,Data,DsfDataMergeActivity" onto "WORKSURFACE,StartSymbol"
    And I double click "WORKSURFACE,Data Merge (1)(DataMergeDesigner)"
    And I send "Test" to "WORKSURFACE,Data Merge (1)(DataMergeDesigner),LargeViewContent,LargeDataGrid,Unlimited.Applications.BusinessDesignStudio.Activities.DataMergeDTO,Column Display Index: 1,UI__Row1_InputVariable_AutoID"
	And I send "%" to "WORKSURFACE,Data Merge (1)(DataMergeDesigner),LargeViewContent,LargeDataGrid,Unlimited.Applications.BusinessDesignStudio.Activities.DataMergeDTO,Column Display Index: 3,UI__At_Row1_AutoID"
	And I click "WORKSURFACE,Data Merge (1)(DataMergeDesigner),DoneButton"
	And I click "WORKSURFACE,Data Merge (1)(DataMergeDesigner),'Using' must be a real number"
	And I send "1" to "WORKSURFACE,Data Merge (1)(DataMergeDesigner),LargeViewContent,LargeDataGrid,Unlimited.Applications.BusinessDesignStudio.Activities.DataMergeDTO,Column Display Index: 3,UI__At_Row1_AutoID"
	And I click "WORKSURFACE,Data Merge (1)(DataMergeDesigner),DoneButton"	
	Given I send "%" to "WORKSURFACE,Data Merge (1)(DataMergeDesigner),LargeViewContent,LargeDataGrid,Unlimited.Applications.BusinessDesignStudio.Activities.DataMergeDTO,dataitem,Column Display Index: 3,UI__At_Row1_AutoID"


Scenario: Assign Large View Test
    Given I have Warewolf running
	Given I click "UI_RibbonHomeTabWorkflowBtn_AutoID"
	And I send "Assign" to "TOOLBOX,PART_SearchBox"
	When I drag "TOOLMULTIASSIGN" onto "ACTIVETAB,StartSymbol"
	And I double click "TABACTIVE,Unsaved 1(FlowchartDesigner),Assign (1)(MultiAssignDesigner)"
	And I send "{TAB} {TAB} {TAB} {TAB} {TAB} [[rec(&).a]]" to "ACTIVETAB,Assign (1)(MultiAssignDesigner)"
	And I click "ACTIVETAB,Assign (1)(MultiAssignDesigner),DoneButton"
	Then close the Studio and Server
	
	
	
	
	
	
	
	
	#And I send "Test" to "Workflow_Designer + ",Data Merge (1)(DataMergeDesigner),LargeViewContent,LargeDataGrid,"Entire Row"" Grid row1" ,UI__Row1_FieldName_AutoID"
	# And I send "Data Merge" to "ACTIVETAB,Dev2.Studio.ViewModels.WorkSurface.WorkSurfaceContextViewModel,UI_WorkflowDesigner_AutoID,UserControl_1,scrollViewer,ActivityTypeDesigner,WorkflowItemPresenter,Assign (1)(MultiAssignDesigner),DisplayNameBox"
	 #And I click "UI_DebugInputWindow_AutoID,UI_Executebtn_AutoID"
	 #Given I click "UI_RibbonDebugBtn_AutoID"
	#Given I click "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,Dev2.Studio.ViewModels.WorkSurface.WorkSurfaceContextViewModel,UI_WorkflowDesigner_AutoID,UserControl_1,scrollViewer,ActivityTypeDesigner,WorkflowItemPresenter,Unsaved 1(FlowchartDesigner),Assign (1)(MultiAssignDesigner),DoneButton"
	


Scenario: Drag Data Merge
    #Given I have Warewolf running
	Given I click new "Workflow"
	And I send "Data Merge" to "TOOLBOX,PART_SearchBox"
	When I drag "TOOLBOX,PART_Tools,Data,Unlimited.Applications.BusinessDesignStudio.Activities.DsfDataMergeActivity" onto "ACTIVETAB,Dev2.Studio.ViewModels.WorkSurface.WorkSurfaceContextViewModel,UI_WorkflowDesigner_AutoID,UserControl_1,scrollViewer,ActivityTypeDesigner,WorkflowItemPresenter,StartSymbol"
	And I send "Assign" to "TOOLBOX,PART_SearchBox"
	When I drag "TOOLBOX,PART_Tools,Data,Unlimited.Applications.BusinessDesignStudio.Activities.DsfMultiAssignActivity" onto "ACTIVETAB,Dev2.Studio.ViewModels.WorkSurface.WorkSurfaceContextViewModel,UI_WorkflowDesigner_AutoID,UserControl_1,scrollViewer,ActivityTypeDesigner,WorkflowItemPresenter,Data Merge (1)(DataMergeDesigner)"


Scenario: Drag Data DataSplit
    #Given I have Warewolf running
	Given I click new "Workflow"
	And I send "Data Split" to "TOOLBOX,PART_SearchBox"
	When I drag "TOOLBOX,PART_Tools,Data,Unlimited.Applications.BusinessDesignStudio.Activities.DsfDataSplitActivity" onto "ACTIVETAB,Dev2.Studio.ViewModels.WorkSurface.WorkSurfaceContextViewModel,UI_WorkflowDesigner_AutoID,UserControl_1,scrollViewer,ActivityTypeDesigner,WorkflowItemPresenter,StartSymbol"


Scenario: Drag Find Record Index
	 #Given I click "UI_RibbonDebugBtn_AutoID"
	 #And I click "UI_DebugInputWindow_AutoID,UI_Executebtn_AutoID"
   # Given I have Warewolf running
	Given I click "UI_RibbonHomeTabWorkflowBtn_AutoID"
	And I send "Data Merge" to "TOOLBOXSEARCH"
	#When I drag "TOOLBOX,PART_Tools,Data,Unlimited.Applications.BusinessDesignStudio.Activities.DsfMultiAssignActivity" onto "ACTIVETAB,Dev2.Studio.ViewModels.WorkSurface.WorkSurfaceContextViewModel,UI_WorkflowDesigner_AutoID,UserControl_1,scrollViewer,ActivityTypeDesigner,WorkflowItemPresenter,StartSymbol"
	When I drag "TOOLDATAMERGE" onto "ACTIVETAB,Dev2.Studio.ViewModels.WorkSurface.WorkSurfaceContextViewModel,UI_WorkflowDesigner_AutoID,UserControl_1,scrollViewer,ActivityTypeDesigner,WorkflowItemPresenter,StartSymbol"
	#When I drag "TOOLBOX,PART_Tools,Recordset,Unlimited.Applications.BusinessDesignStudio.Activities.DsfFindRecordsMultipleCriteriaActivity" onto "ACTIVETAB,Dev2.Studio.ViewModels.WorkSurface.WorkSurfaceContextViewModel,UI_WorkflowDesigner_AutoID,UserControl_1,scrollViewer,ActivityTypeDesigner,WorkflowItemPresenter,StartSymbol"
	 #And I double click "ACTIVETAB,Dev2.Studio.ViewModels.WorkSurface.WorkSurfaceContextViewModel,UI_WorkflowDesigner_AutoID,UserControl_1,scrollViewer,ActivityTypeDesigner,WorkflowItemPresenter,Unsaved 1(FlowchartDesigner),Assign (1)(MultiAssignDesigner)"
	# Given "ACTIVETAB,Dev2.Studio.ViewModels.WorkSurface.WorkSurfaceContextViewModel,UI_WorkflowDesigner_AutoID,UserControl_1,scrollViewer,ActivityTypeDesigner,WorkflowItemPresenter,Find Record Index (1)(FindRecordsMultipleCriteriaDesigner)" is Highlighted
	 



 Scenario: Settings
    #Given I have Warewolf running
	Given I send "{TAB} {TAB} {TAB} {TAB} {TAB} {TAB} {TAB} " to "ACTIVETAB"
    And I click "SECURITYSAVE"

#
#	Given I click "UI_RibbonHomeTabWorkflowBtn_AutoID"
#	And I send "Assign" to "TOOLBOX,PART_SearchBox"
#	When I drag "TOOLMULTIASSIGN" onto "ACTIVETAB,StartSymbol"
#	And I double click "TABACTIVE,Unsaved 1(FlowchartDesigner),Assign (1)(MultiAssignDesigner)"
#	And I send "{TAB} {TAB} {TAB} {TAB} {TAB} [[rec(&).a]]" to "ACTIVETAB,Assign (1)(MultiAssignDesigner)"
#	And I click "ACTIVETAB,Assign (1)(MultiAssignDesigner),DoneButton"
#	Then close the Studio and Server
	



















