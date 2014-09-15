Feature: FileAndFolder-Delete
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Delete
Scenario: Delete Tool Large View And Invalid Variables Expected Error On Done Button
	Given I have Warewolf running
	And all tabs are closed	
	And I click "EXPLORER,UI_localhost_AutoID"
	And I click "RIBBONNEWENDPOINT"
	#Dragging Delete Tool From Tool Box
	Given I send "Delete" to "TOOLBOX,PART_SearchBox"
    Given I drag "TOOLFILEDELETE" onto "WORKSURFACE,StartSymbol"
	#Opening Delete Large View
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Delete(DeleteDesigner)"
	#BuG12561 Passing Invalid Recordset Variable in File or Folder  Field And Checking Validation on Done
	Given I type "[[rec@(1).a]]" in "WORKSURFACE,Delete(DeleteDesigner),LargeViewContent,UI__FileOrFoldertxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Delete(DeleteDesigner),DoneButton"
	#BUg 12561 Given "WORKSURFACE,UI_Error0_AutoID" is visible