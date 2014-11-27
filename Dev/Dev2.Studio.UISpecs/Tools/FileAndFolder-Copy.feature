Feature: FileAndFolder-Copy
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Copy
Scenario:CopyTool Large View And Invalid Variables Expected Error On Done Button
	Given I have Warewolf running
	And all tabs are closed	
	Given I click "EXPLORERCONNECTCONTROL"
	Given I click "U_UI_ExplorerServerCbx_AutoID_localhost"
	And I click "RIBBONNEWENDPOINT"
	#Dragging Copy Tool From Tool Box
	Given I send "Copy" to "TOOLBOX,PART_SearchBox"
    Given I drag "TOOLCOPY" onto "WORKSURFACE,StartSymbol"
	#Opening Copy Large View
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Copy(CopyDesigner)"
	#BuG12561 Passing Invalid Recordset Variable in  File or Folder Field And Checking Validation on Done
	Given I type "[[rec@(1).a]]" in "WORKSURFACE,Copy(CopyDesigner),LargeViewContent,UI__FileOrFoldertxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Copy(CopyDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible
	Given "WORKSURFACE,UI_Error1_AutoID" is visible
	#Given "WORKSURFACE,UI_Error2_AutoID" is visible
	#Correcting Bad Variable
	Given I type "[[rec(1).a]]" in "WORKSURFACE,Copy(CopyDesigner),LargeViewContent,UI__FileOrFoldertxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Copy(CopyDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible 
	#Given "WORKSURFACE,UI_Error1_AutoID" is visible 
	#Testing validation message for empty password
	Given I type "[[rec(1).a]]" in "WORKSURFACE,Copy(CopyDesigner),LargeViewContent,UI__Destinationtxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Copy(CopyDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is invisible within "1" seconds
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Copy(CopyDesigner)"
	Given I type "TestWarewolf" in "WORKSURFACE,Copy(CopyDesigner),LargeViewContent,UI__UserNametxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Copy(CopyDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible
	#Entering Password and checking validation message disappears
	And I send "{TAB}" to "WORKSURFACE,Copy(CopyDesigner),LargeViewContent,UI__UserNametxt_AutoID"
	And I send "Password" to ""
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Copy(CopyDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is invisible within "1" seconds
	Given "WORKSURFACE,Copy(CopyDesigner),SmallViewContent" is visible
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Copy(CopyDesigner)"
	#Testing validation message for empty password in destination side
	And I send "{TAB}" to "WORKSURFACE,Copy(CopyDesigner),LargeViewContent,UI__Destinationtxt_AutoID"
	And I send "Warewolf" to ""
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Copy(CopyDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible
	#Entering Password and checking validation message disappears
	And I send "{TAB}{TAB}" to "WORKSURFACE,Copy(CopyDesigner),LargeViewContent,UI__Destinationtxt_AutoID"
	And I send "Password" to ""
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Copy(CopyDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is invisible within "1" seconds
	Given "WORKSURFACE,Copy(CopyDesigner),SmallViewContent" is visible