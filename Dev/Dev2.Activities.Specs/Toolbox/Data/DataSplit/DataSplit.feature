Feature: DataSplit
	In order to split data
	As a Warewolf user
	I want a tool that splits two or more pieces of data

Scenario: Split text to a recordset using split type index 
	Given A string to split with value "abcde"
	And  assign to variable "[[vowels(*).letters]]" split type "Index" at "1"
	When the data split tool is executed
	Then the split result will be
	| letters |
	| a		  |
	| b		  |
	| c		  |
	| d		  |
	| e		  |