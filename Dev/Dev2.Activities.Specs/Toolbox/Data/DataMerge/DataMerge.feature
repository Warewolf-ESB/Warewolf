Feature: DataMerge
	In order to merge data
	As Warewolf user
	I want a tool that joins two or more pieces of data together

Scenario: Merge a scalar to a scalar using merge type none
	Given A variable "[[a]]" with a value "Warewolf " and merge type "None" and string at as ""	
	And A variable "[[b]]" with a value "Rocks" and merge type "None" and string at as ""	
	When the data merge tool is executed
	Then the merged result is "Warewolf Rocks"

#BB: Do we test using all merge types individualy and all together or one merge that uses all merge types?
#BB: How do we test merging a recordset? - it should repeat the pattern for every record if * is used. Can we define the recordset up front similar to SQl Bulk Insert tool?

#Scenario: Merge a recordset table and free text using all merge types

