Feature: XPath
	In order to run a query against xml
	As a Warewolf user
	I want a tool that I can use to execute xpath queries

Scenario: Use XPath to get data off XML - Id = 1
	Given I have this xml '<root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root>'
	And I have a variable "[[firstNum]]" with xpath "//root/number[@id='1']/text()"
	When the xpath tool is executed
	Then the variable "[[firstNum]]" should have a value "One"

Scenario: Use XPath to get data off XML - Id = 2
	Given I have this xml '<root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root>'
	And I have a variable "[[firstNum]]" with xpath "//root/number[@id='2']/text()"
	When the xpath tool is executed
	Then the variable "[[firstNum]]" should have a value "Two"

#Scenario: Use XPath to build a recordset with 3 fields
#Scenario: Use invalid XPath
#Scenario: Use XPath with blank variable as XML input
#Scenario: Use XPath with invalid XML as input inside a variable
#Scenario: Use XPath with no variable result but valid xpath
#Scenario: Use XPath to get multiple results into a scalar in CSV