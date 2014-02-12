Feature: XPath
	In order to run a query against xml
	As a Warewolf user
	I want a tool that I can use to execute xpath queries

Scenario: Use XPath to get data off XML - Id = 1
	Given I have this xml '<root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root>'
	And I have a variable "[[firstNum]]" output with xpath "//root/number[@id='1']/text()"
	When the xpath tool is executed
	Then the variable "[[firstNum]]" should have a value "One"
	And the execution has "NO" error

Scenario: Use XPath to get data off XML - Id = 2
	Given I have this xml '<root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root>'
	And I have a variable "[[firstNum]]" output with xpath "//root/number[@id='2']/text()"
	When the xpath tool is executed
	Then the variable "[[firstNum]]" should have a value "Two"
	And the execution has "NO" error

Scenario: Use XPath to build a recordset with 2 fields
	Given I have this xml '<root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root>'
	And I have a variable "[[rec().id]]" output with xpath "//root/number/@id"
	And I have a variable "[[rec2().text]]" output with xpath "//root/number/text()"
	When the xpath tool is executed
	Then the xpath result for this varibale "[[rec().id]]" will be
	| rec().id |
	| 1        |
	| 2        |
	| 3        |
	Then the xpath result for this varibale "[[rec2().text]]" will be
	| rec().text |
	| One        |
	| Two        |
	| Three      |
	And the execution has "NO" error

Scenario: Use XPath that does not exist
	Given I have this xml '<root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root>'	
	And I have a variable "[[ids]]" output with xpath "//root/num/@id"
	When the xpath tool is executed
	Then the variable "[[ids]]" should have a value ""
	And the execution has "NO" error

Scenario: Use invalid xpath query
	Given I have this xml '<root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root>' in a variable "[[myxml]]"
	And I assign the variable "[[myxml]]" as xml input
	And I have a variable "[[ids]]" output with xpath "@@#$"
	When the xpath tool is executed
	Then the variable "[[ids]]" should have a value ""
	And the execution has "AN" error

Scenario: Use XPath with invalid XML as input inside a variable
	Given I have this xml '<root></end>' in a variable "[[myxml]]"
	And I assign the variable "[[myxml]]" as xml input
	And I have a variable "[[ids]]" output with xpath "//root"
	When the xpath tool is executed
	Then the variable "[[ids]]" should have a value ""
	And the execution has "AN" error

	
Scenario: Use XPath with no variable result but valid xpath
	Given I have this xml '<root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root>'
	And I have a variable "" output with xpath "//root/number/@id"
	When the xpath tool is executed	
	Then the execution has "NO" error

Scenario: Use XPath to get multiple results into a scalar in CSV
	Given I have this xml '<root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root>'
	And I have a variable "[[ids]]" output with xpath "//root/number/@id"
	When the xpath tool is executed
	Then the variable "[[ids]]" should have a value "1,2,3"
	And the execution has "NO" error

Scenario: Use the XPath to process blank XML
	Given I have this xml ''
	And I have a variable "[[ids]]" output with xpath "//root/number/@id"
	When the xpath tool is executed
	Then the variable "[[ids]]" should have a value ""
	And the execution has "AN" error

Scenario: Use the XPath without any xpath query
	Given I have this xml '<root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root>'
	And I have a variable "[[ids]]" output with xpath ""
	When the xpath tool is executed
	Then the variable "[[ids]]" should have a value ""
	And the execution has "AN" error
	
Scenario: Use XPath with blank variable as XML input	
	Given I have this xml '' in a variable "[[myxml]]"
	And I assign the variable "[[myxml]]" as xml input
	And I have a variable "[[ids]]" output with xpath "//root/num/@id"
	When the xpath tool is executed
	Then the variable "[[ids]]" should have a value ""
	And the execution has "AN" error

Scenario: Use XPath with negative recordset index as XML input	
	Given I have this xml '<x></b></x>' in a variable "[[rec(-1).myxml]]"
	And I assign the variable "[[rec(-1).myxml]]" as xml input
	And I have a variable "[[ids]]" output with xpath "//b"
	When the xpath tool is executed
	Then the variable "[[ids]]" should have a value ""
	And the execution has "AN" error

Scenario: Use XPath with negative recordset index as output variable
	Given I have this xml '<root><number id="1">One</number><number id="2">Two</number><number id="3">Three</number></root>' in a variable "[[xml]]"
	And I assign the variable "[[xml]]" as xml input
	And I have a variable "[[rec(-1).ids]]" output with xpath "//root/number[@id='2']/text()"
	When the xpath tool is executed
	Then the execution has "AN" error

