Feature: FileAndFolder-WriteFile
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@WriteFile
Scenario:WriteTool Large View And Invalid Variables Expected Error On Done Button
	Given I have Warewolf running
	And all tabs are closed	
	Given I click "EXPLORERFILTERCLEARBUTTON"  
	Given I click "EXPLORERCONNECTCONTROL"
	Given I click "U_UI_ExplorerServerCbx_AutoID_localhost"
	And I click "RIBBONNEWENDPOINT"
	#Dragging Write Tool From Tool Box
	And I send "Write" to "TOOLBOX,PART_SearchBox"
    Given I drag "TOOLWRITEFILE" onto "WORKSURFACE,StartSymbol"
	#Opening Write Large View
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Write File(WriteFileDesigner)"
	#BuG12561 Passing Invalid Recordset Variable in  File or Folder Field And Expected Validation on Done
	Given I type "[[rec@(1).a]]" in "WORKSURFACE,Write File(WriteFileDesigner),LargeViewContent,UI__FileNametxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Write File(WriteFileDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible
	Given I type "[[rec(1).a]]" in "WORKSURFACE,Write File(WriteFileDesigner),LargeViewContent,UI__FileNametxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Write File(WriteFileDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is invisible within "1" seconds
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Write File(WriteFileDesigner)"
	#Passing Invalid Variable in Content field and expected validation message on Done button
	Given I type "[[rec(1).$a]]" in "WORKSURFACE,Write File(WriteFileDesigner),LargeViewContent,UI__Contentstxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Write File(WriteFileDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible
	Given I type "[[Content]]" in "WORKSURFACE,Write File(WriteFileDesigner),LargeViewContent,UI__Contentstxt_AutoID"
    And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Write File(WriteFileDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is invisible within "1" seconds
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Write File(WriteFileDesigner)"
	#Clicking on Done without Password expected validation message
	Given I type "Username" in "WORKSURFACE,Write File(WriteFileDesigner),LargeViewContent,UI__UserNametxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Write File(WriteFileDesigner),DoneButton"
    Given "WORKSURFACE,UI_Error0_AutoID" is visible
	Given I send "{TAB}" to "WORKSURFACE,Write File(WriteFileDesigner),LargeViewContent,UI__UserNametxt_AutoID"
	And I send "Password" to ""
	#Clicking on Done with all valid inputs expected no validation messasges
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Write File(WriteFileDesigner),DoneButton"
    Given "WORKSURFACE,UI_Error0_AutoID" is invisible within "1" seconds
	Given "WORKSURFACE,Write File(WriteFileDesigner),SmallViewContent" is visible
	