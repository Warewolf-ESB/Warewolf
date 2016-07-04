Feature: Date - Assign
	In order to use variables 
	As a Warewolf user
	I want a tool that assigns data to variables
	
Scenario: Multiassign With SomeVariable UI Test
	Given I "Assert_NewWorkFlow_RibbonButton_Exists"
	When I "Click_New_Workflow_Ribbon_Button"
	Then I "Assert_StartNode_Exists"
	And I "Assert_Toolbox_Multiassign_Exists"
	
#@NeedsBlankWorkflow
#Given that the unit before this one passed its post asserts
#	Given I "Assert_StartNode_Exists"
#	And I "Assert_Toolbox_Multiassign_Exists"
	When I "Drag_Toolbox_MultiAssign_Onto_DesignSurface"
	Then I "Assert_Assign_Small_View_Row1_Variable_Textbox_Exists"

#@NeedsMultiAssignSmallViewToolWithSomeVariableOnTheDesignSurface
#Scenario: Double Clicking Multi Assign Tool Small View on the Design Surface Opens Large View
#	Given I "Assert_MultiAssign_Exists_OnDesignSurface"
#	And I "Assert_Assign_Small_View_Row1_Variable_Textbox_Text_is_SomeVariable"
	When I "Open_Assign_Tool_Large_View"
	Then I "Assert_Assign_Large_View_Exists_OnDesignSurface"
	And I "Assert_Assign_Large_View_Row1_Variable_Textbox_Exists"

#@NeedsMultiAssignSmallViewToolOnTheDesignSurface
#Scenario: Enter Text into Multi Assign Tool Small View Grid Column 1 Row 1 Textbox has text in text property
#	Given I "Assert_Assign_Large_View_Exists_OnDesignSurface"
#	And I "Assert_Assign_Large_View_Row1_Variable_Textbox_Exists"
	When I "Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariable"
	Then I "Assert_Assign_Large_View_Row1_Variable_Textbox_Text_is_SomeVariable"

#@NeedsMultiAssignLargeViewToolOnTheDesignSurface
#Scenario: Validating Multi Assign Tool with a variable entered into the Large View on the Design Surface Passes Validation and Variable is in the Variable list
#	Given I "Assert_Assign_Large_View_Exists_OnDesignSurface"
#	And I "Assert_Assign_Large_View_Row1_Variable_Textbox_Text_Equals_SomeVariable"
	When I "Click_Assign_Tool_Large_View_DoneButton"
	Then I "Assert_Assign_Small_View_Row1_Variable_Textbox_Text_is_SomeVariable"
	And I "Assert_VariableList_Scalar_Row1_Textbox_Equals_SomeVariable"

#@NeedsMultiAssignSmallViewToolWithSomeVariableOnTheDesignSurface
#Scenario: Clicking Debug Button Shows Debug Input Dialog
#	Given I "Assert_MultiAssign_Exists_OnDesignSurface"
#	And I "Assert_Assign_Small_View_Row1_Variable_Textbox_Text_is_SomeVariable"
	When I "Click_Debug_Ribbon_Button"
	Then I "Assert_DebugInput_Window_Exists"
	And I "Assert_DebugInput_DebugButton_Exists"

#@NeedsDebugInputDialog
#Scenario: Clicking Debug Button In Debug Input Dialog Generates Debug Output
#	Given I "Assert_Debug_Input_Dialog_Exists"
#	And I "Assert_DebugInput_DebugButton_Exists"
	When I "Click_DebugInput_DebugButton"
	Then I "Assert_DebugOutput_Contains_SomeVariable"

#@NeedsMultiAssignSmallViewOnTheDesignSurface
#Scenario: Click Assign Tool QVI Button Opens Qvi
#	Given I "Assert_MultiAssign_Exists_OnDesignSurface"
	When I "Open_Assign_Tool_Qvi_Large_View"
	Then I "Assert_Assign_QVI_Large_View_Exists_OnDesignSurface"

#@NeedsUnsavedWorkflow
#Scenario: Clicking the tab close button prompts to save
	Given I "Assert_Close_Tab_Button_Exists"
	When I "Click_Close_Tab_Button"
	Then I "Assert_MessageBox_Yes_Button_Exists"

#@NeedsSaveUnsavedDialog
#Scenario: Clicking the Yes Button on the Save Unsaved Prompt Opens the save dialog
#	Given I "Assert_MessageBox_Yes_Button_Exists"
	When I "Click_MessageBox_Yes"
	Then I "Assert_SaveDialog_Exists"
	And I "Assert_SaveDialog_ServiceName_Textbox_Exists"

#@NeedsSaveWorkflowDialog
#	Given I "Assert_Save_Workflow_Dialog_Exists"
#	And I "Assert_Workflow_Name_Textbox_Exists"
	When I "Enter_Workflowname_As_SomeWorkflow"
	And I "Click_SaveDialog_YesButton"
	Then "localhost\SomeWorkflow" exists in the explorer tree

@ignore
@Assign
# Coded UI Tests
Scenario: Opening Assign Lare View
	Given I have Assign small view on design surface
	And Assign Small view grid as
	| # | Variable | = | New Value |
	| 1 |          | = |           |
	| 2 |          | = |           |
	And Scroll bar is "Disabled"
	When I open Assign large view
	Then Assign Large view is "Visible"
	And Assign Larege view grid as
	| # | Variable | = | New Value |
	| 1 |          | = |           |
	| 2 |          | = |           |
	And Scroll bar is "Disabled"
	And Done button is "Visible"
	And Quick Variable Input button is "Visible"

@ignore	
Scenario: Passing Variables in small view 
	Given I have Assign small view on design surface
	When I pass variables in Small view grid as
	| # | Variable    | = | New Value |
	| 1 | [[a]]       | = | Test      |
	| 2 | [[rec().a]] | = | Record    |
	| 3 |             | = |           |
	Then Scroll bar is "Enabled"
	When I open Assign large view
	Then Assign Large view is "Visible"
	And Assign Larege view grid as
	| # | Variable    | = | New Value |
	| 1 | [[a]]       | = | Test      |
	| 2 | [[rec().a]] | = | Record    |
	| 3 |             | = |           |
	And Scroll bar is "Disabled"
	And Done button is "Visible"
	And Quick Variable Input button is "Visible"

Scenario: Focus is at row1 when I drag assign
	Given I have Assign small view on design surface
	And "Row 1" is focused
	When I open Assign large view
	Then "Row 1" is focused

@ignore	
Scenario: Assigning long string to a variable in Assign small view
	Given I have Assign small view on design surface
	When I pass variables in Small view grid as
	| # | Variable    | = | New Value                           |
	| 1 | [[a]]       | = | Test  warewolf at dev2 for Business |
	| 2 |             | = |                                     |
	Then scroll bar is appeard at "Row1" NewValue
	And Scroll bar is "Disabled"

@ignore
Scenario: Assigning long string to a variable in Assign Large view
	Given I have Assign Large view on design surface
	When I pass variables in Large view grid as
	| # | Variable | = | New Value                                                                                                                                                             |
	| 1 | [[a]]    | = | Test  warewolf at dev2 by using Acceptance test is a wonderfull experience.  So here I am actually tesrting whether scroll bar is appearing for this long text or not |
	| 2 |          | = |                                                                                                                                                                       |
	Then scroll bar is appeard at "Row1" NewValue
	And Scroll bar is "Disabled"

@ignore
##Enter it"s in MSWord and copy and paste here and expect validation
Scenario: Pasting words which is not a latin character is thrown a validation
	Given I have Assign small view on design surface
    When I pass variables in Small view grid as
	| # | Variable | = | New Value |
	| 1 | [[a]]    | = | It’s      |
	| 2 |          | = |           |
	Then "Invalid text" popup is visible

@ignore
#Copy and Paste text which contains tab space in it and expect validation
Scenario: Pasting a sentence with tabs is thorwn a validation
	Given I have Assign Large view on design surface
	When I pass variables in Large view grid as
	| # | Variable | = | New Value             |
	| 1 | [[a]]    | = | Test	warewolf	at dev2 |
	| 2 |          | = |                       |
	Then "Tabs Pasted" popup is visible

@ignore
Scenario: Done Button Is validating Invalid Scalar Variables
	Given I have Assign small view on design surface
	When I open Assign large view
	Then Assign Large view is "Visible"
	And I pass variables in Assign Larege view grid as
	| # | Variable    | = | New Value |
	| 1 | [[a$]]      | = | Test      |
	| 2 | [[rec().a]] | = | Record    |
	| 3 |             | = |           |
	And Scroll bar is "Disabled"
	And Done button is "Visible"
	When I click on "Done"
	Then Assign small view is "Not Visible"
	And Validation message is thrown "True"
	When I Edit variables in Assign Larege view grid as
	| # | Variable    | = | New Value |
	| 1 | [[a]]       | = | Test      |
	| 2 | [[rec().a]] | = | Record    |
	| 3 |             | = |           |
	And I click on "Done"
	Then Assign small view is "Visible"
	And Validation message is thrown "False"

@ignore
Scenario: Done Button Is validating Invalid Recordset Variables
	Given I have Assign small view on design surface
	When I open Assign large view
	Then Assign Large view is "Visible"
	And I pass variables in Assign Larege view grid as
	| # | Variable     | = | New Value |
	| 1 | [[a]]        | = | Test      |
	| 2 | [[rec()..a]] | = | Record    |
	| 3 |              | = |           |
	And Scroll bar is "Disabled"
	And Done button is "Visible"
	When I click on "Done"
	Then Assign small view is "Not Visible"
	And Validation message is thrown "True"
	When I Edit variables in Assign Larege view grid as
	| # | Variable    | = | New Value |
	| 1 | [[a]]       | = | Test      |
	| 2 | [[rec().a]] | = | Record    |
	| 3 |             | = |           |
	And I click on "Done"
	Then Assign small view is "Visible"
	And Validation message is thrown "False"

@ignore
Scenario: Done Button Is validating Invalid Variables In New Value
	Given I have Assign small view on design surface
	When I open Assign large view
	Then Assign Large view is "Visible"
	And I pass variables in Assign Larege view grid as
	| # | Variable    | = | New Value  |
	| 1 | [[a$]]      | = | Test[[b*]] |
	| 2 | [[rec().a]] | = | Record     |
	| 3 |             | = |            |
	And Scroll bar is "Disabled"
	And Done button is "Visible"
	When I click on "Done"
	Then Assign small view is "Not Visible"
	And Validation message is thrown "True"
	When I Edit variables in Assign Larege view grid as
	| # | Variable    | = | New Value |
	| 1 | [[a]]       | = | Test[[b]] |
	| 2 | [[rec().a]] | = | Record    |
	| 3 |             | = |           |
	And I click on "Done"
	Then Assign small view is "Visible"
	And Validation message is thrown "False"

@ignore
Scenario: Done Button Is validating Invalid Variables In expression
	Given I have Assign small view on design surface
	When I open Assign large view
	Then Assign Large view is "Visible"
	And I pass variables in Assign Larege view grid as
	| # | Variable    | = | New Value           |
	| 1 | [[a$]]      | = | 1                   |
	| 2 | [[rec().a]] | = | 2                   |
	| 3 | [[total]]   | = | =[[a]]+[[rec().a.]] |
	| 4 |             | = |                     |
	And Scroll bar is "Disabled"
	And Done button is "Visible"
	When I click on "Done"
	Then Assign small view is "Not Visible"
	And Validation message is thrown "True"
	When I Edit variables in Assign Larege view grid as
	| # | Variable    | = | New Value          |
	| 1 | [[a$]]      | = | 1                  |
	| 2 | [[rec().a]] | = | 2                  |
	| 3 | [[total]]   | = | =[[a]]+[[rec().a]] |
	| 4 |             | = |                    |
	And I click on "Done"
	Then Assign small view is "Visible"
	And Validation message is thrown "False"

@ignore
Scenario: Inserting Rows in large view
	Given I have Assign small view on design surface
	When I open Assign large view
	Then Assign Large view is "Visible"
	And I pass variables in Assign Larege view grid as
	| # | Variable    | = | New Value |
	| 1 | [[a]]       | = | 1         |
	| 2 | [[rec().a]] | = | 2         |
	| 3 | [[total]]   | = | 87        |
	| 4 |             | = |           |
	And Scroll bar is "Disabled"
	When I Insert Row at "3"
	Then Assign Larege view grid as
	| # | Variable    | = | New Value |
	| 1 | [[a]]       | = | 1         |
	| 2 | [[rec().a]] | = | 2         |
	| 3 |             | = |           |
	| 4 | [[total]]   | = | 87        |
	| 5 |             | = |           |
	And "Row 3" is focused

@ignore
Scenario: Deleting Rows in large view
	Given I have Assign small view on design surface
	When I open Assign large view
	Then Assign Large view is "Visible"
	And I pass variables in Assign Larege view grid as
	| # | Variable    | = | New Value |
	| 1 | [[a]]       | = | 1         |
	| 2 | [[rec().a]] | = | 2         |
	| 3 | [[total]]   | = | 87        |
	| 4 |             | = |           |
	And Scroll bar is "Disabled"
	When I Delete "Row 2"
	Then Assign Larege view grid as
	| # | Variable    | = | New Value |
	| 1 | [[a]]       | = | 1         |
	| 2 | [[total]]   | = | 87        |
	| 3 |             | = |           |
	And "" is focused

@ignore
Scenario: Collapse largeview is closing large view
	Given I have Assign small view on design surface
	When I open Assign large view
	Then Assign Large view is "Visible"
	When I collapse large view
	Then Assign small view is "Visible"

@ignore
Scenario: Opening Assign Quick Variable Input
	Given I have Assign small view on design surface
	When I select "QVI"
	Then "Quick Variable Input" large view is opened
	And Variable list text box is "Visible"
	And Split List On selected as "Chars" with ""
	And Prefix as ""
	And Suffix as ""
	And Append is "Selected"
	And Replace is "Unselected"
	And Preview as
	||
	And Preview button is "Disabled"
	And Add button is "Dsiabled"

@ignore
Scenario: Adding Variables by using QVI
    Given I have Assign small view on design surface
	When I select "QVI"
	Then "Quick Variable Input" large view is opened
	And Variable list text box is "Visible"
	And I enter variables 
	| [[a]] |
	| [[b]] |
	| [[c]] |
	| [[d]] |
	And Split List On selected as "NewLine" with ""
	And Prefix as ""
	And Suffix as ""
	And Append is "Selected"
	And Replace is "Unselected"
	And Preview button is "Enabled"	
	When I click on "Preview"
	Then preview as
	| 1 [[a]] |
	| 2 [[b]] |
	| 3 [[c]] |
	| 4 [[d]] |
	And Add button is "Enabbled"
	When I click on "Add"
	Then Assign small view is "Visible" 
	And Small view grid as
	| # | Variable | = | New Value |
	| 1 | [[a]]    | = |           |
	| 2 | [[b]]    | = |           |
	| 3 | [[c]]    | = |           |
	| 4 | [[d]]    | = |           |

@ignore
Scenario: Adding Variables by using QVI and split on chars
    Given I have Assign Large view on design surface
	When I select "QVI"
	Then "Quick Variable Input" large view is opened
	And Variable list text box is "Visible"
	And I enter variables 
	| [[a]],[[b]],[[c]],[[d]] |
	And Split List On selected as "Chars" with ","
	And Prefix as ""
	And Suffix as ""
	And Append is "Selected"
	And Replace is "Unselected"
	And Preview button is "Enabled"	
	When I click on "Preview"
	Then preview as
	| 1 [[a]] |
	| 2 [[b]] |
	| 3 [[c]] |
	| 4 [[d]] |
	And Add button is "Enabbled"
	When I click on "Add"
	Then Assign Large view is "Visible" 
	And Small view grid as
	| # | Variable | = | New Value |
	| 1 | [[a]]    | = |           |
	| 2 | [[b]]    | = |           |
	| 3 | [[c]]    | = |           |
	| 4 | [[d]]    | = |           |

@ignore
##This split by using "Tab" is not working because I can"t use tab while entering variable list but I can paste 
## So option must work as expected.
Scenario: Adding Variables by using QVI and split on Tab
    Given I have Assign Large view on design surface
	When I select "QVI"
	Then "Quick Variable Input" large view is opened
	And Variable list text box is "Visible"
	And I enter variables 
	| [[a]]	[[b]]	[[c]]	[[d]] |
	And Split List On selected as "Tab" with ""
	And Prefix as ""
	And Suffix as ""
	And Append is "Selected"
	And Replace is "Unselected"
	And Preview button is "Enabled"	
	When I click on "Preview"
	Then preview as
	| 1 [[a]] |
	| 2 [[b]] |
	| 3 [[c]] |
	| 4 [[d]] |
	And Add button is "Enabbled"
	When I click on "Add"
	Then Assign Large view is "Visible" 
	And Small view grid as
	| # | Variable | = | New Value |
	| 1 | [[a]]    | = |           |
	| 2 | [[b]]    | = |           |
	| 3 | [[c]]    | = |           |
	| 4 | [[d]]    | = |           |

@ignore
Scenario: Adding Variables by using QVI and split on Index
    Given I have Assign Large view on design surface
	When I select "QVI"
	Then "Quick Variable Input" large view is opened
	And Variable list text box is "Visible"
	And I enter variables 
	| abcdefgh|
	And Split List On selected as "Index" with "1"
	And Prefix as ""
	And Suffix as ""
	And Append is "Selected"
	And Replace is "Unselected"
	And Preview button is "Enabled"	
	When I click on "Preview"
	Then preview as
	| 1 [[a]] |
	| 2 [[b]] |
	| 3 [[c]] |
	| 4 [[d]] |
	| 5 [[e]] |
	| 6 [[f]] |
	| 7 [[g]] |
	| 8 [[h]] |
	And Add button is "Enabbled"
	When I click on "Add"
	Then Assign Large view is "Visible" 
	And Small view grid as
	| # | Variable | = | New Value |
	| 1 | [[a]]    | = |           |
	| 2 | [[b]]    | = |           |
	| 3 | [[c]]    | = |           |
	| 4 | [[d]]    | = |           |
	| 5 | [[e]]    | = |           |
	| 6 | [[f]]    | = |           |
	| 7 | [[g]]    | = |           |
	| 8 | [[h]]    | = |           |

@ignore
Scenario Outline:  QVI Prefix and Suffix
    Given I have Assign Large view on design surface
	When I select "QVI"
	Then "Quick Variable Input" large view is opened
	And Variable list text box is "Visible"
	And I enter variables 
	| aaaa |
	And Split List On selected as "Index" with "1"
	And Prefix as "<Prefix>"
	And Suffix as "<Suffix>"
	And Append is "<Append>"
	And Replace is "<Replace>"
	And Preview button is "Enabled"	
	When I click on "Preview"
	Then preview as
	| 1 [[aa]] |
	| 2 [[aa]] |
	| 3 [[aa]] |
	| 4 [[aa]] |
	And Add button is "Enabbled"
	When I click on "Add"
	Then Assign Large view is "Visible" 
	And Small view grid as
	| # | Variable | = | New Value |
	| 1 | [[aa]]   | = |           |
	| 2 | [[aa]]   | = |           |
	| 3 | [[aa]]   | = |           |
	| 4 | [[aa]]   | = |           |
	Examples: 
	| No | Prefix | Suffix | Append   | Replace    |
	| 1  | a      | ""     | Selected | Unselected |
	| 2  | ""     | a      | Selected | Unselected |

@ignore
Scenario:  QVI Replace is Replacing Variables
    Given I have Assign Large view on design surface
	And Large view grid as
	| # | Variable | = | New Value |
	| 1 | [[a]]    | = |           |
	| 2 | [[b]]    | = |           |
	| 3 | [[c]]    | = |           |
	When I select "QVI"
	Then "Quick Variable Input" large view is opened
	And Variable list text box is "Visible"
	And I enter variables 
	| [[rec().a]],[[rec().b]] |
	And Split List On selected as "Char" with ","
	And Prefix as ""
	And Suffix as ""
	And Append is "Unselected"
	And Replace is "Selected"
	And Preview button is "Enabled"	
	When I click on "Preview"
	Then preview as
	| 1 [[rec().a]] |
	| 2 [[rec().b]] |
	And Add button is "Enabbled"
	When I click on "Add"
	Then Assign Large view is "Visible" 
	And Small view grid as
	| # | Variable    | = | New Value |
	| 1 | [[rec().a]] | = |           |
	| 2 | [[rec().b]] | = |           |
	













