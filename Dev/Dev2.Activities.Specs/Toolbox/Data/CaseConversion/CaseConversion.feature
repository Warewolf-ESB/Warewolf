Feature: CaseConversion
	In order to convert the case of words
	As a Warewolf user
	I want a tool that converts words from their current case to a selected case


Scenario: Convert a sentence to uppercase
	Given I convert a sentence "Warewolf Rocks" to "UPPER"	
	When the case conversion tool is executed
	Then the sentence will be "WAREWOLF ROCKS"
	And the case convert execution has "NO" error

Scenario: Convert a sentence to lowercase
	Given I convert a sentence "Warewolf Rocks" to "lower"	
	When the case conversion tool is executed
	Then the sentence will be "warewolf rocks"
	And the case convert execution has "NO" error

Scenario: Convert a sentence to Sentence
	Given I convert a sentence "WAREWOLF Rocks" to "Sentence"	
	When the case conversion tool is executed
	Then the sentence will be "Warewolf rocks"
	And the case convert execution has "NO" error

Scenario: Convert a sentence to Title Case
	Given I convert a sentence "WAREWOLF Rocks" to "Title Case"	
	When the case conversion tool is executed
	Then the sentence will be "Warewolf Rocks"
	And the case convert execution has "NO" error

Scenario: Convert a sentence starting with a number to UPPER CASE
	Given I convert a sentence "1 Warewolf Rocks" to "UPPER"	
	When the case conversion tool is executed
	Then the sentence will be "1 WAREWOLF ROCKS"
	And the case convert execution has "NO" error

Scenario: Convert a sentence starting with a number to lower case
	Given I convert a sentence "1 Warewolf Rocks" to "lower"	
	When the case conversion tool is executed
	Then the sentence will be "1 warewolf rocks"
	And the case convert execution has "NO" error

Scenario: Convert a sentence starting with a number to Sentence case
	Given I convert a sentence "1 WAREWOLF Rocks" to "Sentence"	
	When the case conversion tool is executed
	Then the sentence will be "1 warewolf rocks"
	And the case convert execution has "NO" error

Scenario: Convert a sentence starting with a number to Title Case
	Given I convert a sentence "1 WAREWOLF Rocks" to "Title Case"	
	When the case conversion tool is executed
	Then the sentence will be "1 Warewolf Rocks"
	And the case convert execution has "NO" error

Scenario: Convert a blank to Title Case
	Given I convert a sentence "" to "Title Case"	
	When the case conversion tool is executed
	Then the sentence will be ""
	And the case convert execution has "NO" error

Scenario: Convert a blank to Sentencecase
	Given I convert a sentence "" to "Sentence"	
	When the case conversion tool is executed
	Then the sentence will be ""
	And the case convert execution has "NO" error

Scenario: Convert a blank to UPPER CASE
	Given I convert a sentence "" to "UPPER"	
	When the case conversion tool is executed
	Then the sentence will be ""
	And the case convert execution has "NO" error

Scenario: Convert a blank to lowercase
	Given I convert a sentence "" to "lower"	
	When the case conversion tool is executed
	Then the sentence will be ""
	And the case convert execution has "NO" error

Scenario: Convert a recordset * to Upper
	Given I have a CaseConversion recordset
	| rs       | val                 |
	| rs().row | <x id="1">One</x>   |
	| rs().row | <x id="2">two</x>   |
	| rs().row | <x id="3">three</x> |
	And I convert a variable "[[rs(*).row]]" to "UPPER"
	When the case conversion tool is executed
	Then the case convert result for this varibale "rs().row" will be
	| rs       | val                 |
	| rs().row | <X ID="1">ONE</X>   |
	| rs().row | <X ID="2">TWO</X>   |
	| rs().row | <X ID="3">THREE</X> |
	And the case convert execution has "NO" error

Scenario: Convert an empty recordset * to Upper
	Given I have a CaseConversion recordset
	| rs       | val                 |
	And I convert a variable "[[rs(*).row]]" to "UPPER"
	When the case conversion tool is executed
	Then the case convert result for this varibale "rs().row" will be
	| rs       | val                 |
	And the case convert execution has "NO" error

Scenario: Convert a empty sentence starting with a number to upper
	Given I convert a sentence "" to "UPPER"	
	When the case conversion tool is executed
	Then the sentence will be ""
	And the case convert execution has "NO" error

Scenario: Convert a negative recordset index to uppercase
	Given I convert a sentence "[[my(-1).sentenct]]" to "UPPER"	
	When the case conversion tool is executed
	Then the case convert execution has "AN" error

Scenario: Convert a negative recordset index to lowercase
	Given I convert a sentence "[[my(-1).sentenct]]" to "lower"	
	When the case conversion tool is executed
	Then the case convert execution has "AN" error

Scenario: Convert a negative recordset index to Sentence
	Given I convert a sentence "[[my(-1).sentenct]]" to "Sentence"	
	When the case conversion tool is executed
	Then the case convert execution has "AN" error

Scenario: Convert a negative recordset index to Title Case
	Given I convert a sentence "[[my(-1).sentenct]]" to "Title Case"	
	When the case conversion tool is executed
	Then the case convert execution has "AN" error
