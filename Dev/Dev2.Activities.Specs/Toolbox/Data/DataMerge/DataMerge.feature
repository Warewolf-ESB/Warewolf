Feature: DataMerge
	In order to merge data
	As Warewolf user
	I want a tool that joins two or more pieces of data together

Scenario: Merge a scalar to a scalar using merge type none
	Given a merge variable "[[a]]" equal to "Warewolf " 
	And a merge variable "[[b]]" equal to "Rocks"
	And an Input "[[a]]" and merge type "None" and string at as ""	
	And an Input "[[b]]" and merge type "None" and string at as ""	
	When the data merge tool is executed
	Then the merged result is "Warewolf Rocks"
	And there is NO error

Scenario: Merge a recordset table and free text using None
	Given a merge recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 3   |
	And an Input "[[rs(*).row]]0" and merge type "None" and string at as ""
	And an Input "0" and merge type "None" and string at as ""
	When the data merge tool is executed
	Then the merged result is "100200300"
	And there is NO error

Scenario: Merge a recordset table and free text using Chars
	Given a merge recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 3   |
	And an Input "[[rs(*).row]]" and merge type "Chars" and string at as "0"
	And an Input "0" and merge type "Chars" and string at as "0"
	When the data merge tool is executed
	Then the merged result is "100020003000"
	And there is NO error

Scenario: Merge a recordset table and free text using New Line
	Given a merge recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 3   |
	And an Input "[[rs(*).row]]" and merge type "NewLine" and string at as ""
	And an Input "0" and merge type "NewLine" and string at as ""
	When the data merge tool is executed
	Then the merged result is the same as file "NewLineExample.txt"
	And there is NO error

Scenario: Merge a recordset table and free text using Tab
	Given a merge recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 3   |
	And an Input "[[rs(*).row]]tab->" and merge type "Tab" and string at as ""
	And an Input "<-" and merge type "None" and string at as ""
	When the data merge tool is executed
	Then the merged result is "1tab->	<-2tab->	<-3tab->	<-"
	And there is NO error

Scenario: Merge a variable using index that is a char
	Given a merge variable "[[a]]" equal to "aA " 
	And an Input "[[a]]" and merge type "Index" and string at as "b"	
	When the data merge tool is executed
	Then the merged result is ""
	And there is AN error

Scenario: Merge a variable using index that is a variable and is blank
	Given a merge variable "[[a]]" equal to "aA "
	And a merge variable "[[b]]" equal to ""
	And an Input "[[a]]" and merge type "Index" and string at as "[[b]]"	
	When the data merge tool is executed
	Then the merged result is ""
	And there is NO error

Scenario: Merge multiple variables on Chars with blank lines
	Given a merge variable "[[a]]" equal to "Warewolf " 
	And a merge variable "[[b]]" equal to "Rocks"
	And an Input "[[a]]" and merge type "Chars" and string at as "|"	
	And an Input "" and merge type "None" and string at as ""	
	And an Input "[[b]]" and merge type "Chars" and string at as "|"	
	And an Input "" and merge type "Chars" and string at as "|"	
	When the data merge tool is executed
	Then the merged result is "Warewolf |Rocks||"
	And there is NO error

Scenario: Merge a recordset that has xml data using Tabs
	Given a merge recordset
	| rs       | val                 |
	| rs().row | <x id="1">One</x>   |
	| rs().row | <x id="2">two</x>   |
	| rs().row | <x id="3">three</x> |
	And an Input "<recordset>" and merge type "Tab" and string at as ""
	And an Input "[[rs(*).row]]" and merge type "Tab" and string at as ""
	And an Input "</recordset>" and merge type "None" and string at as ""
	When the data merge tool is executed
	Then the merged result is "<record>	<x id="1">One</x>	</record><record>	<x id="2">two</x>	</record><record>	<x id="3">three</x>	</record>"
	And there is NO error

Scenario: Merge a short string using big index and padding and alignment
	Given a merge variable "[[a]]" equal to "Warewolf" 
	And a merge variable "[[b]]" equal to "123"
	And an Input "[[a]]" and merge type "Index" and string at as "10" and Padding " " and Alignment "Left"	
	And an Input "[[b]]" and merge type "Index" and string at as "5" and Padding "0" and Alignment "Right"
	When the data merge tool is executed
	Then the merged result is "Warewolf  00123"
	And there is NO error

Scenario: Merge a long string using small index and padding and allignment
	Given a merge variable "[[a]]" equal to "Warewolf" 
	And a merge variable "[[b]]" equal to "12345"
	And an Input "[[a]]" and merge type "Index" and string at as "3" and Padding " " and Alignment "Left"	
	And an Input "[[b]]" and merge type "Index" and string at as "3" and Padding "0" and Alignment "Right"
	When the data merge tool is executed
	Then the merged result is "War123"
	And there is NO error
