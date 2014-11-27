Feature: FileAndFolder-Delete
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@DeleteFolder
Scenario: Delete Tool Large View And Invalid Variables Expected Error On Done Button
	Given I have Warewolf running
	And all tabs are closed	
	Given I click "EXPLORERCONNECTCONTROL"
	Given I click "U_UI_ExplorerServerCbx_AutoID_localhost"
	And I click "RIBBONNEWENDPOINT"
	#Dragging Delete Tool From Tool Box
	Given I send "Delete" to "TOOLBOX,PART_SearchBox"
    Given I drag "TOOLFILEDELETE" onto "WORKSURFACE,StartSymbol"
	#Opening Delete Large View
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Delete(DeleteDesigner)"
	#BuG12561 Passing Invalid Recordset Variable in File or Folder  Field And Expected Validation on Done
	Given I type "[[rec@(1).a]]" in "WORKSURFACE,Delete(DeleteDesigner),LargeViewContent,UI__FileOrFoldertxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Delete(DeleteDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible
	Given I type "[[rec(1).#]]" in "WORKSURFACE,Delete(DeleteDesigner),LargeViewContent,UI__FileOrFoldertxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Delete(DeleteDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible
	#Fixing the Bad Variable and expected validation disappear when click on done
	Given I type "[[rec(1).a]]" in "WORKSURFACE,Delete(DeleteDesigner),LargeViewContent,UI__FileOrFoldertxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Delete(DeleteDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is invisible within "1" seconds
	Given "WORKSURFACE,Delete(DeleteDesigner),SmallViewContent" is visible
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Delete(DeleteDesigner)"
	#Clicking on Done button without password and expected validation message
	Given I type "TestingDelete" in "WORKSURFACE,Delete(DeleteDesigner),LargeViewContent,UI__UserNametxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Delete(DeleteDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible
	Given I send "{TAB}" to "WORKSURFACE,Delete(DeleteDesigner),LargeViewContent,UI__UserNametxt_AutoID"
	Given I send "Password" to ""
	#Clicking on Done button with Username and password expected no validation message
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Delete(DeleteDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is invisible within "1" seconds
	Given "WORKSURFACE,Delete(DeleteDesigner),SmallViewContent" is visible






	



