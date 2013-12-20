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
	And the data split execution has "NO" error

Scenario: Split characters using Index Going Backwards
	Given A string to split with value "@!?><":}{+_)(*&^~"
	And assign to variable "[[vowels(*).chars]]" split type "Index" at "7"
	And the direction is "RightToLeft"
	When the data split tool is executed
	Then the split result will be
	| vowels().chars		|
	| _)(*&^~	|
	| ><":}{+	|
	| @!?		|
	And the data split execution has "NO" error

Scenario: Split text using All split types - Some with Include selected
	Given A string to split with value "IndexTab	Chars,space end"
	And assign to variable "[[vowels().letters]]" split type "Index" at "5" and Include "Selected" and Escape '\'	
	And  assign to variable "[[vowels(*).letters]]" split type "Tab" at ""	
	And  assign to variable "[[vowels().letters]]" split type as "Chars" at "ars," and escape "" and include is "unselected"
	And  assign to variable "[[vowels().letters]]" split type as "Space" at "1" and escape "\" and include is "unselected"		
	And  assign to variable "[[vowels(*).letters]]" split type "End" at ""
	When the data split tool is executed
	Then the split result will be
	| vowels().letters |
	| Index		  |
	| Tab		  |
	| Chars,	  |
	| space		  |
	| end		  |
	And the data split execution has "NO" error

Scenario: Split CSV file format into recordset - some fields blank
	Given A file "CSVExample.txt" to split	
	And  assign to variable "[[rec().id]]" split type as "Chars" at "," and escape "" and include is "unselected"	
	And  assign to variable "[[rec().name]]" split type as "Chars" at "," and escape "" and include is "unselected"
	And  assign to variable "" split type as "Chars" at "," and escape "" and include is "unselected"
	And  assign to variable "[[rec().phone]]" split type "NewLine" at ""	
	And  assign to variable "" split type as "" at "" and escape "" and include is "unselected"
	When the data split tool is executed
	Then the split result will be
	| rec().id | rec().name | rec().phone |
	| ID       | NAME       | PHONE       |
	| 1        | Barney     | 1234        |
	| 2        | Tshepo     | 5678        |
	|          |            |             |
	| 3        | Mo         |             |
	And the data split execution has "NO" error

Scenario: Split CSV file format into recordset - Skip blank rows selected
	Given A file "CSVExample.txt" to split	
	And  assign to variable "[[rec().id]]" split type as "Chars" at "," and escape "" and include is "unselected"	
	And  assign to variable "[[rec().name]]" split type as "Chars" at "," and escape "" and include is "unselected"	
	And  assign to variable "" split type as "Chars" at "," and escape "" and include is "unselected"
	And  assign to variable "[[rec().phone]]" split type "NewLine" at ""	
	And  assign to variable "" split type as "" at "" and escape "" and include is "selected"
	When the data split tool is executed
	Then the split result will be
	| rec().id | rec().name | rec().phone |
	| ID       | NAME       | PHONE       |
	| 1        | Barney     | 1234        |
	| 2        | Tshepo     | 5678        |
	| 3        | Mo         | 01          |
	And the data split execution has "NO" error

Scenario: Split blank text using All split types
	Given A string to split with value ""
	And assign to variable "[[vowels().letters]]" split type "Index" at "5" and Include "Selected" and Escape '\'	
	And  assign to variable "[[vowels().letters]]" split type "Tab" at ""	
	And  assign to variable "[[vowels().letters]]" split type as "Chars" at "ars," and escape "" and include is "selected"	
	And  assign to variable "[[vowels().letters]]" split type as "Space" at "" and escape "\" and include is "unselected"
	And  assign to variable "[[vowels().letters]]" split type "End" at ""	
	And  assign to variable "[[vowels().letters]]" split type "NewLine" at ""
	When the data split tool is executed
	Then the split result will be
	| vowels().letters |
	And the data split execution has "NO" error

Scenario: Split text using Index where index > provided
	Given A string to split with value "123"	
	And  assign to variable "[[var]]" split type as "Index" at "5" and escape "\" and include is "selected"
	And  assign to variable "[[vowels().letters]]" split type as "Space" at "" and escape "\" and include is "unselected"
	When the data split tool is executed	
	Then the split result for "[[var]]" will be "123"
	And the data split execution has "NO" error

Scenario: Split text using Char and Escape character
	Given A string to split with value "123|,45,1"
	And assign to variable "[[var]]" split type "Chars" at "," and Include "Unselected" and Escape '|'
	When the data split tool is executed
	Then the split result for "[[var]]" will be "123|,45"
	And the data split execution has "NO" error

Scenario: Split blank text	
	Given A string to split with value ""
	And  assign to variable "[[vowels(*).letters]]" split type "Index" at "1"
	When the data split tool is executed
	Then the split result will be
	| vowels().letters |
	And the data split execution has "NO" error

Scenario: Split text to a recordset using a negative Index 
	Given A string to split with value "abcde"
	And  assign to variable "[[vowels(*).letters]]" split type "Index" at "-1"
	When the data split tool is executed
	Then the split result will be
	| vowels().letters |
	And the data split execution has "AN" error

Scenario: Split negative record index as Input
	Given A string to split with value "[[my(-1).var]]"
	And assign to variable "[[vowels().letters]]" split type "Index" at "5" and Include "Selected" and Escape '\'	
	When the data split tool is executed
	Then the data split execution has "AN" error

Scenario: Split text into negative recordset index
	Given A string to split with value "abcd"
	And assign to variable "[[vowels(-1).letters]]" split type "Index" at "5" and Include "Selected" and Escape '\'	
	When the data split tool is executed
	Then the data split execution has "AN" error

Scenario: Split text into negative recordset index as the index to split at
	Given A string to split with value "abcd"
	And assign to variable "[[vowels().letters]]" split type "Index" at "[[my(-1).index]]" and Include "Selected" and Escape '\'	
	When the data split tool is executed
	Then the data split execution has "AN" error

Scenario: Split text using a negative recordset index as escape character
	Given A string to split with value "abcd"
	And assign to variable "[[vowels().letters]]" split type "Index" at "2" and Include "Selected" and Escape '[[my(-1).escape]]'	
	When the data split tool is executed
	Then the data split execution has "AN" error


