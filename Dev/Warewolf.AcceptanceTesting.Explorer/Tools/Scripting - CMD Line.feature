Feature: Scripting - CMD Line
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@mytag
Scenario: Execute Command Line small view
	Given I have CMD Line tool on design surface
	And I have CMD as ""
	When Result as ""

Scenario: Execute Command Line Large view
	Given I have CMD Line tool on design surface
	When I open CMD Line large view
	And I have CMD as ""
	Then I have Priority selected "Normal"
	And Result as ""
	And On Error box consists
	| Put error in this variable | Call this web service |
	|                            |                       |
	And End this workflow is "Unselected"
	And Done button is "Visible"

Scenario: Execute Command Line water marks Large view
	Given I have CMD Line tool on design surface
	When I open CMD Line large view
	And I have CMD Water mark as "CMD"
	Then I have Priority selected "Normal"
	And Result water msrk as "[[Result]]"
	And On Error box consists
	| Put error in this variable | Call this web service        |
	| [[Errors().Message]]       | http://lcl:3142/services/err |
	And End this workflow is "Unselected"
	And Done button is "Visible"

Scenario: Execute Command Line is not thrown error for poper commands
	Given I have CMD Line tool on design surface
	When I open CMD Line large view
	And I have CMD as "pause"
	Then I have Priority selected "Normal"
	And Result as "[[Result]]"
	And End this workflow is "Unselected"
	And Done button is "Visible"
	When I click on "Done"
	Then Validation message is not thrown


Scenario Outline: Execute Command Line Large view validates for incorrect variables
	Given I have CMD Line tool on design surface
	When I open CMD Line large view
	And I have CMD as '<Var>'
	Then I have Priority selected "Normal"
	And Result as "[[Result]]"
	And End this workflow is "Unselected"
	And Done button is "Visible"
	When I click on "Done"
	Then Validation message is thrown '<Validation>'
	Examples: 
	| No | Var              | Validation |
	| 1  | [[a]]            | False      |
	| 2  | [[a]][[b]]       | False      |
	| 3  | [[a]][[rec().a]] | False      |
	| 4  | [[a!]]           | True       |
	| 5  | [[rec().a!]]     | True       |
	| 6  | [[rec([[a]]).a]] | False      |
	| 7  | Pause[[a]]       | False      |


Scenario Outline: Execute Command Line Large view validates for incorrect variables in result
	Given I have CMD Line tool on design surface
	When I open CMD Line large view
	And I have CMD as "[[a]]"
	Then I have Priority selected "Normal"
	And Result as '<Result>'
	And End this workflow is "Unselected"
	And Done button is "Visible"
	When I click on "Done"
	Then Validation message is thrown '<Validation>'
	Examples: 
	| No | Result           | Validation |
	| 1  | result           | False      |
	| 2  | [[result]]       | False      |
	| 3  | [[a]][[b]]       | True       |
	| 4  | [[rec([[a]]).a]] | True       |
	| 5  | [[[[a]]]]        | True       |
	| 6  | [[rec(*).a]]     | False      |
	| 7  | [[rec().a@]]     | True       |


Scenario: Execute Command Line Large view persisting data to small view
	Given I have CMD Line tool on design surface
	When I open CMD Line large view
	And I have CMD as "pause [[a]]"
	Then I have Priority selected "Idle"
	And Result as "[[Result]]"
	And End this workflow is "Unselected"
	And Done button is "Visible"
	When I click on "Done"
	Then Validation message is not thrown 
	Then CMD Line small view is "Visible"
	And I have CMD as "pause [[a]]"
	And Result as "[[Result]]"


Scenario Outline: CMD line Priority is not changing when close and open largeview
	Given I have CMD Line tool on design surface
	When I open CMD Line large view
	And I have CMD as "pause [[a]]"
	Then I have Priority selected '<Priority>'
	And Result as "[[Result]]"
	And End this workflow is "Unselected"
	And Done button is "Visible"
	When I click on "Done"
	Then Validation message is not thrown 
	Then CMD Line small view is "Visible"
	When I open CMD Line large view
	Then I have Priority selected '<Priority>'
Examples: 
    | No | Priority     |
    | 1  | Normal       |
    | 2  | Below Normal |
    | 3  | Above Normal |
    | 4  | Idle         |
    | 5  | High         |
    | 6  | Real Time    |



Scenario: CMD tool large view validates if fields are empty
	Given I have CMD Line tool on design surface
	When I open CMD Line large view
	And I have CMD as ""
	Then I have Priority selected "Normal"
	And Result as ""
	And End this workflow is "Unselected"
	And Done button is "Visible"
	When I click on "Done"
	Then Validation message is thrown






