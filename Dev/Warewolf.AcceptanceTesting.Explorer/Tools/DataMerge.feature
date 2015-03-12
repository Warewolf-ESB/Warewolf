Feature: DataMerge
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@DataMerge
Scenario: DataMerge Small View
	Given I have Datamerge small view on design surface
	And DataMerge small view grid as
	| # | Recordset | Using |  |
	| 1 |           | Index |  |
	| 2 |           | Index |  |
	And result is as ""
	And Scroll bar is "Disabled"

Scenario: DataMerge Large View
	Given I have Datamerge Large view on design surface
	And DataMerge small view grid as
	| # | Recordset | Using |  |
	| 1 |           | Index |  |
	| 2 |           | Index |  |
	And result is as ""
	And Scroll bar is "Enabled"
	And On Error box consists
	| Put error in this variable | Call this web service |
	|                            |                       |
	And End this workflow is "Unselected"
	And Done button is "Visible"

Scenario: Passing Variables in small view
	Given I have Datamerge small view on design surface
	When I Enter  DataMerge small view grid as
	| # | Data | Using |   |
	| 1 | Test | Index | 1 |
	| 2 | Ware | Index | 2 |
	And result is as ""
	And Scroll bar is "Disabled"
	When I Insert Row at "3"