Feature: FileAndFolder-Create
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Create
Scenario: CreateTool Large View And Invalid Variables Expected Error On Done Button
	Given I have Warewolf running
	And all tabs are closed	
	Given I click "EXPLORERCONNECTCONTROL"
	Given I click "U_UI_ExplorerServerCbx_AutoID_localhost"
	And I click "RIBBONNEWENDPOINT"
	#Dragging Copy Tool From Tool Box
	Given I send "Create" to "TOOLBOX,PART_SearchBox"
    Given I drag "TOOLCREATE" onto "WORKSURFACE,StartSymbol"
	#Opening Copy Large View
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Create(CreateDesigner)"
	#BuG12561 Passing Invalid Recordset Variable in  File or Folder Field And Checking Validation on Done
	Given I type "[[rec@(1).a]]" in "WORKSURFACE,Create(CreateDesigner),LargeViewContent,UI__FileNametxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Create(CreateDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible
	#Correcting Bad Variable
	Given I type "[[rec(1).a]]" in "WORKSURFACE,Create(CreateDesigner),LargeViewContent,UI__FileNametxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Create(CreateDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is invisible within "1" seconds
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Create(CreateDesigner)"
	#Testing validation message for empty password
	Given I type "TestWarewolf" in "WORKSURFACE,Create(CreateDesigner),LargeViewContent,UI__UserNametxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Create(CreateDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible
	And I send "{TAB}" to "WORKSURFACE,Create(CreateDesigner),LargeViewContent,UI__UserNametxt_AutoID"
	And I send "Password" to ""
	#Given I type "Password" in "WORKSURFACE,Create(CreateDesigner),LargeViewContent,UI__Passwordtxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Create(CreateDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is invisible within "1" seconds
	Given "WORKSURFACE,Create(CreateDesigner),SmallViewContent" is visible