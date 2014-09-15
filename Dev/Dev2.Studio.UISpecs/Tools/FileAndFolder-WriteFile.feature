Feature: FileAndFolder-WriteFile
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@WriteFile
Scenario:WriteTool Large View And Invalid Variables Expected Error On Done Button
	Given I have Warewolf running
	And all tabs are closed	
	And I click "EXPLORER,UI_localhost_AutoID"
	And I click "RIBBONNEWENDPOINT"
	#Dragging Write Tool From Tool Box
	Given I send "Write" to "TOOLBOX,PART_SearchBox"
    Given I drag "TOOLWRITEFILE" onto "WORKSURFACE,StartSymbol"
	#Opening Write Large View
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Write File(WriteFileDesigner)"
	#BuG12561 Passing Invalid Recordset Variable in  File or Folder Field And Checking Validation on Done
	Given I type "[[rec@(1).a]]" in "WORKSURFACE,Write File(WriteFileDesigner),LargeViewContent,UI__FileNametxt_AutoID"
	#And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Rename(RenameDesigner),DoneButton"
	#BUg 12561
	#Given "WORKSURFACE,UI_Error0_AutoID" is visible
	And I send "{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}" to ""
	And I send "Testware" to ""
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Write File(WriteFileDesigner),DoneButton"
    Given "WORKSURFACE,UI_Error0_AutoID" is visible
	Given I type "Testware@dev2.co.za" in "WORKSURFACE,Write File(WriteFileDesigner),LargeViewContent,UI__UserNametxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Write File(WriteFileDesigner),DoneButton"
	Given "WORKSURFACE,Write File(WriteFileDesigner),SmallViewContent" is visible
	