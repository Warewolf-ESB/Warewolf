Feature: Recordset - FindRecordIndex
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@FindRecordIndex
Scenario: Find Record Index small view
	Given I have Find Record Index Small view on design surface
	And Infield is ""
	And Mtach type "Row 1" is "Select"
	And Match at "Row 1" is "Disabled"
	And Mtach type "Row 2" is "Select"
	And Match at "Row 2" is "Disabled"
	And Scrool bar is "Disabled"
	And find Record Index small view as
	|   |           |          |
	| 1 | Choose... | Disabled |
	| 2 | Choose... | Disabled |
	And result is as ""

	 
Scenario: Find Record Index Large view
	Given I have Find Record Index Large view on design surface
	And Infield is ""
	And Mtach type "Row 1" is "Select"
	And Match at "Row 1" is "Disabled"
	And Mtach type "Row 2" is "Select"
	And Match at "Row 2" is "Disabled"
	And Scrool bar is "Disabled"
	And Find Record Index large view as
	| # | Match Type | Match    |
	| 1 | Choose...  | Disabled |
	| 2 | Choose...  | Disabled |
	And Require All Matches To Be True is "Selected"
	And Require All Fields To Match is "Unseleted"
	And result is ""
	And Done button is "Visible"
	And On Error box consists
	| Put error in this variable | Call this web service |
	|                            |                       |
	And End this workflow is "Unselected"
	And Done button is "Visible"


Scenario: Find Record Index small view water marks
	Given I have Find Record Index Small view on design surface
	And Infield water mark is "[[Recordset().Field]]"
	And Mtach type water mark for "Row 1" is "Choose..."
	And Match at "Row 1" water mark for is "Match"
	And Mtach type water mark for "Row 2" is "Choose..."
	And Match at "Row 2" water mark for is "Match"
	And result water mark is "[[RowWithResult]]"
	
	
Scenario: Find Record Index Large view water marks
	Given I have Find Record Index Large view on design surface
	And Infield water mark is ""
	And Mtach type "Row 1" water mark is "Choose..."
	And Match at "Row 1" water mark is "Match"
	And Mtach type "Row 2" water mark is "Choose..."
	And Match at "Row 2" is "Match"
	And Find Record Index large view as
	| # | Match Type | Match    |
	| 1 | Choose...  | Disabled |
	| 2 | Choose...  | Disabled |
	And Require All Matches To Be True is "Selected"
	And Require All Fields To Match is "Unseleted"
	And result water mark is "[[RowWithResult]]"
	And Done button is "Visible"
	And On Error water mark 
	 | Put error in this variable | Call this web service        | End this workflow |
	 | [[Error().Message]]        | http://lcl:3142/services/err | UnSelected        |
    And Done button is "Visible"

	
	
Scenario: FRI small view to large view data persisting correctly
	Given I have Find Record Index Small view on design surface
	And Infield is "[[rec().a]]"
	When I enter find Record Index small view 
	|   |           |          |
	| 1 | =         | 1        |
	| 2 | Choose... | Disabled |
	And result is "[[Result]]"
	When I open largeview
	And Find Record Index large view as
	| # | Match Type | Match    |
	| 1 | =          | 1        |
	| 2 | Choose...  | Disabled |
	And Scroll bar is "Disabled"
	And Require All Matches To Be True is "Selected"
	And Require All Fields To Match is "Unseleted"
	And result water mark is "[[RowWithResult]]"
	And Done button is "Visible"
	And On Error water mark 
	 | Put error in this variable | Call this web service | End this workflow |
	 |                            |                       | UnSelected        |
    And Done button is "Visible"



Scenario: In small view Rows are creating one after one and scroll bar is enabling
	Given I have Find Record Index Small view on design surface
	And Infield is "[[rec().a]]"
	When I enter find Record Index small view 
	|   |           |          |
	| 1 | =         | 1        |
	| 2 | Choose... | Disabled |
	And Scroll bar is "Disabled"
	And result is "[[Result]]"
	When I enter find Record Index small view 
	|   |           |          |
	| 1 | =         | 1        |
	| 2 | >         | 12       |
	| 3 | Choose... | Disabled |
	And Scroll bar is "Enabled"
	When I open largeview
	And Find Record Index large view as
	| # | Match Type | Match    |
	| 1 | =          | 1        |
	| 2 | >          | 12       |
	| 3 | Choose...  | Disabled |
	And Scroll bar is "Disabled"
	And Require All Matches To Be True is "Selected"
	And Require All Fields To Match is "Unseleted"
	And result water mark is "[[RowWithResult]]"
	And Done button is "Visible"



Scenario: Fields are creating one after one in large view
	Given I have Find Record Index Large view on design surface
	And Infield is "[[rec().a]]"
	And Find Record Index large view as
	| # | Match Type | Match    |
	| 1 | =          | 1        |
	| 2 | >          | 12       |
	| 3 | =          | 1        |
	| 4 | >          | 12       |
	| 5 | =          | 1        |
	| 6 | >          | 12       |
	| 7 | Choose...  | Disabled |
	And Scroll bar is "Enaabled"
	And Require All Matches To Be True is "Selected"
	And Require All Fields To Match is "Unseleted"
	And result is "[[res]]"
	And Done button is "Visible"
	When I close Large View
	Then FRI small view is "Visible"
	And Assign small view as
	|   |           |          |
	| 1 | =         | 1        |
	| 2 | >         | 12       |
	| 3 | =         | 1        |
	| 4 | >         | 12       |
	| 5 | =         | 1        |
	| 6 | >         | 12       |
	| 7 | Choose... | Disabled |
	 And Scroll bar is "Enaabled"


Scenario Outline: LargeView Selecting Match Type from dropbox
	Given I have Find Record Index Large view on design surface
	And Infield is "[[rec().a]]"
	When I Enter Find Record Index large view 
	| # | Match Type | Match    |
	| 1 | <Select>   | 1        |
	| 2 | Choose...  | Disabled |
	When I close large view
	Then FRI small view is "Visible"
	And Small View as
	|   |           |          |
	| 1 | <Select>  | 1        |
	| 2 | Choose... | Disabled |
Examples: 
    | No | Match Type         |
    | 1  | =                  |
    | 2  | >                  |
    | 3  | <                  |
    | 4  | <> (Not Equal)     |
    | 5  | >=                 |
    | 6  | <=                 |
    | 7  | Starts With        |
    | 8  | Ends With          |
    | 9  | Contains           |
    | 10 | Doesn't Start With |
    | 11 | Doesn't End With   |
    | 12 | Doesn't Contain    |
    | 13 | Is Alphanumeric    |
    | 14 | Is Base64          |
    | 15 | Is Between         |
    | 17 | Is Binary          |
    | 18 | Is Date            |
    | 19 | Is Email           |
    | 20 | Is Hex             |
    | 21 | Is Numeric         |
    | 22 | Is Rgex            |
    | 23 | Is Test            |
    | 24 | Is XML             |
    | 25 | Not Alphanumeric   |
    | 26 | Not Base64         |
    | 27 | Not Between        |
    | 28 | Not Binary         |
    | 29 | Not Date           |
    | 30 | Not Email          |
    | 31 | Not Hex            |
    | 32 | Not Numeric        |
    | 33 | Not Rgex           |
    | 34 | Not Test           |
    | 35 | Not XML            |
	

Scenario Outline: Small View Selecting Match Type from dropbox
	Given I have Find Record Index Small view on design surface
	And Infield is "[[rec().a]]"
	When I Enter Find Record Index Small view 
		|   |              |          |
		| 1 | <Match Type> | <Match>  |
		| 2 | Choose...    | Disabled |
    When I open Assign large view
	Then FRI Large view is "Visible"
	And Large view as
	| # | Match Type | Match    |
	| 1 | <Select>   | 1        |
	| 2 | Choose...  | Disabled |
Examples: 
    | No | Match Type         | Match    |
    | 1  | =                  | 123      |
    | 2  | >                  | 123      |
    | 3  | <                  | 123      |
    | 4  | <> (Not Equal)     | 123      |
    | 5  | >=                 | 123      |
    | 6  | <=                 | 123      |
    | 7  | Starts With        | 123      |
    | 8  | Ends With          | 123      |
    | 9  | Contains           | 123      |
    | 10 | Doesn't Start With | 123      |
    | 11 | Doesn't End With   | 123      |
    | 12 | Doesn't Contain    | 123      |
    | 13 | Is Alphanumeric    | Disabled |
    | 14 | Is Base64          | Disabled |
    | 15 | Is Between         |          |
    | 17 | Is Binary          | Disabled |
    | 18 | Is Date            | Disabled |
    | 19 | Is Email           | Disabled |
    | 20 | Is Hex             | Disabled |
    | 21 | Is Numeric         | Disabled |
    | 22 | Is Rgex            | Disabled |
    | 23 | Is Test            | Disabled |
    | 24 | Is XML             | Disabled |
    | 25 | Not Alphanumeric   | Disabled |
    | 26 | Not Base64         | Disabled |
    | 27 | Not Between        |          |
    | 28 | Not Binary         | Disabled |
    | 29 | Not Date           | Disabled |
    | 30 | Not Email          | Disabled |
    | 31 | Not Hex            | Disabled |
    | 32 | Not Numeric        | Disabled |
    | 33 | Not Rgex           | Disabled |
    | 34 | Not Test           | Disabled |
    | 35 | Not XML            | Disabled |


	

Scenario: FRI is validating when I click on done with empty fields
	Given I have Find Record Index Large view on design surface
	And Infield is ""
	And Find Record Index large view as
	| # | Match Type | Match    |
	| 1 | Choose...  | Disabled |
	| 2 | Choose...  | Disabled |
	And Require All Matches To Be True is "Selected"
	And Require All Fields To Match is "Unseleted"
	And result is ""
	And On Error box consists
	| Put error in this variable | Call this web service |
	|                            |                       |
	And End this workflow is "Unselected"
	And Done button is "Visible"
	When I click on "Done"
	Then Validation message is thrown



Scenario Outline: FRI large view is validating Invalid variables in InField
	Given I have Find Record Index Large view on design surface
	And Infield is '<Infields>'
	And Find Record Index large view as
	| # | Match Type | Match    |
	| 1 | =          | 124      |
	| 2 | Choose...  | Disabled |
	And Require All Matches To Be True is "Selected"
	And Require All Fields To Match is "Unseleted"
	And result is ""
	And On Error box consists
	| Put error in this variable | Call this web service |
	|                            |                       |
	And End this workflow is "Unselected"
	And Done button is "Visible"
	When I click on "Done"
	Then Validation message is thrown '<Val>'
	And FRI small view is '<SmallView>'
Examples: 
     | No | InFields               | Val   | SmallView   |
     | 1  | ABC                    | True  | Not Visible |
     | 2  | 123                    | True  | Not Visible |
     | 3  | [[a]]                  | False | Visible     |
     | 4  | [[rec().a]]            | False | Visible     |
     | 5  | [[a]][[b]]             | False | Visible     |
     | 6  | [[rec().a]][[a]]       | False | Visible     |
     | 7  | [[rec().a]][[rec().a]] | False | Visible     |
     | 8  | [[rec([[a]]).a]]       | False | Visible     |
     | 9  | [[rec.a]]              | True  | Not Visible |
     | 10 | [[rec(*).a]]           | False | Visible     |
     | 11 | [[rec(().a]]           | True  | Not Visible |
     | 12 | [[rec().a.]]           | True  | Not Visible |
     | 13 | [[rec().a]]]]          | True  | Not Visible |
     | 14 | [[[[rec().a]]]]        | False | Visible     |
     | 15 | [[rec().a]]abc         | True  | Not Visible |
     | 16 | [[rec([[a]]).a]]       | False | Visible     |
     | 17 | [[a@]]                 | True  | Not Visible |

Scenario Outline: FRI large view is validating Invalid variables in Match Fields
	Given I have Find Record Index Large view on design surface
	And Infield is "[[rec().a]]"
	And Find Record Index large view as
	| # | Match Type | Match    |
	| 1 | =          | <Match>  |
	| 2 | Choose...  | Disabled |
	And Require All Matches To Be True is "Selected"
	And Require All Fields To Match is "Unseleted"
	And result is ""
	And On Error box consists
	| Put error in this variable | Call this web service |
	|                            |                       |
	And End this workflow is "Unselected"
	And Done button is "Visible"
	When I click on "Done"
	Then Validation message is thrown '<Val>'
	And FRI small view is '<SmallView>'
Examples: 
     | No | Match                  | Val   | SmallView   |
     | 1  | ABC                    | False | Visible     |
     | 2  | 123                    | False | Visible     |
     | 3  | [[a]]                  | False | Visible     |
     | 4  | [[rec().a]]            | False | Visible     |
     | 5  | [[a]][[b]]             | False | Visible     |
     | 6  | [[rec().a]][[a]]       | False | Visible     |
     | 7  | [[rec().a]][[rec().a]] | False | Visible     |
     | 8  | [[rec([[a]]).a]]       | False | Visible     |
     | 9  | [[rec.a]]              | True  | Not Visible |
     | 10 | [[rec(*).a]]           | False | Visible     |
     | 11 | [[rec(().a]]           | True  | Not Visible |
     | 12 | [[rec().a.]]           | True  | Not Visible |
     | 13 | [[rec().a]]]]          | True  | Not Visible |
     | 14 | [[[[rec().a]]]]        | False | Visible     |
     | 15 | [[rec().a]]abc         | True  | Not Visible |
     | 16 | [[rec([[a]]).a]]       | False | Visible     |
     | 17 | [[a@]]                 | True  | Not Visible |


Scenario: FRI large view deleting row
	Given I have Find Record Index Large view on design surface
	And Infield is "[[rec().a]]"
	And Find Record Index large view as
	| # | Match Type | Match    |
	| 1 | =          | abc      |
	| 2 | >          | 12       |
	| 3 | <          | 12       |
	| 4 | Choose...  | Disabled |
	And Require All Matches To Be True is "Selected"
	And Require All Fields To Match is "Unseleted"
	And result is "[[Result]]"
	And Done button is "Visible"
	When I Delete "Row 2" 
	Then Find Record Index large view as
	| # | Match Type | Match    |
	| 1 | =          | abc      |
	| 2 | <          | 12       |
	| 3 | Choose...  | Disabled |
	When I click on "Done"
	And FRI small view is "Visible"
	And find Record Index small view as
	|   |           |          |
	| 1 | =         | abc      |
	| 2 | <         | 12       |
	| 3 | Choose... | Disabled |



Scenario Outline: FRI large view is validating Invalid variables in Result Fields
	Given I have Find Record Index Large view on design surface
	And Infield is "[[rec().a]]"
	And Find Record Index large view as
	| # | Match Type | Match    |
	| 1 | =          | 1234567  |
	| 2 | Choose...  | Disabled |
	And Require All Matches To Be True is "Selected"
	And Require All Fields To Match is "Unseleted"
	And result is '<Result>'
	And Done button is "Visible"
	When I click on "Done"
	Then Validation message is thrown '<Val>'
	And FRI small view is '<SmallView>'
Examples: 
     | No | Result                 | Val   | SmallView   |
     | 1  | ABC                    | False | Visible     |
     | 2  | 123                    | True  | Not Visible |
     | 3  | [[a]]                  | False | Visible     |
     | 4  | [[rec().a]]            | False | Visible     |
     | 5  | [[a]][[b]]             | True  | Not Visible |
     | 6  | [[rec().a]][[a]]       | True  | Not Visible |
     | 7  | [[rec().a]][[rec().a]] | True  | Not Visible |
     | 8  | [[rec([[a]]).a]]       | True  | Not Visible |
     | 9  | [[rec.a]]              | True  | Not Visible |
     | 10 | [[rec(*).a]]           | False | Visible     |
     | 11 | [[rec(().a]]           | True  | Not Visible |
     | 12 | [[rec().a.]]           | True  | Not Visible |
     | 13 | [[rec().a]]]]          | True  | Not Visible |
     | 14 | [[[[rec().a]]]]        | True  | Not Visible |
     | 15 | [[rec().a]]abc         | True  | Not Visible |
     | 16 | [[rec([[a]]).a]]       | True  | Not Visible |
     | 17 | [[a@]]                 | True  | Not Visible |




Scenario: FRI is not validating when I close large view with incorrect fields
	Given I have Find Record Index Large view on design surface
	And Infield is "[[rec().a@]]"
	And Find Record Index large view as
	| # | Match Type | Match    |
	| 1 | =          | [[a#]]   |
	| 2 | Choose...  | Disabled |
	And Require All Matches To Be True is "Selected"
	And Require All Fields To Match is "Unseleted"
	And result is "[[result#]]"
	And Done button is "Visible"
	When I close large view
	Then Validation message is not thrown
	And FRI small view is "Visible"
	And Infield is "[[rec().a@]]"
	And find Record Index small view as
	|   |           |          |
	| 1 | =         | [[a#]]   |
	| 2 | Choose... | Disabled |















