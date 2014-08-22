Feature: ExplorerUISpecs
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@mytag
Scenario: CreateVersionAndCheckAndDeleteVersion
	Given I click "EXPLORERFILTERCLEARBUTTON"
	And I send "Utility" to "EXPLORERFILTER"
	And I double click "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Utility - Assign_AutoID"
	And  I click "EXPLORERFILTERCLEARBUTTON"
	And I right click "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Utility - Assign_AutoID"
	When I click "UI_ToggleVersionHistoryContextMenuItem_AutoID"
	#Then "UI_There is no version history to display_AutoID" is visible within "5" seconds
	#When I click "RIBBONSAVE"
	#And I click "RIBBONSAVE"
	#Then "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Utility - Assign_AutoID,UI_v.2  *" is visible
	#When I click "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Utility - Assign_AutoID,UI_v.2  *,UI_CanEdit_AutoID"
	#Then "RIBBONSAVE" is disabled
	#When I right click "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Utility - Assign_AutoID,UI_v.2  *"
	#And I click "UI_DeleteVersionContextMenuItem_AutoID"
	#Then " v2 " is not visible
	#And I right click ""

Scenario: MakeOldVersionCurrentAndHide
	Given I click "EXPLORERFILTERCLEARBUTTON"
	And I send "recordset - recor" to "EXPLORERFILTER"
	And I double click "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Recordset - Records Length_AutoID"
	#Then "WORKFLOWDESIGNER,Recordset - Records Length(FlowchartDesigner),Length(RecordsLengthDesigner)" is visible within "1" seconds
	#And I click "WORKFLOWDESIGNER,Recordset - Records Length(FlowchartDesigner),Length(RecordsLengthDesigner)"
	#When I send "{DELETE}" to "WORKFLOWDESIGNER,Recordset - Records Length(FlowchartDesigner),Length(RecordsLengthDesigner)"
	#And I click "RIBBONSAVE"
	#And I right click "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Utility - Assign_AutoID"
	#And I click "UI_ToggleVersionHistoryContextMenuItem_AutoID"
	#And " v1 " is visible within "1" seconds
	#And I right click " v1 "
	#When I click "UI_RollbackContextMenuItem_AutoID"
	#Then "TABACTIVE,Recordset - Records Length(FlowchartDesigner),Use the Records Length tool to:(CommentDesigner)" is visible within "2" seconds
	#And " v2 * Rollback" is visible within "1" seconds
	#When I right click "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Utility - Assign_AutoID"
	#And I click "UI_ToggleVersionHistoryContextMenuItem_AutoID"
	#And " v2 * Rollback" is not visible within "1" seconds
	
Scenario: RenameCurrentVersionCreatesNewVersion
	Given I send "oldresourcename" to "EXPLORERFILTER"
	And I double click "EXPLORERFOLDERS,UI_OldResourceName_AutoID"
	And I click "ExplorerFilterClearButton"
	And I right click "EXPLORERFOLDERS,UI_OldResourceName_AutoID"
	When I click "UI_ToggleVersionHistoryContextMenuItem_AutoID"
	Then "UI_There is no version history to display_AutoID" is visible
	When I click "RIBBONSAVE"
	Then " v1 " is visible
	Given I right click "EXPLORERFOLDERS,UI_OldResourceName_AutoID"
	When I click "UI_RenameContextMenuItem_AutoID"
	And I send "OldResourceName2" to ""
	And I click "RIBBONSAVE"
	Then "v2 * Rename" is visible within "1" seconds
	And "v2 * Save" is not visible within "1" seconds
	And "v1 * Save" is visible within "1" seconds
	
Scenario: DeployDoesNotHaveVersions
	Given I send "RenameR" to "EXPLORERFILTER"
	And I right click "EXPLORERFOLDERS,UI_RenameResourceTest_AutoID"
	When I click "UI_ToggleVersionHistoryContextMenuItem_AutoID"
	And I click "ExplorerFilterClearButton"
	Then "UI_There is no version history to display_AutoID" is visible
	When I click "RIBBONDEPLOY"
	And I send "RenameR" to "deploy filter"
	Then ",UI_SourceServer_RenameResourceTest_AutoID,UI_SourceServer_There is no version history to display_AutoID" is not visible

	