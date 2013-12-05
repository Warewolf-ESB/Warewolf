Feature: DataMerge
	In order to merge data
	As Warewolf user
	I want a tool that joins two or more pieces of data together

Scenario: Merge a scalar to a scalar using merge type none
	Given A variable "[[a]]" with a value "Warewolf " and merge type "None" and string at as ""	
	And A variable "[[b]]" with a value "Rocks" and merge type "None" and string at as ""	
	When the data merge tool is executed
	Then the merged result is "Warewolf Rocks"

#Scenario: Merge a recordset table and free text using None
#Scenario: Merge a recordset table and free text using Chars
#Scenario: Merge a recordset table and free text using New Line
#Scenario: Merge a recordset table and free text using Tab
#Scenario: Merge a variable using index that is a char
#Scenario: Merge a variable using index that is a variable and is blank
#Scenario: Merge multiple variables on new lines with blank lines
#Scenario: Merge a recordset that has xml data using Tabs

