Feature: CaseConversion
	In order to convert the case of words
	As a Warewolf user
	I want a tool that converts words from their current case to a selected case


Scenario: Convert a sentence to uppercase
	Given I convert a sentence "Warewolf Rocks" to "UPPER"	
	When the case conversion tool is executed
	Then the sentence will be "WAREWOLF ROCKS"

Scenario: Convert a sentence to lowercase
	Given I convert a sentence "Warewolf Rocks" to "lower"	
	When the case conversion tool is executed
	Then the sentence will be "warewolf rocks"

Scenario: Convert a sentence to Sentence
	Given I convert a sentence "WAREWOLF Rocks" to "Sentence"	
	When the case conversion tool is executed
	Then the sentence will be "Warewolf rocks"

Scenario: Convert a sentence to Title Case
	Given I convert a sentence "WAREWOLF Rocks" to "Title Case"	
	When the case conversion tool is executed
	Then the sentence will be "Warewolf Rocks"

#BB

Scenario: Convert a sentence starting with a number to UPPER CASE
	Given I convert a sentence "1 Warewolf Rocks" to "UPPER"	
	When the case conversion tool is executed
	Then the sentence will be "1 WAREWOLF ROCKS"

Scenario: Convert a sentence starting with a number to lower case
	Given I convert a sentence "1 Warewolf Rocks" to "lower"	
	When the case conversion tool is executed
	Then the sentence will be "1 warewolf rocks"

Scenario: Convert a sentence starting with a number to Sentence case
	Given I convert a sentence "1 WAREWOLF Rocks" to "Sentence"	
	When the case conversion tool is executed
	Then the sentence will be "1 warewolf rocks"

Scenario: Convert a sentence starting with a number to Title Case
	Given I convert a sentence "1 WAREWOLF Rocks" to "Title Case"	
	When the case conversion tool is executed
	Then the sentence will be "1 Warewolf Rocks"