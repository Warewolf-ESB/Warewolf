Feature: DataSplit
	In order to Split Data by using a tool
	As a Warewolf User
	I want to Split variable values or sting

@DataSplit

Scenario: DataSplit Small View
	Given I have DataSplit Small View on design surface
	And String To Split "" is visible
	And DataSplit Small View grid has
	| # | Result | With  | Using |
	| 1 |        | Index |       |
	| 2 |        | Index |       |
	And Scroll bar is "Enabled"

Scenario: DataSplit Large View
	Given I have DataSplit Large View on design surface
	And String To Split "" is visible
	And Process Direction is "Selected" as "Forward" 
	And Process Direction is "UnSelected" as "Backward" 
	And Skip blank rows "Unselected"
	And DataSplit Large View grid has
	| # | Result | With  | Using | Include | Escape |
	| 1 |        | Index |       |         |        |
	| 2 |        | Index |       |         |        |
	And Scroll bar is "Disabled"
	And On Error box consists
	| Put error in this variable | Call this web service |
	|                            |                       |
	And End this workflow is "Unselected"
	And Done button is "Visible"

Scenario: DataSplit Small View and large view focus is at row1
	Given I have DataSplit Small View on design surface
	And "Row 1" is focused
	When I have LargeView on design surface
	Then "Row1" is focused


Scenario: Passing Variables in Datasplit Small View and inserting row
	Given I have DataSplit Small View on design surface
	And I enter String to Split as "Test Warewolf" 
	When I Enter  DataSplit Small View grid has
	| # | Results | With  | Using |
	| 1 | [[a]]   | Index | 1     |
	| 2 | [[b]]   | Index | 2     |
	| 3 |         | Index |       |
	And result is as ""
	And Scroll bar is "Disabled"
	When I Insert Row at "2"
	Then DataSplit Small View grid has
	| # | Results | With  | Using |
	| 1 | [[a]]   | Index | 1     |
	| 2 |         | Index |       |
	| 3 | [[b]]   | Index | 2     |
	| 4 |         | Index |       |
	And Scroll bar is "Enabled"

Scenario: Deleting rows in Datasplit Small View
	Given I have DataSplit Small View on design surface
	And I enter String to Split as "Test Warewolf" 
	When I Enter  DataSplit Small View grid has
	| # | Results | With  | Using |
	| 1 | [[a]]   | Index | 1     |
	| 2 | [[b]]   | Index | 2     |
	| 3 |         | Index |       |
	And result is as ""
	And Scroll bar is "Enabled"
	When I delete Row at "2"
	Then DataSplit Small View grid has
	| # | Results | With  | Using |
	| 1 | [[a]]   | Index | 1     |
	| 2 |         | Index |       |
	And Scroll bar is "Disabled"

Scenario: Passing Variables in Datasplit Large View and inserting row
	Given I have DataSplit Large View on design surface
	And I enter String to Split as "Test Warewolf" 
	When I Enter  DataSplit Large View grid has
	| # | Results | With  | Using | Include    | Escape |
	| 1 | [[a]]   | Index | 1     | Unselected |        |
	| 2 | [[b]]   | Index | 2     | Unselected |        |
	| 3 |         | Index |       | Unselected |        |
	And result is as ""				
	And Scroll bar is "Enabled"
	When I Insert Row at "2"
	Then DataSplit Small View grid has
	| # | Results | With  | Using | Include    | Escape |
	| 1 | [[a]]   | Index | 1     | Unselected |        |
	| 2 |         | Index |       | Unselected |        |
	| 3 | [[b]]   | Index | 2     | Unselected |        |
	| 4 |         | Index |       | Unselected |        |
	And Scroll bar is "Enabled"

Scenario: Deleting rows in Datasplit Large View
	Given I have DataSplit Large View on design surface
	And I enter String to Split as "Test Warewolf" 
	When I Enter  DataSplit Large View grid has
	| # | Results | With  | Using | Include    | Escape |
	| 1 | [[a]]   | Index | 1     | Unselected |        |
	| 2 | [[b]]   | Index | 2     | Unselected |        |
	| 3 |         | Index |       | Unselected |        |
	And result is as ""			       
	And Scroll bar is "Enabled"
	When I delete Row at "2"
	Then DataSplit Small View grid has
	| # | Results | With  | Using | Include    | Escape |
	| 1 | [[a]]   | Index | 1     | Unselected |        |
	| 2 |         | Index |       | Unselected |        |
	And Scroll bar is "Disabled"
							
Scenario: Deleting rows in Datasplit is adjusting number sequence correctly
	Given I have DataSplit Large View on design surface
	And I enter String to Split as "Test Warewolf" 
	When I Enter  DataSplit Large View grid has
	| # | Results | With  | Using | Include    | Escape |
	| 1 | [[a]]   | Index | 1     | Unselected |        |
	| 2 | [[b]]   | Index | 2     | Unselected |        |
	| 3 | [[a]]   | Index | 1     | Unselected |        |
	| 4 | [[b]]   | Index | 2     | Unselected |        |
	| 5 |         | Index |       | Unselected |        |
	And result is as ""			       
	And Scroll bar is "Enabled"
	When I delete Row at "2"
	Then DataSplit Small View grid has
	| # | Results | With  | Using | Include    | Escape |
	| 1 | [[a]]   | Index | 1     | Unselected |        |
	| 2 | [[a]]   | Index | 1     | Unselected |        |
	| 3 | [[b]]   | Index | 2     | Unselected |        |
	| 4 |         | Index |       | Unselected |        |
	And Scroll bar is "Enabled"



Scenario Outline: DataSplit Large View is validating incorrect string variable 
	Given I have DataSplit Large View on design surface
	And String To Split '<Var>' is visible
	And Process Direction is "Selected" as "Forward" 
	And Process Direction is "UnSelected" as "Backward" 
	And Skip blank rows "Unselected"
	And DataSplit Large View grid has
	| # | Result | With  | Using | Include | Escape |
	| 1 | [[a]]  | Index | 1     |         |        |
	| 2 |        | Index |       |         |        |
	And Scroll bar is "Disabled"
	When I click on "Done"
	Then Validation message is thrown '<Validation>'
	Then DataSplit Small View is "NotVisible"
Examples: 
	| No | Var              | Validation |
	| 1  | [[rec(@).a]]     | True       |
	| 2  | [[[rec().a]]     | True       |
	| 3  | [[[[a]]]         | True       |
	| 4  | [[a]]            | False      |
	| 5  | Split            | False      |
	| 6  | [[a]][[rec().a]] | True       |
	| 7  | [[rec([[a]]).a]] | True       |
	| 8  | 12324            | False      |
	| 9  | [[a]][[rec().a]] | False      |




Scenario Outline: Data Split Large View using and Escape is enabled and disabled
	Given I have DataSplit Large View on design surface
	And I enter String to Split as "Test Warewolf" 
	When I select "Row 1"  with as '<With>' then using is '<Using>' and Escape is '<Escape>'
Examples: 
    | No | With     | Using    | Escape   |
    | 1  | End      | Disabled | Disabled |
    | 2  | Index    | Enabled  | Disabled |
    | 3  | New Line | Disabled | Enabled  |
    | 4  | Chars    | Enabled  | Disabled |
    | 5  | Tab      | Enabled  | Disabled |
    | 6  | Space    | Disabled | Enabled  |

	
Scenario Outline: Data Split Small View using is enabled and disabled
	Given I have DataSplit Large View on design surface
	And I enter String to Split as "Test Warewolf" 
	When I select "Row 1"  with as '<With>' then using is '<Using>' 
Examples: 
     | No | With     | Using    |
     | 1  | End      | Disabled |
     | 2  | Index    | Enabled  |
     | 3  | New Line | Disabled |
     | 4  | Chars    | Enabled  |
     | 5  | Tab      | Enabled  |
	 | 6  | Space    | Disabled |	
	
	
Scenario: Data Split Large View is validating invalid variables on done
	Given I have DataSplit Large View on design surface
	And I enter String to Split as "Test Warewolf" 
	When I Enter DataSplit Large View grid has
	| # | Results | With  | Using | Include    | Escape |
	| 1 | [[a#]]  | Index | 1     | Unselected |        |
	| 2 |         | Index |       | Unselected |        |
	And Scroll bar is "Disabled"
	When I click on "Done"
	Then Validation message is thrown "True"
	Then DataSplit Small View is "NotVisible"
	When I Edit DataSplit Large View grid has
	| # | Data  | With  | Using | Padding | Align |
	| 1 | [[a]] | Index | 1     |         | Left  |
	| 2 |       | Index |       |         | Left  |
	When I click on "Done"
	Then Validation message is thrown "False"
	And DataSplit Small View is "Visible"

Scenario: Data Split Large View is validating invalid recordsets on done
	Given I have DataSplit Large View on design surface
	And I enter String to Split as "Test Warewolf" 
	When I Enter DataSplit Large View grid has
	| # | Results      | With  | Using | Include    | Escape |
	| 1 | [[rec().a.]] | Index | 1     | Unselected |        |
	| 2 |              | Index |       | Unselected |        |
	And result is as ""			    
	And Scroll bar is "Disabled"
	When I click on "Done"
	Then Validation message is thrown "True"
	Then DataSplit Small View is "NotVisible"
	When I Edit DataSplit Large View grid has
	| # | Results     | With  | Using | Include    | Escape |
	| 1 | [[rec().a]] | Index | 1     | Unselected |        |
	| 2 |             | Index |       | Unselected |        |
	When I click on "Done"
	Then Validation message is thrown "False"
	And DataSplit Small View is "Visible"


Scenario Outline: Data Split Large View is validating invalid variables
	Given I have DataSplit Large View on design surface
	And I enter String to Split as "Test Warewolf" 
	When I Enter DataSplit Large View grid has
	| # | Results | With  | Using | Include    | Escape |
	| 1 | '<Var>' | Index | 1     | Unselected |        |
	| 2 |         | Index |       | Unselected |        |
	And result is as ""			       
	And Scroll bar is "Disabled"
	When I click on "Done"
	Then Validation message is thrown '<Validation>'
Examples: 
    | No | Var              | Validation |
    | 1  | [[rec(@).a]]     | True       |
    | 2  | [[[rec().a]]     | True       |
    | 3  | [[[[a]]]         | True       |
    | 4  | [[a]]            | False      |
    | 5  | Split            | False      |
    | 6  | [[a]][[rec().a]] | True       |
    | 7  | [[rec([[a]]).a]] | True       |


Scenario Outline: Invalid variables in using is validating on done
	Given I have DataSplit Large View on design surface
	And I enter String to Split as "Test Warewolf" 
	When I Enter DataSplit Large View grid has
	| # | Results | With  | Using   | Include    | Escape |
	| 1 | [[a]]   | Index | '<Var>' | Unselected |        |
	| 2 | [[b]]   | Index | 2       | Unselected |        |
	| 3 |         |       |         |            |        |
	And result is as ""			       
	And Scroll bar is "Disabled"
	When I click on "Done"
	Then Validation message is thrown '<Validation>'
Examples: 
    | No | Var              | Validation |
    | 1  | [[rec(@).a]]     | True       |
    | 2  | [[[rec().a]]     | True       |
    | 3  | [[[[a]]]         | True       |
    | 4  | [[a]]            | False      |
    | 5  | Split            | True       |
    | 6  | [[a]][[rec().a]] | False      |
    | 7  | [[rec([[a]]).a]] | False      |
    | 8  | 12               | False      |


Scenario Outline: Invalid variables in Escape is validating on done
	Given I have DataSplit Large View on design surface
	And I enter String to Split as "Test Warewolf" 
	When I Enter DataSplit Large View grid has
	| # | Results | With  | Using | Include    | Escape  |
	| 1 | [[a]]   | Chars | 2     | Unselected | '<Var>' |
	| 2 | [[b]]   | Index | 2     | Unselected |         |
	| 3 |         |       |       |            |         |
	And result is as ""			       
	And Scroll bar is "Disabled"
	When I click on "Done"
	Then Validation message is thrown '<Validation>'
Examples: 
    | No | Var              | Validation |
    | 1  | [[rec(@).a]]     | True       |
    | 2  | [[[rec().a]]     | True       |
    | 3  | [[[[a]]]         | True       |
    | 4  | [[a]]            | False      |
    | 5  | Split            | False      |
    | 6  | [[a]][[rec().a]] | False      |
    | 7  | [[rec([[a]]).a]] | False      |
    | 8  | 12               | False      |
    | 9  | a                | False      |
    | 10 | ,                | False      |



Scenario: Collapse largeview is closing large view
	Given I have DataSplit Small View on design surface
	And I enter String to Split as "Test Warewolf" 
	When I open DataSplit large view
	Then DataSplit Large view is "Visible"
	When I Enter DataSplit Large View grid has
	| # | Results | With  | Using | Include    | Escape |
	| 1 | [[a]]   | Chars | 2     | Unselected |        |
	| 2 | [[b]]   | Index | 2     | Unselected |        |
	| 3 |         |       |       |            |        |
	When I collapse large view
	And Validation message is thrown "False"
	Then DataSplit Small View is "Visible"


Scenario: Opening DataSplit Quick Variable Input
	Given I have DataSplit Small View on design surface
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


Scenario: Adding DataSplit Variables by using QVI
    Given I have DataSplit Small View on design surface
	When I select "QVI"
	Then "Quick Variable Input" large view is opened
	And Variable list text box is "Visible"
	And I enter variables 
	| a |
	| b |
	| c |
	| d |
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
	And Add button is "Enabled"
	When I click on "Add"
	Then DataSplit Small View is "Visible" 
	When I Enter  DataSplit Small View grid has
	| # | Data  | With  | Using |
	| 1 | [[a]] | Index |       |
	| 2 | [[b]] | Index |       |
	| 3 | [[c]] | Index |       |
	| 4 | [[d]] | Index |       |
	And Scroll bar is "Enabled"

Scenario: Adding DataSplit Variables by using QVI and split on chars
    Given I have DataSplit Large view on design surface
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
	And Add button is "Enabled"
	When I click on "Add"
	Then DataSplit Large view is "Visible" 
	And Large View grid has
	When I Enter DataSplit Large View grid has
	| # | Results | With  | Using | Include    | Escape |
	| 1 | [[a]]   | Index |       | Unselected |        |
	| 2 | [[b]]   | Index |       | Unselected |        |
	| 3 | [[c]]   | Index |       | Unselected |        |
	| 4 | [[d]]   | Index |       | Unselected |        |
	| 5 |         |       |       |            |        |
	And Scroll bar is "Enabled"



##This split by using 'Tab' is not working because I can't use tab while entering variable list but I can paste 
## So option must work as expected.
Scenario: Adding DataSplit Variables by using QVI and split on Tab
    Given I have DataSplit Large view on design surface
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
	And Add button is "Enabled"
	When I click on "Add"
	Then DataSplit Large view is "Visible" 
	And Large View grid has
	| # | Results | With  | Using | Include    | Escape |
	| 1 | [[a]]   | Index |       | Unselected |        |
	| 2 | [[b]]   | Index |       | Unselected |        |
	| 3 | [[c]]   | Index |       | Unselected |        |
	| 4 | [[d]]   | Index |       | Unselected |        |
	| 5 |         |       |       |            |        |
	And Scroll bar is "Enabled"

Scenario: Adding Variables in DataSplit QVI and split on chars
    Given I have DataSplit Large view on design surface
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
	And Add button is "Enabled"
	When I click on "Add"
	Then DataSplit Large view is "Visible" 
	| # | Results | With  | Using | Include    | Escape |
	| 1 | [[a]]   | Index |       | Unselected |        |
	| 2 | [[b]]   | Index |       | Unselected |        |
	| 3 | [[c]]   | Index |       | Unselected |        |
	| 4 | [[d]]   | Index |       | Unselected |        |
	| 5 | [[e]]   | Index |       | Unselected |        |
	| 6 | [[f]]   | Index |       | Unselected |        |
	| 7 | [[g]]   | Index |       | Unselected |        |
	| 8 | [[h]]   | Index |       | Unselected |        |
	| 9 |         |       |       |            |        |
	And Scroll bar is "Enabled"

Scenario Outline: DataSplit QVI Prefix and Suffix
    Given I have DataSplit Large view on design surface
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
	And Add button is "Enabled"
	When I click on "Add"
	Then DataSplit Large view is "Visible" 
	And Large View grid has
	Then DataSplit Large view is "Visible" 
	| # | Results | With  | Using | Include    | Escape |
	| 1 | [[aa]]  | Index |       | Unselected |        |
	| 2 | [[aa]]  | Index |       | Unselected |        |
	| 3 | [[aa]]  | Index |       | Unselected |        |
	| 4 | [[aa]]  | Index |       | Unselected |        |
	| 5 |         | Index |       | Unselected |        |
	Examples: 
	| No | Prefix | Suffix | Append   | Replace    |
	| 1  | a      | ""     | Selected | Unselected |
	| 2  | ""     | a      | Selected | Unselected |
	And Scroll bar is "Enabled"

Scenario:  DataSplit QVI Replace is Replacing Variables
    Given I have DataSplit Large view on design surface
	And Large view grid has
	| # | Results | With  | Using | Include    | Escape |
	| 1 | [[a]]   | Index |       | Unselected |        |
	| 2 | [[b]]   | Index |       | Unselected |        |
	| 3 | [[c]]   | Index |       | Unselected |        |
	| 4 |         | Index |       | unselected |        |
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
	And Add button is "Enabled"
	When I click on "Add"
	Then DataSplit Large view is "Visible" 
	And Large View grid has
	| # | Results     | With  | Using | Include    | Escape |
	| 1 | [[rec().a]] | Index |       | Unselected |        |
	| 2 | [[rec().b]] | Index |       | Unselected |        |
	| 3 | [[c]]       | Index |       | Unselected |        |
	| 4 |             | Index |       | unselected |        |
	And Scroll bar is "Enabled"

	 