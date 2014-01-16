Feature: DataMerge
	In order to merge data
	As Warewolf user
	I want a tool that joins two or more pieces of data together

Scenario: Merge a scalar to a scalar using merge type none
	Given a merge variable "[[a]]" equal to "Warewolf " 
	And a merge variable "[[b]]" equal to "Rocks"		
	And an Input "[[a]]" and merge type "None" and string at as "" and Padding "" and Alignment "Left"	
	And an Input "[[b]]" and merge type "None" and string at as "" and Padding "" and Alignment "Left"
	When the data merge tool is executed
	Then the merged result is "Warewolf Rocks"
	And the execution has "NO" error
	And the debug look like this
	| variable | value                                    |
	| Step     | DataMerge                                |
	| Type     | Assign                                   |
	| Inputs   | 1 Merge [[a]] = Warewolf With None       |
	|          | 1 Merge [[b]] = Rocks With None          |
	| Outputs  |  WarewolfRocks                           |

Scenario: Merge a recordset table and free text using None
	Given a merge recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 3   |	
	And an Input "[[rs(*).row]]0" and merge type "None" and string at as "" and Padding "" and Alignment "Left"	
	And an Input "0" and merge type "None" and string at as "" and Padding "" and Alignment "Left"
	When the data merge tool is executed
	Then the merged result is "100200300"
	And the execution has "NO" error

Scenario: Merge a recordset table and free text using Chars
	Given a merge recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 3   |	
	And an Input "[[rs(*).row]]" and merge type "Chars" and string at as "0" and Padding "" and Alignment "Left"	
	And an Input "0" and merge type "Chars" and string at as "0" and Padding "" and Alignment "Left"
	And an Input "0" and merge type "None" and string at as "" and Padding "" and Alignment "Left"
	When the data merge tool is executed
	Then the merged result is "100002000030000"
	And the execution has "NO" error

Scenario: Merge a recordset table and free text using New Line
	Given a merge recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 3   |	
	And an Input "[[rs(*).row]]" and merge type "New Line" and string at as "" and Padding "" and Alignment "Left"	
	And an Input "0" and merge type "New Line" and string at as "" and Padding "" and Alignment "Left"
	When the data merge tool is executed
	Then the merged result is the same as file "NewLineExample.txt"
	And the execution has "NO" error

Scenario: Merge a recordset table and free text using Tab
	Given a merge recordset
	| rs       | val |
	| rs().row | 1   |
	| rs().row | 2   |
	| rs().row | 3   |	
	And an Input "[[rs(*).row]]tab->" and merge type "Tab" and string at as "" and Padding "" and Alignment "Left"	
	And an Input "<-" and merge type "None" and string at as "" and Padding "" and Alignment "Left"
	When the data merge tool is executed
	Then the merged result is "1tab->	<-2tab->	<-3tab->	<-"
	And the execution has "NO" error

Scenario: Merge a variable using index that is a char
	Given a merge variable "[[a]]" equal to "aA " 	
	And an Input "[[a]]" and merge type "Index" and string at as "b" and Padding "" and Alignment "Left"
	When the data merge tool is executed
	Then the merged result is ""
	And the execution has "NO" error

Scenario: Merge a variable using index that is a variable and is blank
	Given a merge variable "[[a]]" equal to "aA "
	And a merge variable "[[b]]" equal to ""	
	And an Input "[[a]]" and merge type "Index" and string at as "[[b]]" and Padding "" and Alignment "Left"
	When the data merge tool is executed
	Then the merged result is ""
	And the execution has "AN" error

Scenario: Merge multiple variables on Chars with blank lines
	Given a merge variable "[[a]]" equal to "Warewolf " 
	And a merge variable "[[b]]" equal to "Rocks"	
	And an Input "[[a]]" and merge type "Chars" and string at as "|" and Padding " " and Alignment "Left"	
	And an Input "" and merge type "None" and string at as "" and Padding " " and Alignment "Left"	
	And an Input "[[b]]" and merge type "Chars" and string at as "|" and Padding " " and Alignment "Left"	
	And an Input "" and merge type "Chars" and string at as "|" and Padding " " and Alignment "Left"
	When the data merge tool is executed
	Then the merged result is "Warewolf |Rocks||"
	And the execution has "NO" error

Scenario: Merge a recordset that has xml data using Tabs
	Given a merge recordset
	| rs       | val                 |
	| rs().row | <x id="1">One</x>   |
	| rs().row | <x id="2">two</x>   |
	| rs().row | <x id="3">three</x> |	
	And an Input "<record>" and merge type "Tab" and string at as "" and Padding "" and Alignment "Left"		
	And an Input "[[rs(*).row]]" and merge type "Tab" and string at as "" and Padding "" and Alignment "Left"		
	And an Input "</record>" and merge type "None" and string at as "" and Padding "" and Alignment "Left"
	When the data merge tool is executed
	Then the merged result is "<record>	<x id="1">One</x>	</record><record>	<x id="2">two</x>	</record><record>	<x id="3">three</x>	</record>"
	And the execution has "NO" error

Scenario: Merge a short string using big index and padding and alignment
	Given a merge variable "[[a]]" equal to "Warewolf" 
	And a merge variable "[[b]]" equal to "123"
	And an Input "[[a]]" and merge type "Index" and string at as "10" and Padding " " and Alignment "Left"	
	And an Input "[[b]]" and merge type "Index" and string at as "5" and Padding "0" and Alignment "Right"
	When the data merge tool is executed
	Then the merged result is "Warewolf  00123"
	And the execution has "NO" error

Scenario: Merge a long string using small index and padding and alignment
	Given a merge variable "[[a]]" equal to "Warewolf" 
	And a merge variable "[[b]]" equal to "12345"
	And an Input "[[a]]" and merge type "Index" and string at as "3" and Padding " " and Alignment "Left"	
	And an Input "[[b]]" and merge type "Index" and string at as "3" and Padding "0" and Alignment "Right"
	When the data merge tool is executed
	Then the merged result is "War123"
	And the execution has "NO" error

	 
Scenario: Merge a long string using small index and padding and alignment at invalid index
	Given a merge variable "[[a]]" equal to "Warewolf" 
	And a merge variable "[[b]]" equal to "12345"
	And an Input "[[a]]" and merge type "Index" and string at as "-1" and Padding " " and Alignment "Left"	
	And an Input "[[b]]" and merge type "Index" and string at as "-1" and Padding "0" and Alignment "Right"
	When the data merge tool is executed
	Then the merged result is ""
	And the execution has "AN" error
	And the debug look like this
	| variable | value                                    |
	| Step     | DataMerge                                |
	| Type     | Assign                                   |
	| Inputs   | 1 Merge [[a]] = Warewolf With Index "-1" |
	|          | 1 Merge [[b]] = 12345 With Index "-1"    |
	| Outputs  |                                          |



Scenario: Merge a negative recordset index Input
	Given an Input "[[my(-1).a]]" and merge type "Index" and string at as "10" and Padding " " and Alignment "Left"	
	When the data merge tool is executed
	Then the execution has "AN" error

Scenario: Merge a negative recordset index for String At
	Given an Input "12" and merge type "Index" and string at as "[[my(-1).a]]" and Padding " " and Alignment "Left"	
	When the data merge tool is executed
	Then the execution has "AN" error

Scenario: Merge a negative recordset index for Padding
	Given an Input "12" and merge type "Index" and string at as "10" and Padding "[[my(-1).a]]" and Alignment "Left"	
	When the data merge tool is executed
	Then the execution has "AN" error

Scenario: Merge a variable using index that is a variable and is not blank
	Given a merge variable "[[a]]" equal to "aA "
	And a merge variable "[[b]]" equal to "bB "
	And a merge variable "[[c]]" equal to "1"	
	And an Input "[[a]]" and merge type "Index" and string at as "[[c]]" and Padding "" and Alignment "Left"
	And an Input "[[b]]" and merge type "Index" and string at as "[[c]]" and Padding "" and Alignment "Left"
	When the data merge tool is executed
	Then the merged result is "ab"
	And the execution has "NO" error
	And the debug look like this
	| variable | value                                 |
	| Step     | DataMerge                             |
	| Type     | Assign                                |
	| Inputs   | 1 Merge [[a]] = aA with Index [[c]]=1 |
	|          | 1 Merge [[b]] = bB with Index [[c]]=1 |
	| Outputs  | b                                     |

Scenario: Merge a variable using index that is blank
	Given a merge variable "[[a]]" equal to "aA "
	And a merge variable "[[b]]" equal to "bB "
	And a merge variable "[[c]]" equal to "1"	
	And an Input "[[a]]" and merge type "Index" and string at as "" and Padding "" and Alignment "Left"
	And an Input "[[b]]" and merge type "Index" and string at as "[[c]]" and Padding "" and Alignment "Left"
	When the data merge tool is executed
	Then the merged result is "b"
	And the execution has "AN" error
	And the debug look like this
	| variable | value                                 |
	| Step     | DataMerge                             |
	| Type     | Assign                                |
	| Inputs   | 1 Merge [[a]] = aA with Index ""      |
	|          | 1 Merge [[b]] = bB with Index [[c]]=1 |
	| Outputs  | b                                     |
	| Error    | 1 The At value cannot be blank        |