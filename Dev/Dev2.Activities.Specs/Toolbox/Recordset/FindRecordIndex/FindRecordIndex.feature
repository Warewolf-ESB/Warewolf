Feature: FindRecordIndex
	In order to search for pieces of data in a recordset
	As a Warewolf user
	I want a tool I can use to find an index 

Scenario: Find an index of data in a recordset
	Given I have the following recordset to search
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search type is "Starts With" and criteria is "Warewolf"
	When the find records index tool is executed
	Then the index result should be 5


	#Find records search type is = and record(s) exists
	#Find records search type is = and record(s) exists multiple results (indexes)
	#Find records search type is = and record(s) does not exist
	 
	#Find records search type is > and record(s) exists
	#Find records search type is > and record(s) exists multiple results (indexes)
	#Find records search type is > and record(s) does not exist
	
	#Find records search type is < and record(s) exists
	#Find records search type is < and record(s) exists multiple results (indexes)
	#Find records search type is < and record(s) does not exist

	#Find records search type is <> and record(s) exists
	#Find records search type is <> and record(s) exists multiple results (indexes)
	#Find records search type is <> and record(s) does not exist

	#Find records search type is >= and record(s) exists
	#Find records search type is >= and record(s) exists multiple results (indexes)
	#Find records search type is >= and record(s) does not exist

	#Find records search type is <= and record(s) exists
	#Find records search type is <= and record(s) exists multiple results (indexes)
	#Find records search type is <= and record(s) does not exist

	#Find records search type is Contains and record(s) exists
	#Find records search type is Contains and record(s) exists multiple results (indexes)
	#Find records search type is Contains and record(s) does not exist

	#Find records search type is Doesn't Contain and record(s) exists
	#Find records search type is Doesn't Contain and record(s) exists multiple results (indexes)
	#Find records search type is Doesn't Contain and record(s) does not exist

	#Find records search type is Ends with and record(s) exists
	#Find records search type is Ends with and record(s) exists multiple results (indexes)
	#Find records search type is Ends with and record(s) does not exist

	#Find records search type is Is Alpha numeric and record(s) exists
	#Find records search type is Is Alpha numeric and record(s) exists multiple results (indexes)
	#Find records search type is Is Alpha numeric and record(s) does not exist

	#Find records search type is Is Date and record(s) exists
	#Find records search type is Is Date and record(s) exists multiple results (indexes)
	#Find records search type is Is Date and record(s) does not exist

	#Find records search type is Is Email and record(s) exists
	#Find records search type is Is Email and record(s) exists multiple results (indexes)
	#Find records search type is Is Email and record(s) does not exist

	#Find records search type is Is Regex and record(s) exists
	#Find records search type is Is Regex and record(s) exists multiple results (indexes)
	#Find records search type is Is Regex and record(s) does not exist

	#Find records search type is Is Text and record(s) exists
	#Find records search type is Is Text and record(s) exists multiple results (indexes)
	#Find records search type is Is Text and record(s) does not exist

	#Find records search type is Is XML and record(s) exists
	#Find records search type is Is XML and record(s) exists multiple results (indexes)
	#Find records search type is Is XML and record(s) does not exist
	
	#Find records search type is Not Alphanumeric and record(s) exists
	#Find records search type is Not Alphanumeric and record(s) exists multiple results (indexes)
	#Find records search type is Not Alphanumeric and record(s) does not exist

	#Find records search type is Not Date and record(s) exists
	#Find records search type is Not Date and record(s) exists multiple results (indexes)
	#Find records search type is Not Date and record(s) does not exist

	#Find records search type is Not Numeric and record(s) exists
	#Find records search type is Not Numeric and record(s) exists multiple results (indexes)
	#Find records search type is Not Numeric and record(s) does not exist

	#Find records search type is Not Text and record(s) exists
	#Find records search type is Not Text and record(s) exists multiple results (indexes)
	#Find records search type is Not Text and record(s) does not exist

	#Find records search type is Not XML and record(s) exists
	#Find records search type is Not XML and record(s) exists multiple results (indexes)
	#Find records search type is Not XML and record(s) does not exist