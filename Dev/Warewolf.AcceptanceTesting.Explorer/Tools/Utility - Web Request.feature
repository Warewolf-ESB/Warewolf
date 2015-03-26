Feature: Utility - Web Request
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@WebRequest
Scenario: Web Request Tool small view
	Given I have Web Request small view on design surface
	And I have URL ""
	And result is ""
	And Header is Not Visible
	And Preview is not visible

Scenario: Web Request Tool Large view
	Given I have Web Request small view on design surface
	When I open Web Request Large View
	Then I have URL ""
	And I have header ""
	And i result is ""
	And Preview button is "Disabled"
	And On Error box consists
	| Put error in this variable | Call this web service |
	|                            |                       |
	And End this workflow is "Unselected"
	And Done button is "Visible"

Scenario: Web Request Tool Large view is not throwing error for valid data
	Given I have Web Request small view on design surface
	When I open Web Request Large View
	Then I Enter URL "http://[[site]][[file]]l"
	And I Enter header ""
	And I enter result is "[[Result]]"
	And Preview button is "Enabled"	
	And Preview is
	|            |
	| [[site]] = |
	| [[file]] = |
	And Done button is "Visible"
	When I click on "Preview"
	Then Validation message is thrown
	When I Enter in preview
	|                                                 |
	| [[site]] = rsaklfsvrtfsbld/IntegrationTestSite/ |
	| [[file]] = Proxy.ashx?html                      |
	When I click on "Preview"
	Then Validation message is not thrown


Scenario Outline: Web Request Tool Large view is validating incorrect variables
	Given I have Web Request small view on design surface
	When I open Web Request Large View
	Then I Enter URL '<URL>'
	And I Enter header ""
	And I enter result is "[[Result]]"
	And Preview button is "Enabled"	
	And Done button is "Visible"
	When I click on "Done"
	Then Validation message is thrown '<Validation>'
Examples: 
   | No | URL                        | Validation |
   | 1  | http://[[site#]][[file!]]l | True       |
   | 2  | [[a@@#]]                   | True       |
   | 3  | [[a]]                      | False      |
   | 4  | [[a]][[b]]                 | False      |
   | 5  | http://[[site]][[file]]    | False      |
   | 6  | [[Rec().a]]                | False      |
   | 7  | [[[[rec().a]]              | True       |



Scenario Outline: Web Request Tool Large view is validating incorrect variables in Header
	Given I have Web Request small view on design surface
	When I open Web Request Large View
	Then I Enter URL "http://[[site]][[file]]"
	And I Enter header '<Header>'
	And I enter result is "[[Result]]"
	And Preview button is "Enabled"	
	And Done button is "Visible"
	When I click on "Done"
	Then Validation message is thrown '<Validation>'
Examples: 
   | No | URL                        | Validation |
   | 1  | http://[[site#]][[file!]]l | True       |
   | 2  | [[a@@#]]                   | True       |
   | 3  | [[a]]                      | False      |
   | 4  | [[a]][[b]]                 | False      |
   | 5  | http://[[site]][[file]]    | False      |
   | 6  | [[Rec().a]]                | False      |
   | 7  | [[[[rec().a]]              | True       |


Scenario Outline: Web Request Tool Large view is validating incorrect variables in Header
	Given I have Web Request small view on design surface
	When I open Web Request Large View
	Then I Enter URL "http://[[site]][[file]]"
	And I Enter header '<Header>'
	And I enter result is "[[Result]]"
	And Preview button is "Enabled"	
	And Done button is "Visible"
	When I click on "Done"
	Then Validation message is thrown '<Validation>'
Examples: 
   | No | URL                        | Validation |
   | 1  | http://[[site#]][[file!]]l | True       |
   | 2  | [[a@@#]]                   | True       |
   | 3  | [[a]]                      | False      |
   | 4  | [[a]][[b]]                 | False      |
   | 5  | http://[[site]][[file]]    | False      |
   | 6  | [[Rec().a]]                | False      |
   | 7  | [[[[rec().a]]              | True       |




Scenario Outline: Web Request Tool Large view is validating incorrect variables in Header
	Given I have Web Request small view on design surface
	When I open Web Request Large View
	Then I Enter URL "http://[[site]][[file]]"
	And I Enter header ""
	And I enter result is '<Result>'
	And Preview button is "Enabled"	
	And Done button is "Visible"
	When I click on "Done"
	Then Validation message is thrown '<Validation>'
Examples: 
   | No | Result              | Validation |
   | 1  | result           | False      |
   | 2  | [[result]]       | False      |
   | 3  | [[a]][[b]]       | True       |
   | 4  | [[rec([[a]]).a]] | True       |
   | 5  | [[[[a]]]]        | True       |
   | 6  | [[rec(*).a]]     | False      |
   | 7  | [[rec().a@]]     | True       |






    