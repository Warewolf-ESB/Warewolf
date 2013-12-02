Feature: Replace
	In order to search and replace
	As a Warewolf user
	I want a tool I can use to search and replace for words


Scenario: Replace placeholders in a sentence with names
	Given I have a sentence "Dear Mr XXXX, We welcome you as a customer"
	And I want to find the characters "XXXX"
	And I want to replace them with "Warewolf user"
	When the replace tool is executed
	Then the replaced result should be "Dear Mr Warewolf user, We welcome you as a customer"
