Feature: ExplorerUISpecs
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@mytag
Scenario: CreateVersionAndCheckAndDeleteVersion
	Given I click "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Utility - Assign_AutoID"
	And I double click "EXPLORER,UI_localhost_AutoID,UI_EXAMPLES_AutoID,UI_Utility - Assign_AutoID"
	And "WORKFLOWDESIGNER,Utility - Assign(FlowchartDesigner)" is visible within "2" seconds
	And I right click "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Utility - Assign_AutoID"
	When I click "UI_ToggleVersionHistoryContextMenuItem_AutoID"
	Then "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Utility - Assign_AutoID,UI_There is no version history to display_AutoID" is visible within "2" seconds
	When I click "RIBBONSAVE"
	Then "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Utility - Assign_AutoID,v.1*" is visible within "3" seconds
	When I click "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Utility - Assign_AutoID,v.1*,UI_CanEdit_AutoID"
	#Then "RIBBONSAVE" is disabled within "2" seconds
	#When I right click "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Utility - Assign_AutoID,v.1*"
	#And I click "UI_DeleteVersionContextMenuItem_AutoID"
	#Then "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Utility - Assign_AutoID,v.1*" is not visible
	#And I right click ""

Scenario: CreateNewVersionANDRenameANDMakeOldVersionCurrentANDCheckDeployANDHide
	Given I click "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Recordset - Records Length_AutoID"
	And I double click "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Recordset - Records Length_AutoID"
	Then "WORKFLOWDESIGNER,Recordset - Records Length(FlowchartDesigner),Length(RecordsLengthDesigner)" is visible within "2" seconds
	#CreateNewVersion
	When I send "{DELETE}" to "WORKFLOWDESIGNER,Recordset - Records Length(FlowchartDesigner),Length(RecordsLengthDesigner)"
	And I click "RIBBONSAVE"
	And I right click "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Recordset - Records Length_AutoID"
	And I click "UI_ToggleVersionHistoryContextMenuItem_AutoID"
	And "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Recordset - Records Length_AutoID,v.1*" is visible within "1" seconds
	#Rename
	And I right click "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Recordset - Records Length_AutoID"
	And I click "UI_RenameContextMenuItem_AutoID"
	And I send "Recordset - Records Length RENAME{ENTER}" to ""
	Then "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Recordset - Records Length RENAME_AutoID" is visible within "1" seconds
	And "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Recordset - Records Length RENAME_AutoID,v.2*" is visible within "1" seconds
	#MakeOldVersionCurrent
	And I right click "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Recordset - Records Length RENAME_AutoID,v.1*"
	When I click "UI_RollbackContextMenuItem_AutoID"
	And I click "UI_MessageBox_AutoID,UI_YesButton_AutoID"
	Then "WORKFLOWDESIGNER,Recordset - Records Length(FlowchartDesigner),Length(RecordsLengthDesigner)" is visible within "2" seconds
	And "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Recordset - Records Length_AutoID,v.3*" is visible within "1" seconds
	#CheckDeploy [does not have any versions showing up]
	When I click "RIBBONDEPLOY"
	Then "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Recordset - Records Length_AutoID,v.3*" is invisible within "3" seconds	
	#Hide
	And I right click "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Recordset - Records Length_AutoID"
	And I click "UI_ToggleVersionHistoryContextMenuItem_AutoID"
	#Then "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Recordset - Records Length_AutoID,v.2*" is invisible within "1" seconds
	
	
Scenario: DeployDoesNotHaveVersions
	Given I send "RenameR" to "EXPLORERFILTER"
	And I right click "EXPLORERFOLDERS,UI_RenameResourceTest_AutoID"
	When I click "UI_ToggleVersionHistoryContextMenuItem_AutoID"
	And I click "ExplorerFilterClearButton"
	Then "UI_There is no version history to display_AutoID" is visible
	When I click "RIBBONDEPLOY"
	And I send "RenameR" to "deploy filter"
	Then ",UI_SourceServer_RenameResourceTest_AutoID,UI_SourceServer_There is no version history to display_AutoID" is not visible

	