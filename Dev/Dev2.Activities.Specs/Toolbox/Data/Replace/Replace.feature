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


	#replace when the in field(s) is blank
	#replace when  text to find is blank (I ran this on the Studio - looks like a bug)
	#replace when the replace with is blank
