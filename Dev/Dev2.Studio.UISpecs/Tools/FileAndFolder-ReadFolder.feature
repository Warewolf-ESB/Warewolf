Feature: FileAndFolder-ReadFolder
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@mytag
Scenario:ReadFolder Tool Large View And Invalid Variables Expected Error On Done Button
	Given I have Warewolf running
	And all tabs are closed	
	And I click "RIBBONNEWENDPOINT"
	#Dragging ReadFolder Tool From Tool Box
	Given I send "Read" to "TOOLBOX,PART_SearchBox"
    Given I drag "TOOLREADFOLDER" onto "WORKSURFACE,StartSymbol"
	#Opening ReadFolder Large View
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Read Folder(ReadFolderDesigner)"
	#BuG12561 Passing Invalid Recordset Variable in  File or Folder Field And Checking Validation on Done
	Given I type "[[rec@(1).a]]" in "WORKSURFACE,Read Folder(ReadFolderDesigner),LargeViewContent,UI__Directorytxt_AutoID"
	#And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Read File(ReadFileDesigner),DoneButton"
	#BUg 12561
	#Given "WORKSURFACE,UI_Error0_AutoID" is visible
	And I send "{TAB}{TAB}{TAB}{TAB}{TAB}" to ""
	And I send "Testware" to ""
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Read Folder(ReadFolderDesigner),DoneButton"
    Given "WORKSURFACE,UI_Error0_AutoID" is visible