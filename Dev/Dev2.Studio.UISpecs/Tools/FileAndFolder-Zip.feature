Feature: FileAndFolder-Zip
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Zip
Scenario:zip Tool Large View And Invalid Variables Expected Error On Done Button
	Given I have Warewolf running
	And all tabs are closed	
	And I click "EXPLORER,UI_localhost_AutoID"
	And I click "RIBBONNEWENDPOINT"
	#Dragging zip Tool From Tool Box
	Given I send "zip" to "TOOLBOX,PART_SearchBox"
    Given I drag "TOOLZIP" onto "WORKSURFACE,StartSymbol"
	#Opening zip Large View
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Zip(ZipDesigner)"
	#BuG12561 Passing Invalid Recordset Variable in  File or Folder Field And Checking Validation on Done
	Given I type "[[rec@(1).a]]" in "WORKSURFACE,Zip(ZipDesigner),LargeViewContent,UI__FileOrFoldertxt_AutoID"
	#And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Rename(RenameDesigner),DoneButton"
	#BUg 12561
	#Given "WORKSURFACE,UI_Error0_AutoID" is visible
	And I send "{TAB}{TAB}" to ""
	And I send "Testware" to ""
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Zip(ZipDesigner),DoneButton"
    Given "WORKSURFACE,UI_Error0_AutoID" is visible
	Given I type "Testware@dev2.co.za" in "WORKSURFACE,Zip(ZipDesigner),LargeViewContent,UI__UserNametxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Zip(ZipDesigner),DoneButton"
	Given "WORKSURFACE,Zip(ZipDesigner),SmallViewContent" is visible
