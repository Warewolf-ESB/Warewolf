Feature: DataMerge
	In order to merege to values
	As a Warewolf User
	I want to mere variable vaules are values

@DataMerge
Scenario: DataMerge Small View
	Given I have DataMerge Small View on design surface
	And DataMerge Small View grid as
	| # | Recordset | With  | Using |
	| 1 |           | Index |       |
	| 2 |           | Index |       |
	And result is as ""
	And Scroll bar is "Disabled"

Scenario: DataMerge Large View
	Given I have DataMerge Large View on design surface
	And DataMerge Small View grid as
	| # | Recordset | With  | Using |
	| 1 |           | Index |       |
	| 2 |           | Index |       |
	And result is as ""
	And Scroll bar is "Enabled"
	And On Error box consists
	| Put error in this variable | Call this web service |
	|                            |                       |
	And End this workflow is "Unselected"
	And Done button is "Visible"

Scenario: Passing Variables in Small View and inserting row
	Given I have DataMerge Small View on design surface
	When I Enter  DataMerge Small View grid as
	| # | Data | With  | Using |
	| 1 | Test | Index | 1     |
	| 2 | Ware | Index | 2     |
	And result is as ""
	And Scroll bar is "Enabled"
	When I Insert Row at "2"
	Then DataMerge Small View grid as
	| # | Data | With  | Using |
	| 1 | Test | Index | 1     |
	| 2 |      | Index |       |
	| 3 | Ware | Index | 2     |

Scenario: Deleting rows in Small View
	Given I have DataMerge Small View on design surface
	When I Enter  DataMerge Small View grid as
	| # | Data | With  | Using |
	| 1 | Test | Index | 1     |
	| 2 | Ware | Index | 2     |
	| 3 |      | Index |       |
	And result is as ""
	And Scroll bar is "Enabled"
	When I delete Row at "2"
	Then DataMerge Small View grid as
	| # | Data | With  | Using |
	| 1 | Test | Index | 1     |
	| 2 |      | Index |       |
	

Scenario: Passing Variables in Large View and inserting row
	Given I have DataMerge Large View on design surface
	When I Enter  DataMerge Large View grid as
	| # | Data | With  | Using | Padding | Align |
	| 1 | Test | Index | 1     |         | Left  |
	| 2 | Ware | Index | 2     |         | Right |
	| 3 |      | Index |       |         | Left  |
	And result is as ""
	And Scroll bar is "Enabled"
	When I Insert Row at "2"
	Then DataMerge Small View grid as
	| # | Data | With  | Using | Padding | Align |
	| 1 | Test | Index | 1     |         | Left  |
	| 2 |      | Index |       |         | Right |
	| 3 | Ware | Index | 2     |         |       |
	And Scroll bar is "Enabled"

Scenario: Deleting rows in Large View
	Given I have DataMerge Large View on design surface
	When I Enter  DataMerge Large View grid as
	| # | Data | With  | Using |Padding | Align |
	| 1 | Test | Index | 1     |        | Left  |
	| 2 | Ware | Index | 2     |        | Right |
	And result is as ""			       
	And Scroll bar is "Enabled"
	When I delete Row at "2"
	Then DataMerge Small View grid as
	| # | Data | With  | Using | Padding | Align |
	| 1 | Test | Index | 1     |         | Left  |
	| 2 |      | Index |       |         |       |
	And Scroll bar is "Enabled"
							


Scenario Outline: Data Merge Large View using and padding is enabled and disabled
	Given I have DataMerge Large View on design surface
	When I select "Row 1"  with as '<With>' then using is '<Using>' and padding is '<Padding>'
Examples: 
    | No | With     | Using    | Padding  |
    | 1  | None     | Disabled | Disabled |
    | 2  | Index    | Enabled  | Enabled  |
    | 3  | New Line | Enabled  | Enabled  |
    | 4  | Chars    | Enabled  | Disabled |
    | 5  | Tab      | Disabled | Disabled |
	
	
Scenario Outline: Data Merge Small View using is enabled and disabled
	Given I have DataMerge Large View on design surface
	When I select "Row 1"  with as '<With>' then using is '<Using>' 
Examples: 
    | No | With     | Using    |
    | 1  | None     | Disabled |
    | 2  | Index    | Enabled  |
    | 3  | New Line | Enabled  |
    | 4  | Chars    | Enabled  |
    | 5  | Tab      | Disabled |
		
	
	
Scenario: Data Merge Large View is validating invalid variables on done
	Given I have DataMerge Large View on design surface
	When I Enter DataMerge Large View grid as
	| # | Data   | With  | Using | Padding | Align |
	| 1 | [[a#]] | Index |       |         | Left  |
	| 2 |        | Index |       |         | Left  |
	And result is as ""			   
	And Scroll bar is "Enabled"
	When I click on "Done"
	Then Validation message is thrown "True"
	Then DataMerge Small View is "NotVisible"
	When I Edit DataMerge Large View grid as
	| # | Data  | With  | Using | Padding | Align |
	| 1 | [[a]] | Index |       |         | Left  |
	| 2 |       | Index |       |         | Left  |
	When I click on "Done"
	Then Validation message is thrown "False"
	And DataMerge Small View is "Visible"

Scenario: Data Merge Large View is validating invalid recordsets on done
	Given I have DataMerge Large View on design surface
	When I Enter DataMerge Large View grid as
	| # | Data         | With  | Using | Padding | Align |
	| 1 | [[rec().a.]] | Index |       |         | Left  |
	| 2 |              | Index |       |         | Left  |
	And result is as ""			    
	And Scroll bar is "Enabled"
	When I click on "Done"
	Then Validation message is thrown "True"
	Then DataMerge Small View is "NotVisible"
	When I Edit DataMerge Large View grid as
	| # | Data        | With  | Using | Padding | Align |
	| 1 | [[rec().a]] | Index |       |         | Left  |
	| 2 |             | Index |       |         | Left  |
	When I click on "Done"
	Then Validation message is thrown "False"
	And DataMerge Small View is "Visible"


Scenario Outline: Data Merge Large View is validating invalid variables
	Given I have DataMerge Large View on design surface
	When I Enter DataMerge Large View grid as
	| # | Data    | With  | Using | Padding | Align |
	| 1 | '<Var>' | Index |       |         | Left  |
	| 2 |         | Index |       |         | Left  |
	And result is as ""			       
	And Scroll bar is "Enabled"
	When I click on "Done"
	Then Validation message is thrown '<Validation>'
Examples: 
    | No | Var              | Validation |
    | 1  | [[rec(@).a]]     | True       |
    | 2  | [[[rec().a]]     | True       |
    | 3  | [[[[a]]]         | True       |
    | 4  | [[a]]            | False      |
    | 5  | merge            | False      |
    | 6  | [[a]][[rec().a]] | False      |
    | 7  | [[rec([[a]]).a]] | True       |


Scenario Outline: Invalid variables in using is validating on done
	Given I have DataMerge Large View on design surface
	When I Enter DataMerge Large View grid as
	| # | Data    | With  | Using   | Padding | Align |
	| 1 | abcdef' | Index | '<Var>' |         | Left  |
	| 2 |         | Index |         |         | Left  |
	And result is as ""			       
	And Scroll bar is "Enabled"
	When I click on "Done"
	Then Validation message is thrown '<Validation>'
Examples: 
    | No | Var              | Validation |
    | 1  | [[rec(@).a]]     | True       |
    | 2  | [[[rec().a]]     | True       |
    | 3  | [[[[a]]]         | True       |
    | 4  | [[a]]            | False      |
    | 5  | merge            | True       |
    | 6  | [[a]][[rec().a]] | False      |
    | 7  | [[rec([[a]]).a]] | False      |
    | 8  | 12               | False      |


Scenario Outline: Invalid variables in Padding is validating on done
	Given I have DataMerge Large View on design surface
	When I Enter DataMerge Large View grid as
	| # | Data    | With  | Using | Padding | Align |
	| 1 | abcdef' | Index | 1     | '<Var>' | Left  |
	| 2 |         | Index |       |         | Left  |
	And result is as ""			       
	And Scroll bar is "Enabled"
	When I click on "Done"
	Then Validation message is thrown '<Validation>'
Examples: 
    | No | Var              | Validation |
    | 1  | [[rec(@).a]]     | True       |
    | 2  | [[[rec().a]]     | True       |
    | 3  | [[[[a]]]         | True       |
    | 4  | [[a]]            | False      |
    | 5  | merge            | True       |
    | 6  | [[a]][[rec().a]] | False      |
    | 7  | [[rec([[a]]).a]] | False      |
    | 8  | 12               | False      |
    | 9  | a                | False      |
    | 10 | ,                | False      |

Scenario Outline: Invalid variables in Result is validating on done
	Given I have DataMerge Large View on design surface
	When I Enter DataMerge Large View grid as
	| # | Data    | With  | Using | Padding | Align |
	| 1 | abcdef' | Index | 1     | ,       | Left  |
	| 2 |         | Index |       |         | Left  |
	And result is as '<Var>'			       
	And Scroll bar is "Enabled"
	When I click on "Done"
	Then Validation message is thrown '<Validation>'
Examples: 
    | No | Var              | Validation |
    | 1  | [[rec(@).a]]     | True       |
    | 2  | [[[rec().a]]     | True       |
    | 3  | [[[[a]]]         | True       |
    | 4  | [[a]]            | False      |
    | 5  | merge            | False      |
    | 6  | [[a]][[rec().a]] | True       |
    | 7  | [[rec([[a]]).a]] | True       |
    | 8  | 12               | True       |
    | 9  | a                | False      |
    | 10 | ,                | True       |


Scenario: Collapse largeview is closing large view
	Given I have DataMerge Small View on design surface
	When I open DataMerge large view
	Then DataMerge Large view is "Visible"
	When I Enter DataMerge Large View grid as
	| # | Data   | With  | Using | Padding | Align |
	| 1 | [[a#]] | Index | 1     |         | Left  |
	| 2 |        | Index |       |         | Left  |
	When I collapse large view
	And Validation message is thrown "False"
	Then DataMerge Small View is "Visible"


Scenario: Opening DataMerge Quick Variable Input
	Given I have DataMerge Small View on design surface
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


Scenario: Adding DataMerge Variables by using QVI
    Given I have DataMerge Small View on design surface
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
	And Add button is "Enabled"
	When I click on "Add"
	Then DataMerge Small View is "Visible" 
	When I Enter  DataMerge Small View grid as
	| # | Data  | With  | Using |
	| 1 | [[a]] | Index |       |
	| 2 | [[b]] | Index |       |
	| 3 | [[c]] | Index |       |
	| 4 | [[d]] | Index |       |
	| 5 |       | Index |       |
	And Scroll bar is "Enabled"

Scenario: Adding DataMerge Variables by using QVI and split on chars
    Given I have DataMerge Large view on design surface
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
	Then DataMerge Large view is "Visible" 
	And Large View grid as
	Then DataMerge Large View grid as
	| # | Data  | With  | Using | Padding | Align |
	| 1 | [[a]] | Index |       |         | Left  |
	| 2 | [[b]] | Index |       |         | Left  |
	| 3 | [[c]] | Index |       |         | Left  |
	| 4 | [[d]] | Index |       |         | Left  |
	| 5 |       | Index |       |         | Left  |
	And Scroll bar is "Enabled"

##This split by using 'Tab' is not working because I can't use tab while entering variable list but I can paste 
## So option must work as expected.
Scenario: Adding DataMerge Variables by using QVI and split on Tab
    Given I have DataMerge Large view on design surface
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
	Then DataMerge Large view is "Visible" 
	And Large View grid as
	| # | Data  | With  | Using | Padding | Align |
	| 1 | [[a]] | Index |       |         | Left  |
	| 2 | [[b]] | Index |       |         | Left  |
	| 3 | [[c]] | Index |       |         | Left  |
	| 4 | [[d]] | Index |       |         | Left  |
	| 5 |       | Index |       |         | Left  |
	And Scroll bar is "Enabled"

Scenario: Adding Variables in Datamerge QVI and split on chars
    Given I have DataMerge Large view on design surface
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
	Then DataMerge Large view is "Visible" 
	| # | Data  | With  | Using |
	| 1 | [[a]] | Index |       |
	| 2 | [[b]] | Index |       |
	| 3 | [[c]] | Index |       |
	| 4 | [[d]] | Index |       |
	| 5 | [[e]] | Index |       |
	| 6 | [[f]] | Index |       |
	| 7 | [[g]] | Index |       |
	| 8 | [[h]] | Index |       |
	| 9 |       | Index |       |
	And Scroll bar is "Enabled"

Scenario Outline: DataMerge QVI Prefix and Suffix
    Given I have DataMerge Large view on design surface
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
	Then DataMerge Large view is "Visible" 
	And Large View grid as
	| # | Data   | With  | Using | Padding | Align |
	| 1 | [[aa]] | Index |       |         | Left  |
	| 2 | [[aa]] | Index |       |         | Left  |
	| 3 | [[aa]] | Index |       |         | Left  |
	| 4 | [[aa]] | Index |       |         | Left  |
	| 5 |        | Index |       |         | Left  |
	And Scroll bar is "Enabled"
	Examples: 
	| No | Prefix | Suffix | Append   | Replace    |
	| 1  | a      | ""     | Selected | Unselected |
	| 2  | ""     | a      | Selected | Unselected |


Scenario:  DataMerge QVI Replace is Replacing Variables
    Given I have DataMerge Large view on design surface
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
	And Add button is "Enabled"
	When I click on "Add"
	Then DataMerge Large view is "Visible" 
	And Large View grid as
	| # | Data        | With  | Using |
	| 1 | [[rec().a]  | Index |       |
	| 2 | [[rec().b]] | Index |       |
	| 3 |             | Index |       |
	And Scroll bar is "Enabled"




	 