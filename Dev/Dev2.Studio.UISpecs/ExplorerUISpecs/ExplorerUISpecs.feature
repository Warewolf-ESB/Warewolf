Feature: ExplorerUISpecs
	In order to work with versions
	I want to know that versioning is working in the system
	So that I can rollback, rename and delete with comfort.

@mytag
Scenario: CreateNewVersionANDRenameANDMakeOldVersionCurrentANDCheckDeployANDDeleteANDConfirmReadOnlyANDHide
	Given I have Warewolf running
	And I click "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Recordset - Records Length_AutoID"
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
	And I click "RIBBONDEPLOY"
	Then "DEPLOYSOURCE,UI_SourceServer_UI_localhost_AutoID_AutoID,UI_SourceServer_UI_EXAMPLES_AutoID_AutoID,UI_SourceServer_UI_Recordset - Records Length_AutoID_AutoID,v.1*" is invisible within "3" seconds	
	And all tabs are closed
	#Delete
	When I right click "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Recordset - Records Length_AutoID,v.3*"
	And I click "UI_DeleteVersionContextMenuItem_AutoID"
	And I click "UI_MessageBox_AutoID,UI_YesButton_AutoID"
	Then "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Recordset - Records Length_AutoID,v.3*" is invisible within "1" seconds
	#ConfirmReadOnly
	When I double click "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Recordset - Records Length_AutoID,v.2*"
	Then "WORKFLOWDESIGNER,Recordset - Records Length(FlowchartDesigner)" is visible within "2" seconds
	Then "RIBBONSAVE" is disabled
	Then "RIBBONDEBUG" is disabled
	#Hide
	And I right click "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Recordset - Records Length_AutoID"
	And I click "UI_ToggleVersionHistoryContextMenuItem_AutoID"
	Then "EXPLORERFOLDERS,UI_EXAMPLES_AutoID,UI_Recordset - Records Length_AutoID,v.2*" is invisible within "1" seconds
	And I close Studio
	And I close Server

	