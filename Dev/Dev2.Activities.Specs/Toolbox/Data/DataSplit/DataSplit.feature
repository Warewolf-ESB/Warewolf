Feature: DataSplit
	In order to split data
	As a Warewolf user
	I want a tool that splits two or more pieces of data

Scenario: Split text to a recordset using Index 
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

#BB
#Scenario: Split characters using Index Going Backwards
#	Given A string to split with value "%$#@!?><":|}{+_)(*&^~"
#	And assign to variable "[[vowels(*).chars]]" split type "Index" at "7"
#	When the data split tool is executed
#	Then the split result will be
#	| chars		|
#	| _)(*&^~	|
#	| <":|}{+	|
#	| %$#@!?>	|
	

#Scenario: Split text using All split types - Some with Include selected
#	Given A string to split with value "IndexTab	Chars,space end"
#	And assign to variable "[[vowels(*).letters]]" split type "Index" at "5" and Include Selected and Escapre "\"
#	And assign to variable "[[vowels(*).letters]]" split type "Tab"
#	And assign to variable "[[vowels(*).letters]]" split type "Chars" at "ars," and Include "Selected"
#	And assign to variable "[[vowels(*).letters]]" split type "Space" and Escapre "\"
#	And assign to variable "[[vowels(*).letters]]" split type "End"
#	When the data split tool is executed
#	Then the split result will be
#	| letters |
#	| Index		  |
#	| Tab		  |
#	| Chars,	  |
#	| space		  |
#	| end		  |


#Scenario: Split CSV file format into recordset - some fields blank
#Scenario: Split CSV file format into recordset - Skip blank rows selected
#Scenario: Split CSV file format into recordset - ignoring some fields
#Scenario: Split blank text using All split types
#Scenario: Split text using Index where index > provided index-Padding is NULL
#Scenario: Split text using Index where index > provided index-Padding is 0
#Scenario: Split number using Index where index > provided index-Padding is NULL
#Scenario: Split number using Index where index > provided index-Padding is 0
#Scenario: Split text using Char and Escape character
