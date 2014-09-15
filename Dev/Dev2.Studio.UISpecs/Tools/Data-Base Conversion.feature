Feature: Data-Base Conversion
	In order to use Base Conversion Tool
	As a Warewolf User
	I want to be able to use Base Convert Tool

@BaseConversion
Scenario: DragOnBaseCovert
    Given I have Warewolf running
	Given all tabs are closed
	And I click "EXPLORER,UI_localhost_AutoID"
	And I click new "Workflow"
	And I double click "TOOLBOX,PART_SearchBox"
    And I send "{DELETE}" to ""
	And I send "Base" to "TOOLBOX,PART_SearchBox"
	When I drag "TOOLBASECONVERT" onto "TABACTIVE,StartSymbol"
	Given I send "[[rec().a]]" to "TOOLBASECONVERTSMALLVIEWGRID,UI__Row1_FromExpressiontxt_AutoID"
	Given I click "TOOLBASECONVERTSMALLVIEWGRID,UI__Row1_SearchType_AutoID,UI_ComboBoxItem_Base 64_AutoID"
	Given I click "TOOLBASECONVERTSMALLVIEWGRID,UI__Row1_ToTypecbx_AutoID,UI_ComboBoxItem_Hex_AutoID"
	Given I send "[[rec().a]]" to "TOOLBASECONVERTSMALLVIEWGRID,UI__Row2_FromExpressiontxt_AutoID"
    Given I right click "TOOLDESIGNER,Base Conversion (2)(BaseConvertDesigner),SmallViewContent,SmallDataGrid,UI__Row2_SearchType_AutoID"
    And I send "{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{ENTER}" to ""
	Given "TOOLBASECONVERTSMALLVIEWGRID,UI__Row4_FromExpressiontxt_AutoID" contains text ""
