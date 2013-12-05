Feature: FindRecordsetIndexMultiple
	In order to search for pieces of data in a recordset
	As a Warewolf user
	I want a tool I can use to find an index 

Scenario: Find an index of data in a recordset with Starts With
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |
	| rs().row | best     |
	| rs().row | Warewolf |
	| rs().row | user     |
	And search the recordset with type "Starts With" and criteria is "Warewolf"
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 5

#Scenario: Find an index of data in a recordset with Is Between
#	Given I have the following recordset to search for multiple criteria
#	| rs       | value    |
#	| rs().row | 1|
#	| rs().row | 15|
#	| rs().row | 20|	
#	| rs().row | 34|	
#	And search the recordset with type "Is Between" and criteria is "16" and "33"
#	When the find records index multiple tool is executed
#	Then the find records index multiple result should be 3
	
Scenario: Find an index of data in a recordset with Is Base64
	Given I have the following recordset to search for multiple criteria
	| rs       | value    |
	| rs().row | You      |
	| rs().row | are      |
	| rs().row | the      |	
	| rs().row | d2FyZXdvbGY= |	
	And search the recordset with type "Is Base64" and criteria is ""
	When the find records index multiple tool is executed
	Then the find records index multiple result should be 4

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