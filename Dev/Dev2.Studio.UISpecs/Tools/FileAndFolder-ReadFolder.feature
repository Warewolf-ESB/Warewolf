Feature: FileAndFolder-ReadFolder
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@ReadFolder
Scenario:ReadFolder Tool Large View And Invalid Variables Expected Error On Done Button
	Given I have Warewolf running
	And all tabs are closed	
	Given I click "EXPLORERFILTERCLEARBUTTON"  
	Given I click "EXPLORERCONNECTCONTROL"
	Given I click "U_UI_ExplorerServerCbx_AutoID_localhost"
	And I click "RIBBONNEWENDPOINT"
	#Dragging ReadFolder Tool From Tool Box
	Given I send "Read" to "TOOLBOX,PART_SearchBox"
    Given I drag "TOOLREADFOLDER" onto "WORKSURFACE,StartSymbol"
	#Opening ReadFolder Large View
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Read Folder(ReadFolderDesigner)"
	#BuG12561 Passing Invalid Recordset Variable in  File or Folder Field And Checking Validation on Done
	Given I type "[[rec@(1).a]]" in "WORKSURFACE,Read Folder(ReadFolderDesigner),LargeViewContent,UI__Directorytxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Read Folder(ReadFolderDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible
	Given I type "[[Var#]]" in "WORKSURFACE,Read Folder(ReadFolderDesigner),LargeViewContent,UI__Directorytxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Read Folder(ReadFolderDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible
	#Fixing the Bad Variable and expected validation disappear when click on done
	Given I type "[[rec(1).a]]" in "WORKSURFACE,Read Folder(ReadFolderDesigner),LargeViewContent,UI__Directorytxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Read Folder(ReadFolderDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is invisible within "1" seconds
	Given "WORKSURFACE,Read Folder(ReadFolderDesigner),SmallViewContent" is visible
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Read Folder(ReadFolderDesigner)"
	#Clicking on Done button without password and expected validation message
	Given I type "TestingDelete" in "WORKSURFACE,Read Folder(ReadFolderDesigner),LargeViewContent,UI__UserNametxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Read Folder(ReadFolderDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible
	Given I send "{TAB}" to "WORKSURFACE,Read Folder(ReadFolderDesigner),LargeViewContent,UI__UserNametxt_AutoID"
	Given I send "Password" to ""
	#Clicking on Done button with Username and password expected no validation message
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Read Folder(ReadFolderDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is invisible within "1" seconds
	Given "WORKSURFACE,Read Folder(ReadFolderDesigner),SmallViewContent" is visible