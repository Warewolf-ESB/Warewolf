Feature: FileAndFolder-Rename
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Rename
Scenario:Rename Tool Large View And Invalid Variables Expected Error On Done Button
	Given I have Warewolf running
	And all tabs are closed	
	Given I click "EXPLORERCONNECTCONTROL"
	Given I click "U_UI_ExplorerServerCbx_AutoID_localhost"
	And I click "RIBBONNEWENDPOINT"
	#Dragging Rename Tool From Tool Box
	Given I send "Rename" to "TOOLBOX,PART_SearchBox"
    Given I drag "TOOLRENAME" onto "WORKSURFACE,StartSymbol"
	#Opening Rename Large View
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Rename(RenameDesigner)"
	#BuG12561 Passing Invalid Recordset Variable in  File or Folder Field And Checking Validation on Done
	Given I type "[[rec@(1).a]]" in "WORKSURFACE,Rename(RenameDesigner),LargeViewContent,UI__FileOrFoldertxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Rename(RenameDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible
	Given "WORKSURFACE,UI_Error1_AutoID" is visible
	Given "WORKSURFACE,UI_Error2_AutoID" is visible
	#Correcting File or Folder Field bad variable and expected no error on done button
	Given I type "[[rec(1).a]]" in "WORKSURFACE,Rename(RenameDesigner),LargeViewContent,UI__FileOrFoldertxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Rename(RenameDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible
	Given "WORKSURFACE,UI_Error1_AutoID" is visible
	#Passing Invalid Recordset Variable in  Destination Field And Expected Validation on Done button
    Given I type "[[rec(1).%a]]" in "WORKSURFACE,Rename(RenameDesigner),LargeViewContent,UI__NewNametxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Rename(RenameDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible
	#Correcting Destination Field bad variable and expected no error on done button
	Given I type "[[rec(1).a]]" in "WORKSURFACE,Rename(RenameDesigner),LargeViewContent,UI__NewNametxt_AutoID"
    And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Rename(RenameDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is invisible within "1" seconds
	#Opening Large View
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Rename(RenameDesigner)"
	#Expecting error when click on done with username only without password
	Given I type "TestingMove" in "WORKSURFACE,Rename(RenameDesigner),LargeViewContent,UI__UserNametxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Rename(RenameDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible
	And I send "{TAB}" to "WORKSURFACE,Rename(RenameDesigner),LargeViewContent,UI__UserNametxt_AutoID"
	And I send "Password" to ""
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Rename(RenameDesigner),DoneButton"
	Given "WORKSURFACE,Rename(RenameDesigner),SmallViewContent" is visible
	#Expecting error when click on done with username only without password in destination side
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Rename(RenameDesigner)"
	Given I send "{TAB}" to "WORKSURFACE,Rename(RenameDesigner),LargeViewContent,UI__NewNametxt_AutoID"
	And I send "Testwareusername" to ""
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Rename(RenameDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible
	Given I send "{TAB}{TAB}" to "WORKSURFACE,Rename(RenameDesigner),LargeViewContent,UI__NewNametxt_AutoID"
	And I send "Password2" to ""
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Rename(RenameDesigner),DoneButton"
	Given "WORKSURFACE,Rename(RenameDesigner),SmallViewContent" is visible
