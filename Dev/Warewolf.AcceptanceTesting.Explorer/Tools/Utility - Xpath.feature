Feature: Utility - c
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Xpath
Scenario: Xpath small view and Large view
	Given I have Xpath small view on design surface
	And I have XML is ""
	And Xpath snall view grid has
	| # | Result | Xpath |
	| 1 |        |       |
	| 2 |        |       |
	When I open LArge View
	Then I have XML is ""
	And Xpath snall view grid has
	| # | Result | Xpath |
	| 1 |        |       |
	| 2 |        |       |
	And Scroll bar is "Disabled"
	And Done button is "Visible"


Scenario: Xpath Large view is validating empty fields
	Given I have Xpath Large view on design surface
	Then I have XML is ""
	And Xpath snall view grid has
	| # | Result | Xpath |
	| 1 |        |       |
	| 2 |        |       |
	And Scroll bar is "Disabled"
	And Done button is "Visible"
	When I click on Done
	Then Validation message is thrown


Scenario: Xpath water marks small view and Large view
	Given I have Xpath small view on design surface
	And I have XML watermarks is "[[Xml]]"
	And Xpath snall view grid has watermarks
	| # | Result          | Xpath |
	| 1 | [[recset().F1]] | Xpath |
	| 2 |                 | Xpath |
	When I open LArge View
	Then I have XML  watermarks "[[Xml]]"
	Then I have XML watermarks is "[[Xml]]"
	And Xpath snall view grid has watermarks
	| # | Result          | Xpath |
	| 1 | [[recset().F1]] | Xpath |
	| 2 |                 | Xpath |
	And Scroll bar is "Disabled"
	And Done button is "Visible"

Scenario Outline: Xpath Large view is validating Incorrect Variables
	Given I have Xpath Large view on design surface
	Then I enter XML is ""
	And Xpath snall view grid has
	| # | Result | Xpath                         |
	| 1 | [[a]]  | //root/number[@id='1']/text() |
	| 2 |        |                               |
	And Scroll bar is "Disabled"
	And Done button is "Visible"
	When I click on Done
	Then Validation message is thrown '<VaL>'
Examples: 
    | No | XML                                           | VaL   |
    | 1  | <root><number id="1">One</number><number      | False |
    | 2  | [[a]]                                         | False |
    | 3  | [[rec(*).a]]                                  | False |
    | 4  | [[rec([[a]]).a]]                              | False |
    | 5  | [[a]][[b]]                                    | False |
    | 6  | [[a..]]                                       | True  |
    | 7  | [[rec().a.]]                                  | True  |
    | 8  | [[[[a]]                                       | True  |
    | 9  | [[rec().a]]]]                                 | True  |
    | 10 | <root><number id="1">One</number><number[[a]] | False |
    | 11 |                                               | True  |


Scenario Outline: Xpath Large view is validating Incorrect Output variables
	Given I have Xpath Large view on design surface
	Then I enter XML is "<root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root>"
	And Xpath snall view grid has
	| # | Result   | Xpath                         |
	| 1 | <Output> | //root/number[@id='1']/text() |
	| 2 |          |                               |
	And Scroll bar is "Disabled"
	And Done button is "Visible"
	When I click on Done
	Then Validation message is thrown '<VaL>'
Examples: 
    | No | Output           | VaL   |
    | 1  | result           | False |
    | 2  | [[result]]       | False |
    | 3  | [[a]][[b]]       | True  |
    | 4  | [[rec([[a]]).a]] | True  |
    | 5  | [[[[a]]]]        | True  |
    | 6  | [[rec(*).a]]     | False |
    | 7  | [[rec().a@]]     | True  |
    | 8  | [[a]]]]          | True  |


Scenario Outline: Inserting Rows in large view
	Given I have Xpath Large view on design surface
	Then I enter XML is "<root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root>"
	And Xpath Large view grid has
	| # | Result        | Xpath                         |
	| 1 | [[rec(1).id]] | //root/number[@id='1']/text() |
	| 2 | [[rec(2).id]] |                               |
	| 3 | [[rec(2).id]] |                               |
	| 4 | [[rec(1).id]] | //root/number[@id='1']/text() |
	| 5 | [[rec(2).id]] |                               |
	| 6 | [[rec(2).id]] |                               |
	| 7 |               |                               |
	And Scroll bar is "Enaabled"
	And Done button is "Visible"
	When I Insert Row at "2"
	| # | Result        | Xpath                         |
	| 1 | [[rec(1).id]] | //root/number[@id='1']/text() |
	| 2 | [[Insert      |                               |
	| 3 | [[rec(2).id]] |                               |
	| 4 | [[rec(2).id]] |                               |
	| 5 | [[rec(1).id]] | //root/number[@id='1']/text() |
	| 6 | [[rec(2).id]] |                               |
	| 7 | [[rec(2).id]] |                               |
	| 8 |               |                               |

Scenario Outline: Deleting Rows in large view
	Given I have Xpath Large view on design surface
	Then I enter XML is "<root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root>"
	And Xpath Laarge view grid has
	| # | Result        | Xpath                         |
	| 1 | [[rec(1).id]] | //root/number[@id='1']/text() |
	| 2 | [[rec(2).id]] |                               |
	| 3 | [[rec(3).id]] |                               |
	| 4 | [[rec(4).id]] | //root/number[@id='1']/text() |
	| 5 | [[rec(5).id]] |                               |
	| 6 | [[rec(6).id]] |                               |
	| 7 |               |                               |
	And Scroll bar is "Enaabled"
	And Done button is "Visible"
	When I Delete Row at "2"
	And Xpath Laarge view grid has
	| # | Result        | Xpath                         |
	| 1 | [[rec(1).id]] | //root/number[@id='1']/text() |
	| 2 | [[rec(3).id]] |                               |
	| 3 | [[rec(4).id]] | //root/number[@id='1']/text() |
	| 4 | [[rec(5).id]] |                               |
	| 5 | [[rec(6).id]] |                               |
	| 6 |               |                               |







