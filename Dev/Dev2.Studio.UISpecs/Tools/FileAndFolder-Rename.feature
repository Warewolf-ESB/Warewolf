Feature: FileAndFolder-Rename
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Rename
Scenario:Rename Tool Large View And Invalid Variables Expected Error On Done Button
	Given I have Warewolf running
	And all tabs are closed	
	And I click "EXPLORER,UI_localhost_AutoID"
	And I click "RIBBONNEWENDPOINT"
	#Dragging Rename Tool From Tool Box
	Given I send "Rename" to "TOOLBOX,PART_SearchBox"
    Given I drag "TOOLRENAME" onto "WORKSURFACE,StartSymbol"
	#Opening Rename Large View
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Rename(RenameDesigner)"
	#BuG12561 Passing Invalid Recordset Variable in  File or Folder Field And Checking Validation on Done
	Given I type "[[rec@(1).a]]" in "WORKSURFACE,Rename(RenameDesigner),LargeViewContent,UI__FileOrFoldertxt_AutoID"
	#And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Rename(RenameDesigner),DoneButton"
	#BUg 12561
	#Given "WORKSURFACE,UI_Error0_AutoID" is visible
	And I send "{TAB}{TAB}{TAB}{TAB}{TAB}" to ""
	And I send "Testware" to ""
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Rename(RenameDesigner),DoneButton"
    Given "WORKSURFACE,UI_Error0_AutoID" is visible