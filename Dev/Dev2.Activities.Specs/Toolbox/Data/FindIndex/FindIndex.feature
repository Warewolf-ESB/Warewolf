Feature: FindIndex
	In order to find where characters or values are in sentences or words
	As a Warewolf user
	I want a tool that finds indexes

Scenario: Find the first occurence of a character in a sentence
	Given Given the sentence "I have managed to spend time in real innovation since I started using Warewolf"
	And I selected Index "First Occurrence"
	And I search for characters "since"
	And I selected direction as "Left to Right"
	When the data find index tool is executed
	Then the find index result is "49"

#BB
#Scenario: Find all occurences of a word in a sentence and output to scalar going left to right
#Scenario: Find all occurences of a word in a sentence and output to recordset going right to left 
#Scenario: Find last occurence of a bracet in a sentence
#Scenario: Find first occurence of a character in a blank string
#Scenario: Find first occurence of a character in a string where it doesnt exist
#Scenario: Find all occurences of a character in a string where it doesnt exist

