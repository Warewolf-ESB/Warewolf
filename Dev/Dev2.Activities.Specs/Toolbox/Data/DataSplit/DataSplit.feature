Feature: DataSplit
	In order to split data
	As a Warewolf user
	I want a tool that splits two or more pieces of data

Scenario: Split text to a recordset using Index 
	Given A string to split with value "abcde"
	And  assign to variable "[[vowels(*).letters]]" split type "Index" at "1"
	When the data split tool is executed
	Then the split result will be
	| vowels().letters |
	| a		  |
	| b		  |
	| c		  |
	| d		  |
	| e		  |
	And there is NO error

Scenario: Split characters using Index Going Backwards
	Given A string to split with value "@!?><":}{+_)(*&^~"
	And assign to variable "[[vowels(*).chars]]" split type "Index" at "7"
	When the data split tool is executed
	Then the split result will be
	| vowels().chars		|
	| _)(*&^~	|
	| ><":}{+	|
	| @!?		|
	And there is NO error

Scenario: Split text using All split types - Some with Include selected
	Given A string to split with value "IndexTab	Chars,space end"
	And assign to variable "[[vowels().letters]]" split type "Index" at "5" and Include "Selected" and Escape "\"
	And assign to variable "[[vowels().letters]]" split type "Tab"
	And assign to variable "[[vowels().letters]]" split type "Chars" at "ars," and Include "Selected"
	And assign to variable "[[vowels().letters]]" split type "Space" and Escapre "\"
	And assign to variable "[[vowels().letters]]" split type "End"
	When the data split tool is executed
	Then the split result will be
	| vowels().letters |
	| Index		  |
	| Tab		  |
	| Chars,	  |
	| space		  |
	| end		  |
	And there is NO error


Scenario: Split CSV file format into recordset - some fields blank
	Given A file "CSVExample.txt" to split
	And assign to variable "[[rec().id]]" split type "Chars" at "," and Include "Unselected"
	And assign to variable "[[rec().name]]" split type "Chars" at "," and Include "Unselected"
	And assign to variable "" split type "Chars" at "," and Include "Unselected"
	And assign to variable "[[rec().phone]]" split type "NewLine"
	And skip blank rows is "Unselected"
	When the data split tool is executed
	Then the split result will be
	| rec().id | rec().name | rec().phone |
	| ID       | NAME       | PHONE       |
	| 1        | Barney     | 1234        |
	| 2        | Tshepo     | 5678        |
	|          |            |             |
	| 3        | Mo         |             |
	And there is NO error

Scenario: Split CSV file format into recordset - Skip blank rows selected
	Given A file "CSVExample.txt" to split
	And assign to variable "[[rec().id]]" split type "Chars" at "," and Include "Unselected"
	And assign to variable "[[rec().name]]" split type "Chars" at "," and Include "Unselected"
	And assign to variable "" split type "Chars" at "," and Include "Unselected"
	And assign to variable "[[rec().phone]]" split type "NewLine"
	And skip blank rows is "Selected"
	When the data split tool is executed
	Then the split result will be
	| rec().id | rec().name | rec().phone |
	| ID       | NAME       | PHONE       |
	| 1        | Barney     | 1234        |
	| 2        | Tshepo     | 5678        |
	| 3        | Mo         | 01          |
	And there is NO error

Scenario: Split blank text using All split types
	Given A string to split with value ""
	And assign to variable "[[vowels().letters]]" split type "Index" at "5" and Include "Selected" and Escape "\"
	And assign to variable "[[vowels().letters]]" split type "Tab"
	And assign to variable "[[vowels().letters]]" split type "Chars" at "ars," and Include "Selected"
	And assign to variable "[[vowels().letters]]" split type "Space" and Escapre "\"
	And assign to variable "[[vowels().letters]]" split type "End"
	And assign to variable "[[vowels().letters]]" split type "NewLine"
	When the data split tool is executed
	Then the split result will be
	| vowels().letters |
	And there is NO error

Scenario: Split text using Index where index > provided
	Given A string to split with value "123"
	And assign to variable "[[var]]" split type "Index" at "5" and Include "Selected"
	When the data split tool is executed
	Then the split result for "[[var]]" will be "123"
	And there is NO error

Scenario: Split text using Char and Escape character
	Given A string to split with value "123|,45,1"
	And assign to variable "[[var]]" split type "Chars" at "," and Include "Unselected" and Escape "|"
	When the data split tool is executed
	Then the split result for "[[var]]" will be "123|,45"
	And there is NO error

