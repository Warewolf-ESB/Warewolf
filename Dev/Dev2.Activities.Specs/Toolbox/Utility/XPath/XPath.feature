Feature: XPath
	In order to run a query against xml
	As a Warewolf user
	I want to a tool that I can use to execute xpath queries

Scenario: Use XPath to get data off XML
	Given I have this xml '<root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root>'
	And I have a variable "[[firstNum]]" with xpath "//root/number[@id='1']/text()"
	When the xpath tool is executed
	Then the variable "[[firstNum]]" should have a value "One"
