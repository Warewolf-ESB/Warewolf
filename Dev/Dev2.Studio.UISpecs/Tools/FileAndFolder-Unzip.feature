Feature: FileAndFolder-Unzip
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Unzip
Scenario:Unzip Tool Large View And Invalid Variables Expected Error On Done Button
	Given I have Warewolf running
	And all tabs are closed	
	And I click "EXPLORER,UI_localhost_AutoID"
	And I click "RIBBONNEWENDPOINT"
	#Dragging Unzip Tool From Tool Box
	Given I send "Unzip" to "TOOLBOX,PART_SearchBox"
    Given I drag "TOOLUNZIP" onto "WORKSURFACE,StartSymbol"
	#Opening Unzip Large View
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Unzip(UnzipDesigner)"
	#BuG12561 Passing Invalid Recordset Variable in  File or Folder Field And Checking Validation on Done
	Given I type "[[rec@(1).a]]" in "WORKSURFACE,Unzip(UnzipDesigner),LargeViewContent,UI__ZipNametxt_AutoID"
	#And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Rename(RenameDesigner),DoneButton"
	#BUg 12561
	#Given "WORKSURFACE,UI_Error0_AutoID" is visible
	And I send "{TAB}{TAB}" to ""
	And I send "Testware" to ""
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Unzip(UnzipDesigner),DoneButton"
    Given "WORKSURFACE,UI_Error1_AutoID" is visible
	Given I type "Testware@dev2.co.za" in "WORKSURFACE,Unzip(UnzipDesigner),LargeViewContent,UI__UserNametxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Unzip(UnzipDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible
	Given I type "Testware@dev2.co.za" in "WORKSURFACE,Unzip(UnzipDesigner),LargeViewContent,UI__Destinationtxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Unzip(UnzipDesigner),DoneButton"