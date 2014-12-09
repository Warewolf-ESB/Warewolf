Feature: FileAndFolder-Copy
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Copy
Scenario:CopyTool Large View And Invalid Variables Expected Error On Done Button
	Given I have Warewolf running
	And all tabs are closed	
	And I click "EXPLORER,UI_localhost_AutoID"
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
	Given "WORKSURFACE,UI_Error2_AutoID" is visible
	#Correcting Bad Variable
	Given I type "[[rec(1).a]]" in "WORKSURFACE,Copy(CopyDesigner),LargeViewContent,UI__FileOrFoldertxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Copy(CopyDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible 
	Given "WORKSURFACE,UI_Error1_AutoID" is visible 
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
	

Scenario:CopyTool Testing Tab Order and UiRepondingFine as expected
	Given I have Warewolf running
	And all tabs are closed	
	Given I click "EXPLORERCONNECTCONTROL"
	Given I click "U_UI_ExplorerServerCbx_AutoID_localhost"
	And I click "RIBBONNEWENDPOINT"
	#Dragging Copy Tool From Tool Box
	Given I send "Copy" to "TOOLBOX,PART_SearchBox"
    Given I drag "TOOLCOPY" onto "WORKSURFACE,StartSymbol"
	##Opening Copy Large View
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Copy(CopyDesigner)"
	##Passing Data Into the tool by using Tabs
    And I send "[[rec(1).a]]{TAB}" to "WORKSURFACE,Copy(CopyDesigner),LargeViewContent,UI__FileOrFoldertxt_AutoID"
	And I send "Source@Username{TAB}" to ""
    And I send "Password{TAB}" to ""
	And I send "[[rec(2).a]]{TAB}" to ""
	And I send "Destination{TAB}" to ""
    And I send "Password{TAB}{SPACE}{TAB}" to ""
	And I send "[[Result]]" to ""
	Given "WORKSURFACE,Copy(CopyDesigner),LargeViewContent,UI__Resulttxt_AutoID" contains text "[[Result]]" 








#Given I type "Warewolf@dev2.co.za" in ""
#And I send "{TAB}" to ""
#And I type "dev2" in ""
#
#And I send "{TAB}" to ""
#And I type "[[Destination]]" in ""
#
#And I send "{TAB}" to ""
#And I type "Warewolf@dev2.co.za" in ""
#
#
#And I send "{TAB}" to ""
#And I type "dev2" in ""
#
#And I send "{TAB}" to ""
#And I type "[[Result]]" in ""
#
#
#And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Copy(CopyDesigner),DoneButton"
#
#	Given "WORKSURFACE,UI_Error0_AutoID" is invisible within "1" seconds


