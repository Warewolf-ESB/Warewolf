Feature: FileAndFolder-Move
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Move
Scenario:Move Tool Large View And Invalid Variables Expected Error On Done Button
	Given I have Warewolf running
	And all tabs are closed	
	And I click "EXPLORER,UI_localhost_AutoID"
	And I click "RIBBONNEWENDPOINT"
	#Dragging Move Tool From Tool Box
	Given I send "Move" to "TOOLBOX,PART_SearchBox"
    Given I drag "TOOLMOVE" onto "WORKSURFACE,StartSymbol"
	#Opening Move Large View
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Move(MoveDesigner)"
	#BuG12561 Passing Invalid Recordset Variable in  File or Folder Field And Checking Validation on Done
	Given I type "[[rec@(1).a]]" in "WORKSURFACE,Move(MoveDesigner),LargeViewContent,UI__FileOrFoldertxt_AutoID"
	#And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Read File(ReadFileDesigner),DoneButton"
	#BUg 12561
	#Given "WORKSURFACE,UI_Error0_AutoID" is visible
	And I send "{TAB}{TAB}" to ""
	And I send "Testware" to ""
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Move(MoveDesigner),DoneButton"
    Given "WORKSURFACE,UI_Error0_AutoID" is visible