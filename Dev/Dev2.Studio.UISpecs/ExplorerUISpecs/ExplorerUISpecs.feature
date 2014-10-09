Feature: ExplorerUISpecs
	In order to work with versions
	I want to know that versioning is working in the system
	So that I can rollback, rename and delete with comfort.

@ExplorerUISpecs
Scenario: CreateNewVersionANDRenameANDMakeOldVersionCurrentANDCheckDeployANDDeleteANDConfirmReadOnlyANDHide
	Given I have Warewolf running
	And all tabs are closed
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I click "EXPLORER,UI_localhost_AutoID"
	And I click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Recordset - Records Length_AutoID"
	And I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Recordset - Records Length_AutoID"
	Then "WORKFLOWDESIGNER,Recordset - Records Length(FlowchartDesigner),Length(RecordsLengthDesigner)" is visible within "2" seconds
	#CreateNewVersion
	When I send "{DELETE}" to "WORKFLOWDESIGNER,Recordset - Records Length(FlowchartDesigner),Length(RecordsLengthDesigner)"
	And I click "RIBBONSAVE"
	And I right click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Recordset - Records Length_AutoID"
	And I click "UI_ToggleVersionHistoryContextMenuItem_AutoID"
	And "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Recordset - Records Length_AutoID,v.1*" is visible within "2" seconds
	#Rename
	And I right click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Recordset - Records Length_AutoID"
	And I click "UI_RenameContextMenuItem_AutoID"
	And I type "Recordset - Records Length RENAME" in "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Recordset - Records Length_AutoID,UI_RenameTexbox_AutoID"
	And I send "{ENTER}" to ""
	Then "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Recordset - Records Length RENAME_AutoID" is visible within "1" seconds
	And "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Recordset - Records Length RENAME_AutoID,v.2*" is visible within "1" seconds
	#MakeOldVersionCurrent
	And I right click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Recordset - Records Length RENAME_AutoID,v.1*"
	When I click "UI_RollbackContextMenuItem_AutoID"
	And I click "UI_MessageBox_AutoID,UI_YesButton_AutoID"
	Then "WORKFLOWDESIGNER,Recordset - Records Length(FlowchartDesigner),Length(RecordsLengthDesigner)" is visible within "2" seconds
	And "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Recordset - Records Length_AutoID,v.3*" is visible within "1" seconds
	#CheckDeploy [does not have any versions showing up]
	And I click "RIBBONDEPLOY"
	Then "DEPLOYSOURCE,UI_SourceServer_UI_localhost_AutoID_AutoID,UI_SourceServer_UI_Examples_AutoID_AutoID,UI_SourceServer_UI_Recordset - Records Length_AutoID_AutoID,v.1*" is invisible within "3" seconds	
	And all tabs are closed
	#Delete [Also proves that rollback after rename worked as UI Auto ID no longer contains RENAME]
	When I right click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Recordset - Records Length_AutoID,v.3*"
	And I click "UI_DeleteVersionContextMenuItem_AutoID"
	And I click "UI_MessageBox_AutoID,UI_YesButton_AutoID"
	Then "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Recordset - Records Length_AutoID,v.3*" is invisible within "1" seconds
	#ConfirmReadOnly
	When I double click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Recordset - Records Length_AutoID,v.2*"
	Then "WORKFLOWDESIGNER,Recordset - Records Length(FlowchartDesigner)" is visible within "2" seconds
	Then "RIBBONSAVE" is disabled
	Then "RIBBONDEBUG" is disabled
	#Hide
	And I right click "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Recordset - Records Length_AutoID"
	And I click "UI_ToggleVersionHistoryContextMenuItem_AutoID"
	Then "EXPLORERFOLDERS,UI_Examples_AutoID,UI_Recordset - Records Length_AutoID,v.2*" is invisible within "1" seconds
	And I close Studio
	And I close Server


#Bug 12166
#Scenario: Delete A Resource In Explorer By Mouse Right Click And Check Its Dependency Error Message
#	Given I have Warewolf running
#	#Filtering Resource
#	And I click "EXPLORER,UI_DatalistFilterTextBox_AutoID,UI_FilterButton_AutoID"
#	And I send "GetCategoryTable" to "EXPLORER,UI_DatalistFilterTextBox_AutoID,UI_TextBox_AutoID"
#	And I click "EXPLORER,UI_ExplorerTree_AutoID,UI_localhost_AutoID,UI_Sample Project_AutoID,UI_GetCategoryTable_AutoID"
#	#Deleting Resource By Using Mouse Right Click
#	And I right click "EXPLORERFOLDERS,UI_Sample Project_AutoID,UI_GetCategoryTable_AutoID"
#	And I click "UI_DeleteContextMenuItem_AutoID"
#	#Checking Dependency Error Popup 
#	Then "UI_DeleteResourceText_AutoID" is visible within "2" seconds
#	And I click "UI_DeleteResourceShowDependenciesBtn_AutoID"
#	Given "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,myScrollViewer,[DependencyGraph_Sample Project\GetCategoryTable_IsCircular_False]" is visible
#


Scenario: Saving A Workflow In NewFolder Is Saved And Delete Rename Works As Expected
	Given I have Warewolf running
	And all tabs are closed
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I click "EXPLORER,UI_localhost_AutoID"
	And I click "RIBBONNEWENDPOINT"
	#Saving a workflow 1
	And I click "RIBBONSAVE"
	And "WebBrowserWindow" is visible within "3" seconds
	And I send "{TAB}{TAB}{Enter}" to ""
	#Creating a new folder in save dialog
	And I send "UIFolder" to ""
	And I click point "304,305" on "WebBrowserWindow"
	And I send "{TAB}{TAB}{TAB}" to ""
	And I send "S1" to ""
	And I click point "400,415" on "WebBrowserWindow"
	And "EXPLORERFOLDERS,UI_UIFolder_AutoID,UI_S1_AutoID" is visible within "3" seconds
	And I click "EXPLORERFOLDERS,UI_UIFolder_AutoID,UI_S1_AutoID"
	#Renaming a resource 
	And I right click "EXPLORERFOLDERS,UI_UIFolder_AutoID"
	And I click "UI_RenameContextMenuItem_AutoID"
	And I type "RENAME" in "EXPLORERFOLDERS,UI_UIFolder_AutoID,UI_RenameTexbox_AutoID"
	And I send "{ENTER}" to ""
	Given I click "EXPLORERFOLDERS,UI_RENAME_AutoID,UI_S1_AutoID"
	#Checking Renamed Resources are visible in explorer when filter is ON
	Then "EXPLORERFOLDERS,UI_RENAME_AutoID" is visible within "5" seconds
	Then "EXPLORERFOLDERS,UI_RENAME_AutoID,UI_S1_AutoID" is visible within "1" seconds
	#Deleting Resources in Explorer
	And I right click "EXPLORERFOLDERS,UI_RENAME_AutoID"
	And I click "UI_DeleteContextMenuItem_AutoID"
	And I click "UI_MessageBox_AutoID,UI_YesButton_AutoID"
	When I send "RENAME" to "EXPLORERFILTER"
	Given "EXPLORERFOLDERS,UI_RENAME_AutoID" is invisible within "5" seconds
	And I click "RIBBONNEWENDPOINT"
	#Saving a workflow 2
	And I click "RIBBONSAVE"
	And "WebBrowserWindow" is visible within "3" seconds
	And I send "{TAB}{TAB}{Enter}" to ""
	#Creating a new folder
	And I send "UIFolderFilter" to ""
	And I click point "304,305" on "WebBrowserWindow"
	And I send "{TAB}{TAB}{TAB}" to ""
	And I send "FilterTest" to ""
	And I click point "400,415" on "WebBrowserWindow"
	And I click "EXPLORERFILTERCLEARBUTTON"
	#Renaming Resource when filter is ON
	When I send "UIFolderFilter" to "EXPLORERFILTER"
	And I right click "EXPLORERFOLDERS,UI_UIFolderFilter_AutoID"
	And I click "UI_RenameContextMenuItem_AutoID"
	And I type "RENAME1" in "EXPLORERFOLDERS,UI_UIFolderFilter_AutoID,UI_RenameTexbox_AutoID"
	And I send "{ENTER}" to ""
	Given I click "EXPLORERFOLDERS,UI_RENAME1_AutoID,UI_FilterTest_AutoID"
	#Checking Resource renamed successfully when filter is ON
	Then "EXPLORERFOLDERS,UI_RENAME1_AutoID" is visible within "5" seconds
	Then "EXPLORERFOLDERS,UI_RENAME1_AutoID,UI_FilterTest_AutoID" is visible within "1" seconds
	#Checking Resource renamed successfully when filter is OFF
	And I click "EXPLORERFILTERCLEARBUTTON"
	Then "EXPLORERFOLDERS,UI_RENAME1_AutoID" is visible within "5" seconds
	Then "EXPLORERFOLDERS,UI_RENAME1_AutoID,UI_FilterTest_AutoID" is visible within "1" seconds
	When I send "RENAME1" to "EXPLORERFILTER"
	#Deleting Resource when filter is ON
	And I right click "EXPLORERFOLDERS,UI_RENAME1_AutoID"
	And I click "UI_DeleteContextMenuItem_AutoID"
	And I click "UI_MessageBox_AutoID,UI_YesButton_AutoID"
	#Checking Resource deleted successfully
	Then "EXPLORERFOLDERS,UI_RENAME1_AutoID,UI_FilterTest_AtoID" is invisible within "1" seconds
	Then "EXPLORERFOLDERS,UI_RENAME1_AutoID,UI_FilterTest_AtoID" is invisible within "1" seconds

	

	

