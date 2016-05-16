﻿Feature: Date - Assign
	In order to use variables 
	As a Warewolf user
	I want a tool that assigns data to variables

@Assign
# Coded UI Tests
@ignore
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

@ignore
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


##Enter it's in MSWord and copy and paste here and expect validation
@ignore
Scenario: Pasting words which is not a latin character is thrown a validation
	Given I have Assign small view on design surface
    When I pass variables in Small view grid as
	| # | Variable | = | New Value |
	| 1 | [[a]]    | = | It’s      |
	| 2 |          | = |           |
	Then "Invalid text" popup is visible

#Copy and Paste text which contains tab space in it and expect validation
@ignore
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

##This split by using 'Tab' is not working because I can't use tab while entering variable list but I can paste 
## So option must work as expected.
@ignore
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
	And Prefix as '<Prefix>'
	And Suffix as '<Suffix>'
	And Append is '<Append>'
	And Replace is '<Replace>'
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
	And Append is 'Unselected'
	And Replace is 'Selected'
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
	













