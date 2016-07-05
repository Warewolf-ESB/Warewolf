Feature: Utility - Web Request
	In order to download html content
	As a Warewolf user
	I want a tool that I can input a url and get a html document
	
@NeedsBlankWorkflow
Scenario: Drag toolbox Get Web Request Tool onto a new workflow creates Get Web Request tool with large view on the design surface
	When I "Drag_GetWeb_RequestTool_Onto_DesignSurface"
	Then I "Assert_GetWeb_RequestTool_small_View_Exists_OnDesignSurface"

#@NeedsWebRequestSmallViewOnTheDesignSurface
#Scenario: Double Clicking Web Request Tool Small View on the Design Surface Opens Large View
	When I "Open_WebRequest_LargeView"
	Then I "Assert_GetWeb_RequestTool_Large_View_Exists_OnDesignSurface"

@ignore
@WebRequest
# Coded Ui Test
Scenario: Web Request Tool small view
	Given I have Web Request small view on design surface
	And I have URL ""
	And result is ""
	And Header is Not Visible
	And Preview is not visible
@ignore
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
@ignore
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

@ignore
Scenario Outline: Web Request Tool Large view is validating incorrect variables
	Given I have Web Request small view on design surface
	When I open Web Request Large View
	Then I Enter URL "<URL>"
	And I Enter header ""
	And I enter result is "[[Result]]"
	And Preview button is "Enabled"	
	And Done button is "Visible"
	When I click on "Done"
	Then Validation message is thrown "<Validation>"
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
	And I Enter header "<Header>"
	And I enter result is "[[Result]]"
	And Preview button is "Enabled"	
	And Done button is "Visible"
	When I click on "Done"
	Then Validation message is thrown "<Validation>"
Examples: 
   | No | URL                        | Validation |
   | 1  | http://[[site#]][[file!]]l | True       |
   | 2  | [[a@@#]]                   | True       |
   | 3  | [[a]]                      | False      |
   | 4  | [[a]][[b]]                 | False      |
   | 5  | http://[[site]][[file]]    | False      |
   | 6  | [[Rec().a]]                | False      |
   | 7  | [[[[rec().a]]              | True       |






    