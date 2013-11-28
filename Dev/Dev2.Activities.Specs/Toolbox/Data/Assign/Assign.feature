Feature: Assign
	In order to use variables 
	As a Warewolf user
	I want a tool that assigns data to variables

Scenario: Assign a value to a variable
	Given I assign the value 10 to a variable "[[var]]"	
	When the assign tool is executed
	Then the value of "[[var]]" equals 10

Scenario: Assign a variable to a variable
	Given I assign the value 20 to a variable "[[var]]"	
	And I assign the value 60 to a variable "[[test]]"
	And I assign the value "[[test]]" to a variable "[[var]]"
	When the assign tool is executed
	Then the value of "[[var]]" equals 60

Scenario: Assign multiple variables to a variable
	Given I assign the value "Hello" to a variable "[[var]]"	
	And I assign the value "World" to a variable "[[test]]"
	And I assign the value "[[var]][[test]]" to a variable "[[value]]"
	When the assign tool is executed
	Then the value of "[[value]]" equals "HelloWorld"