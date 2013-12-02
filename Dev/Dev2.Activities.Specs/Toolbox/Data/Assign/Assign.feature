Feature: Assign
	In order to use variables 
	As a Warewolf user
	I want a tool that assigns data to variables

Scenario: Assign a value to a variable
	Given I assign the value 10 to a variable "[[var]]"	
	When the assign tool is executed
	Then the value of "[[var]]" equals 10

Scenario: Assign a variable to a variable
	Given I assign the value "20" to a variable "[[var]]"	
	And I assign the value "60" to a variable "[[test]]"
	And I assign the value "[[test]]" to a variable "[[var]]"
	When the assign tool is executed
	Then the value of "[[var]]" equals "60"

Scenario: Assign a variable to mixed scalar, char and recordset values
	Given I assign the value "Hello" to a variable "[[var]]"	
	And I assign the value "World" to a variable "[[rec(1).set]]"
	And I assign the value "[[var]] [[rec(1).set]] !" to a variable "[[value]]"
	When the assign tool is executed
	Then the value of "[[value]]" equals "Hello World !"

#BB
Scenario: Assign multiple variables to the end of a recordset
	Given I assign the value "10" to a variable "[[rec().set]]"	
	And I assign the value "20" to a variable "[[rec().set]]"
	And I assign the value "30" to a variable "[[rec().set]]"
	And I assign the value "[[rec(3).set]]" to a variable "[[value]]"
	When the assign tool is executed
	Then the value of "[[value]]" equals 30

Scenario: Assign all recordset values to a single variable
	Given I assign the value "10" to a variable "[[rec(1).set]]"	
	And I assign the value "20" to a variable "[[rec(2).set]]"
	And I assign the value "30" to a variable "[[rec(3).set]]"
	And I assign the value "" to a variable "[[rec(*).set]]"
	When the assign tool is executed
	Then the value of "[[rec(3).set]]" equals ""
	And the value of "[[rec(2).set]]" equals ""
	And the value of "[[rec(1).set]]" equals ""

Scenario: Assign a record set to a scalar
#Assign a scalar equal to a record set
#Assign a scalar equal to all records (*)
#Assign a scalar equal to a calculation
#Assign a variable equal to a group calculation (SUM)

