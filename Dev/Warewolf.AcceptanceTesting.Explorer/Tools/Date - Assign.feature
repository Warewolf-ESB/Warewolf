Feature: Date - Assign
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Assign
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
	And Scroll bar is "Enabled"
	And Done button is "Visible"
	And Quick Variable Input button is "Visible"