Feature: Replace
	In order to search and replace
	As a Warewolf user
	I want a tool I can use to search and replace for words


Scenario: Replace placeholders in a sentence with names
	Given I have a replace variable "[[sentence]]" equal to "Dear Mr XXXX, We welcome you as a customer"
	And I have a sentence "[[sentence]]"
	And I want to find the characters "XXXX"
	And I want to replace them with "Warewolf user"
	When the replace tool is executed
	Then the result should be "1"
	And "[[sentence]]" should be "Dear Mr Warewolf user, We welcome you as a customer"

Scenario: Replace when the in field(s) is blank
	Given I have a replace variable "[[sentence]]" equal to ""
	And I have a sentence "[[sentence]]"
	And I want to find the characters "XXXX"
	And I want to replace them with "Warewolf user"
	When the replace tool is executed
	Then the result should be "0"
	And "[[sentence]]" should be ""

Scenario: Replace when text to find is blank 
	Given I have a replace variable "[[sentence]]" equal to "Dear Mr XXXX, We welcome you as a customer"
	And I have a sentence "[[sentence]]"
	And I want to find the characters ""
	And I want to replace them with "Warewolf user"
	When the replace tool is executed
	Then the result should be "0"
	And "[[sentence]]" should be "Dear Mr XXXX, We welcome you as a customer"

Scenario: Replace when the replace with is blank
	Given I have a replace variable "[[sentence]]" equal to "Dear Mr XXXX, We welcome you as a customer"
	And I have a sentence "[[sentence]]"
	And I want to find the characters "XXXX"
	And I want to replace them with ""
	When the replace tool is executed
	Then the result should be "1"
	And "[[sentence]]" should be "Dear Mr , We welcome you as a customer"

Scenario: Replace using lower case to find uppercase value
	Given I have a replace variable "[[sentence]]" equal to "Dear Mr AAAA, We welcome you as a customer"
	And I have a sentence "[[sentence]]"
	And I have a replace variable "[[find]]" equal to "aaaa"
	And I want to find the characters "[[find]]"
	And I want to replace them with "Case"
	When the replace tool is executed
	Then the result should be "1"
	And "[[sentence]]" should be "Dear Mr Case, We welcome you as a customer"